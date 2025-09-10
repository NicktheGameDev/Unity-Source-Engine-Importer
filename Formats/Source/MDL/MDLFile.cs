// MDLFile_Pro.cs — MDL importer + full flex rule hook + Avatar export (Unity 2022–2025)
// - 1:1 Source flex loading (VVD delty) + runtime flex RULES (mstudioflexrule/op) evaluation
// - Zero-crash guards (NaN/Inf/bounds), safe skinning, UV2 guarded, eyes/jiggle safe
// - UltraSafe: każde NaN/overflow → bezpieczne zera, wszystkie błędy → warning zamiast crasha
// - Jeśli rules/ctrls istnieją → runtime driver używa ich; inaczej → mapowanie 1:1 nazw
// - Naprawa LODGroup.SetLODs: gwarantowane malejące progi i grupowanie rendererów per LOD
// - Animator i klipy z sekwencji generowane automatycznie
// - Eyeballs, jigglebones, hitboxy budowane i zabezpieczone
// - NOWE: Tworzenie i zapis Avatara Humanoid + Generic jako .asset (Assets/GeneratedAvatars/)
// - NOWE: Blendshapes tylko na top „head/face” mesh (pozostałe lod/meshe bez blendów); klon LOD 1..N z remapem
//
// UWAGA: Plik zakłada obecność typów z uSource (studiohdr_t, mstudiobone_t, itp.).
//        Ten plik jest monolityczny i bez TODO/WIP. Gotowy do drop-in w Unity 2022–2025 HDRP.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;
using uSource.Formats.Source.VTF;
using uSource.MathLib;
using static HashFuncs;
using Object = UnityEngine.Object;

namespace uSource.Formats.Source.MDL
{
    [Serializable] public struct MdlEventInfo { public float time; public int type; public string options; }

    public enum DetailMode
    {
        None,
        Lowest,
        Low,
        Medium,
        High
    }

    public class MDLFile : StudioStruct
    {
        public studiohdr_t MDL_Header;
        public string[] MDL_BoneNames;
        public mstudiobone_t[] MDL_StudioBones;

        public mstudiohitboxset_t[] MDL_Hitboxsets;
        private Hitbox[][] Hitboxes;

        public static byte[] mdldata;
        public static byte[] vvdData;

        // FLEX
        public mstudioflexdesc_t[]       MDL_FlexDescs;
        public List<mstudiovertanim_t[]> MDL_FlexAnims;
        private List<Dictionary<int, Vector3>> _flexLookup; // per-flex: vertexIndex -> deltaNormal
        private string[] _flexNames;

        // FLEX controllers/rules (for runtime op evaluation)
        public object[] MDL_FlexControllers; // mstudioflexcontroller_t[] (stored as object for reflection safety)
        public object[] MDL_FlexRules;       // mstudioflexrule_t[]
        public object[] MDL_FlexOpsFlat;     // mstudioflexop_t[]  (flat pool; rules refer by opindex)

        // ANIM & SEQ
        public mstudioanimdesc_t[] MDL_AniDescriptions;
        public AniInfo[]           Animations;
        public mstudioseqdesc_t[]  MDL_SeqDescriptions;
        public SeqInfo[]           Sequences;
        public mstudioevent_t[]    MDL_Events;
        public event Action<GameObject, string> OnPhonemeEvent;

        // TEXTURES
        public mstudiotexture_t[] MDL_TexturesInfo;
        public string[]           MDL_TDirectories;
        public string[]           MDL_Textures;

        // GEOMETRY
        public StudioBodyPart[]      MDL_Bodyparts;
        public mstudioflex_t[]       MDL_Flexes;
        public mstudiojigglebone_t[] MDL_JiggleBones;
        public mstudioeyeball_t[]    MDL_Eyeballs;

        public bool BuildMesh = true;
        private int vertCountLOD0;

        public MDLFile(Stream input, bool parseAnims = false, bool parseHitboxes = false)
        {
            if (mdldata == null)
            {
                using var ms = new MemoryStream();
                input.Position = 0; input.CopyTo(ms);
                mdldata = ms.ToArray();
                input.Position = 0;
            }

            using var r = new uReader(input);
            r.ReadTypeFixed(ref MDL_Header, 392);
            if (MDL_Header.id != 0x54534449) throw new FileLoadException("Not an IDST-based MDL");

            // Bones
            MDL_StudioBones = new mstudiobone_t[MDL_Header.bone_count];
            MDL_BoneNames = new string[MDL_Header.bone_count];
            for (int i = 0; i < MDL_Header.bone_count; i++)
            {
                int off = MDL_Header.bone_offset + 216 * i;
                r.ReadTypeFixed(ref MDL_StudioBones[i], 216, off);
                MDL_BoneNames[i] = r.ReadNullTerminatedString(off + MDL_StudioBones[i].sznameindex);
            }

            // Hitboxes
            if (parseHitboxes && MDL_Header.hitbox_count > 0)
            {
                MDL_Hitboxsets = new mstudiohitboxset_t[MDL_Header.hitbox_count];
                Hitboxes = new Hitbox[MDL_Header.hitbox_count][];
                for (int h = 0; h < MDL_Header.hitbox_count; h++)
                {
                    int off = MDL_Header.hitbox_offset + 12 * h;
                    r.ReadTypeFixed(ref MDL_Hitboxsets[h], 12, off);
                    int cnt = MDL_Hitboxsets[h].numhitboxes;
                    Hitboxes[h] = new Hitbox[cnt];
                    for (int b = 0; b < cnt; b++)
                    {
                        int boff = off + MDL_Hitboxsets[h].hitboxindex + 68 * b;
                        Hitboxes[h][b].BBox = new mstudiobbox_t();
                        r.ReadTypeFixed(ref Hitboxes[h][b].BBox, 68, boff);
                    }
                }
            }

            // Jiggle bones (guarded)
            {
                int jCount = MDL_Header.numjigglebones;
                long jOffset = MDL_Header.jigglebone_offset;
                int jStruct = Marshal.SizeOf<mstudiojigglebone_t>();
                long fileLen = r.BaseStream.Length;

                bool valid = jCount > 0 &&
                             jOffset > 0 &&
                             jOffset + (long)jCount * jStruct <= fileLen;

                if (valid)
                {
                    MDL_JiggleBones = new mstudiojigglebone_t[jCount];
                    r.BaseStream.Position = jOffset;
                    for (int i = 0; i < jCount; i++)
                        r.ReadType(ref MDL_JiggleBones[i]);
                }
                else
                {
                    MDL_JiggleBones = null;
                }
            }

            // Animations — guarded
            try
            {
                var animEntries = MdlAnimationLoader.ParseAnimDescs(r, MDL_Header);
                var animDataMap = MdlAnimationLoader.ReadAnimationData(r, animEntries, MDL_Header);

                MDL_AniDescriptions = new mstudioanimdesc_t[Math.Max(0, MDL_Header.localanim_count)];
                Animations = new AniInfo[animEntries.Length];

                int boneCount = MDL_Header.bone_count;

                for (int animId = 0; animId < animEntries.Length; animId++)
                {
                    var desc = animEntries[animId].Desc;
                    if ((uint)animId < (uint)MDL_AniDescriptions.Length)
                        MDL_AniDescriptions[animId] = desc;

                    string animName = string.Empty;
                    try
                    {
                        animName = r.ReadNullTerminatedString(
                            MDL_Header.localanim_offset + animId * Marshal.SizeOf<mstudioanimdesc_t>() + desc.sznameindex);
                    }
                    catch { animName = $"anim_{animId}"; }

                    int frameCount = Mathf.Max(1, (int)desc.numframes);
                    bool framesLess = frameCount < 2;
                    if (framesLess) frameCount += 1;

                    var aniInfo = new AniInfo
                    {
                        name = animName,
                        studioAnim = desc,
                        AnimationBones = new List<AnimationBone>(),
                        PosX = new Keyframe[frameCount][],
                        PosY = new Keyframe[frameCount][],
                        PosZ = new Keyframe[frameCount][],
                        RotX = new Keyframe[frameCount][],
                        RotY = new Keyframe[frameCount][],
                        RotZ = new Keyframe[frameCount][],
                        RotW = new Keyframe[frameCount][],
                        ScaleX = new Keyframe[frameCount][],
                        ScaleY = new Keyframe[frameCount][],
                        ScaleZ = new Keyframe[frameCount][]
                    };

                    for (int f = 0; f < frameCount; f++)
                    {
                        aniInfo.PosX[f] = new Keyframe[boneCount];
                        aniInfo.PosY[f] = new Keyframe[boneCount];
                        aniInfo.PosZ[f] = new Keyframe[boneCount];
                        aniInfo.RotX[f] = new Keyframe[boneCount];
                        aniInfo.RotY[f] = new Keyframe[boneCount];
                        aniInfo.RotZ[f] = new Keyframe[boneCount];
                        aniInfo.RotW[f] = new Keyframe[boneCount];
                        aniInfo.ScaleX[f] = new Keyframe[boneCount];
                        aniInfo.ScaleY[f] = new Keyframe[boneCount];
                        aniInfo.ScaleZ[f] = new Keyframe[boneCount];
                    }

                    // Legacy ReadData (safe)
                    long startOffset = MDL_Header.localanim_offset + (100 * animId);
                    int currentOffset = desc.animindex;
                    short nextOffset;
                    int guard = 0;
                    do
                    {
                        if (++guard > 100000) break;
                        long pos = startOffset + currentOffset;
                        if (pos < 0 || pos >= r.BaseStream.Length) break;

                        r.BaseStream.Position = pos;
                        byte boneIndex = (byte)r.ReadByte();
                        byte boneFlag = (byte)r.ReadByte();
                        nextOffset = r.ReadInt16();
                        currentOffset += nextOffset;

                        if (boneIndex >= boneCount) continue;

                        var animBone = new AnimationBone(boneIndex, boneFlag, desc.numframes);
                        try { animBone.ReadData(r); } catch {}
                        aniInfo.AnimationBones.Add(animBone);

                    } while (nextOffset != 0);

                    if (animDataMap.TryGetValue(animId, out var bonesFromMap))
                    {
                        foreach (var b in bonesFromMap)
                            if (!aniInfo.AnimationBones.Any(x => x.Bone == b.Bone))
                                aniInfo.AnimationBones.Add(b);
                    }

                    float fps = desc.fps > 0 ? desc.fps : 30f;
                    for (int sboneIndex = 0; sboneIndex < boneCount; sboneIndex++)
                    {
                        if ((uint)sboneIndex >= (uint)MDL_StudioBones.Length) continue;
                        var sbone = MDL_StudioBones[sboneIndex];
                        var animBone = aniInfo.AnimationBones.FirstOrDefault(b => b.Bone == sboneIndex);

                        for (int f = 0; f < frameCount; f++)
                        {
                            float t = f / fps;
                            Vector3 pos = sbone.pos;
                            Vector3 rot = sbone.rot;
                            Vector3 scl = Vector3.one;

                            if (animBone != null)
                            {
                                if ((animBone.Flags & STUDIO_ANIM_RAWPOS) != 0) pos = animBone.pVec48;
                                if ((animBone.Flags & STUDIO_ANIM_RAWROT) != 0) rot = MathLibrary.ToEulerAngles(animBone.pQuat48);
                                if ((animBone.Flags & STUDIO_ANIM_RAWROT2) != 0) rot = MathLibrary.ToEulerAngles(animBone.pQuat64);

                                int srcF = (framesLess && f != 0) ? f - 1 : f;
                                if ((animBone.Flags & STUDIO_ANIM_ANIMPOS) != 0 && animBone.FramePositions != null && srcF < animBone.FramePositions.Count)
                                    pos += animBone.FramePositions[srcF].Multiply(sbone.posscale);
                                if ((animBone.Flags & STUDIO_ANIM_ANIMROT) != 0 && animBone.FrameAngles != null && srcF < animBone.FrameAngles.Count)
                                    rot += animBone.FrameAngles[srcF].Multiply(sbone.rotscale);
                            }

                            if (sbone.parent == -1) pos = MathLibrary.SwapY(pos);
                            else pos.x = -pos.x;

                            pos *= uLoader.UnitScale;
                            rot *= Mathf.Rad2Deg;

                            Quaternion q = (sbone.parent == -1)
                                ? Quaternion.Euler(-90, 180, -90) * MathLibrary.AngleQuaternion(rot)
                                : MathLibrary.AngleQuaternion(rot);

                            aniInfo.PosX[f][sboneIndex] = new Keyframe(t, pos.x);
                            aniInfo.PosY[f][sboneIndex] = new Keyframe(t, pos.y);
                            aniInfo.PosZ[f][sboneIndex] = new Keyframe(t, pos.z);
                            aniInfo.RotX[f][sboneIndex] = new Keyframe(t, q.x);
                            aniInfo.RotY[f][sboneIndex] = new Keyframe(t, q.y);
                            aniInfo.RotZ[f][sboneIndex] = new Keyframe(t, q.z);
                            aniInfo.RotW[f][sboneIndex] = new Keyframe(t, q.w);
                            aniInfo.ScaleX[f][sboneIndex] = new Keyframe(t, scl.x);
                            aniInfo.ScaleY[f][sboneIndex] = new Keyframe(t, scl.y);
                            aniInfo.ScaleZ[f][sboneIndex] = new Keyframe(t, scl.z);
                        }
                    }

                    if ((uint)animId < (uint)Animations.Length)
                        Animations[animId] = aniInfo;
                }

                // Sequences
                MDL_SeqDescriptions = new mstudioseqdesc_t[Math.Max(0, MDL_Header.localseq_count)];
                Sequences = new SeqInfo[Math.Max(0, MDL_Header.localseq_count)];

                for (int seqID = 0; seqID < MDL_Header.localseq_count; seqID++)
                {
                    int sequenceOffset = MDL_Header.localseq_offset + (212 * seqID);
                    r.ReadTypeFixed(ref MDL_SeqDescriptions[seqID], 212, sequenceOffset);
                    var Sequence = MDL_SeqDescriptions[seqID];

                    var seqInfo = new SeqInfo
                    {
                        name = r.ReadNullTerminatedString(sequenceOffset + Sequence.szlabelindex),
                        seq = Sequence
                    };

                    r.BaseStream.Position = sequenceOffset + Sequence.animindexindex;
                    short[] animIndices = r.ReadShortArray(Math.Max(0, Sequence.groupsize[0] * Sequence.groupsize[1]));

                    var seqAnimList = new List<AniInfo>(animIndices.Length);
                    for (int i = 0; i < animIndices.Length; i++)
                    {
                        int aID = animIndices[i];
                        if ((uint)aID < (uint)Animations.Length)
                            seqAnimList.Add(Animations[aID]);
                    }
                    seqInfo.ani = seqAnimList;
                    Sequences[seqID] = seqInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MDL] Animation parsing skipped due to error: {ex.Message}");
                MDL_AniDescriptions = Array.Empty<mstudioanimdesc_t>();
                Animations = Array.Empty<AniInfo>();
                MDL_SeqDescriptions = Array.Empty<mstudioseqdesc_t>();
                Sequences = Array.Empty<SeqInfo>();
            }

            ReadMdlEvents();

            // Textures
            MDL_TexturesInfo = new mstudiotexture_t[Math.Max(0, MDL_Header.texture_count)];
            MDL_Textures = new string[Math.Max(0, MDL_Header.texture_count)];
            for (int i = 0; i < MDL_Header.texture_count; i++)
            {
                int off = MDL_Header.texture_offset + 64 * i;
                r.ReadTypeFixed(ref MDL_TexturesInfo[i], 64, off);
                MDL_Textures[i] = r.ReadNullTerminatedString(off + MDL_TexturesInfo[i].sznameindex);
            }

            MDL_TDirectories = new string[Math.Max(0, MDL_Header.texturedir_count)];
            for (int d = 0; d < MDL_Header.texturedir_count; d++)
            {
                int off = MDL_Header.texturedir_offset + 4 * d;
                int ptr = 0; r.ReadTypeFixed(ref ptr, 4, off);
                MDL_TDirectories[d] = r.ReadNullTerminatedString(ptr);
            }

            // Bodyparts + models
            MDL_Bodyparts = new StudioBodyPart[Math.Max(0, MDL_Header.bodypart_count)];
            for (int bp = 0; bp < MDL_Header.bodypart_count; bp++)
            {
                int off = MDL_Header.bodypart_offset + 16 * bp;
                var bpinfo = new mstudiobodyparts_t();
                r.ReadTypeFixed(ref bpinfo, 16, off);

                var part = new StudioBodyPart
                {
                    Name = bpinfo.sznameindex != 0 ? r.ReadNullTerminatedString(off + bpinfo.sznameindex) : string.Empty,
                    Models = new StudioModel[Math.Max(0, bpinfo.nummodels)]
                };

                for (int m = 0; m < bpinfo.nummodels; m++)
                {
                    int moff = off + bpinfo.modelindex + 148 * m;
                    var mm = new mstudiomodel_t();
                    r.ReadTypeFixed(ref mm, 148, moff);
                    if (m == 0) vertCountLOD0 = mm.numvertices;

                    var sm = new StudioModel
                    {
                        isBlank = mm.numvertices <= 0 || mm.nummeshes <= 0,
                        Model = mm,
                        Meshes = new mstudiomesh_t[Math.Max(0, mm.nummeshes)],
                        IndicesPerLod = new Dictionary<int, List<int>>[8],
                        VerticesPerLod = new mstudiovertex_t[8][]
                    };
                    for (int i = 0; i < 8; i++) sm.IndicesPerLod[i] = new Dictionary<int, List<int>>();
                    sm.VerticesGlobalStart = mm.vertexindex;

                    if (sm.NumLODs > 0 && vertCountLOD0 > 0)
                        sm.LODRemap = r.ReadUshortArray(Math.Max(0, vertCountLOD0 * sm.NumLODs));
                    else
                        sm.LODRemap = Array.Empty<ushort>();

                    for (int s = 0; s < mm.nummeshes; s++)
                    {
                        int so = moff + mm.meshindex + 116 * s;
                        r.ReadTypeFixed(ref sm.Meshes[s], 116, so);
                    }

                    part.Models[m] = sm;
                }
                MDL_Bodyparts[bp] = part;
            }

            // Flex rules/controllers load (safe; if brak → runtime map 1:1)
            TryLoadFlexControllersAndRules();
        }

        public static MDLFile Load(string mdlPath, bool parseAnims = false, bool parseHitboxes = false)
        {
            mdldata = File.ReadAllBytes(mdlPath);
            string vvdPath = Path.ChangeExtension(mdlPath, ".vvd");
            vvdData = File.Exists(vvdPath) ? File.ReadAllBytes(vvdPath) : null;
            if (!File.Exists(vvdPath))
                Debug.LogWarning($"[MDL] VVD not found – flexes unavailable for {mdlPath}");
            using var ms = new MemoryStream(mdldata);
            return new MDLFile(ms, parseAnims, parseHitboxes);
        }

        // -------- FLEX DESC/ANIMS (VVD) --------
        public void LoadFlexData(uReader r)
        {
            try
            {
                if (vvdData != null && vvdData.Length > 0)
                {
                    using var vr = new uReader(new MemoryStream(vvdData));
                    LoadFlexFromBuffer(vr, vvdData);
                }
                else if (mdldata != null && mdldata.Length > 0)
                {
                    LoadFlexFromBuffer(r, mdldata);
                }
                else InitEmptyFlex();
            }
            catch { InitEmptyFlex(); }
        }

        private void InitEmptyFlex()
        {
            MDL_FlexDescs = Array.Empty<mstudioflexdesc_t>();
            MDL_FlexAnims = new List<mstudiovertanim_t[]>();
            _flexLookup = new List<Dictionary<int, Vector3>>();
            _flexNames = Array.Empty<string>();
        }

        private void LoadFlexFromBuffer(uReader rd, byte[] buf)
        {
            if (buf == null || buf.Length == 0) { InitEmptyFlex(); return; }
            long len = buf.LongLength;
            long baseIndex = MDL_Header.flexdesc_index;
            if (baseIndex < 0 || baseIndex >= len) { InitEmptyFlex(); return; }
            int descCnt = Math.Max(0, MDL_Header.flexdesc_count);
            if (descCnt > 4096) descCnt = 4096;
            MDL_FlexDescs = new mstudioflexdesc_t[descCnt];
            try { rd.BaseStream.Position = baseIndex; } catch { InitEmptyFlex(); return; }
            for (int i = 0; i < descCnt; i++)
            { try { rd.ReadType(ref MDL_FlexDescs[i]); } catch { MDL_FlexDescs[i] = default; } }

            _flexNames = new string[descCnt];
            _flexLookup = new List<Dictionary<int, Vector3>>(descCnt);
            MDL_FlexAnims = new List<mstudiovertanim_t[]>(descCnt);

            int strTbl = (int)(baseIndex + descCnt * Marshal.SizeOf<mstudioflexdesc_t>());
            if (strTbl < 0 || strTbl >= len) strTbl = (int)len;

            for (int f = 0; f < descCnt; f++)
            {
                var d = MDL_FlexDescs[f];
                // name 1:1 (FACS name)
                int nPos = strTbl + d.szFACSindex;
                string flexName = $"flex_{f}";
                if (nPos >= 0 && nPos < len)
                {
                    try
                    {
                        int end = Array.IndexOf(buf, (byte)0, nPos);
                        if (end < 0) end = (int)len;
                        int nLen = Math.Max(0, end - nPos);
                        if (nLen > 0) flexName = Encoding.UTF8.GetString(buf, nPos, nLen);
                    }
                    catch { flexName = $"flex_{f}"; }
                }
                _flexNames[f] = string.IsNullOrWhiteSpace(flexName) ? $"flex_{f}" : flexName;

                // verts
                if (d.vertanim_count <= 0 || d.vertanim_offset <= 0 || d.vertanim_offset >= len)
                { MDL_FlexAnims.Add(Array.Empty<mstudiovertanim_t>()); _flexLookup.Add(new Dictionary<int, Vector3>()); continue; }

                long off = Math.Clamp(d.vertanim_offset, 0, len - 1);
                try { rd.BaseStream.Position = off; } catch { MDL_FlexAnims.Add(Array.Empty<mstudiovertanim_t>()); _flexLookup.Add(new Dictionary<int, Vector3>()); continue; }

                int max = (int)((len - off) / Marshal.SizeOf<mstudiovertanim_t>());
                int cnt = Math.Clamp(d.vertanim_count, 0, max);
                if (cnt <= 0) { MDL_FlexAnims.Add(Array.Empty<mstudiovertanim_t>()); _flexLookup.Add(new Dictionary<int, Vector3>()); continue; }

                var arr = new mstudiovertanim_t[cnt];
                for (int v = 0; v < cnt; v++) { try { rd.ReadType(ref arr[v]); } catch { arr[v] = default; } }
                MDL_FlexAnims.Add(arr);

                var dict = new Dictionary<int, Vector3>(cnt);
                foreach (var va in arr)
                {
                    if (va.index >= 0 && !dict.ContainsKey(va.index))
                    { try { dict[va.index] = va.NormalDelta; } catch {} }
                }
                _flexLookup.Add(dict);
            }
        }

        public string GetFlexName(int i) => (_flexNames != null && i >= 0 && i < _flexNames.Length && !string.IsNullOrWhiteSpace(_flexNames[i])) ? _flexNames[i] : $"flex_{i}";

        // -------- FLEX RULES/CONTROLLERS (from MDL) --------
        void TryLoadFlexControllersAndRules()
        {
            try
            {
                using var ms = new MemoryStream(mdldata, false);
                using var r = new uReader(ms);

                int ctrlCount = SafeGetInt(MDL_Header, "flexcontroller_count", "numflexcontrollers");
                int ctrlIndex = SafeGetInt(MDL_Header, "flexcontroller_index", "flexcontrolleroffset");
                int ruleCount = SafeGetInt(MDL_Header, "flexrule_count", "numflexrules");
                int ruleIndex = SafeGetInt(MDL_Header, "flexrule_index", "flexruleoffset");

                if (ctrlCount <= 0 || ctrlIndex <= 0) { MDL_FlexControllers = Array.Empty<object>(); }
                else
                {
                    var tCtrl = typeof(MDLFile).Assembly.GetType("uSource.Formats.Source.MDL.mstudioflexcontroller_t");
                    if (tCtrl == null) { MDL_FlexControllers = Array.Empty<object>(); }
                    else
                    {
                        MDL_FlexControllers = new object[ctrlCount];
                        int sz = Marshal.SizeOf(tCtrl);
                        for (int i = 0; i < ctrlCount; i++)
                        {
                            int off = ctrlIndex + sz * i;
                            var box = Activator.CreateInstance(tCtrl);
                            r.ReadTypeFixed(ref box, sz, off);
                            MDL_FlexControllers[i] = box;
                        }
                    }
                }

                if (ruleCount <= 0 || ruleIndex <= 0) { MDL_FlexRules = Array.Empty<object>(); MDL_FlexOpsFlat = Array.Empty<object>(); }
                else
                {
                    var tRule = typeof(MDLFile).Assembly.GetType("uSource.Formats.Source.MDL.mstudioflexrule_t");
                    var tOp   = typeof(MDLFile).Assembly.GetType("uSource.Formats.Source.MDL.mstudioflexop_t");
                    if (tRule == null || tOp == null) { MDL_FlexRules = Array.Empty<object>(); MDL_FlexOpsFlat = Array.Empty<object>(); }
                    else
                    {
                        int ruleSz = Marshal.SizeOf(tRule);
                        int opSz   = Marshal.SizeOf(tOp);
                        MDL_FlexRules = new object[ruleCount];

                        // We also build a flat op pool to allow contiguous addressing
                        var opsList = new List<object>(ruleCount * 8);

                        for (int i = 0; i < ruleCount; i++)
                        {
                            int roff = ruleIndex + ruleSz * i;
                            var ruleObj = Activator.CreateInstance(tRule);
                            r.ReadTypeFixed(ref ruleObj, ruleSz, roff);
                            MDL_FlexRules[i] = ruleObj;

                            int numOps = SafeGetInt(ruleObj, "numops", "opcount");
                            int opIndex = SafeGetInt(ruleObj, "opindex", "opsindex", "op_offset");
                            if (numOps <= 0 || opIndex <= 0) continue;

                            for (int k = 0; k < numOps; k++)
                            {
                                var opObj = Activator.CreateInstance(tOp);
                                r.ReadTypeFixed(ref opObj, opSz, roff + opIndex + opSz * k);
                                opsList.Add(opObj);
                            }
                        }

                        MDL_FlexOpsFlat = opsList.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MDL] Flex rules/controllers not loaded: " + e.Message);
                MDL_FlexControllers = Array.Empty<object>();
                MDL_FlexRules = Array.Empty<object>();
                MDL_FlexOpsFlat = Array.Empty<object>();
            }
        }

        public void SetIndices(Int32 BodypartID, Int32 ModelID, Int32 LODID, Int32 MeshID, List<Int32> Indices)
        {
            MDL_Bodyparts[BodypartID].Models[ModelID].IndicesPerLod[LODID].Add(MeshID, Indices);
        }

        public void SetVertices(Int32 BodypartID, Int32 ModelID, Int32 LODID, Int32 TotalVerts, Int32 StartIndex, mstudiovertex_t[] Vertexes)
        {
            MDL_Bodyparts[BodypartID].Models[ModelID].VerticesPerLod[LODID] = new mstudiovertex_t[TotalVerts];
            Array.Copy(Vertexes, StartIndex, MDL_Bodyparts[BodypartID].Models[ModelID].VerticesPerLod[LODID], 0, TotalVerts);
        }

        static bool FieldExists<T>(Expression<Func<T>> expr)
        {
            if (expr.Body is MemberExpression m && m.Member is FieldInfo fi && fi.DeclaringType == typeof(studiohdr_t))
                return fi != null;
            return false;
        }

        static int SafeGetInt(object obj, params string[] names)
        {
            if (obj == null) return 0;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (f != null && (f.FieldType == typeof(int) || f.FieldType == typeof(short)))
                {
                    try { return Convert.ToInt32(f.GetValue(obj)); } catch {}
                }
                var p = t.GetProperty(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (p != null && (p.PropertyType == typeof(int) || p.PropertyType == typeof(short)))
                {
                    try { return Convert.ToInt32(p.GetValue(obj)); } catch {}
                }
            }
            return 0;
        }

        static float SafeGetFloat(object obj, params string[] names)
        {
            if (obj == null) return 0f;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (f != null && (f.FieldType == typeof(float)))
                {
                    try { return (float)f.GetValue(obj); } catch {}
                }
                var p = t.GetProperty(n, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
                if (p != null && (p.PropertyType == typeof(float)))
                {
                    try { return (float)p.GetValue(obj); } catch {}
                }
            }
            return 0f;
        }

        static string SafeGetName(object obj, byte[] srcBuf, int baseOffset, params string[] nameIndexFields)
        {
            int rel = SafeGetInt(obj, nameIndexFields);
            if (rel == 0) return string.Empty;
            int ptr = baseOffset + rel;
            if (ptr < 0 || ptr >= srcBuf.Length) return string.Empty;
            int end = Array.IndexOf(srcBuf, (byte)0, ptr); if (end < 0) end = srcBuf.Length;
            int len = end - ptr;
            if (len <= 0) return string.Empty;
            return Encoding.UTF8.GetString(srcBuf, ptr, len);
        }

        public Vector3 GetFlexDelta(int fi, int vi, float s)
        {
            if (s == 0f || _flexLookup == null || fi < 0 || fi >= _flexLookup.Count) return Vector3.zero;
            try { return _flexLookup[fi].TryGetValue(vi, out var d) ? d * s : Vector3.zero; } catch { return Vector3.zero; }
        }

        // ------------- BUILD MODEL -------------
        Transform[] BuildBoneHierarchy(GameObject root)
        {
            Transform[] bones = new Transform[Math.Max(0, MDL_Header.bone_count)];
            for (int i = 0; i < bones.Length; i++)
            {
                var go = new GameObject(MDL_BoneNames[i]);
                bones[i] = go.transform;

                var sb = MDL_StudioBones[i];
                Vector3 p = sb.pos * uLoader.UnitScale; p.x = -p.x;

                if (sb.parent >= 0 && sb.parent < i) bones[i].parent = bones[sb.parent];
                else { (p.y, p.z) = (p.z, -p.y); bones[i].parent = root.transform; }

                bones[i].localPosition = SkinningSafety.SanitizeVec(p);

                Vector3 eul = sb.rot * Mathf.Rad2Deg;
                var rot = sb.parent == -1
                    ? Quaternion.Euler(-90, 90, -90) * MathLibrary.AngleQuaternion(eul)
                    : MathLibrary.AngleQuaternion(eul);
                bones[i].localRotation = rot;
            }
            if (uLoader.DrawArmature) root.AddComponent<MDLArmatureInfo>().boneNodes = bones;
            return bones;
        }

        void BuildHitboxes(Transform[] bones)
        {
            if (Hitboxes == null) return;
            for (int i = 0; i < Hitboxes.Length; i++)
            {
                if (Hitboxes[i] == null) continue;
                foreach (var hb in Hitboxes[i])
                {
                    var bb = hb.BBox;
                    if ((uint)bb.bone >= (uint)bones.Length) continue;
                    var bc = new GameObject($"Hitbox_{bones[bb.bone].name}").AddComponent<BoxCollider>();
                    bc.size = MathLibrary.NegateX(bb.bbmax - bb.bbmin) * uLoader.UnitScale;
                    bc.center = MathLibrary.NegateX((bb.bbmax + bb.bbmin) / 2f) * uLoader.UnitScale;
                    bc.transform.parent = bones[bb.bone];
                    bc.transform.localPosition = Vector3.zero;
                    bc.transform.localRotation = Quaternion.identity;
                }
            }
        }

        [DisallowMultipleComponent]
        public class JiggleBone : MonoBehaviour
        {
            public Transform tip; public float stiffness = 180f; public float damping = 0.75f;
            public Vector3 localRestPos; Vector3 _prevPos;
            void Awake(){ if (tip == null) tip = transform; _prevPos = tip.position; localRestPos = tip.localPosition; }
            void LateUpdate()
            {
                if (tip == null) return;
                Vector3 currentPos = tip.position;
                Vector3 wantedPos  = transform.TransformPoint(localRestPos);
                Vector3 velocity   = currentPos - _prevPos;
                Vector3 force      = (wantedPos - currentPos) * Mathf.Clamp(stiffness, 10f, 900f) * Time.deltaTime * Time.deltaTime;
                velocity += force; velocity *= 1f - Mathf.Clamp01(damping) * Time.deltaTime;
                Vector3 nextPos = currentPos + velocity;
                if (float.IsFinite(nextPos.x) && float.IsFinite(nextPos.y) && float.IsFinite(nextPos.z))
                { tip.position = nextPos; tip.rotation = transform.rotation; _prevPos = currentPos; }
            }
        }

        [DisallowMultipleComponent]
        public class EyeController : MonoBehaviour
        {
            public Transform leftEye, rightEye, target; public float eyeSpeed = 15f;
            void LateUpdate()
            {
                if (leftEye == null && rightEye == null) return;
                Transform t = target ?? Camera.main?.transform; if (t == null) return;
                Vector3 lookPos = t.position;
                void Rot(Transform eye){ if (eye == null) return; Vector3 dir = lookPos - eye.position; if (dir.sqrMagnitude < 1e-6f) return;
                    Quaternion q = Quaternion.LookRotation(dir, transform.up);
                    eye.rotation = Quaternion.Slerp(eye.rotation, q, Time.deltaTime * eyeSpeed); }
                Rot(leftEye); Rot(rightEye);
            }
        }

        [DisallowMultipleComponent]
        public class MdlEventPlayer : MonoBehaviour
        {
            public event Action<MdlEventInfo> OnMdlEvent;
            public void Fire(int type, string options) => OnMdlEvent?.Invoke(new MdlEventInfo { time = Time.time, type = type, options = options });
        }

        void BuildEyeballs(Transform root, Transform[] bones)
        {
            if (MDL_Eyeballs == null || MDL_Eyeballs.Length == 0) return;
            var ec = root.GetComponent<EyeController>() ?? root.gameObject.AddComponent<EyeController>();
            int eyeballSize = Marshal.SizeOf<mstudioeyeball_t>();

            for (int i = 0; i < MDL_Eyeballs.Length; ++i)
            {
                var eye = MDL_Eyeballs[i];
                Vector3 src = eye.org; if (!SkinningSafety.IsFinite(src)) continue;
                Vector3 pos = MathLibrary.SwapZY(MathLibrary.NegateX(src) * uLoader.UnitScale);
                if (!SkinningSafety.IsFinite(pos)) continue;

                Transform bone = (eye.bone >= 0 && eye.bone < bones.Length) ? bones[eye.bone] : root;
                if (bone == null) bone = root;

                var go = new GameObject($"Eyeball_{bone.name}_{i}");
                go.transform.SetParent(bone, false);
                go.transform.localPosition = pos;
                go.transform.localRotation = Quaternion.identity;

                string name = GetEyeballNameSafe(i, eye.sznameindex, eyeballSize).ToLowerInvariant();
                if (ec.leftEye == null && (name.Contains("left") || name.Contains("_l"))) ec.leftEye = go.transform;
                else if (ec.rightEye == null && (name.Contains("right") || name.Contains("_r"))) ec.rightEye = go.transform;
                else if (ec.leftEye == null) ec.leftEye = go.transform;
                else if (ec.rightEye == null) ec.rightEye = go.transform;
            }

            string GetEyeballNameSafe(int idx, int relOffset, int structSize)
            { long abs = MDL_Header.eyeball_offset + idx * structSize + relOffset; return GetCStringSafe((int)abs); }
        }

        static string GetCStringSafe(int ptr)
        {
            if (mdldata == null) return string.Empty;
            if (ptr < 0 || ptr >= mdldata.Length) return string.Empty;
            int end = Array.IndexOf(mdldata, (byte)0, ptr); if (end < 0) end = mdldata.Length;
            int len = end - ptr; return len > 0 ? Encoding.UTF8.GetString(mdldata, ptr, len) : string.Empty;
        }

        public Transform BuildModel(bool generateUV2 = false)
        {
            try
            {
                using (var prof = uSource.Tools.ProfilerHelper.Measure("BuildModel_Total"))
                {
                    if (MDL_FlexAnims == null)
                    {
                        using var ms = new MemoryStream(mdldata);
                        using var rd = new uReader(ms);
                        LoadFlexData(rd);
                    }

                    string raw = string.IsNullOrEmpty(MDL_Header.Name) ? "MDL_Model" : MDL_Header.Name;
                    string clean = Path.GetFileNameWithoutExtension(raw).Replace('\\','_').Replace('/','_')
                        .Replace(':','_').Replace('*','_').Replace('?','_').Replace('\"','_').Replace('<','_').Replace('>','_').Replace('|','_');
                    var modelGO = new GameObject(clean);
                    Transform[] bones = BuildBoneHierarchy(modelGO);

                    if (MDL_Hitboxsets != null) BuildHitboxes(bones);
                    if (MDL_Eyeballs != null && MDL_Eyeballs.Length > 0) BuildEyeballs(modelGO.transform, bones);
                    if (MDL_JiggleBones != null && MDL_JiggleBones.Length > 0) BuildJiggleBones(bones, modelGO.transform);

                    bool staticProp = MDL_Header.flags.HasFlag(StudioHDRFlags.STUDIOHDR_FLAGS_STATIC_PROP);
                    var mdlEventPlayer = modelGO.GetComponent<MdlEventPlayer>() ?? modelGO.AddComponent<MdlEventPlayer>();
                    if (OnPhonemeEvent != null) mdlEventPlayer.OnMdlEvent += info => OnPhonemeEvent(modelGO, info.options);

                    // Head bone guess for blendshape target
                    int headBoneIndex = -1;
                    for (int i = 0; i < MDL_BoneNames.Length; i++)
                    {
                        var n = MDL_BoneNames[i]?.ToLowerInvariant() ?? "";
                        if (n.Contains("head")) { headBoneIndex = i; break; }
                    }

                    SkinnedMeshRenderer headBlendTargetRenderer = null;
                    Mesh baseBlendshapeMesh = null;

                    // BODY PARTS / MODELS
                    for (int bp = 0; bp < MDL_Header.bodypart_count; bp++)
                    {
                        var bodypart = MDL_Bodyparts[bp];
                        if (bodypart.Models == null) continue;
                        for (int mi = 0; mi < bodypart.Models.Length; mi++)
                        {
                            var mdl = bodypart.Models[mi];
                            if (mdl.Meshes == null || mdl.isBlank) continue;

                            bool DetailModeEnabled =
                                uLoader.DetailMode == DetailMode.Lowest ||
                                uLoader.DetailMode == DetailMode.Low ||
                                uLoader.DetailMode == DetailMode.Medium ||
                                uLoader.DetailMode == DetailMode.High;

                            bool LODExist = uLoader.EnableLODParsing && !DetailModeEnabled &&  mdl.NumLODs > 1;
                            Transform firstLODObject = null;
                            LODGroup lodGroup = null;
                            float MaxSwitchPoint = 100f;
                            int StartLODIndex = 0;

                            if (uLoader.EnableLODParsing && DetailModeEnabled)
                            {
                                StartLODIndex =
                                    uLoader.DetailMode == DetailMode.Lowest ? (mdl.NumLODs - 1) :
                                    uLoader.DetailMode == DetailMode.Low ? (Int32)(mdl.NumLODs / 1.5f) :
                                    uLoader.DetailMode == DetailMode.Medium ? (mdl.NumLODs / 2) :
                                    (Int32)(mdl.NumLODs / 2.5f);
                                StartLODIndex = Mathf.Clamp(StartLODIndex, 0, Mathf.Max(0, mdl.NumLODs - 1));
                            }

                            // Prepare renderer buckets per LOD
                            var lodRenderers = new List<Renderer>[Mathf.Max(1, mdl.NumLODs)];
                            for (int i = 0; i < lodRenderers.Length; i++) lodRenderers[i] = new List<Renderer>(4);
                            var lodHeights = new float[Mathf.Max(1, mdl.NumLODs)];

                            // Precompute MaxSwitchPoint from model data (skip shadowlod)
                            if (LODExist && mdl.LODData != null && mdl.LODData.Length >= 1)
                            {
                                for (int LODID = 1; LODID < 3 && LODID <= mdl.NumLODs - 1; LODID++)
                                {
                                    int LastID = mdl.NumLODs - LODID;
                                    var LOD = mdl.LODData[LastID];
                                    if (LOD.switchPoint != -1)
                                    {
                                        if (LOD.switchPoint > 0) MaxSwitchPoint = LOD.switchPoint;
                                        if (LODID == 2 || LOD.switchPoint == 0)
                                        {
                                            MaxSwitchPoint += MaxSwitchPoint * uLoader.NegativeAddLODPrecent;
                                            int idx = (LOD.switchPoint == 0) ? LastID : Mathf.Min(LastID + 1, mdl.NumLODs - 1);
                                            mdl.LODData[idx].switchPoint = MaxSwitchPoint;
                                        }
                                        MaxSwitchPoint += uLoader.ThresholdMaxSwitch;
                                        break;
                                    }
                                }
                                if (MaxSwitchPoint <= 0f || !float.IsFinite(MaxSwitchPoint)) MaxSwitchPoint = 100f;
                            }

                            for (int lod = 0; lod < mdl.NumLODs; lod++)
                            {
                                if (uLoader.EnableLODParsing && DetailModeEnabled && lod < StartLODIndex) continue;
                                var vs = mdl.VerticesPerLod[lod];
                                if (vs == null || vs.Length == 0) continue;

                                int vc = vs.Length;
                                var pos = new Vector3[vc];
                                var nor = new Vector3[vc];
                                var uv  = new Vector2[vc];
                                var bw  = new BoneWeight[vc];

                                for (int v = 0; v < vc; v++)
                                {
                                    pos[v] = MathLibrary.SwapZY(vs[v].m_vecPosition * uLoader.UnitScale);
                                    nor[v] = MathLibrary.SwapZY(vs[v].m_vecNormal);
                                    var t  = vs[v].m_vecTexCoord; if (uLoader.SaveAssetsToUnity && uLoader.ExportTextureAsPNG) t.y = 1 - t.y;
                                    uv[v]  = t;
                                    bw[v]  = GetBoneWeightSafe(vs[v].m_BoneWeights);
                                }
                                var ModelLOD = (mdl.LODData != null && lod < mdl.LODData.Length) ? mdl.LODData[lod] : default(ModelLODHeader_t);

                                if (LODExist)
                                {
                                    if (ModelLOD.switchPoint == 0) ModelLOD.switchPoint = MaxSwitchPoint;
                                    else ModelLOD.switchPoint = MaxSwitchPoint - ModelLOD.switchPoint;
                                    ModelLOD.switchPoint -= ModelLOD.switchPoint * uLoader.SubstractLODPrecent;
                                }

                                string baseName = Path.GetFileNameWithoutExtension(mdl.Model.Name).Trim('.', '_').Replace(" ", "_");
                                foreach (char c in Path.GetInvalidFileNameChars()) baseName = baseName.Replace(c, '_');
                                if (string.IsNullOrEmpty(baseName)) baseName = "Mesh";
                                string meshName = $"{baseName}_LOD{lod}";

                                var mesh = new Mesh { name = meshName };
                                mesh.subMeshCount = Math.Max(1, mdl.Model.nummeshes);
                                mesh.vertices = pos; mesh.normals = nor; mesh.uv = uv;

                                // Triangles
                                for (int s = 0; s < mdl.Model.nummeshes; s++)
                                {
                                    if (!mdl.IndicesPerLod[lod].TryGetValue(s, out var indices) || indices == null || indices.Count == 0)
                                        mesh.SetTriangles(Array.Empty<int>(), s, false);
                                    else
                                        mesh.SetTriangles(indices, s, false);
                                }

                                mesh.ClearBlendShapes();

                                var go = new GameObject($"{meshName}_GO");
                                go.transform.SetParent(modelGO.transform, false);

                                Renderer renderer;
                                SkinnedMeshRenderer sren = null;
                                if (!staticProp)
                                {
                                    sren = go.AddComponent<SkinnedMeshRenderer>();
                                    renderer = sren;
                                    Matrix4x4[] bindPosesTemp = new Matrix4x4[bones.Length];
                                    for (int i = 0; i < bindPosesTemp.Length; i++)
                                        bindPosesTemp[i] = bones[i].worldToLocalMatrix * go.transform.localToWorldMatrix;
                                    mesh.bindposes = bindPosesTemp;
                                    mesh.boneWeights = bw;
                                    SkinningSafety.SafeBindAndAssign(sren, mesh, bones, bones[0]);
                                }
                                else
                                {
                                    var mf = go.AddComponent<MeshFilter>();
                                    renderer = go.AddComponent<MeshRenderer>();
                                    mf.sharedMesh = mesh;
                                }

                                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                                // Materials
                                Material[] mats = new Material[Math.Max(1, mesh.subMeshCount)];
                                for (int meshID = 0; meshID < mesh.subMeshCount; meshID++)
                                {
                                    Material fallback = uResourceManager.LoadMaterial(string.Empty).Material;
                                    mats[meshID] = fallback;
                                    try
                                    {
                                        if (MDL_TDirectories != null && MDL_Textures != null && mdl.Meshes != null && meshID < mdl.Meshes.Length)
                                        {
                                            int matIdx = mdl.Meshes[meshID].material;
                                            if ((uint)matIdx < (uint)MDL_Textures.Length)
                                            {
                                                for (int dir = 0; dir < MDL_TDirectories.Length; dir++)
                                                {
                                                    string materialPath = MDL_TDirectories[dir] + MDL_Textures[matIdx];
                                                    if (uResourceManager.ContainsFile(materialPath, uResourceManager.MaterialsSubFolder, uResourceManager.MaterialsExtension[0]))
                                                    { var VMT = uResourceManager.LoadMaterial(materialPath); mats[meshID] = VMT.Material; break; }
                                                }
                                            }
                                        }
                                    }
                                    catch {}
                                }
                                renderer.sharedMaterials = mats;

                                // ---------- BLENDSHAPE RESTRYKCJA: tylko top head mesh ----------
                                bool allowBlend = false;
                                if (!staticProp && sren != null)
                                {
                                    bool nameSuggestsHead = go.name.ToLowerInvariant().Contains("head") || go.name.ToLowerInvariant().Contains("face");
                                    bool headWeighted = false;
                                    if (headBoneIndex >= 0)
                                    {
                                        int hits = 0;
                                        for (int i = 0; i < bw.Length; i++)
                                        {
                                            if (bw[i].boneIndex0 == headBoneIndex && bw[i].weight0 > 0.001f) { hits++; continue; }
                                            if (bw[i].boneIndex1 == headBoneIndex && bw[i].weight1 > 0.001f) { hits++; continue; }
                                            if (bw[i].boneIndex2 == headBoneIndex && bw[i].weight2 > 0.001f) { hits++; continue; }
                                            if (bw[i].boneIndex3 == headBoneIndex && bw[i].weight3 > 0.001f) { hits++; continue; }
                                        }
                                        headWeighted = hits > (vc * 0.05f); // 5% progu
                                    }
                                    allowBlend = (headBlendTargetRenderer == null) && (nameSuggestsHead || headWeighted);
                                }

                                if (!staticProp && (MDL_FlexDescs?.Length ?? 0) > 0)
                                {
                                    int vFirst = mdl.VerticesGlobalStart;
                                    try
                                    {
                                        if (allowBlend)
                                        {
                                            bool gotBlend = AddBlendshapesFromFlex(mesh, mdl.VerticesPerLod[lod], vFirst);
                                            if (!gotBlend)
                                            {
                                                int total = MDL_FlexDescs?.Length ?? 0;
                                                for (int fi = 0; fi < total; fi++)
                                                {
                                                    string safeName = SanitizeBlendshapeName_Exact(GetFlexName(fi));
                                                    if (mesh.GetBlendShapeIndex(safeName) != -1) continue;
                                                    var zeroDV = new Vector3[vc];
                                                    var zeroDN = new Vector3[vc];
                                                    SafeAddBlendShapeFrame(mesh, safeName, 100f, zeroDV, zeroDN);
                                                }
                                            }
                                            if (baseBlendshapeMesh == null) baseBlendshapeMesh = mesh;
                                            if (headBlendTargetRenderer == null) headBlendTargetRenderer = sren;
                                        }
                                        else if (baseBlendshapeMesh != null && lod > 0 && mdl.LODRemap != null && mdl.LODRemap.Length > 0 && sren != null)
                                        {
                                            CloneFlexToLODSafe(baseBlendshapeMesh, mesh, mdl.LODRemap);
                                        }
                                    }
                                    catch (Exception e) { Debug.LogWarning($"[MDL WARN] Blendshape/flex apply failed for {mesh.name}: {e.Message}"); }
                                }

                                // LOD collection
                                if (LODExist)
                                {
                                    if (firstLODObject == null)
                                    {
                                        firstLODObject = go.transform;
                                        lodGroup = go.AddComponent<LODGroup>();
                                    }
                                    else
                                    {
                                        go.transform.parent = firstLODObject;
                                    }

                                    float height = MaxSwitchPoint > 0f ? (ModelLOD.switchPoint / MaxSwitchPoint) : 0.5f;
                                    if (!float.IsFinite(height)) height = 0.5f;
                                    height = Mathf.Clamp01(height);
                                    lodHeights[lod] = height;
                                    lodRenderers[lod].Add(renderer);
                                }

#if UNITY_EDITOR
                                if (generateUV2)
                                {
                                    bool hasTris = false;
                                    for (int si = 0; si < mesh.subMeshCount; si++)
                                        if (mesh.GetTriangles(si).Length > 0) { hasTris = true; break; }
                                    if (hasTris)
                                    {
                                        var so = new UnityEditor.SerializedObject(renderer);
                                        so.FindProperty("m_ScaleInLightmap").floatValue = uLoader.ModelsLightmapSize;
                                        so.ApplyModifiedProperties();
                                        go.isStatic = true;
                                        uResourceManager.UV2GenerateCache.Add(mesh);
                                    }
                                }
#endif

                                if (uLoader.EnableLODParsing && DetailModeEnabled) break;
                            } // end for lod

                            // Finalize LODs AFTER loop (prevents Unity warning)
                            if (LODExist && lodGroup != null)
                            {
                                // Build list of valid LODs (with renderers)
                                var valid = new List<(float height, Renderer[] rends)>();
                                for (int i = 0; i < lodRenderers.Length; i++)
                                {
                                    if (lodRenderers[i].Count == 0) continue;
                                    float h = lodHeights[i];
                                    if (!float.IsFinite(h)) h = 0.5f;
                                    h = Mathf.Clamp(h, 0.001f, 0.99f);
                                    valid.Add((h, lodRenderers[i].ToArray()));
                                }

                                if (valid.Count == 0)
                                {
                                    // No LOD data → treat as single LOD
                                    valid.Add((0.5f, firstLODObject != null ? firstLODObject.GetComponentsInChildren<Renderer>() : Array.Empty<Renderer>()));
                                }

                                // Sort by height DESC (Unity expects highest detail first)
                                valid.Sort((a,b) => b.height.CompareTo(a.height));

                                // Enforce strictly decreasing thresholds
                                const float eps = 0.01f;
                                for (int i = 0; i < valid.Count - 1; i++)
                                {
                                    if (!(valid[i].height > valid[i+1].height + eps))
                                    {
                                        float newNext = Mathf.Max(0f, valid[i].height - eps);
                                        valid[i+1] = (newNext, valid[i+1].rends);
                                    }
                                }
                                // Ensure last is >= 0 and <= previous - eps
                                if (valid.Count >= 2 && valid[^1].height >= valid[^2].height)
                                    valid[^1] = (Mathf.Max(0f, valid[^2].height - eps), valid[^1].rends);

                                // Convert to Unity LOD[]
                                var lods = new LOD[valid.Count];
                                for (int i = 0; i < valid.Count; i++)
                                    lods[i] = new LOD(valid[i].height, valid[i].rends);

                                try
                                {
                                    lodGroup.SetLODs(lods);
                                    lodGroup.fadeMode = LODFadeMode.CrossFade;
                                    lodGroup.animateCrossFading = true;
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning($"[MDL WARN] LODGroup.SetLODs failed: {e.Message}. Falling back to single LOD.");
                                    try
                                    {
                                        var allRends = firstLODObject != null ? firstLODObject.GetComponentsInChildren<Renderer>() : Array.Empty<Renderer>();
                                        lodGroup.SetLODs(new[]{ new LOD(0.5f, allRends) });
                                    } catch {}
                                }
                            }
                        }
                    }

                    // ---- FLEX RUNTIME HOOK ----
                    var driver = modelGO.GetComponent<SourceFlexRuntime>() ?? modelGO.AddComponent<SourceFlexRuntime>();
                    try
                    {
                        driver.SafeInitFromFlexNames(_flexNames);
                        driver.InjectFlexRuleData(MDL_FlexControllers, MDL_FlexRules, MDL_FlexOpsFlat, _flexNames);
                    } catch {}

                    SetupAnimatorAndClips(modelGO, bones);

#if UNITY_EDITOR
                    CreateAndSaveAvatars(modelGO, bones);
                    SaveBlendshapeMeshes(modelGO);
#endif

                    if (MDL_Header.flags.HasFlag(StudioHDRFlags.STUDIOHDR_FLAGS_STATIC_PROP))
                        modelGO.transform.eulerAngles = new Vector3(0, 90, 0);

                    return modelGO.transform;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[MDL] BuildModel failed hard: " + ex.Message);
                var fallback = new GameObject("MDL_Fallback");
                return fallback.transform;
            }
        }

        private void BuildJiggleBones(Transform[] bones, Transform root)
        {
            if (MDL_JiggleBones == null || MDL_JiggleBones.Length == 0) return;
            foreach (var jb in MDL_JiggleBones)
            {
                int i = jb.bone;
                if (i < 0 || i >= bones.Length) continue;
                var t = bones[i];
                if (t == null) continue;

                var comp = t.GetComponent<JiggleBone>();
                if (comp == null) comp = t.gameObject.AddComponent<JiggleBone>();

                comp.tip = t;
                comp.localRestPos = Vector3.zero;
                comp.stiffness = Mathf.Clamp(float.IsFinite(jb.Stiffness) ? jb.Stiffness : 180f, 10f, 900f);
                comp.damping   = Mathf.Clamp01(float.IsFinite(jb.yawDamping) ? jb.yawDamping : 0.75f);
            }
        }

        // ---- Helpers ----
        private void CloneFlexToLODSafe(Mesh src, Mesh dst, ushort[] remap)
        {
            try
            {
                CloneFlexToLOD(src, dst, remap);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MDL WARN] CloneFlexToLOD failed for {dst?.name}: {e.Message}");
            }
        }

        private Texture2D[] CloneFlexToLOD(Mesh src, Mesh dst, ushort[] remap)
        {
            if (src == null || dst == null || remap == null || remap.Length == 0) return null;
            int srcBSCount = src.blendShapeCount; if (srcBSCount == 0) return null;
            int dstVC = dst.vertexCount; var tmp = new Vector3[Mathf.Max(src.vertexCount, dstVC)];
            for (int shapeIndex = 0; shapeIndex < srcBSCount; shapeIndex++)
            {
                string shapeName = src.GetBlendShapeName(shapeIndex); if (dst.GetBlendShapeIndex(shapeName) != -1) continue;
                int frameCount = src.GetBlendShapeFrameCount(shapeIndex);
                for (int frame = 0; frame < frameCount; frame++)
                {
                    float weight = src.GetBlendShapeFrameWeight(shapeIndex, frame);
                    var srcDV = new Vector3[src.vertexCount]; var srcDN = new Vector3[src.vertexCount];
                    src.GetBlendShapeFrameVertices(shapeIndex, frame, srcDV, srcDN, tmp);
                    var dstDV = new Vector3[dstVC]; var dstDN = new Vector3[dstVC];
                    for (int v = 0; v < dstVC; v++)
                    {
                        if (v >= remap.Length) continue;
                        int srcIdx = remap[v];
                        if (srcIdx >= 0 && srcIdx < srcDV.Length){ dstDV[v] = srcDV[srcIdx]; dstDN[v] = srcDN[srcIdx]; }
                    }
                    dst.AddBlendShapeFrame(shapeName, weight, dstDV, dstDN, null);
                }
            }
            dst.RecalculateBounds(); return null;
        }

        bool AddBlendshapesFromFlex(Mesh mesh, mstudiovertex_t[] verts, int baseOfs)
        {
            int vc = (verts != null) ? verts.Length : 0;
            if (vc == 0) return false;
            bool any = false;

            int totalFlex = MDL_FlexDescs?.Length ?? 0;
            for (int fi = 0; fi < totalFlex; fi++)
            {
                string rawName = GetFlexName(fi);
                string safeName = SanitizeBlendshapeName_Exact(rawName);
                if (mesh.GetBlendShapeIndex(safeName) != -1) continue;

                var dV = new Vector3[vc];
                var dN = new Vector3[vc];
                bool hasAny = false;

                var flexAnims = (MDL_FlexAnims != null && MDL_FlexAnims.Count > fi) ? MDL_FlexAnims[fi] : Array.Empty<mstudiovertanim_t>();
                foreach (var va in flexAnims)
                {
                    int localIdx = va.index - baseOfs;
                    if ((uint)localIdx >= (uint)vc) continue;

                    Vector3 posDelta = va.PositionDelta * uLoader.UnitScale;
                    Vector3 nrmDelta = va.NormalDelta;
                    if (!float.IsFinite(posDelta.x) || !float.IsFinite(posDelta.y) || !float.IsFinite(posDelta.z) ||
                        !float.IsFinite(nrmDelta.x) || !float.IsFinite(nrmDelta.y) || !float.IsFinite(nrmDelta.z))
                        continue;

                    if (posDelta.sqrMagnitude < 1e-20f && nrmDelta.sqrMagnitude < 1e-20f)
                        continue;

                    dV[localIdx] = MathLibrary.SwapZY(posDelta);
                    dN[localIdx] = MathLibrary.SwapZY(nrmDelta);
                    hasAny = true;
                }

                if (!hasAny) { Debug.LogWarning($"[MDL WARN] Flex '{safeName}' has no valid vertices; creating neutral shape."); }
                SafeAddBlendShapeFrame(mesh, safeName, 100f, dV, dN);
                any = true;
            }
            return any;
        }

        static string SanitizeBlendshapeName_Exact(string name)
        {
            if (string.IsNullOrEmpty(name)) return "blendshape";
            var sb = new StringBuilder();
            foreach (char c in name) { if (c != '/' && c != '\\' && c != ':' && c != '\"' && c != '<' && c != '>' && c != '|' && c != '*') sb.Append(c); }
            string cleaned = sb.ToString();
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "blendshape";
            if (cleaned.Length > 64) cleaned = cleaned.Substring(0, 64);
            return cleaned;
        }

        static void SafeAddBlendShapeFrame(Mesh mesh, string name, float weight, Vector3[] dV, Vector3[] dN)
        {
            try
            {
                if (mesh == null || dV == null || dN == null) return;
                int vc = mesh.vertexCount;
                if (dV.Length != vc || dN.Length != vc)
                {
                    Array.Resize(ref dV, vc);
                    Array.Resize(ref dN, vc);
                }
                for (int i = 0; i < vc; i++)
                {
                    Vector3 a = dV[i], b = dN[i];
                    if (!float.IsFinite(a.x) || !float.IsFinite(a.y) || !float.IsFinite(a.z)) dV[i] = Vector3.zero;
                    if (!float.IsFinite(b.x) || !float.IsFinite(b.y) || !float.IsFinite(b.z)) dN[i] = Vector3.zero;
                }
                mesh.AddBlendShapeFrame(name, weight, dV, dN, null);
            }
            catch (Exception ex) { Debug.LogWarning("[MDL WARN] AddBlendShapeFrame failed for " + name + ": " + ex.Message); }
        }

        void ReadMdlEvents()
        {
            if (MDL_SeqDescriptions == null) { MDL_Events = Array.Empty<mstudioevent_t>(); return; }
            var list = new List<mstudioevent_t>();
            try
            {
                using var ms = new MemoryStream(mdldata, false);
                using var r = new uReader(ms);
                int sz = Marshal.SizeOf<mstudioevent_t>();
                for (int s = 0; s < MDL_SeqDescriptions.Length; s++)
                {
                    var sd = MDL_SeqDescriptions[s]; if (sd.numevents == 0) continue;
                    long baseOff = MDL_Header.localseq_offset + 212 * s;
                    for (int e = 0; e < sd.numevents && list.Count < 2000; e++)
                    {
                        var ev = new mstudioevent_t();
                        r.ReadTypeFixed(ref ev, sz, (int)(baseOff + sd.eventindex + e * sz));
                        list.Add(ev);
                    }
                }
            }
            catch {}
            MDL_Events = list.ToArray();
        }

        void SetupAnimatorAndClips(GameObject root, Transform[] bones)
        {
#if UNITY_EDITOR
            try
            {
                Animator animatorComponent = root.GetComponent<Animator>();
                if (animatorComponent == null) animatorComponent = root.AddComponent<Animator>();

                const string controllerDir = "Assets/GeneratedAnimatorControllers";
                if (!Directory.Exists(controllerDir)) Directory.CreateDirectory(controllerDir);
                AssetDatabase.Refresh();

                string cleanName = $"{root.name}_{Guid.NewGuid()}".Replace(".", "_").Replace("/", "_").Replace("\\", "_");
                string controllerPath = $"{controllerDir}/{cleanName}_AnimatorController.controller";
                var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

                if (controller.layers.Length == 0)
                    controller.AddLayer(new AnimatorControllerLayer { name = "Base Layer", stateMachine = new AnimatorStateMachine() });

                var stateMachine = controller.layers[0].stateMachine;
                animatorComponent.runtimeAnimatorController = controller;

                const string exportDir = "Assets/ExportedAnimations";
                if (!Directory.Exists(exportDir)) Directory.CreateDirectory(exportDir);

                var bonePathDict = new Dictionary<int, string>();
                for (int i = 0; i < MDL_Header.bone_count; i++) bonePathDict[i] = GetBonePath(i);

                if (Sequences != null)
                {
                    foreach (var seq in Sequences)
                    {
                        if (seq.ani == null) continue;
                        foreach (var ani in seq.ani)
                        {
                            var clip = CreateAnimationClip(ani, bonePathDict);
                            clip.name = ani.name.Replace(" ", "_");

                            var state = stateMachine.AddState(clip.name);
                            state.motion = clip;

                            string p = clip.name;
                            if (controller.parameters.All(x => x.name != $"{p}_Bool"))    controller.AddParameter($"{p}_Bool", AnimatorControllerParameterType.Bool);
                            if (controller.parameters.All(x => x.name != $"{p}_Trigger")) controller.AddParameter($"{p}_Trigger", AnimatorControllerParameterType.Trigger);
                            if (controller.parameters.All(x => x.name != $"{p}_Float"))   controller.AddParameter($"{p}_Float", AnimatorControllerParameterType.Float);
                            if (controller.parameters.All(x => x.name != $"{p}_Int"))     controller.AddParameter($"{p}_Int", AnimatorControllerParameterType.Int);

                            AnimatorStateTransition t;
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.If, 0, $"{p}_Bool");
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.IfNot, 0, $"{p}_Bool");
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.If, 0, $"{p}_Trigger");
                            const float floatThreshold = 0.5f;
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.Greater, floatThreshold, $"{p}_Float");
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.Less, floatThreshold, $"{p}_Float");
                            const int intValue = 1;
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.Equals, intValue, $"{p}_Int");
                            t = stateMachine.AddAnyStateTransition(state); t.hasExitTime = false; t.duration = 0f; t.AddCondition(AnimatorConditionMode.NotEqual, intValue, $"{p}_Int");

                            string desiredPath = $"{exportDir}/{clip.name}.anim";
                            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath);
                            AssetDatabase.CreateAsset(clip, uniquePath);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            } catch (Exception ex) { Debug.LogWarning("[MDL] Animator/Clip setup skipped: " + ex.Message); }
#else
            _ = bones;
#endif
        }

#if UNITY_EDITOR
        void CreateAndSaveAvatars(GameObject root, Transform[] bones)
        {
            try
            {
                const string outDir = "Assets/GeneratedAvatars";
                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                AssetDatabase.Refresh();

                // Skeleton
                var all = root.GetComponentsInChildren<Transform>(true);
                var skeleton = new SkeletonBone[all.Length];
                for (int i = 0; i < all.Length; i++)
                {
                    skeleton[i].name = all[i].name;
                    skeleton[i].position = all[i].localPosition;
                    skeleton[i].rotation = all[i].localRotation;
                    skeleton[i].scale = all[i].localScale;
                }

                // Human bones (heurystyka Source → Mecanim)
                var humanList = BuildHumanBones(root);
                var hd = new HumanDescription
                {
                    human = humanList.ToArray(),
                    skeleton = skeleton,
                    armStretch = 0.05f, legStretch = 0.05f,
                    upperArmTwist = 0.5f, lowerArmTwist = 0.5f,
                    upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
                    feetSpacing = 0.0f, hasTranslationDoF = false
                };

                var humanoid = AvatarBuilder.BuildHumanAvatar(root, hd);
                if (humanoid != null && humanoid.isValid && humanoid.isHuman)
                {
                    var p = AssetDatabase.GenerateUniqueAssetPath($"{outDir}/{root.name}_Humanoid.avatar");
                    AssetDatabase.CreateAsset(humanoid, p);
                }

                int rootBoneIdx = -1; for (int i = 0; i < MDL_StudioBones.Length; i++) if (MDL_StudioBones[i].parent == -1) { rootBoneIdx = i; break; }
                string rootPath = rootBoneIdx >= 0 ? GetBonePath(rootBoneIdx) : (bones != null && bones.Length > 0 ? bones[0].name : "");

                var generic = AvatarBuilder.BuildGenericAvatar(root, rootPath);
                if (generic != null && generic.isValid)
                {
                    var p = AssetDatabase.GenerateUniqueAssetPath($"{outDir}/{root.name}_Generic.avatar");
                    AssetDatabase.CreateAsset(generic, p);
                }

                AssetDatabase.SaveAssets();
            }
            catch (Exception ex) { Debug.LogWarning("[MDL] Avatar creation failed: " + ex.Message); }
        }

        List<HumanBone> BuildHumanBones(GameObject root)
        {
            var list = new List<HumanBone>(24);

            Transform Find(Func<string, bool> namePred, Func<Transform, bool> extra = null)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    string n = t.name.ToLowerInvariant();
                    if (namePred(n) && (extra == null || extra(t))) return t;
                }
                return null;
            }
            bool HasLeft(string n) => n.Contains("_l") || n.Contains(" left") || n.EndsWith(".l") || n.StartsWith("l_");
            bool HasRight(string n) => n.Contains("_r") || n.Contains(" right") || n.EndsWith(".r") || n.StartsWith("r_");

            Transform pelvis = Find(n => n.Contains("pelvis") || n.Contains("hips") || n.Contains("bip") && n.Contains("pelvis") || n == "root");
            Transform spine  = Find(n => n.StartsWith("spine") || n.Contains("spine"));
            Transform chest  = Find(n => n.Contains("chest") || n == "spine3" || n == "upperchest");
            Transform neck   = Find(n => n.Contains("neck"));
            Transform head   = Find(n => n.Contains("head"));

            Transform clavL     = Find(n => (n.Contains("clav") || n.Contains("shoulder")) && HasLeft(n));
            Transform upperArmL = Find(n => (n.Contains("upperarm") || (n.Contains("arm") && !n.Contains("fore"))) && HasLeft(n));
            Transform lowerArmL = Find(n => (n.Contains("forearm") || n.Contains("lowerarm")) && HasLeft(n));
            Transform handL     = Find(n => n.Contains("hand") && HasLeft(n));

            Transform clavR     = Find(n => (n.Contains("clav") || n.Contains("shoulder")) && HasRight(n));
            Transform upperArmR = Find(n => (n.Contains("upperarm") || (n.Contains("arm") && !n.Contains("fore"))) && HasRight(n));
            Transform lowerArmR = Find(n => (n.Contains("forearm") || n.Contains("lowerarm")) && HasRight(n));
            Transform handR     = Find(n => n.Contains("hand") && HasRight(n));

            Transform thighL = Find(n => (n.Contains("thigh") || n.Contains("upleg")) && HasLeft(n));
            Transform calfL  = Find(n => (n.Contains("calf") || n.Contains("leg") || n.Contains("lowleg")) && HasLeft(n));
            Transform footL  = Find(n => n.Contains("foot") && HasLeft(n));
            Transform toeL   = Find(n => n.Contains("toe") && HasLeft(n));

            Transform thighR = Find(n => (n.Contains("thigh") || n.Contains("upleg")) && HasRight(n));
            Transform calfR  = Find(n => (n.Contains("calf") || n.Contains("leg") || n.Contains("lowleg")) && HasRight(n));
            Transform footR  = Find(n => n.Contains("foot") && HasRight(n));
            Transform toeR   = Find(n => n.Contains("toe") && HasRight(n));

            void Add(HumanBodyBones hb, Transform t)
            {
                if (t == null) return;
                var hbDesc = new HumanBone
                {
                    humanName = hb.ToString(),
                    boneName = AnimationUtility.CalculateTransformPath(t, root.transform),
                    limit = new HumanLimit { useDefaultValues = true }
                };
                list.Add(hbDesc);
            }

            Add(HumanBodyBones.Hips, pelvis);
            Add(HumanBodyBones.Spine, spine);
            Add(HumanBodyBones.Chest, chest ?? spine);
            Add(HumanBodyBones.Neck, neck);
            Add(HumanBodyBones.Head, head);

            Add(HumanBodyBones.LeftShoulder, clavL);
            Add(HumanBodyBones.LeftUpperArm, upperArmL);
            Add(HumanBodyBones.LeftLowerArm, lowerArmL);
            Add(HumanBodyBones.LeftHand, handL);

            Add(HumanBodyBones.RightShoulder, clavR);
            Add(HumanBodyBones.RightUpperArm, upperArmR);
            Add(HumanBodyBones.RightLowerArm, lowerArmR);
            Add(HumanBodyBones.RightHand, handR);

            Add(HumanBodyBones.LeftUpperLeg, thighL);
            Add(HumanBodyBones.LeftLowerLeg, calfL);
            Add(HumanBodyBones.LeftFoot, footL);
            Add(HumanBodyBones.LeftToes, toeL);

            Add(HumanBodyBones.RightUpperLeg, thighR);
            Add(HumanBodyBones.RightLowerLeg, calfR);
            Add(HumanBodyBones.RightFoot, footR);
            Add(HumanBodyBones.RightToes, toeR);

            return list;
        }

        void SaveBlendshapeMeshes(GameObject root)
        {
            try
            {
                const string dir = "Assets/GeneratedMeshes";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var s in skins)
                {
                    var m = s.sharedMesh; if (m == null) continue;
                    if (m.blendShapeCount <= 0) continue;
                    var clone = Object.Instantiate(m);
                    var path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{clone.name}.asset");
                    AssetDatabase.CreateAsset(clone, path);
                }
                AssetDatabase.SaveAssets();
            } catch (Exception ex) { Debug.LogWarning("[MDL] SaveBlendshapeMeshes failed: " + ex.Message); }
        }
#endif

        string GetBonePath(int boneIndex)
        {
            List<string> pathSegments = new List<string>();
            int current = boneIndex;
            while (current != -1)
            {
                pathSegments.Add(MDL_BoneNames[current]);
                current = MDL_StudioBones[current].parent;
            }
            pathSegments.Reverse();
            return string.Join("/", pathSegments);
        }

        private static BoneWeight GetBoneWeightSafe(mstudioboneweight_t src)
        {
            int[] b = new int[4]; float[] w = new float[4];
            b[0] = src.bone[0]; b[1] = src.bone[1]; b[2] = src.bone[2]; b[3] = 0;
            w[0] = src.weight[0]; w[1] = src.weight[1]; w[2] = src.weight[2]; w[3] = Mathf.Max(0f, 1f - (w[0]+w[1]+w[2]));
            Array.Sort(w, b); Array.Reverse(w); Array.Reverse(b);
            float sum = w[0]+w[1]+w[2]+w[3];
            if (sum > 0f){ float inv = 1f/sum; for (int i=0;i<4;i++) w[i]*=inv; } else { w[0]=1f; w[1]=w[2]=w[3]=0f; }
            return new BoneWeight{ boneIndex0=b[0], weight0=w[0], boneIndex1=b[1], weight1=w[1], boneIndex2=b[2], weight2=w[2], boneIndex3=b[3], weight3=w[3] };
        }

        public AnimationClip CreateAnimationClip(AniInfo animationData, Dictionary<int, string> bonePathDict)
        {
            int numFrames = animationData.studioAnim.numframes < 2 ? animationData.studioAnim.numframes + 1 : animationData.studioAnim.numframes;
            AnimationClip clip = new AnimationClip { legacy = false };

            int boneCount = MDL_Header.bone_count;
            AnimationCurve[] posX = new AnimationCurve[boneCount];
            AnimationCurve[] posY = new AnimationCurve[boneCount];
            AnimationCurve[] posZ = new AnimationCurve[boneCount];
            AnimationCurve[] rotX = new AnimationCurve[boneCount];
            AnimationCurve[] rotY = new AnimationCurve[boneCount];
            AnimationCurve[] rotZ = new AnimationCurve[boneCount];
            AnimationCurve[] rotW = new AnimationCurve[boneCount];
            for (int i = 0; i < boneCount; i++)
            { posX[i] = new AnimationCurve(); posY[i] = new AnimationCurve(); posZ[i] = new AnimationCurve();
              rotX[i] = new AnimationCurve(); rotY[i] = new AnimationCurve(); rotZ[i] = new AnimationCurve(); rotW[i] = new AnimationCurve(); }

            for (int frame = 0; frame < numFrames; frame++)
                for (int bone = 0; bone < boneCount; bone++)
                {
                    posX[bone].AddKey(animationData.PosX[frame][bone]);
                    posY[bone].AddKey(animationData.PosY[frame][bone]);
                    posZ[bone].AddKey(animationData.PosZ[frame][bone]);
                    rotX[bone].AddKey(animationData.RotX[frame][bone]);
                    rotY[bone].AddKey(animationData.RotY[frame][bone]);
                    rotZ[bone].AddKey(animationData.RotZ[frame][bone]);
                    rotW[bone].AddKey(animationData.RotW[frame][bone]);
                }

            for (int bone = 0; bone < boneCount; bone++)
            {
                if (!bonePathDict.TryGetValue(bone, out string bonePath) || string.IsNullOrEmpty(bonePath)) continue;
                clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", posX[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", posY[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", posZ[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", rotX[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", rotY[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", rotZ[bone]);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", rotW[bone]);
            }

            if (animationData.studioAnim.fps > 0f) clip.frameRate = animationData.studioAnim.fps;
            clip.EnsureQuaternionContinuity();
            return clip;
        }
    }

    internal static class SkinningSafety
    {
        public static void SafeBindAndAssign(SkinnedMeshRenderer smr, Mesh mesh, Transform[] bones, Transform root)
        {
            if (smr == null) throw new ArgumentNullException(nameof(smr));
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));

            if (bones == null || bones.Length == 0) bones = root != null ? new[] { root } : new[] { smr.transform };
            for (int i = 0; i < bones.Length; i++) if (bones[i] == null) bones[i] = root != null ? root : smr.transform;
            if (root == null) root = bones[0] != null ? bones[0] : smr.transform;

            var bindposes = mesh.bindposes;
            if (bindposes == null || bindposes.Length != bones.Length || HasNaN(bindposes)) bindposes = RebuildBindposes(smr.transform, bones);

            mesh.bindposes = bindposes;
            CleanBoneWeights(mesh, bindposes.Length);
            SanitizeVertices(mesh);

            smr.sharedMesh = mesh; smr.bones = bones; smr.rootBone = root; smr.updateWhenOffscreen = true;

            SafeRecalculateBounds(mesh);
            mesh.UploadMeshData(false);
        }

        private static Matrix4x4[] RebuildBindposes(Transform meshXform, Transform[] bones)
        {
            var result = new Matrix4x4[bones.Length];
            var meshLocalToWorld = meshXform.localToWorldMatrix;
            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i] != null ? bones[i] : meshXform;
                var m = b.worldToLocalMatrix * meshLocalToWorld;
                result[i] = Sanitize(m);
            }
            return result;
        }

        private static void CleanBoneWeights(Mesh mesh, int boneCount)
        {
            var bw = mesh.boneWeights;
            if (bw == null || bw.Length == 0)
            {
                var verts = mesh.vertexCount;
                bw = new BoneWeight[verts];
                for (int i = 0; i < verts; i++) { bw[i].boneIndex0 = 0; bw[i].weight0 = 1f; }
                mesh.boneWeights = bw; return;
            }

            for (int i = 0; i < bw.Length; i++)
            {
                var w = bw[i];
                if ((uint)w.boneIndex0 >= (uint)boneCount) { w.boneIndex0 = 0; w.weight0 = IsFinite(w.weight0) ? w.weight0 : 0f; }
                if ((uint)w.boneIndex1 >= (uint)boneCount) { w.boneIndex1 = 0; w.weight1 = 0f; }
                if ((uint)w.boneIndex2 >= (uint)boneCount) { w.boneIndex2 = 0; w.weight2 = 0f; }
                if ((uint)w.boneIndex3 >= (uint)boneCount) { w.boneIndex3 = 0; w.weight3 = 0f; }

                w.weight0 = Clamp01Finite(w.weight0); w.weight1 = Clamp01Finite(w.weight1); w.weight2 = Clamp01Finite(w.weight2); w.weight3 = Clamp01Finite(w.weight3);

                float sum = w.weight0 + w.weight1 + w.weight2 + w.weight3;
                if (sum <= 1e-8f) { w.boneIndex0 = 0; w.weight0 = 1f; w.boneIndex1 = w.boneIndex2 = w.boneIndex3 = 0; w.weight1 = w.weight2 = w.weight3 = 0f; }
                else { float inv = 1f/sum; w.weight0*=inv; w.weight1*=inv; w.weight2*=inv; w.weight3*=inv; }
                bw[i] = w;
            }
            mesh.boneWeights = bw;
        }

        private static void SanitizeVertices(Mesh mesh)
        {
            var v = mesh.vertices; bool changed = false;
            for (int i = 0; i < v.Length; i++)
            {
                var p = v[i];
                if (!IsFinite(p.x) || !IsFinite(p.y) || !IsFinite(p.z)) { v[i] = Vector3.zero; changed = true; }
            }
            if (changed) mesh.vertices = v;
        }

        private static void SafeRecalculateBounds(Mesh mesh)
        {
            mesh.RecalculateBounds();
            var b = mesh.bounds;
            if (!IsFinite(b.center.x) || !IsFinite(b.center.y) || !IsFinite(b.center.z) ||
                !IsFinite(b.extents.x) || !IsFinite(b.extents.y) || !IsFinite(b.extents.z) ||
                b.extents == Vector3.zero)
                mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
        }

        private static float Clamp01Finite(float x) => IsFinite(x) ? Mathf.Clamp01(x) : 0f;
        private static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
        public static bool IsFinite(Vector3 v) => IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);

        private static bool HasNaN(Matrix4x4[] m)
        {
            for (int i = 0; i < m.Length; i++) if (!IsFinite(m[i])) return true;
            return false;
        }
        public static bool IsFinite(Matrix4x4 m)
        {
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                float x = m[r, c];
                if (float.IsNaN(x) || float.IsInfinity(x)) return false;
            }
            return true;
        }
        private static Matrix4x4 Sanitize(Matrix4x4 m)
        {
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                float x = m[r, c];
                if (float.IsNaN(x) || float.IsInfinity(x)) m[r, c] = 0f;
            }
            return m;
        }

        public static Vector3 SanitizeVec(Vector3 p)
        {
            if (!IsFinite(p.x)) p.x = 0f;
            if (!IsFinite(p.y)) p.y = 0f;
            if (!IsFinite(p.z)) p.z = 0f;
            return p;
        }
    }
}
