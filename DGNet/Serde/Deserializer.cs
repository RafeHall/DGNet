namespace DGNet.Serde;


public ref struct Deserializer(Span<byte> bytes)
{
    private Span<byte> _bytes = bytes;

    // TODO: implement serialization with granularity at the bit level
    // private byte _bit = 0;

    public delegate T ArrayCallback<T>(ref Deserializer de, int index);

    public T[] DeserializeArray<T>(ArrayCallback<T> callback)
    {
        var count = DeserializeInt32();
        T[] values = new T[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = callback(ref this, i);
        }
        return values;
    }

    public string DeserializeString()
    {
        int length = DeserializeInt32();
        var bytes = _bytes[..length];
        var value = System.Text.Encoding.UTF8.GetString(bytes);
        _bytes = _bytes[length..];
        return value;
    }

    public long DeserializeInt64()
    {
        // NOTE: On little endian the C# runtime jit that converts the IL to ASM will omit 
        // this check and reverse completely since BitConverter.IsLittleEndian is a constant value.
        // Therefore anyone on any remotely common platform for games this will occur no overhead but
        // on the off chance some insane individual is running something big endian we'll be prepared.
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        var value = BitConverter.ToInt64(_bytes);
        _bytes = _bytes[8..];
        return value;
    }

    public ulong DeserializeUInt64()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        var value = BitConverter.ToUInt64(_bytes);
        _bytes = _bytes[8..];
        return value;
    }

    public int DeserializeInt32()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        var value = BitConverter.ToInt32(_bytes);
        _bytes = _bytes[4..];
        return value;
    }

    public uint DeserializeUInt32()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        var value = BitConverter.ToUInt32(_bytes);
        _bytes = _bytes[4..];
        return value;
    }

    public short DeserializeInt16()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..2]);
        }
        var value = BitConverter.ToInt16(_bytes);
        _bytes = _bytes[2..];
        return value;
    }

    public ushort DeserializeUInt16()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..2]);
        }
        var value = BitConverter.ToUInt16(_bytes);
        _bytes = _bytes[2..];
        return value;
    }

    public sbyte DeserializeInt8()
    {
        var value = _bytes[0];
        _bytes = _bytes[1..];
        return (sbyte)value;
    }

    public byte DeserializeUInt8()
    {
        var value = _bytes[0];
        _bytes = _bytes[1..];
        return value;
    }

    public bool DeserializeBool()
    {
        var value = _bytes[0];
        _bytes = _bytes[1..];
        return value == 1;
    }

    public float DeserializeFloat()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        var value = BitConverter.ToSingle(_bytes);
        _bytes = _bytes[4..];
        return value;
    }

    public double DeserializeDouble()
    {
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        var value = BitConverter.ToDouble(_bytes);
        _bytes = _bytes[8..];
        return value;
    }
}