// File: Assets/uSource-master/Formats/Source/SourceBlendShapeExtensions.cs

using System;
using System.IO;
using UnityEngine;
using uSource.Formats.Source.MDL;

namespace uSource.Formats.Source
{
    /// <summary>
    /// Extension methods for MDLFile to safely extract flex (blendshape) data.
    /// Wraps the raw .mdl parsing in try/catch to avoid IndexOutOfRange.
    /// </summary>
    public static class SourceBlendShapeExtensions
    {
        struct FlexDescInternal
        {
            public int NameOffset;
            public int VertAnimCount;
            public int VertAnimOffset;
        }

        static FlexDescInternal[] _flexDescInts;
        static mstudioflexdesc_t[] _flexDescs;
        static mstudiovertanim_t[][] _vertAnims;

        /// <summary>
        /// Returns the list of flex descriptors (names) for this MDL.
        /// </summary>
        public static mstudioflexdesc_t[] FlexDescs(this MDLFile mdl, string mdlPath)
        {
            if (_flexDescs == null)
                mdl.LoadFlexes(mdlPath);
            return _flexDescs;
        }

        /// <summary>
        /// Returns the per-vertex anim entries for flex #i.
        /// </summary>
        public static mstudiovertanim_t[] GetVertAnims(this MDLFile mdl, int i, string mdlPath)
        {
            if (_vertAnims == null)
                mdl.LoadFlexes(mdlPath);
            if (i < 0 || i >= _vertAnims.Length)
                return Array.Empty<mstudiovertanim_t>();
            return _vertAnims[i];
        }

        /// <summary>
        /// Parses the raw .mdl file from disk and extracts flex descriptors and their vertex-anims.
        /// Any out‑of‑bounds errors are caught and result in empty arrays.
        /// </summary>
        public static void LoadFlexes(this MDLFile mdl, string mdlPath)
        {
            try
            {
                byte[] raw = File.ReadAllBytes(mdlPath);
                using var br = new BinaryReader(new MemoryStream(raw));

                // 1) Seek to flexdesc metadata in header
                br.BaseStream.Seek(76, SeekOrigin.Begin);
                int headerLen = br.ReadInt32();
                br.BaseStream.Seek(76 + 4 + 8 * 16, SeekOrigin.Begin);

                // 2) Read counts & offsets
                int flexDescCount = br.ReadInt32();
                int flexDescOffset = br.ReadInt32();
                int flexVertTotal = br.ReadInt32();  // total verts count
                int flexVertOffset = br.ReadInt32();  // total verts offset

                _flexDescInts = new FlexDescInternal[flexDescCount];
                _flexDescs = new mstudioflexdesc_t[flexDescCount];
                _vertAnims = new mstudiovertanim_t[flexDescCount][];

                // 3) Read FlexDescInternals
                br.BaseStream.Seek(flexDescOffset, SeekOrigin.Begin);
                for (int x = 0; x < flexDescCount; x++)
                {
                    _flexDescInts[x].NameOffset = br.ReadInt32();
                    br.ReadInt32(); // skip index offset
                    _flexDescInts[x].VertAnimCount = br.ReadInt32();
                    _flexDescInts[x].VertAnimOffset = br.ReadInt32();
                    br.BaseStream.Seek(12, SeekOrigin.Current); // skip padding
                }

                // 4) Parse each flex
                for (int x = 0; x < flexDescCount; x++)
                {
                    var inf = _flexDescInts[x];

                    // Read the flex name
                    br.BaseStream.Seek(inf.NameOffset, SeekOrigin.Begin);
                    string name = "";
                    char c;
                    while ((c = br.ReadChar()) != 0)
                        name += c;
                    _flexDescs[x] = new mstudioflexdesc_t { szFACSindex = inf.NameOffset };

                    // Read its mstudiovertanim_t entries
                    _vertAnims[x] = new mstudiovertanim_t[inf.VertAnimCount];
                    br.BaseStream.Seek(inf.VertAnimOffset, SeekOrigin.Begin);

                    for (int v = 0; v < inf.VertAnimCount; v++)
                    {
                        _vertAnims[x][v] = new mstudiovertanim_t
                        {
                            index = br.ReadUInt16(),
                            speed = br.ReadSByte(),
                            side = br.ReadByte(),
                            sPositionX = br.ReadInt16(),
                            sPositionY = br.ReadInt16(),
                            sPositionZ = br.ReadInt16(),
                            sNormalX = br.ReadInt16(),
                            sNormalY = br.ReadInt16(),
                            sNormalZ = br.ReadInt16(),
                            // remaining bytes (flexdesc & padding) are skipped below
                        };
                        // Skip flexdesc byte + 3 padding bytes
                        br.BaseStream.Seek(1 + 3, SeekOrigin.Current);
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.LogWarning($"[SourceBlendShapeExtensions] LoadFlexes: out of range parsing '{Path.GetFileName(mdlPath)}': {ex.Message}");
                _flexDescs = Array.Empty<mstudioflexdesc_t>();
                _vertAnims = Array.Empty<mstudiovertanim_t[]>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SourceBlendShapeExtensions] LoadFlexes: failed parsing '{Path.GetFileName(mdlPath)}': {ex.Message}");
                _flexDescs = Array.Empty<mstudioflexdesc_t>();
                _vertAnims = Array.Empty<mstudiovertanim_t[]>();
            }
        }
    }
}
