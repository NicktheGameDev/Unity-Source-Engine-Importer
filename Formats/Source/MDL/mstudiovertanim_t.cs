using UnityEngine;
namespace uSource.Formats.Source.MDL
{

    // =========================================================================
    // Source Engine vertex animation (flex) struct – 32 bytes
    // =========================================================================
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct mstudiovertanim_t
    {
        public ushort index;                      // vertex index
        public sbyte speed;                       // TODO: not used for flexes
        public byte side;                         // TODO: not used
        public short sPositionX;
        public short sPositionY;
        public short sPositionZ;
        public short sNormalX;
        public short sNormalY;
        public short sNormalZ;
        public byte unused0;                      // holds flex descriptor in Source2013
        public byte unused1, unused2, unused3;    // padding

        public Vector3 PositionDelta => new Vector3(
            sPositionX * (1.0f / 32767.0f),
            sPositionY * (1.0f / 32767.0f),
            sPositionZ * (1.0f / 32767.0f));

        public Vector3 NormalDelta => new Vector3(
            sNormalX * (1.0f / 32767.0f),
            sNormalY * (1.0f / 32767.0f),
            sNormalZ * (1.0f / 32767.0f));

        public byte FlexDescriptor => unused0;
    }

}
