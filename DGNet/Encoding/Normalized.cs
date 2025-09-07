using System.Numerics;

namespace DGNet.Encoding;

public class NormalizedByte<TBase, TFrom> : IEncoding<TFrom, byte>
where TBase : IEncoding<TFrom, float>
{
    public static byte Encode(TFrom value)
    {
        float to = TBase.Encode(value);
        return (byte)(to * byte.MaxValue);
    }

    public static TFrom Decode(byte value)
    {
        float from = value / (float)byte.MaxValue;
        return TBase.Decode(from);
    }
}

public class NormalizedIVec2Byte<TBase, TFrom> : IEncoding<TFrom, (byte, byte)>
where TBase : IEncoding<TFrom, Vector2>
{
    public static (byte, byte) Encode(TFrom value)
    {
        Vector2 to = TBase.Encode(value);
        return ((byte)(to.X * byte.MaxValue), (byte)(to.Y * byte.MaxValue));
    }

    public static TFrom Decode((byte, byte) value)
    {
        Vector2 from = new(value.Item1 / (float)byte.MaxValue, value.Item2 / (float)byte.MaxValue);
        return TBase.Decode(from);
    }
}