
using System;
using System.Collections.Generic;

namespace uSource
{
    // Launch parameter support
    public static class CommandLineArgs
    {
        public static readonly Dictionary<string, string> Args = new Dictionary<string, string>();

        static CommandLineArgs()
        {
            foreach(var arg in Environment.GetCommandLineArgs())
            {
                if(arg.StartsWith("-"))
                {
                    var kv = arg.Substring(1).Split('=');
                    if(kv.Length == 2)
                        Args[kv[0]] = kv[1];
                    else
                        Args[kv[0]] = "true";
                }
            }
        }

        public static bool Has(string key) => Args.ContainsKey(key);
        public static string Get(string key, string def = "") => Args.TryGetValue(key, out var v) ? v : def;
    }
}
