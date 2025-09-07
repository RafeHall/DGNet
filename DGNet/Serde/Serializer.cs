namespace DGNet.Serde;

public ref struct Serializer(Span<byte> buffer)
{
    private Span<byte> _bytes = buffer;
    private readonly Span<byte> _buffer = buffer;

    // TODO: implement deserialization with granularity at the bit level
    // private byte _bit = 0;

    // public static void Example()
    // {
    //     Span<byte> buffer = stackalloc byte[MAX_SIZE];
    //     Serializer serializer = new(buffer);
    //     int written = serializer.Finish();
    // }

    public delegate void ArrayCallback<T>(Serializer se, int index, T value);

    public void SerializeArray<T>(ReadOnlySpan<T> values, ArrayCallback<T> callback)
    {
        SerializeInt32(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            callback(this, i, values[i]);
        }
    }

    public void SerializeString(string value)
    {
        var span = value.AsSpan();
        int length = System.Text.Encoding.UTF8.GetByteCount(span);
        SerializeInt32(length);

        if (!System.Text.Encoding.UTF8.TryGetBytes(span, _bytes, out _))
        {
            throw new OutOfMemoryException();
        }
        _bytes = _bytes[length..];
    }

    public void SerializeInt64(long value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        // NOTE: On little endian the C# runtime jit that converts the IL to ASM will omit 
        // this check and reverse completely since BitConverter.IsLittleEndian is a constant value.
        // Therefore anyone on any remotely common platform for games this will occur no overhead but
        // on the off chance some insane individual is running something big endian we'll be prepared.
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        _bytes = _bytes[8..];
    }

    public void SerializeUInt64(ulong value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        _bytes = _bytes[8..];
    }

    public void SerializeInt32(int value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        _bytes = _bytes[4..];
    }

    public void SerializeUInt32(uint value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        _bytes = _bytes[4..];
    }

    public void SerializeInt16(short value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..2]);
        }
        _bytes = _bytes[2..];
    }

    public void SerializeUInt16(ushort value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..2]);
        }
        _bytes = _bytes[2..];
    }

    public void SerializeInt8(sbyte value)
    {
        if (_bytes.Length <= 0)
        {
            throw new OutOfMemoryException();
        }
        _bytes[0] = (byte)value;
        _bytes = _bytes[1..];
    }

    public void SerializeUInt8(byte value)
    {
        if (_bytes.Length <= 0)
        {
            throw new OutOfMemoryException();
        }
        _bytes[0] = value;
        _bytes = _bytes[1..];
    }

    public void SerializeBool(bool value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        _bytes = _bytes[1..];
    }

    public void SerializeFloat(float value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..4]);
        }
        _bytes = _bytes[4..];
    }

    public void SerializeDouble(double value)
    {
        if (!BitConverter.TryWriteBytes(_bytes, value))
        {
            throw new OutOfMemoryException();
        }
        if (!BitConverter.IsLittleEndian)
        {
            MemoryExtensions.Reverse(_bytes[..8]);
        }
        _bytes = _bytes[8..];
    }

    internal readonly int Finish()
    {
        unsafe
        {
            fixed (byte* bytesPtr = &_bytes[0])
            fixed (byte* bufferPtr = &_buffer[0])
            {
                return (int)(bytesPtr - bufferPtr);
            }
        }
    }
}