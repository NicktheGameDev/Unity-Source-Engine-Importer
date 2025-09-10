using System;
using UnityEngine;

namespace uSource
{
    public abstract class MaterialLoader
    {
        public abstract void ApplyMaterial(VMTLoader.VMTFile vmtFile, Material material);

       
        protected Texture2D LoadTexture(string path, string exportData)
        {
            return uResourceManager.LoadTexture(path, exportData)[0, 0];
        }
    }
}
