using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SourceEntity
{
    private Dictionary<string, string> data;
    private string header;

    public SourceEntity(string header)
    {
        this.header = header;
        data = new Dictionary<string, string>();
    }

    public void SetKey(string key, string value)
    {
        data[key] = value;
    }

    public string GetStringValue(string key)
    {
        if (data.ContainsKey(key))
        {
            return data[key];
        }
        return string.Empty;
    }

    public Vector3 GetVectorValue(string key)
    {
        if (!data.ContainsKey(key)) return Vector3.zero;

        MatchCollection matches = Regex.Matches(data[key], @"-?\d+\.?\d*");
        if (matches.Count == 3)
        {
            return new Vector3(
                float.Parse(matches[0].Value),
                float.Parse(matches[1].Value),
                float.Parse(matches[2].Value)
            );
        }

        Debug.LogWarning($"Vector data for key '{key}' is malformed.");
        return Vector3.zero;
    }

    public float GetFloatValue(string key)
    {
        if (data.ContainsKey(key) && float.TryParse(data[key], out float result))
        {
            return result;
        }
        return 0f;
    }

    public int GetIntValue(string key)
    {
        if (data.ContainsKey(key) && int.TryParse(data[key], out int result))
        {
            return result;
        }
        return 0;
    }

    public bool HasKey(string key)
    {
        return data.ContainsKey(key);
    }
}
