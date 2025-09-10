using System;
using System.Collections.Generic;
namespace ValveImporter.Editor.Parsers
{
    public class QCCommand
    {
        public string Name;
        public List<string> Args=new();
    }
    public static class QCParser
    {
        public static List<QCCommand> Parse(string[] lines){
            var cmds=new List<QCCommand>();
            foreach(var raw in lines){
                var line=raw.Trim();
                if(string.IsNullOrEmpty(line)||!line.StartsWith("$")) continue;
                var parts=line.Split(new[]{' ','\t','"'},StringSplitOptions.RemoveEmptyEntries);
                var cmd=new QCCommand{Name=parts[0].ToLower()};
                for(int i=1;i<parts.Length;i++) cmd.Args.Add(parts[i]);
                cmds.Add(cmd);
            }
            return cmds;
        }
    }
}
