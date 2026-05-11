namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    // Returns true if this element is strictly lexicographically larger than its negation,
    // i.e., value > (p-1)/2. Used for compressed point serialization.
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

    // sqrt via Shank's method: p ≡ 3 (mod 4) so sqrt(a) = a^((p+1)/4).
    // Returns true if value is a quadratic residue; sqrt is undefined otherwise.
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

    // Square-and-multiply with fixed exponent given as 6 LE limbs (expLE[0] = LSB, expLE[5] = MSB).
    // Variable-time w.r.t. the exponent only — safe for public, non-secret exponents.
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
                result = Select((e >> bit) & 1UL, multiplied, result);
            }
        }
        return result;
    }
}
