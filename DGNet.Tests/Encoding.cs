using System;
using System.Numerics;

using DGNet.Encoding;

namespace DGNet.Tests;

public class Encoding
{
    [Fact]
    public void TestOctahedralByte()
    {
        var value = Vector3.Normalize(new Vector3(12398.0f, -19458.0f, -12.5f));

        var encoded = NormalizedIVec2Byte<Octahedral, Vector3>.Encode(value);
        var decoded = NormalizedIVec2Byte<Octahedral, Vector3>.Decode(encoded);

        var maxError = 4.0f / byte.MaxValue;

        Assert.True(MathF.Abs(value.X - decoded.X) <= maxError);
        Assert.True(MathF.Abs(value.Y - decoded.Y) <= maxError);
        Assert.True(MathF.Abs(value.Z - decoded.Z) <= maxError);
    }

    [Fact]
    public void TestOctahedral()
    {
        var value = Vector3.Normalize(new Vector3(12398.0f, -19458.0f, -12.5f));
        var encoded = Octahedral.Encode(value);
        var decoded = Octahedral.Decode(encoded);

        Assert.True(Vector3.Distance(value, decoded) < 0.01f);
    }
}
