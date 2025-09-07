namespace DGNet.Encoding;


public interface IEncoding<TFrom, TTo>
{
    public static abstract TTo Encode(TFrom value);
    public static abstract TFrom Decode(TTo value);
}