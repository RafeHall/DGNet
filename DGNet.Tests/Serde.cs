

using DGNet.Serde;

public class SerdeTests
{
    [Fact]
    public void Serialize()
    {
        Span<byte> buffer = stackalloc byte[1024];
        Serializer serializer = new(buffer);
        Assert.Equal(0, serializer.BytesWritten());

        // 4 * 4 + 4 = 20
        serializer.SerializeArray(
            [.. Enumerable.Range(0, 4)],
            (ref se, _, value) => se.SerializeInt32(value)
        );
        Assert.Equal(20, serializer.BytesWritten());

        // 4 + 13 + 16 = 37
        serializer.SerializeString("Hello, World!");
        Assert.Equal(37, serializer.BytesWritten());

        // 8 * 2 + 33 = 53
        serializer.SerializeInt64(long.MaxValue / 2);
        serializer.SerializeUInt64(ulong.MaxValue / 2);
        Assert.Equal(53, serializer.BytesWritten());

        // 4 * 2 + 49 = 61
        serializer.SerializeInt32(int.MaxValue / 2);
        serializer.SerializeUInt32(uint.MaxValue / 2);
        Assert.Equal(61, serializer.BytesWritten());

        // 2 * 2 + 57 = 65
        serializer.SerializeInt16(short.MaxValue / 2);
        serializer.SerializeUInt16(ushort.MaxValue / 2);
        Assert.Equal(65, serializer.BytesWritten());

        // 2 + 61 = 67
        serializer.SerializeInt8(sbyte.MaxValue / 2);
        serializer.SerializeUInt8(byte.MaxValue / 2);
        Assert.Equal(67, serializer.BytesWritten());

        // 1 + 63 = 68
        serializer.SerializeBool(true);
        Assert.Equal(68, serializer.BytesWritten());

        // 12 + 64 = 80
        serializer.SerializeFloat(0.1f);
        serializer.SerializeDouble(0.0001);
        Assert.Equal(80, serializer.BytesWritten());
    }

    [Fact]
    public void Deserialize()
    {
        byte[] bytes = [
            4, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0,
            13, 0, 0, 0, 72, 101, 108, 108, 111, 44, 32, 87, 111, 114,
            108, 100, 33, 255, 255, 255, 255, 255, 255, 255, 63, 255,
            255, 255, 255, 255, 255, 255, 127, 255, 255, 255, 63, 255,
            255, 255, 127, 255, 63, 255, 127, 63, 127, 1, 205, 204, 204,
            61, 0, 0, 0, 224, 226, 54, 26, 63
        ];

        Deserializer deserializer = new(bytes);
        var ints = deserializer.DeserializeArray((ref de, _) => de.DeserializeInt32());
        Assert.Equal<int[]>([0, 1, 2, 3], ints);

        var str = deserializer.DeserializeString();
        Assert.Equal("Hello, World!", str);

        var int64 = deserializer.DeserializeInt64();
        var uInt64 = deserializer.DeserializeUInt64();
        Assert.Equal(long.MaxValue / 2, int64);
        Assert.Equal(ulong.MaxValue / 2, uInt64);

        var int32 = deserializer.DeserializeInt32();
        var uInt32 = deserializer.DeserializeUInt32();
        Assert.Equal(int.MaxValue / 2, int32);
        Assert.Equal(uint.MaxValue / 2, uInt32);

        var int16 = deserializer.DeserializeInt16();
        var uInt16 = deserializer.DeserializeUInt16();
        Assert.Equal(short.MaxValue / 2, int16);
        Assert.Equal(ushort.MaxValue / 2, uInt16);

        var int8 = deserializer.DeserializeInt8();
        var uInt8 = deserializer.DeserializeUInt8();
        Assert.Equal(sbyte.MaxValue / 2, int8);
        Assert.Equal(byte.MaxValue / 2, uInt8);

        var b = deserializer.DeserializeBool();
        Assert.True(b);

        var f = deserializer.DeserializeFloat();
        Assert.True(Math.Abs(f - 0.1f) < 0.01f);


        var d = deserializer.DeserializeDouble();
        Assert.True(Math.Abs(d - 0.0001) < 0.001);
    }
}