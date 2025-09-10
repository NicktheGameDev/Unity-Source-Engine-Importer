using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct IntegerAndSingleUnion
{
    [FieldOffset(0)]
    public int i;
    [FieldOffset(0)]
    public float s;
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Float16
{
    public float GetFloat
    {
        get
        {
            int sign = GetSign(bits);
            int floatSign = (sign == 1) ? -1 : 1;

            if (IsInfinity(bits))
                return maxfloat16bits * floatSign;

            if (IsNaN(bits))
                return 0;

            int mantissa = GetMantissa(bits);
            int biased_exponent = GetBiasedExponent(bits);

            if (biased_exponent == 0 && mantissa != 0)
            {
                float floatMantissa = mantissa / 1024.0F;
                return floatSign * floatMantissa * half_denorm;
            }
            else
            {
                return GetSingle();
            }
        }
    }

    private int GetMantissa(ushort value) => value & 0x3FF;
    private int GetBiasedExponent(ushort value) => (value & 0x7C00) >> 10;
    private int GetSign(ushort value) => (value & 0x8000) >> 15;

    private float GetSingle()
    {
        IntegerAndSingleUnion bitsResult = new IntegerAndSingleUnion { i = 0 };

        int mantissa = GetMantissa(bits);
        int biased_exponent = GetBiasedExponent(bits);
        int sign = GetSign(bits);

        int resultMantissa = mantissa << (23 - 10);
        int resultBiasedExponent = (biased_exponent == 0) ? 0 : (biased_exponent - float16bias + float32bias) << 23;
        int resultSign = sign << 31;

        bitsResult.i = resultSign | resultBiasedExponent | resultMantissa;

        return bitsResult.s;
    }

    private bool IsInfinity(ushort value) => GetBiasedExponent(value) == 31 && GetMantissa(value) == 0;
    private bool IsNaN(ushort value) => GetBiasedExponent(value) == 31 && GetMantissa(value) != 0;

    internal void SetFloat(float value) => bits = ConvertFloatTo16bits(value);

    public static ushort ConvertFloatTo16bits(float input)
    {
        if (input > maxfloat16bits) input = maxfloat16bits;
        else if (input < -maxfloat16bits) input = -maxfloat16bits;

        Float16bits output = new Float16bits();
        Float32bits inFloat = new Float32bits { rawFloat = input };

        output.bits.sign = (ushort)inFloat.bits.sign;

        if (inFloat.bits.biased_exponent == 0 && inFloat.bits.mantissa == 0)
        {
            output.bits.mantissa = 0;
            output.bits.biased_exponent = 0;
        }
        else if (inFloat.bits.biased_exponent == 0 && inFloat.bits.mantissa != 0)
        {
            output.bits.mantissa = 0;
            output.bits.biased_exponent = 0;
        }
        else if (inFloat.bits.biased_exponent == 255 && inFloat.bits.mantissa == 0)
        {
            output.bits.mantissa = 0x3FF;
            output.bits.biased_exponent = 0x1E;
        }
        else if (inFloat.bits.biased_exponent == 255 && inFloat.bits.mantissa != 0)
        {
            output.bits.mantissa = 0;
            output.bits.biased_exponent = 0;
        }
        else
        {
            int newExp = (int)(inFloat.bits.biased_exponent - 127);
            if (newExp < -24)
            {
                output.bits.mantissa = 0;
                output.bits.biased_exponent = 0;
            }
            else if (newExp < -14)
            {
                output.bits.biased_exponent = 0;
                int expVal = (int)(-14 - (inFloat.bits.biased_exponent - float32bias));
                if (expVal > 0 && expVal < 11)
                {
                    output.bits.mantissa = (ushort)((1 << (10 - expVal)) + (inFloat.bits.mantissa >> (13 + expVal)));
                }
            }
            else if (newExp > 15)
            {
                output.bits.mantissa = 0x3FF;
                output.bits.biased_exponent = 0x1E;
            }
            else
            {
                output.bits.biased_exponent = (ushort)(newExp + 15);
                output.bits.mantissa = (ushort)(inFloat.bits.mantissa >> 13);
            }
        }
        return output.rawWord;
    }

    private const int float32bias = 127;
    private const int float16bias = 15;
    private const float maxfloat16bits = 65504.0F;
    private const float half_denorm = 1.0F / 16384.0F;

    public ushort bits;

    [StructLayout(LayoutKind.Explicit)]
    private struct Float32bits
    {
        [FieldOffset(0)]
        public float rawFloat;
        [FieldOffset(0)]
        public Bits bits;

        [StructLayout(LayoutKind.Sequential)]
        public struct Bits
        {
            public uint mantissa;
            public uint biased_exponent;
            public uint sign;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Float16bits
    {
        [FieldOffset(0)]
        public ushort rawWord;
        [FieldOffset(0)]
        public Bits bits;

        [StructLayout(LayoutKind.Sequential)]
        public struct Bits
        {
            public ushort mantissa;
            public ushort biased_exponent;
            public ushort sign;
        }
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FVector48
{
    public Float16 x;
    public Float16 y;
    public Float16 z;

    public Vector3 ToVector3()
    {
        return new Vector3(x.GetFloat, y.GetFloat, z.GetFloat);
    }

    public static explicit operator Vector3(FVector48 obj)
    {
        return obj.ToVector3();
    }

    public static implicit operator FVector48(Vector3 vec)
    {
        return new FVector48 { x = new Float16 { bits = Float16.ConvertFloatTo16bits(vec.x) }, y = new Float16 { bits = Float16.ConvertFloatTo16bits(vec.y) }, z = new Float16 { bits = Float16.ConvertFloatTo16bits(vec.z) } };
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Quaternion48
{
    public ushort theXInput;
    public ushort theYInput;
    public ushort theZWInput;

    public float x => (theXInput - 32768) * (1 / 32768.0f);
    public float y => (theYInput - 32768) * (1 / 32768.0f);
    public float z => ((theZWInput & 0x7FFF) - 16384) * (1 / 16384.0f);
    public float w => Mathf.Sqrt(1 - x * x - y * y - z * z) * wneg;
    public float wneg => ((theZWInput & 0x8000) > 0) ? -1 : 1;

    public Quaternion quaternion => new Quaternion(x, y, z, w);
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Quaternion64
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] theBytes;

    public float x
    {
        get
        {
            IntegerAndSingleUnion bitsResult = new IntegerAndSingleUnion();
            int byte0 = theBytes[0] & 0xFF;
            int byte1 = (theBytes[1] & 0xFF) << 8;
            int byte2 = (theBytes[2] & 0x1F) << 16;

            bitsResult.i = byte2 | byte1 | byte0;
            float result = (bitsResult.i - 1048576) * (1 / 1048576.5f);
            return float.IsNaN(result) ? 0.0f : result;
        }
    }

    public float y
    {
        get
        {
            IntegerAndSingleUnion bitsResult = new IntegerAndSingleUnion();
            int byte2 = (theBytes[2] & 0xE0) >> 5;
            int byte3 = (theBytes[3] & 0xFF) << 3;
            int byte4 = (theBytes[4] & 0xFF) << 11;
            int byte5 = (theBytes[5] & 0x3) << 19;

            bitsResult.i = byte5 | byte4 | byte3 | byte2;
            float result = (bitsResult.i - 1048576) * (1 / 1048576.5f);
            return float.IsNaN(result) ? 0.0f : result;
        }
    }

    public float z
    {
        get
        {
            IntegerAndSingleUnion bitsResult = new IntegerAndSingleUnion();
            int byte5 = (theBytes[5] & 0xFC) >> 2;
            int byte6 = (theBytes[6] & 0xFF) << 6;
            int byte7 = (theBytes[7] & 0x7F) << 14;

            bitsResult.i = byte7 | byte6 | byte5;
            float result = (bitsResult.i - 1048576) * (1 / 1048576.5f);
            return float.IsNaN(result) ? 0.0f : result;
        }
    }

    public float w => Mathf.Sqrt(1 - x * x - y * y - z * z) * wneg;
    public float wneg => (theBytes[7] & 0x80) > 0 ? -1 : 1;

    public Quaternion quaternion => new Quaternion(x, y, z, w);
}

// Continue with Vector32, Normal32, Quaternion32 classes

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vector32
{
    private ushort x;
    private ushort y;
    private ushort z;
    private ushort exp;

    public static float[] expScale = { 4.0f, 16.0f, 32.0f, 64.0f };

    public Vector32(Vector3 vec)
    {
        float fmax = Mathf.Max(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        fmax = Mathf.Max(fmax, Mathf.Abs(vec.z));

        for (exp = 0; exp < 3; exp++)
        {
            if (fmax < expScale[exp])
                break;
        }

        float fexp = 512.0f / expScale[exp];

        x = (ushort)Mathf.Clamp((int)(vec.x * fexp) + 512, 0, 1023);
        y = (ushort)Mathf.Clamp((int)(vec.y * fexp) + 512, 0, 1023);
        z = (ushort)Mathf.Clamp((int)(vec.z * fexp) + 512, 0, 1023);
    }

    public static implicit operator Vector3(Vector32 vec32)
    {
        Vector3 vec = new Vector3();

        float fexp = expScale[vec32.exp] / 512.0f;

        vec.x = ((int)vec32.x - 512) * fexp;
        vec.y = ((int)vec32.y - 512) * fexp;
        vec.z = ((int)vec32.z - 512) * fexp;

        return vec;
    }


    public static implicit operator Vector32(Vector3 vec)
    {
        return new Vector32(vec);
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Normal32
{
    private ushort x;
    private ushort y;
    private ushort zneg;

    public Normal32(Vector3 vec)
    {
        x = (ushort)Mathf.Clamp((int)(vec.x * 16384) + 16384, 0, 32767);
        y = (ushort)Mathf.Clamp((int)(vec.y * 16384) + 16384, 0, 32767);
        zneg = (ushort)(vec.z < 0 ? 1 : 0);
    }

    public static implicit operator Vector3(Normal32 normal)
    {
        Vector3 vec = new Vector3
        {
            x = ((int)normal.x - 16384) * (1 / 16384.0f),
            y = ((int)normal.y - 16384) * (1 / 16384.0f),
            z = Mathf.Sqrt(vec.z = -1) * (normal.zneg > 0 ? -1 : 1)
        };

        return vec;
    }

    public static implicit operator Normal32(Vector3 vec)
    {
        return new Normal32(vec);
    }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Quaternion32
{
    private uint x;
    private uint y;
    private uint z;
    private uint wneg;

    public Quaternion32(Quaternion quat)
    {
        x = (uint)Mathf.Clamp((int)(quat.x * 1024) + 1024, 0, 2047);
        y = (uint)Mathf.Clamp((int)(quat.y * 512) + 512, 0, 1023);
        z = (uint)Mathf.Clamp((int)(quat.z * 512) + 512, 0, 1023);
        wneg = (uint)(quat.w < 0 ? 1 : 0);
    }

    public static implicit operator Quaternion(Quaternion32 quat32)
    {
        Quaternion quat = new Quaternion
        {
            x = ((int)quat32.x - 1024) * (1 / 1024.0f),
            y = ((int)quat32.y - 512) * (1 / 512.0f),
            z = ((int)quat32.z - 512) * (1 / 512.0f),
            w = Mathf.Sqrt(quat.w = -1) * (quat32.wneg > 0 ? -1 : 1)
        };

        return quat;
    }

    public static implicit operator Quaternion32(Quaternion quat)
    {
        return new Quaternion32(quat);
    }
}

public class TestCompressedVector
{
    public static void Main()
    {
        Vector3 originalVec = new Vector3(1.0f, 2.0f, 3.0f);
        FVector48 vec48 = originalVec;
        Vector3 decompressedVec = vec48.ToVector3();
        Debug.Log($"Original: {originalVec}, Decompressed: {decompressedVec}");

        Quaternion originalQuat = new Quaternion(1.0f, 0.5f, 0.25f, 0.75f);
        Quaternion48 quat48 = new Quaternion48
        {
            theXInput = (ushort)Float16.ConvertFloatTo16bits(originalQuat.x),
            theYInput = (ushort)Float16.ConvertFloatTo16bits(originalQuat.y),
            theZWInput = (ushort)((Float16.ConvertFloatTo16bits(originalQuat.z) & 0x7FFF) | ((originalQuat.w < 0 ? 1 : 0) << 15))
        };
        Quaternion decompressedQuat = quat48.quaternion;
        Debug.Log($"Original: {originalQuat}, Decompressed: {decompressedQuat}");
    }
}
