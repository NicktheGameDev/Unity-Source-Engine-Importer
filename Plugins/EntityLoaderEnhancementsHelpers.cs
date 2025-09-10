using System;
using System.Reflection;

namespace uSource
{
    internal static class EntityLoaderEnhancementsHelpers
    {
        public static void RegisterFields(object entity, KeyValues.KVSection section)
        {
            foreach (var kv in section.Children)
            {
                var field = entity.GetType().GetField(kv.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    var val = Convert.ChangeType(kv.Value, field.FieldType);
                    field.SetValue(entity, val);
                }
            }
        }
    }
}