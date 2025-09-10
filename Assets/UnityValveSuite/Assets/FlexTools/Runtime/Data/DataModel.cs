
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FlexTools.Runtime.Data
{
    /// <summary>Simple DMX‑style element.</summary>
    [Serializable]
    public class Element
    {
        public string name;
        public string type;
        public string id;
        public Dictionary<string, object> attrs = new Dictionary<string, object>();

        public Element() { }

        public Element(string name, string type, string id)
        {
            this.name = name;
            this.type = type;
            this.id   = id;
        }
    }

    /// <summary>Very lightweight data‑model used to store flex controller blocks.</summary>
    [Serializable]
    public class DataModel
    {
        public Element root;
        public List<Element> all = new List<Element>();

        public DataModel(string rootName, string rootId = null)
        {
            root = AddElement(rootName, "root", rootId ?? rootName);
        }

        public Element AddElement(string name, string type, string id = null)
        {
            var e = new Element(name, type, id ?? Guid.NewGuid().ToString("N"));
            all.Add(e);
            return e;
        }

        /// <summary>
        /// Serialize to a *very* simple JSON object – good enough for debug and post‑processing tools.
        /// </summary>
        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            for (int i = 0; i < all.Count; i++)
            {
                Element e = all[i];
                sb.Append("  \"").Append(e.id).Append("\": { \"name\":\"")
                  .Append(e.name).Append("\", \"type\":\"")
                  .Append(e.type).Append("\", \"attrs\": {} }");
                if (i < all.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
