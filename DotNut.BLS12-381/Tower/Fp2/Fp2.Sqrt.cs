namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    /// <summary>
    /// Returns <see langword="true"/> if this element is lexicographically larger than its negation.
    /// C1 dominates; C0 is used only when C1 is zero.
    /// Used by serialization to select the canonical sign of a square root.
    /// </summary>
    public static bool LexicographicallyLargest(Fp2 value)
    {
        var c1Largest = Fp.LexicographicallyLargest(value.C1);
        var c1IsZero = Fp.Equal(value.C1, Fp.Zero);
        return c1Largest | (c1IsZero & Fp.LexicographicallyLargest(value.C0));
    }

    /// <summary>
    /// Attempts to compute a square root of <paramref name="value"/> in Fp2.
    /// Implements Algorithm 9 from https://eprint.iacr.org/2012/685.pdf.
    /// </summary>
    /// <param name="value">The element to take the square root of. Must be in Montgomery form.</param>
    /// <param name="sqrt">
    /// Set to a square root of <paramref name="value"/> when this method returns <see langword="true"/>.
    /// The returned root is not necessarily the lexicographically-largest; call <see cref="LexicographicallyLargest"/>
    /// on the result if the canonical (positive) root is needed. Set to the zero element on failure.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="value"/> is a quadratic residue in Fp2.</returns>
    public static bool TrySqrt(Fp2 value, out Fp2 sqrt)
    {
        // a1 = value^((p-3)/4)
        var a1 = PowVartime(value, [
            0xee7fbfffffffeaaaUL,
            0x07aaffffac54ffffUL,
            0xd9cc34a83dac3d89UL,
            0xd91dd2e13ce144afUL,
            0x92c6e9ed90d2eb35UL,
            0x0680447a8e5ff9a6UL
        ]);
        var alpha = Multiply(Square(a1), value);
        var x0 = Multiply(a1, value);

        // alpha == -1 case: sqrt = x0.c1 - x0.c0*u
        if (Equal(Add(alpha, One), Zero))
        {
            sqrt = new Fp2(x0.C1, Fp.Negate(x0.C0));
            return true;
        }

        // General case: sqrt = (1 + alpha)^((p-1)/2) * x0
        var b = PowVartime(Add(One, alpha), [
            0xdcff7fffffffd555UL,
            0x0f55ffff58a9ffffUL,
            0xb39869507b587b12UL,
            0xb23ba5c279c2895fUL,
            0x258dd3db21a5d66bUL,
            0x0d0088f51cbff34dUL
        ]);
        sqrt = Multiply(b, x0);
        return Equal(Square(sqrt), value);
    }

    /// <summary>
    /// Computes <paramref name="value"/>^e in Fp2 where the exponent e is given as 6 little-endian 64-bit limbs
    /// (expLE[0] is the least-significant limb). Processes bits MSB-first.
    /// </summary>
    /// <remarks>
    /// Variable-time with respect to the exponent — safe only for public, non-secret exponents
    /// such as the hard-coded constants used in <see cref="TrySqrt"/>.
    /// </remarks>
    /// <param name="value">Base element in Montgomery form.</param>
    /// <param name="expLE">Exponent as 6 little-endian 64-bit limbs.</param>
    /// <returns>value^e in Montgomery form.</returns>
    internal static Fp2 PowVartime(Fp2 value, ReadOnlySpan<ulong> expLE)
    {
        var result = One;
        for (var limb = expLE.Length - 1; limb >= 0; limb--)
        {
            var e = expLE[limb];
            for (var bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                if (((e >> bit) & 1) == 1)
                    result = Multiply(result, value);
            }
        }
        return result;
    }
}
