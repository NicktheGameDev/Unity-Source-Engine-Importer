using System.Collections.Generic;
using UnityEngine;
using uSource.Formats.Source.MDL;

public class TextureManager
{
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private WadFile wadFile;

    public TextureManager(string wadFilePath)
    {
        wadFile = new WadFile(wadFilePath);
    }

 public Texture2D LoadTexture(string textureName)
{
    if (textures.ContainsKey(textureName))
        return textures[textureName];

    WadLump lump = wadFile.GetFile(textureName);
    if (lump is MipTex mipTex)
    {
        // Zamiast pobierać bajty, pobieramy gotową teksturę.
        Texture2D texture = mipTex.LoadTexture();
        textures[textureName] = texture;
        return texture;
    }

    return null;
}
}
