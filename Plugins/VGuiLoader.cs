
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static uSource.KeyValues;

namespace uSource
{
    /// <summary>
    /// Loader dla VGUI: parsuje układy i generuje Unity UI.
    /// </summary>
    public static class VGuiLoader
    {
        public static GameObject LoadLayout(string layoutPath)
        {
            var root = KVSerializer.Load(layoutPath);
            var go = CreateControl(root);
            // Zakładamy, że Canvas w scenie istnieje
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
                go.transform.SetParent(canvas.transform, false);
            return go;
        }

        private static GameObject CreateControl(KeyValues.KVSection sec)
        {
            var go = new GameObject(sec.Name);
            var rt = go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();

            // Wczytuje pozycję i rozmiar
            int x = GetInt(sec, "x"), y = GetInt(sec, "y");
            int w = GetInt(sec, "wide"), h = GetInt(sec, "tall");
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);

            // Dodatkowe atrybuty: kolor tła
            if (sec.Children.Exists(c => c.Name == "bgcolor"))
            {
                Color ccol;
                if (ColorUtility.TryParseHtmlString(sec.Children.Find(c => c.Name == "bgcolor").Value, out ccol))
                    image.color = ccol;
            }

            // Rekurencyjne tworzenie potomków
            foreach (var child in sec.Children)
            {
                if (child.Name == "x" || child.Name == "y" || child.Name == "wide" || child.Name == "tall" || child.Name == "bgcolor")
                    continue;
                var childGo = CreateControl(child);
                childGo.transform.SetParent(go.transform, false);
            }
            return go;
        }

        private static int GetInt(KeyValues.KVSection sec, string key)
        {
            var c = sec.Children.Find(ch => ch.Name == key);
            return c != null && int.TryParse(c.Value, out int v) ? v : 0;
        }
    }
}
