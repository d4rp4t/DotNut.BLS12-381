namespace DotNut.BLS12_381.HashToCurve;

internal sealed class ShakeDigest : KeccakDigest
{
    internal ShakeDigest(int bitLength) : base(bitLength switch
    {
        128 or 256 => bitLength,
        _ => throw new ArgumentException($"{bitLength} not supported for SHAKE", nameof(bitLength)),
    })
    {
    }

    internal void Output(Span<byte> output)
    {
        if (!squeezing)
            AbsorbBits(0x0F, 4);
        Squeeze(output);
    }

    internal void OutputFinal(Span<byte> output)
    {
        Output(output);
        Reset();
    }
}
