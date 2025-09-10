
using System.IO;
using UnityEngine;

using uSource.Formats.Source.MDL;

namespace uSource
{
    // Realistic MDL loader matching Source SDK 13 with flex and LOD via command-line flags
    public static class RealMDLLoader
    {
        public static GameObject Load(string mdlPath)
        {
            if(!File.Exists(mdlPath))
                throw new FileNotFoundException("MDL file not found", mdlPath);

            // Load MDL file
            var mdl = MDLFile.Load(mdlPath,parseAnims: true, parseHitboxes: false);

            // Initialize importer with full sequences, bodygroups, flexes, physics, attachments
            var importer = new MDLImporter();

            // Apply command-line overrides (e.g., -mdlNoFlex, -mdlSkipPhysics)
            importer.ApplyConVars(CommandLineArgs.Args);

            // Build Unity GameObject
            return importer.ImportToUnity();
        }
    }
}
