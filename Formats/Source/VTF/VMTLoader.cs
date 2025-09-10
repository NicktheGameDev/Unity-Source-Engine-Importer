using System.Collections.Generic;

using System.Globalization;
using System.IO;
using UnityEngine;

namespace uSource
{
    public class VMTLoader
    {
        public class VMTFile
        {
            public string basetexture;
            public string bumpmap;
            public string ssbump;
            public string surfaceprop;
            public bool alphatest;
            public bool translucent;
            public bool selfillum;
            public bool additive;
            public string envmap;
            public float basealphaenvmapmask;
            public float envmapcontrast;
            public float envmapsaturation;
            public Vector3 envmaptint;
            public string shader;
            public string basetexture2;
            public string bumpmap2;

            // Other necessary fields
        }

        public static VMTFile ParseVMTFile(string name)
        {
            VMTFile material = new VMTFile();
            string path = uResourceManager.LoadMaterial("materials/" + name + ".vmt")?.ToString();

            if (path == null)
            {
                Debug.LogError($"materials/{name}.vmt: Not Found");
                return null;
            }

            string[] file = File.ReadAllLines(path);
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string line in file)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//")) continue;

                if (trimmedLine.Contains(" "))
                {
                    string[] keyValue = trimmedLine.Split(new char[] { ' ' }, 2);
                    parameters[keyValue[0].Trim().ToLower()] = keyValue[1].Trim().Trim('"');
                }
            }

            if (parameters.ContainsKey("$basetexture"))
                material.basetexture = parameters["$basetexture"];
            if (parameters.ContainsKey("$bumpmap"))
                material.bumpmap = parameters["$bumpmap"];
            if (parameters.ContainsKey("$ssbump"))
                material.ssbump = parameters["$ssbump"];
            if (parameters.ContainsKey("$surfaceprop"))
                material.surfaceprop = parameters["$surfaceprop"];
            if (parameters.ContainsKey("$alphatest"))
                material.alphatest = parameters["$alphatest"] == "1";
            if (parameters.ContainsKey("$selfillum"))
                material.selfillum = parameters["$selfillum"] == "1";
            if (parameters.ContainsKey("$translucent"))
                material.translucent = parameters["$translucent"] == "1";
            if (parameters.ContainsKey("$additive"))
                material.additive = parameters["$additive"] == "1";
            if (parameters.ContainsKey("$envmap"))
                material.envmap = parameters["$envmap"];
            if (parameters.ContainsKey("$basealphaenvmapmask"))
                material.basealphaenvmapmask = float.Parse(parameters["$basealphaenvmapmask"]);
            if (parameters.ContainsKey("$envmapcontrast"))
                material.envmapcontrast = float.Parse(parameters["$envmapcontrast"]);
            if (parameters.ContainsKey("$envmapsaturation"))
                material.envmapsaturation = float.Parse(parameters["$envmapsaturation"]);
            if (parameters.ContainsKey("$envmaptint"))
                material.envmaptint = ParseVector3(parameters["$envmaptint"]);

            return material;
        }

        private static Vector3 ParseVector3(string value)
        {
            string[] parts = value.Split(' ');
            return new Vector3(
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
        }
    }
}
