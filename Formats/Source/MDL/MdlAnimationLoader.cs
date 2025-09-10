using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using static uSource.Formats.Source.MDL.StudioStruct;
namespace uSource.Formats.Source.MDL
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AnimDescEntry
    {
        public mstudioanimdesc_t Desc;
        public long FilePos;
    }

    internal struct AnimBlockReader
    {
        public BinaryReader rdr;
        public long ofs;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct mdl_animblock_t
    {
        public int datastart, dataend;
    }

    internal static class MdlAnimationLoader
    {
        internal static AnimDescEntry[] ParseAnimDescs(BinaryReader br, studiohdr_t hdr)
        {
            int sz = Marshal.SizeOf<mstudioanimdesc_t>();
            var entries = new AnimDescEntry[hdr.localanim_count];
            long basePos = hdr.localanim_offset;

            for (int i = 0; i < entries.Length; ++i)
            {
                long pos = basePos + i * sz;
                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                entries[i].Desc = BinaryReaderExt.ReadStruct<mstudioanimdesc_t>(br);
                entries[i].FilePos = pos;
            }

            return entries;
        }

        internal static Dictionary<int, List<AnimationBone>> ReadAnimationData(
            BinaryReader mdl, AnimDescEntry[] entries, studiohdr_t hdr)
        {
            string mdlPath = (mdl.BaseStream as FileStream)?.Name;
            var aniMap = mdlPath != null
                ? LoadAnimBlocks(mdlPath, hdr)
                : new Dictionary<int, AnimBlockReader>();

            var result = new Dictionary<int, List<AnimationBone>>(entries.Length);
            int boneCount = hdr.bone_count;

            for (int animId = 0; animId < entries.Length; animId++)
            {
                var entry = entries[animId];
                var desc = entry.Desc;

                // Wybór źródła
                Stream src;
                long ofs;
                if (desc.animblock == 0)
                {
                    src = mdl.BaseStream;
                    ofs = desc.animindex != 0 ? entry.FilePos + desc.animindex : desc.baseptr;
                }
                else
                {
                    if (!aniMap.TryGetValue(desc.animblock, out var blk))
                        continue;
                    src = blk.rdr.BaseStream;
                    ofs = blk.ofs + desc.animindex;
                }

                if (ofs <= 0 || ofs >= src.Length)
                    continue;

                src.Seek(ofs, SeekOrigin.Begin);
                var reader = new uReader(src);

                int headerCount = desc.numlocalhierarchy > 0 ? (int)desc.numlocalhierarchy : boneCount;
                var animHeaders = ReadArraySafe<HashFuncs.mstudioanim_t>(reader, headerCount);

                var bones = new List<AnimationBone>();
                int frames = Mathf.Max(1, (int)desc.numframes);

                foreach (var h in animHeaders)
                {
                    // Jeżeli bone index jest niepoprawny – pomiń
                    if (h.bone < 0 || h.bone >= boneCount)
                        continue;

                    var ab = new AnimationBone(h.bone, h.flags, frames);

                    // Rotacje animowane
                    if ((ab.Flags & STUDIO_ANIM_ANIMROT) != 0)
                        AnimationParser.ReadAnimationFrameValues(reader, hdr, ab, h, frames, true,
                            (ab.Flags & STUDIO_ANIM_DELTA) != 0);

                    // Pozycje animowane
                    if ((ab.Flags & STUDIO_ANIM_ANIMPOS) != 0)
                        AnimationParser.ReadAnimationFrameValues(reader, hdr, ab, h, frames, false,
                            (ab.Flags & STUDIO_ANIM_DELTA) != 0);

                    // Raw rotacje
                    if ((ab.Flags & STUDIO_ANIM_RAWROT) != 0)
                    {
                        var q48 = new Quaternion48();
                        reader.ReadTypeFixed(ref q48, 6);
                        ab.pQuat48 = q48.quaternion;
                    }

                    if ((ab.Flags & STUDIO_ANIM_RAWROT2) != 0)
                    {
                        var q64 = new Quaternion64();
                        reader.ReadTypeFixed(ref q64, 8);
                        ab.pQuat64 = q64.quaternion;
                    }

                    // Raw pozycje
                    if ((ab.Flags &STUDIO_ANIM_RAWPOS) != 0)
                    {
                        var v48 = new FVector48();
                        reader.ReadTypeFixed(ref v48, 6);
                        ab.pVec48 = v48.ToVector3();
                    }

                    // Uzupełnianie braków do pełnej liczby klatek
                    while (ab.FrameAngles.Count < frames)
                        ab.FrameAngles.Add(Vector3.zero);
                    while (ab.FramePositions.Count < frames)
                        ab.FramePositions.Add(Vector3.zero);

                    bones.Add(ab);
                }

                // Jeżeli nie ma żadnych kości animowanych – dodaj puste
                if (bones.Count == 0)
                {
                    for (int b = 0; b < boneCount; b++)
                        bones.Add(new AnimationBone((byte)b, 0, frames));
                }

                result[animId] = bones;
            }

            foreach (var v in aniMap.Values)
                v.rdr.Dispose();

            return result;
        }

        private static Dictionary<int, AnimBlockReader> LoadAnimBlocks(string mdlPath, studiohdr_t hdr)
        {
            var map = new Dictionary<int, AnimBlockReader>();
            if (hdr.animblocks_count == 0) return map;

            string ani = Path.ChangeExtension(mdlPath, ".ani");
            if (!File.Exists(ani)) return map;

            var blocks = new mdl_animblock_t[hdr.animblocks_count];
            using (var br = new BinaryReader(File.OpenRead(mdlPath)))
            {
                br.BaseStream.Seek(hdr.animblocks_index, SeekOrigin.Begin);
                for (int i = 0; i < blocks.Length; ++i)
                    blocks[i] = BinaryReaderExt.ReadStruct<mdl_animblock_t>(br);
            }

            var fs = File.OpenRead(ani);
            var rdr = new BinaryReader(fs);
            for (int i = 0; i < blocks.Length; ++i)
                if (blocks[i].datastart >= 0 && blocks[i].datastart < fs.Length)
                    map[i] = new AnimBlockReader { rdr = rdr, ofs = blocks[i].datastart };
            return map;
        }

        private static T[] ReadArraySafe<T>(uReader r, int count) where T : struct
        {
            var arr = new T[count];
            for (int i = 0; i < count; ++i)
                arr[i] = BinaryReaderExt.ReadStruct<T>(r);
            return arr;
        }
    


        private static Quaternion NormalizeQuaternion(Quaternion q)
        {
            if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w))
                return Quaternion.identity;

            return Quaternion.Normalize(q);
        }
    }
}
        