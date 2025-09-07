using System.Numerics;

namespace DGNet.Encoding;

public class Octahedral : IEncoding<Vector3, Vector2>
{
    public static Vector2 Encode(Vector3 value)
    {
        var result = EncodeV(value);
        return new (result.X, result.Y);
    }

    public static Vector3 Decode(Vector2 value)
    {
        return DecodeV(new(value.X, value.Y));
    }

    private static Vector2 EncodeV(Vector3 from)
    {
        Vector3 abs = Vector3.Abs(from);
        float sum = abs.X + abs.Y + abs.Z;
        float recip = 1.0f / sum;
        Vector3 scaled = from * recip;

        Vector2 result = (scaled.Y >= 0.0f)
            ? new Vector2(scaled.X, scaled.Z)
            : new Vector2(
                (1.0f - MathF.Abs(scaled.Z)) * MathF.Sign(from.X),
                (1.0f - MathF.Abs(scaled.X)) * MathF.Sign(from.Z)
            );

        return result * Vector2.Create(0.5f) + Vector2.Create(0.5f);
    }

    private static Vector3 DecodeV(Vector2 from)
    {
        Vector2 mapped = from * Vector2.Create(2.0f) - Vector2.One;

        Vector3 result = new(
            mapped.X,
            1.0f - MathF.Abs(mapped.X) - MathF.Abs(mapped.Y),
            mapped.Y
        );

        if (result.Y < 0.0f)
        {
            result.X += result.Y * MathF.Sign(result.X);
            result.Z += result.Y * MathF.Sign(result.Z);
        }

        return Vector3.Normalize(result);
    }
}