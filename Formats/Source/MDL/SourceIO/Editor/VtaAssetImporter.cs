// Assets/Editor/VtaAssetImporter.cs
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using USource.Model.Flex;                 // VtaSeparatorSmooth
// oraz VtaData (już w Runtime)
using static USource.Model.Flex.VtaSeparatorSmooth;

[ScriptedImporter(1, "vta")]
public class VtaScriptedImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // 1) utwórz instancję istniejącego ScriptableObject
        var so = ScriptableObject.CreateInstance<VtaData>();

        // 2) sparsuj plik VTA → słownik  flex‑index → List<FlexFrame>
        var dict = VtaSeparatorSmooth.ParseVta(ctx.assetPath, smooth: true);

        // 3) zapisz we właściwości runtime’owej klasy
        //    (framesPerFlex lub flexFrames – zależnie od definicji)
        so.flexFrames = dict
            .OrderBy(kvp => kvp.Key)              // rosnąco wg flex‑index
            .SelectMany(kvp => kvp.Value)         // flatten, bo VtaData trzyma jedną listę
            .ToList();

        // 4) zarejestruj asset
        ctx.AddObjectToAsset("VtaData", so);
        ctx.SetMainObject(so);
    }
}