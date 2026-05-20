using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="a"/> represents the zero scalar.
    /// Equivalent to checking if all limbs are zero.
    /// </summary>
    public static bool IsZero(Scalar a) => IsZeroMask(a) == 1UL;

    /// <summary>
    /// Returns <see langword="true"/> if a and b are the same scalar (constant-time comparison).
    /// Compares the raw Montgomery-form limbs using <see cref="CryptographicOperations.FixedTimeEquals"/>.
    /// Two equal field values always have the same Montgomery representation, so this is correct.
    /// </summary>
    public static bool Equal(Scalar a, Scalar b)
    {
        var spanA = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a, 1));
        var spanB = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref b, 1));
        return CryptographicOperations.FixedTimeEquals(spanA, spanB);
    }

    /// <summary>
    /// Compares two scalars by their canonical value in [0, r).
    /// Converts both to canonical form first, then compares MSB-first using <see cref="CommonMath.CmpLimb"/>.
    /// Not constant-time.
    /// </summary>
    /// <returns>Negative if a &lt; b, zero if a == b, positive if a &gt; b.</returns>
    public static int Compare(Scalar a, Scalar b)
    {
        var ca = ToCanonical(a);
        var cb = ToCanonical(b);

        ulong gt = 0, lt = 0;

        CommonMath.CmpLimb(ca.L3, cb.L3, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L2, cb.L2, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L1, cb.L1, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L0, cb.L0, ref gt, ref lt);

        return (int)gt - (int)lt;
    }
    


    /// <inheritdoc cref="Equal"/>
    public override bool Equals(object? obj) => obj is Scalar other && Equal(this, other);

    /// <summary>Returns a hash code based on the raw Montgomery-form limbs.</summary>
    public override int GetHashCode() => HashCode.Combine(L0, L1, L2, L3);
}
