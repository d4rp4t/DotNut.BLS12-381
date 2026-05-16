namespace DotNut.BLS12_381.HashToCurve;

internal sealed class Shake
{
    private readonly ShakeDigest _digest;

    private Shake(int bits) => _digest = new ShakeDigest(bits);

    internal static Shake Create(XofAlgorithm algorithm) =>
        new(algorithm == XofAlgorithm.Shake128 ? 128 : 256);

    internal void AppendData(ReadOnlySpan<byte> data) => _digest.BlockUpdate(data);

    internal void GetHashAndReset(Span<byte> output) => _digest.OutputFinal(output);
}
