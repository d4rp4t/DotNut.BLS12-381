namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    /// <summary>
    /// Determines whether the element is lexicographically larger than (p-1)/2.
    /// </summary>
    /// <remarks>
    /// This is used in elliptic curve point compression to encode the sign bit
    /// of the x-coordinate.
    ///
    /// The check is performed on the canonical representation of the field element.
    /// </remarks>
    /// <param name="value">Field element in Montgomery form.</param>
    /// <returns>
    /// True if value &gt; (p-1)/2, otherwise false.
    /// </returns>
    public static bool LexicographicallyLargest(Fp value)
    {
        // Subtract (p+1)/2 from the canonical value. If no underflow (borrow == 0),
        // value >= (p+1)/2, meaning value > (p-1)/2.
        var canonical = ToCanonical(value);
        SubtractUnchecked(canonical, new Fp(
            0xdcff7fffffffd556UL,
            0x0f55ffff58a9ffffUL,
            0xb39869507b587b12UL,
            0xb23ba5c279c2895fUL,
            0x258dd3db21a5d66bUL,
            0x0d0088f51cbff34dUL
        ), out ulong borrow);
        return borrow == 0;
    }

    /// <summary>
    /// Computes a square root of the field element if it exists.
    /// </summary>
    /// <remarks>
    /// Implements the exponentiation method for fields where p ≡ 3 (mod 4):
    /// sqrt(a) = a^((p+1)/4).
    ///
    /// The result is not guaranteed to be a canonical representative
    /// (both r and -r are valid square roots).
    ///
    /// Verification is performed by checking:
    /// Square(sqrt) == value
    /// </remarks>
    /// <param name="value">Field element in Montgomery form.</param>
    /// <param name="sqrt">
    /// If the function returns true, contains a valid square root of <paramref name="value"/>.
    /// Otherwise contains an undefined value.
    /// </param>
    /// <returns>
    /// True if the input is a quadratic residue in Fp, otherwise false.
    /// </returns>
    public static bool TrySqrt(Fp value, out Fp sqrt)
    {
        sqrt = PowVartime(value, [
            0xee7fbfffffffeaabUL,  // L0 — (p+1)/4 in LE limbs
            0x07aaffffac54ffffUL,
            0xd9cc34a83dac3d89UL,
            0xd91dd2e13ce144afUL,
            0x92c6e9ed90d2eb35UL,
            0x0680447a8e5ff9a6UL   // L5 (most significant)
        ]);
        return Equal(Square(sqrt), value);
    }

    /// <summary>
    /// Computes exponentiation using square-and-multiply in Montgomery form.
    /// </summary>
    /// <remarks>
    /// The exponent is provided in little-endian 64-bit limbs
    /// (expLE[0] is the least significant limb).
    ///
    /// This implementation is variable-time with respect to the exponent
    /// only and is safe when the exponent is public.
    ///
    /// WARNING: This is NOT constant-time with respect to the base value.
    /// </remarks>
    /// <param name="value">Base element in Montgomery form.</param>
    /// <param name="expLE">
    /// Exponent represented as 6 little-endian 64-bit limbs.
    /// </param>
    /// <returns>value raised to the given exponent in Fp.</returns>
    internal static Fp PowVartime(Fp value, ReadOnlySpan<ulong> expLE)
    {
        var result = One;
        for (var limb = expLE.Length - 1; limb >= 0; limb--)
        {
            var e = expLE[limb];
            for (var bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                var multiplied = Multiply(result, value);
                result = ConditionalSelect((e >> bit) & 1UL, multiplied, result);
            }
        }
        return result;
    }
}
