
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace uSource
{
    // Full SoundScript and phoneme loader for lip sync
    public class SoundScriptLoader
    {
        public class PhonemeEvent { public float time; public string phoneme; }

        // Simple KeyValue parser
        private static Dictionary<string, object> ParseKV(TextReader reader)
        {
            var dict = new Dictionary<string, object>();
            string line;
            while((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if(line.StartsWith("\""))
                {
                    var parts = line.Split('"');
                    if(parts.Length >= 3)
                    {
                        string key = parts[1];
                        string val = parts[3];
                        dict[key] = val;
                    }
                }
            }
            return dict;
        }

        public static List<PhonemeEvent> LoadPhonemeScript(string scriptPath)
        {
            var events = new List<PhonemeEvent>();
            using(var sr = new StreamReader(scriptPath))
            {
                var kv = ParseKV(sr);
                if(kv.ContainsKey("phonemeEvents"))
                {
                    // phonemeEvents expected as semicolon-separated list
                    var raw = kv["phonemeEvents"] as string;
                    var entries = raw.Split(';');
                    foreach(var e in entries)
                    {
                        var kvp = e.Split(',');
                        if(kvp.Length == 2)
                        {
                            if(float.TryParse(kvp[0], out float t))
                            {
                                string p = kvp[1];
                                events.Add(new PhonemeEvent{time = t, phoneme = p});
                            }
                        }
                    }
                }
            }
            return events;
        }

        public static void ApplyLipSync(GameObject actor, AudioClip clip, List<PhonemeEvent> events)
        {
            var lipSync = actor.AddComponent<USourceLipSync>();
            lipSync.audioSource.clip = clip;
            lipSync.PhonemeEvents = events;
            lipSync.Initialize();
        }
    }
}
