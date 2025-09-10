using System.IO;
using uSource;

public class FlexController
{
    internal float Default;
    internal float Current;

    public string Group { get; set; }
    public string Name { get; set; }
    public int LocalToGlobal { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }

    public static FlexController FromReader(BinaryReader reader)
    {
        string group = reader.ReadNullTerminatedString();
        string name = reader.ReadNullTerminatedString();
        int localToGlobal = reader.ReadInt32();
        float min = reader.ReadSingle();
        float max = reader.ReadSingle();

        return new FlexController
        {
            Group = group,
            Name = name,
            LocalToGlobal = localToGlobal,
            Min = min,
            Max = max
        };
    }
}
