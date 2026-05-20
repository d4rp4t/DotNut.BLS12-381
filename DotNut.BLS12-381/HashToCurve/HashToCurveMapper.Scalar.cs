namespace DotNut.BLS12_381.HashToCurve;

internal static partial class HashToCurveMapper
{
    internal static Scalar ScalarFromOkm(ReadOnlySpan<byte> okm)
    {
        if (okm.Length != 48)
        {
            throw new ArgumentException("OKM must be 48 bytes.", nameof(okm));
        }

        Span<byte> bs = stackalloc byte[64];
        bs.Clear();
        okm.CopyTo(bs[16..]);
        bs.Reverse();
        return Scalar.FromBytesWide(bs);
    }
}