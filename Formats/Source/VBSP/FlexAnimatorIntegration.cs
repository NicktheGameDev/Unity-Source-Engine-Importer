using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USource.Model.Flex;
using uSource.Formats.Source.MDL;

/// <summary>
/// Integrator blendshape animation (flexes) z VTA i phonemów.
/// </summary>
public static class FlexAnimatorIntegration
{
    // Mapa phonema → nazwa visemu (blendshape)
    private static readonly Dictionary<string, string> PhonemeVisemeMap = new Dictionary<string, string>
    {
        {"AA", "Flex_aa"},
        {"AE", "Flex_ae"},
        {"AH", "Flex_ah"},
        {"AO", "Flex_ao"},
        {"EH", "Flex_eh"},
        {"ER", "Flex_er"},
        {"IH", "Flex_ih"},
        {"IY", "Flex_iy"},
        {"OW", "Flex_ow"},
        {"UH", "Flex_uh"},
        {"UW", "Flex_uw"},
        {"OH", "Flex_ow"}
    };

    // Sekwencje vertanim dla phonemów
    private static Dictionary<string, List<VtaSeparatorSmooth.FlexFrame>> vertAnimMap;

    /// <summary>
    /// Inicjalizuje integrator: subskrybuje phonemy i ładuje mapowania
    /// </summary>
    /// <param name="root">Root GameObject z MDLFileHolder</param>
    /// <param name="flexInfoDict">Mapa flex name → FlexInfo</param>
    /// <param name="framesDict">Mapa flexIndex → lista klatek VTA</param>
    public static void Setup(GameObject root, Dictionary<string, QcFlexData.FlexInfo> flexInfoDict, Dictionary<int, List<VtaSeparatorSmooth.FlexFrame>> framesDict)
    {
        // Przygotowujemy sekwencje vertanim dla każdego phonemu
        vertAnimMap = new Dictionary<string, List<VtaSeparatorSmooth.FlexFrame>>();
        foreach (var kv in PhonemeVisemeMap)
        {
            string phoneme = kv.Key;
            string visemeName = kv.Value;
            // Znajdź entry po nazwie visemu
            var entry = flexInfoDict.FirstOrDefault(x => x.Value.name == visemeName);
            if (!string.IsNullOrEmpty(entry.Key) && framesDict.TryGetValue(Convert.ToInt32(entry.Key), out var frames))
            {
                vertAnimMap[phoneme] = frames;
            }
        }

        // Subskrybujemy event phonemów
        var holder = root.GetComponent<MDLFileHolder>();
        if (holder != null)
        {
            holder.mdlFile.OnPhonemeEvent += (obj, phoneme) => OnPhonemeEvent((GameObject)obj, phoneme.ToString());
        }
    }

    /// <summary>
    /// Handler phonemy: odtwarza vertanim, jeśli dostępny, inaczej statyczny blendshape
    /// </summary>
    public static void OnPhonemeEvent(GameObject root, string phoneme)
    {
        if (vertAnimMap != null && vertAnimMap.TryGetValue(phoneme.ToUpper(), out var frames))
        {
            PhonemeRunner.OnPhonemeEvent(root, frames);
            return;
        }

        if (!PhonemeVisemeMap.TryGetValue(phoneme.ToUpper(), out var blendName))
            return;

        foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            int idx = smr.sharedMesh.GetBlendShapeIndex(blendName);
            if (idx >= 0)
                smr.SetBlendShapeWeight(idx, 100f);
        }
    }
}
