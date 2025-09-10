using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Text;

namespace uSource
{
    public class uReader : BinaryReader
    {
        public Quaternion ReadQuaternion48( )
        {
            var q48 = new Quaternion48();
            ReadTypeFixed(ref q48, 6);
            return q48.quaternion;
        }

        public Quaternion ReadQuaternion64()
        {
            var q64 = new Quaternion64();
            ReadTypeFixed(ref q64, 8);
            return q64.quaternion;
        }

        public Vector3 ReadVector48()
        {
            var v48 = new FVector48();
            ReadTypeFixed(ref v48, 6);
            return v48.ToVector3();
        }

        public Stream InputStream;

        public uReader(Stream InputStream)
            : base(InputStream)
        {
            this.InputStream = InputStream;

            if (!InputStream.CanRead)
                throw new InvalidDataException("Stream unreadable!");
        }

        public byte[] GetBytes(int Count, long Offset)
        {
            if (!Offset.Equals(0) && !Offset.Equals(InputStream.Position))
                InputStream.Seek(Offset, SeekOrigin.Begin);

            byte[] Buffer = new byte[Count];
            InputStream.Read(Buffer, 0, Buffer.Length);

            return Buffer;
        }
  
        public void ReadType<T>(ref T Variable, long? Offset = null)
        {
            if (Offset.HasValue)
                InputStream.Seek(Offset.Value, SeekOrigin.Begin);

            Byte[] Buffer = new byte[Marshal.SizeOf(typeof(T))];
            InputStream.Read(Buffer, 0, Buffer.Length);

            GCHandle Handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            try
            {
                Variable = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                Handle.Free();
            }
        }

        public void ReadArray<T>(ref T[] Array, long? Offset = null)
        {
            if (Offset.HasValue)
                InputStream.Seek(Offset.Value, SeekOrigin.Begin);

            for (Int32 i = 0; i < Array.Length; i++)
                ReadType(ref Array[i]);
        }

        public void ReadTypeFixed<T>(ref T Variable, Int32 TypeSizeOf, long? Offset = null)
        {
            if (Offset.HasValue)
                InputStream.Seek(Offset.Value, SeekOrigin.Begin);

            Byte[] Buffer = new byte[TypeSizeOf];
            InputStream.Read(Buffer, 0, Buffer.Length);

            GCHandle Handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            try
            {
                Variable = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                Handle.Free();
            }
        }

        public void ReadArrayFixed<T>(ref T[] Array, Int32 TypeSizeOf, long? Offset = null)
        {
            if (Offset.HasValue)
                InputStream.Seek(Offset.Value, SeekOrigin.Begin);

            for (Int32 i = 0; i < Array.Length; i++)
                ReadTypeFixed(ref Array[i], TypeSizeOf);
        }

        [ThreadStatic]
        private static StringBuilder _sBuilder;
        public string ReadNullTerminatedString(int? offset = null)
        {
            // if caller passed an explicit offset, seek there (but only if valid)
            if (offset.HasValue)
            {
                long len = BaseStream.Length;
                if (offset < 0 || offset >= len)
                    return String.Empty;
                BaseStream.Position = offset.Value;
            }

            // now read until we hit a 0 byte or the end of the stream
            var sb = new System.Text.StringBuilder();
            long max = BaseStream.Length;
            while (BaseStream.Position < max)
            {
                int b = ReadByte();
                if (b == 0) break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        public Vector3 ReadVector2D()
        {
            Vector2 Vector2D;// = new Vector2(ReadSingle(), ReadSingle());
            Vector2D.x = ReadSingle();
            Vector2D.y = ReadSingle();

            return Vector2D;
        }
        // Czyta tablicę struktur o stałym rozmiarze
        public T[] ReadTypeFixed<T>(int count, int offset) where T : struct
        {
            int stride = Marshal.SizeOf<T>();
            var arr = new T[count];

            for (int i = 0; i < count; i++)
            {
                // wczytujemy elementy jeden po drugim
                ReadTypeFixed(ref arr[i], stride, offset + i * stride);
            }
            return arr;
        }

        public Vector3 ReadVector3D(bool SwapZY = true)
        {
            Vector3 Vector3D;// = new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
            Vector3D.x = ReadSingle();
            Vector3D.y = ReadSingle();
            Vector3D.z = ReadSingle();

            if (SwapZY)
            {
                //float x = Vector3D.x;
                //float y = Vector3D.y;
                //float z = Vector3D.z;

                Single tempX = Vector3D.x;

                Vector3D.x = -Vector3D.y;
                Vector3D.y = Vector3D.z;
                Vector3D.z = tempX;
            }

            return Vector3D;
        }

        public Vector3 ReadVector4D()
        {
            Vector4 Vector4D;
            Vector4D.x = ReadSingle();
            Vector4D.y = ReadSingle();
            Vector4D.z = ReadSingle();
            Vector4D.w = ReadSingle();

            return Vector4D;
        }
    }
}