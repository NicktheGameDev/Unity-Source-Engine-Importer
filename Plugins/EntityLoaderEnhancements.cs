
using UnityEngine;
using System;
using System.Reflection;

namespace uSource
{
    // Enhanced entity loading with DEFINE_KEYFIELD, Think, Precache
    public static class EntityLoaderEnhancements
    {
        public static void RegisterFields(object entity, KeyValues.KVSection section)
        {
            foreach(var kv in section.Children)
            {
                var field = entity.GetType().GetField(kv.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(field != null)
                {
                    var val = Convert.ChangeType(kv.Value, field.FieldType);
                    field.SetValue(entity, val);
                }
            }
        }

        public static void CallThink(object entity)
        {
            var method = entity.GetType().GetMethod("Think", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            method?.Invoke(entity, null);
        }

        public static void CallPrecache(object entity)
        {
            var method = entity.GetType().GetMethod("Precache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            method?.Invoke(entity, null);
        }
    }
}
