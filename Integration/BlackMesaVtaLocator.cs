// BlackMesaVtaLocator.cs
// Finds or extracts VTA files for a given MDL path automatically.
// Searches beside the MDL, in Black Mesa \bms\models, or inside VPKs.
// No user action required – importer calls GetVtaPath() and proceeds.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uSource.Integration
{
    public static class BlackMesaVtaLocator
    {
        private static readonly string[] SteamCommonRoots =
        {
            // Typical Steam library locations – extend if needed.
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common")
        };

        /// <summary>
        /// Returns absolute VTA file path that matches given MDL, or null when none found.
        /// </summary>
        public static string GetVtaPath(string mdlPath)
        {
            if (string.IsNullOrEmpty(mdlPath) || !File.Exists(mdlPath))
                return null;

            var mdlName = Path.GetFileNameWithoutExtension(mdlPath);
            var vtaCandidate = Path.ChangeExtension(mdlPath, ".vta");
            if (File.Exists(vtaCandidate))
                return vtaCandidate;

            // Search in neighbouring folders (typical Crowbar dumps)
            var dir = Path.GetDirectoryName(mdlPath);
            var neighbour = Directory.EnumerateFiles(dir, mdlName + ".vta", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(neighbour))
                return neighbour;

            // Search inside Black Mesa installation
            foreach (var root in SteamCommonRoots.Where(Directory.Exists))
            {
                var bmsModels = Path.Combine(root, "Black Mesa", "bms", "models");
                if (Directory.Exists(bmsModels))
                {
                    var vta = Directory.EnumerateFiles(bmsModels, mdlName + ".vta", SearchOption.AllDirectories).FirstOrDefault();
                    if (!string.IsNullOrEmpty(vta))
                        return vta;
                }
            }

            // Attempt VPK extraction (only if _dir.vpk present)
            var vpkDir = FindDirVpkForModel(mdlPath);
            if (!string.IsNullOrEmpty(vpkDir) && File.Exists(vpkDir))
            {
                try
                {
                    return ExtractVtaFromVpk(vpkDir, mdlName);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[BlackMesaVtaLocator] VPK extract failed: {ex.Message}");
                }
            }

            return null; // no luck
        }

        private static string FindDirVpkForModel(string mdlPath)
        {
            var dir = Path.GetDirectoryName(mdlPath);
            while (!string.IsNullOrEmpty(dir))
            {
                var vpk = Directory.EnumerateFiles(dir, "*_dir.vpk", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (!string.IsNullOrEmpty(vpk))
                    return vpk;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        private static string ExtractVtaFromVpk(string dirVpkPath, string mdlName)
        {
            // Minimal VPK extractor – supports VPK v1 (Black Mesa) uncompressed entries.
            // Reads directory tree to locate <mdlName>.vta and dumps it next to mdlPath.

            using (var fs = File.OpenRead(dirVpkPath))
            using (var br = new BinaryReader(fs))
            {
                var signature = br.ReadUInt32(); // 0x55AA1234
                var version   = br.ReadUInt32(); // should be 1
                if (signature != 0x55AA1234 || version != 1)
                    throw new InvalidDataException("Unsupported VPK format");

                var dirLength = br.ReadUInt32(); // directory tree length
                br.ReadUInt32(); // embedded checksum (ignored)

                var dirBytes = br.ReadBytes((int)dirLength);
                using (var ms = new MemoryStream(dirBytes))
                using (var dr = new BinaryReader(ms))
                {
                    while (true)
                    {
                        var extension = ReadNullString(dr);
                        if (string.IsNullOrEmpty(extension)) break;

                        while (true)
                        {
                            var path = ReadNullString(dr);
                            if (string.IsNullOrEmpty(path)) break;

                            while (true)
                            {
                                var file = ReadNullString(dr);
                                if (string.IsNullOrEmpty(file)) break;

                                var fullName = Path.Combine(path, file + "." + extension).Replace("\\", "/").Trim('/');
                                var crc      = dr.ReadUInt32();
                                var small    = dr.ReadUInt16();
                                var archive  = dr.ReadUInt16();
                                var offset   = dr.ReadUInt32();
                                var length   = dr.ReadUInt32();
                                var terminator = dr.ReadUInt16(); // 0xFFFF

                                if (string.Equals(fullName, $"models/{file}.{extension}", StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(Path.GetFileNameWithoutExtension(fullName), mdlName, StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(extension, "vta", StringComparison.OrdinalIgnoreCase))
                                {
                                    var splitPath = dirVpkPath.Replace("_dir.vpk", $"_{archive:D3}.vpk");
                                    using (var dataFs = File.OpenRead(splitPath))
                                    {
                                        dataFs.Seek(offset, SeekOrigin.Begin);
                                        var buffer = new byte[length];
                                        dataFs.Read(buffer, 0, buffer.Length);

                                        var outPath = Path.Combine(Path.GetDirectoryName(dirVpkPath), mdlName + ".vta");
                                        File.WriteAllBytes(outPath, buffer);
                                        UnityEngine.Debug.Log($"[BlackMesaVtaLocator] Extracted VTA → {outPath}");
                                        return outPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static string ReadNullString(BinaryReader br)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = br.ReadByte()) != 0)
                bytes.Add(b);
            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
