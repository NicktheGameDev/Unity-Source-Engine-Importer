
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using static uSource.KeyValues;

namespace uSource
{
    /// <summary>
    /// Kompletny loader mapy: ładuje BSP, parsuje encje i instancjuje prefabrykaty.
    /// </summary>
    public static class MapLoader
    {
        public static List<GameObject> LoadMap(string bspPath)
        {
            // Ekstrakcja lumpu encji z BSP
            string entitiesText = BSPLoader.LoadEntities(bspPath);
            // Zapis tymczasowy i parse KeyValues
            string temp = Path.Combine(Application.temporaryCachePath, "entities.kv");
            File.WriteAllText(temp, entitiesText);
            var root = KVSerializer.Load(temp);
            var results = new List<GameObject>();

            foreach (var sec in root.Children)
            {
                string className = sec.Name;
                // Ładujemy prefab z Resources/Entities/<className>
                GameObject prefab = Resources.Load<GameObject>("Entities/" + className);
                if (prefab != null)
                {
                    var go = GameObject.Instantiate(prefab);
                    // Rejestrujemy pola, precache i think
                    var comp = go.GetComponent(sec.Name) ?? go.GetComponent(typeof(Component));
                    if (comp != null)
                    {
                        EntityLoaderEnhancements.RegisterFields(comp, sec);
                        EntityLoaderEnhancements.CallPrecache(comp);
                        EntityLoaderEnhancements.CallThink(comp);
                    }
                    results.Add(go);
                }
            }
            return results;
        }
    }
}
