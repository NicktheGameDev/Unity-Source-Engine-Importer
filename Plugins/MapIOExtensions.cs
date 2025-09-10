
using System;
using UnityEngine;
using System.IO;

namespace uSource
{
    // Map I/O support
    public static class MapIOExtensions
    {
        public static void ProcessIOEntity(string ioScriptPath)
        {
            var kv = KeyValues.KVSerializer.Load(ioScriptPath);
            foreach(var e in kv.Children)
            {
                // handle input/output keyfields
                // dynamic binding via reflection
                var entityType = Type.GetType("uSource." + e.Name);
                if(entityType != null)
                {
                    var obj = GameObject.Instantiate(Resources.Load(e.Name)) as GameObject;
                    EntityLoaderEnhancements.RegisterFields(obj.GetComponent(entityType), e);
                }
            }
        }
    }
}
