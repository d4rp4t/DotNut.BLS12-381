using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

/// <summary>
/// Wraps the raw Fp12 output of a Miller loop before the final exponentiation.
/// The value is not yet in GT and is not a meaningful pairing output until
/// <see cref="FinalExponentiation"/> is called.
/// </summary>
public readonly struct MillerLoopResult
{
    internal readonly Fp12 Value;

    internal MillerLoopResult(Fp12 value) => Value = value;

    /// <summary>
    /// The neutral element for combining Miller loop results via <see cref="Add"/>.
    /// Corresponds to f = 1 (the Fp12 multiplicative identity).
    /// </summary>
    public static readonly MillerLoopResult Default = new(Fp12.One);

    /// <summary>
    /// Applies the final exponentiation f^((p^12 − 1)/r) to project this value into the GT group.
    /// Must be called after all desired Miller loop accumulations are complete.
    /// </summary>
    /// <returns>The pairing value as a <see cref="Gt"/> element.</returns>
    public Gt FinalExponentiation() => new(Fp12.FinalExponentiation(Value));

    /// <summary>
    /// Combines two Miller loop results by multiplying their underlying Fp12 values.
    /// Used to accumulate multiple pairings before applying <see cref="FinalExponentiation"/> once.
    /// Equivalent to e(P1, Q1) · e(P2, Q2) in GT when followed by final exponentiation.
    /// </summary>
    public static MillerLoopResult Add(MillerLoopResult a, MillerLoopResult b)
        => new(Fp12.Multiply(a.Value, b.Value));
}
