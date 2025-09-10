using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class BSPParser
{
    public static List<SourceEntity> ParseEntities(string bspEntityData)
    {
        // Check if bspEntityData is null
        if (bspEntityData == null)
        {
            Debug.LogError("bspEntityData is null. Make sure the input string is initialized before parsing.");
            return new List<SourceEntity>(); // Return an empty list to avoid further errors.
        }

        List<SourceEntity> entities = new List<SourceEntity>();
        string[] lines = bspEntityData.Split('\n');
        int depth = 0;
        SourceEntity currentEntity = null;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine == "{")
            {
                depth++;
                if (depth == 1)
                {
                    currentEntity = new SourceEntity("");
                }
            }
            else if (trimmedLine == "}")
            {
                depth--;
                if (depth == 0 && currentEntity != null)
                {
                    entities.Add(currentEntity);
                    currentEntity = null;
                }
            }
            else if (depth == 1 && currentEntity != null)
            {
                Match match = Regex.Match(trimmedLine, "\"(.*?)\"\\s*\"(.*?)\"");
                if (match.Success)
                {
                    currentEntity.SetKey(match.Groups[1].Value, match.Groups[2].Value);
                }
            }
        }

        return entities;
    }
}
