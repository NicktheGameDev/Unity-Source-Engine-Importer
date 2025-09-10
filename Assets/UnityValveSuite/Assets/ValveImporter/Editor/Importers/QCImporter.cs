using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ValveImporter.Editor.Importers
{
    public sealed class QCImporter : EditorWindow
    {
        [MenuItem("uSource/Import QC – universal")]
        private static void Run() => ImportQCFile();

        public static void ImportQCFile()
        {
            var qc = EditorUtility.OpenFilePanel("Select QC", "", "qc");
            if (string.IsNullOrEmpty(qc)) return;
            Import(qc);
        }

        private static void Import(string qcPath)
        {
            var dir   = Path.GetDirectoryName(qcPath);
            var content = File.ReadAllText(qcPath);

            var smdRx = new Regex(@"\$(?:model|body|bodygroup)\s+""[^""]*""\s+""([^""]+\.smd)""", RegexOptions.IgnoreCase);
            var smds = smdRx.Matches(content).Select(m => Path.GetFullPath(Path.Combine(dir, m.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar))))
                           .Where(File.Exists).Distinct().ToList();
            if (smds.Count == 0) { Debug.LogError("QCImporter: no SMD refs"); return; }

            var vtas = new List<string>();
            var flexRx = new Regex(@"\$flexfile\s+""([^""]+\.vta)""", RegexOptions.IgnoreCase);
            vtas.AddRange(flexRx.Matches(content).Select(m => Path.GetFullPath(Path.Combine(dir, m.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar))))
                                                 .Where(File.Exists));
            vtas.AddRange(Directory.GetFiles(dir, "*.vta", SearchOption.TopDirectoryOnly));
            vtas = vtas.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Import
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(qcPath) + "_Root");
            Transform[] bones = null;
            int injected = 0;

            foreach (var smd in smds)
            {
                GameObject part;
                SMDImporter.ImportSMDPath(smd, bones, out part);
                if (part == null) continue;
                part.transform.SetParent(root.transform, false);

                if (bones == null)
                    bones = part.GetComponentInChildren<SkinnedMeshRenderer>()?.bones;

                var smr = part.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) continue;

                foreach (var vta in vtas)
                    if (VTAImporter.InjectBlendShapes(smr.gameObject, vta))
                    { injected++; break; }
            }

            Debug.Log($"QCImporter: parts:{smds.Count} blend-shapes:{injected} vtas:{vtas.Count}");
            Selection.activeObject = root;
        }

        private void OnGUI()
        {
            GUILayout.Label("QC universal importer – działa z VTA z BST, Crowbar, StudioCompiler.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Select QC File…", GUILayout.Height(40)))
                ImportQCFile();
        }
    }
}
