
/*  MDL_BinaryExtensions.cs — adds ReadUShortArray for offset tables  */
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace uSource.Formats.Source.MDL
{
    public static class BinaryReaderExt
    {
        public static T ReadStruct<T>(this BinaryReader br) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buf = br.ReadBytes(size);
            if (buf.Length != size) throw new EndOfStreamException();
            var pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
            try { return (T)Marshal.PtrToStructure(pin.AddrOfPinnedObject(), typeof(T)); }
            finally { pin.Free(); }
        }

        public static T ReadStruct<T>(uReader r) where T : struct
            => ReadStruct<T>(new BinaryReader(r.BaseStream));

        public static short[] ReadShortArray(this BinaryReader br, int count)
        {
            var arr = new short[count];
            for (int i = 0; i < count; i++) arr[i] = br.ReadInt16();
            return arr;
        }

        public static ushort[] ReadUShortArray(this BinaryReader br, int count)
        {
            var arr = new ushort[count];
            for (int i = 0; i < count; i++) arr[i] = br.ReadUInt16();
            return arr;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Quaternion48
    {
        public ushort ix, iy, iz;
        public Quaternion quaternion
        {
            get
            {
                float x = ((short)ix) / 32767f;
                float y = ((short)iy) / 32767f;
                float z = ((short)iz) / 32767f;
                float w = Mathf.Sqrt(Mathf.Max(0f, 1f - x*x - y*y - z*z));
                return new Quaternion(x, y, z, w).normalized;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Quaternion64
    {
        public short x, y, z, w;
        public Quaternion quaternion =>
            new Quaternion(x / 32767f, y / 32767f, z / 32767f, w / 32767f).normalized;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FVector48
    {
        public short x, y, z;
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
}
