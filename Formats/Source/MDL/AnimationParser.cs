using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    public static partial class AnimationParser
    {
        // Overload wymagany przez MdlAnimationLoader.ReadAnimationData
        public static void ReadAnimationFrameValues(
            uReader br,
            studiohdr_t hdr,
            AnimationBone dst,
            HashFuncs.mstudioanim_t header,
            int numFrames,
            bool isRotation,
            bool delta)
        {
            // Schemat: w tym miejscu strumienia są 3 offsety (X,Y,Z) dla kanału,
            // każdy offset jest liczony względem startPos.
            var s = br.BaseStream;
            long startPos = s.Position;

            short[] offsets = br.ReadShortArray(3);
            long endPos = s.Position;

            // bufor [frame][axis]
            var tmp = new float[numFrames][];
            for (int i = 0; i < numFrames; i++) tmp[i] = new float[3];

            for (int axis = 0; axis < 3; axis++)
            {
                short off = (offsets != null && offsets.Length > axis) ? offsets[axis] : (short)0;
                if (off == 0) continue;

                long target = startPos + (ushort)off; // uint16 – nigdy ujemny
                if (target <= 0 || target >= s.Length) continue;
                s.Position = target;

                int frame = 0;
                short prev = 0;
                bool havePrev = false;

                while (frame < numFrames)
                {
                    if (s.Position + 2 > s.Length) break;

                    int total = s.ReadByte();
                    int valid = s.ReadByte();
                    if (total <= 0) break;

                    // wartości
                    for (int i = 0; i < valid && frame < numFrames; i++, frame++)
                    {
                        short v = ReadInt16LE(s);
                        if (delta && havePrev) v = (short)(v + prev);
                        tmp[frame][axis] = prev = v;
                        havePrev = true;
                    }

                    // powtórzenia
                    for (int i = valid; i < total && frame < numFrames; i++, frame++)
                    {
                        tmp[frame][axis] = prev;
                    }
                }
            }

            // Zapis do docelowych list
            if (isRotation)
            {
                EnsureListLength(dst.FrameAngles, numFrames);
                for (int f = 0; f < numFrames; f++)
                    dst.FrameAngles[f] = new Vector3(tmp[f][0], tmp[f][1], tmp[f][2]);
            }
            else
            {
                EnsureListLength(dst.FramePositions, numFrames);
                for (int f = 0; f < numFrames; f++)
                    dst.FramePositions[f] = new Vector3(tmp[f][0], tmp[f][1], tmp[f][2]);
            }

            s.Position = endPos;
        }

        private static short ReadInt16LE(Stream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            if ((b0 | b1) < 0) return 0;
            return (short)(b0 | (b1 << 8));
        }

        private static void EnsureListLength(List<Vector3> list, int count)
        {
            while (list.Count < count) list.Add(Vector3.zero);
        }
    }
}
