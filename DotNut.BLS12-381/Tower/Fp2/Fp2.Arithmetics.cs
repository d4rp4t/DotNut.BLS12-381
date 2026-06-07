using System.Runtime.CompilerServices;

namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    /// <summary>
    /// Returns a + b in Fp2. Delegates to component-wise <see cref="Fp.Add"/>.
    /// Both inputs must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp2 Add(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Add(a.C0, b.C0),
            Fp.Add(a.C1, b.C1)
        );
    }

    /// <summary>
    /// Returns a − b in Fp2. Delegates to component-wise <see cref="Fp.Subtract"/>.
    /// Both inputs must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp2 Subtract(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Subtract(a.C0, b.C0),
            Fp.Subtract(a.C1, b.C1)
        );
    }

    /// <summary>
    /// Returns −a in Fp2. Delegates to component-wise <see cref="Fp.Negate"/>.
    /// Input must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp2 Negate(Fp2 a)
    {
        return new Fp2(Fp.Negate(a.C0), Fp.Negate(a.C1));
    }

    /// <summary>
    /// Returns a · b in Fp2 using the identity (a0 + a1·u)(b0 + b1·u) = (a0·b0 − a1·b1) + (a0·b1 + a1·b0)·u,
    /// where u² = −1. Uses 4 Fp multiplications.
    /// Both inputs must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Fp2 Multiply(Fp2 a, Fp2 b)
    {
        // (a0 + a1*u)(b0 + b1*u), u^2 = -1
        Fp t0 = Fp.Multiply(a.C0, b.C0);
        Fp t1 = Fp.Multiply(a.C1, b.C1);
        Fp c0 = Fp.Subtract(t0, t1);
        Fp c1 = Fp.Add(Fp.Multiply(a.C0, b.C1), Fp.Multiply(a.C1, b.C0));
        return new Fp2(c0, c1);
    }

    /// <summary>
    /// Returns a² in Fp2 using the identity (a0 + a1·u)² = (a0² − a1²) + 2·a0·a1·u.
    /// Uses 2 Fp squarings and 1 Fp multiplication, fewer than calling <see cref="Multiply"/>(a, a).
    /// Input must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Fp2 Square(Fp2 a)
    {
        // (a0 + a1*u)^2 = (a0^2 - a1^2) + 2*a0*a1*u
        Fp a0a0 = Fp.Square(a.C0);
        Fp a1a1 = Fp.Square(a.C1);
        Fp c0 = Fp.Subtract(a0a0, a1a1);
        Fp a0a1 = Fp.Multiply(a.C0, a.C1);
        Fp c1 = Fp.Add(a0a1, a0a1);
        return new Fp2(c0, c1);
    }

    /// <summary>
    /// Returns a⁻¹ in Fp2 using (a0 + a1·u)⁻¹ = (a0 − a1·u) / (a0² + a1²).
    /// The denominator a0² + a1² is an element of Fp; its inverse is computed via <see cref="Fp.Invert"/>.
    /// </summary>
    /// <remarks>Behaviour for the zero element is determined by <see cref="Fp.Invert"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Fp2 Invert(Fp2 a)
    {
        // (a0 + a1*u)^-1 = (a0 - a1*u)/(a0^2 + a1^2)
        Fp denom = Fp.Add(Fp.Square(a.C0), Fp.Square(a.C1));
        Fp inv = Fp.Invert(denom);
        return new Fp2(
            Fp.Multiply(a.C0, inv),
            Fp.Negate(Fp.Multiply(a.C1, inv))
        );
    }

    /// <summary>
    /// Returns a · ξ where ξ = 1 + u is the Fp6 cubic non-residue.
    /// Expands to (a0 − a1) + (a0 + a1)·u. Used extensively in Fp6 arithmetic.
    /// Input must be in Montgomery form with each Fp component in [0, p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp2 MultiplyByNonResidue(Fp2 a)
    {
        // (a0 + a1*u) * (1 + u) = (a0 - a1) + (a0 + a1)u
        return new Fp2(
            Fp.Subtract(a.C0, a.C1),
            Fp.Add(a.C0, a.C1)
        );
    }

    /// <summary>
    /// Applies the p-power Frobenius endomorphism φ^<paramref name="power"/> to <paramref name="a"/>.
    /// For BLS12-381, u^p ≡ −u mod p, so odd powers conjugate the element: C1 is negated.
    /// Even powers are the identity.
    /// </summary>
    /// <param name="a">Input element in Montgomery form.</param>
    /// <param name="power">The Frobenius power to apply.</param>
    /// <returns>φ^power(a); the result is in Montgomery form.</returns>
    public static Fp2 FrobeniusMap(Fp2 a, int power)
    {
        // For BLS12-381 base field p, u^p = -u
        return (power & 1) == 0 ? a : new Fp2(a.C0, Fp.Negate(a.C1));
    }

    /// <summary>
    /// Computes <paramref name="value"/>^<paramref name="exponent"/> in Fp2 using square-and-multiply (LSB-first).
    /// </summary>
    /// <param name="value">Base element in Montgomery form.</param>
    /// <param name="exponent">Non-negative exponent; negative values throw <see cref="ArgumentOutOfRangeException"/>.</param>
    /// <returns>value^exponent in Montgomery form.</returns>
    public static Fp2 Pow(Fp2 value, System.Numerics.BigInteger exponent)
    {
        if (exponent.Sign < 0) throw new ArgumentOutOfRangeException(nameof(exponent));
        var result = One;
        var baseValue = value;
        var e = exponent;
        while (e > 0)
        {
            if (!e.IsEven)
                result = Multiply(result, baseValue);
            baseValue = Square(baseValue);
            e >>= 1;
        }
        return result;
    }

    /// <summary>
    /// Constant-time conditional selection. Returns <paramref name="whenOne"/> when
    /// <paramref name="bit"/> is 1, <paramref name="whenZero"/> when 0.
    /// Delegates component-wise to <see cref="Fp.ConditionalSelect(ulong,DotNut.BLS12_381.Tower.Fp,DotNut.BLS12_381.Tower.Fp)"/>.
    /// </summary>
    internal static Fp2 ConditionalSelect(ulong bit, Fp2 whenOne, Fp2 whenZero)
        => new(Fp.ConditionalSelect(bit, whenOne.C0, whenZero.C0),
               Fp.ConditionalSelect(bit, whenOne.C1, whenZero.C1));

    /// <summary>
    /// Returns 1 if both components are zero, 0 otherwise.
    /// Operates on raw Montgomery limbs without canonicalizing.
    /// </summary>
    internal static ulong IsZeroMask(Fp2 value)
        => Fp.IsZeroMask(value.C0) & Fp.IsZeroMask(value.C1);
    

    /// <summary>
    /// Sign function for Fp2 as defined in RFC 9380 §4.1:
    /// <c>sgn0(a + b·u) = sgn0(a)</c> if a ≠ 0, else <c>sgn0(b)</c>.
    /// Returns 1 if the element is "negative", 0 if "non-negative".
    /// </summary>
    internal static ulong Sgn0(Fp2 value)
    {
        var sign0  = Fp.Sgn0(value.C0);
        var zero0  = Fp.IsZeroMask(value.C0);
        var sign1  = Fp.Sgn0(value.C1);
        return sign0 | (zero0 & sign1);
    }
}
