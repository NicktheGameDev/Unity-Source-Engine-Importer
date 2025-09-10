using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Flex
{
    public int FlexDescIndex { get; set; }
    public Vector4 Targets { get; set; }
    public int? PartnerIndex { get; set; }
    public VertexAnimationType VertexAnimType { get; set; }
    public List<VertexAnimation> VertexAnimations { get; set; } = new List<VertexAnimation>();
    public string Name { get; internal set; }

    public static Flex FromReader(BinaryReader reader, int version)
    {
        int flexDescIndex = reader.ReadInt32();
        Vector4 targets = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        int vertCount = reader.ReadInt32();
        int vertOffset = reader.ReadInt32();

        int? partnerIndex = null;
        VertexAnimationType vertexAnimType = VertexAnimationType.NORMAL;

        if (version > 36)
        {
            partnerIndex = reader.ReadInt32();
            vertexAnimType = (VertexAnimationType)reader.ReadByte();
            reader.BaseStream.Position += (3 + 6 * sizeof(float)); // Skip unused bytes
        }

        var animations = new List<VertexAnimation>();
        if (vertCount > 0 && vertOffset != 0)
        {
            reader.BaseStream.Seek(vertOffset, SeekOrigin.Begin);
            for (int i = 0; i < vertCount; i++)
            {
                animations.Add(VertexAnimation.FromReader(reader));
            }
        }

        return new Flex
        {
            FlexDescIndex = flexDescIndex,
            Targets = targets,
            PartnerIndex = partnerIndex,
            VertexAnimType = vertexAnimType,
            VertexAnimations = animations
        };
    }
}

public enum VertexAnimationType
{
    NORMAL = 0,
    WRINKLE = 1
}

public class VertexAnimation
{
    public int Index { get; set; }
    public Vector3 PositionDelta { get; set; }
    public Vector3 NormalDelta { get; set; }

    public static VertexAnimation FromReader(BinaryReader reader)
    {
        int index = reader.ReadUInt16();
        Vector3 positionDelta = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        Vector3 normalDelta = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        return new VertexAnimation
        {
            Index = index,
            PositionDelta = positionDelta,
            NormalDelta = normalDelta
        };
    }
}
