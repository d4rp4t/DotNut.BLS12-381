using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

public readonly struct MillerLoopResult
{
    internal readonly Fp12 Value;

    internal MillerLoopResult(Fp12 value) => Value = value;

    public static readonly MillerLoopResult Default = new(Fp12.One);

    public Gt FinalExponentiation() => new(Fp12.FinalExponentiation(Value));

    public static MillerLoopResult Add(MillerLoopResult a, MillerLoopResult b)
        => new(Fp12.Multiply(a.Value, b.Value));
}
