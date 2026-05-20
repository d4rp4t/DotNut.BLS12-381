namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6
{
    private static readonly System.Numerics.BigInteger P = System.Numerics.BigInteger.Parse(
        "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
        System.Globalization.NumberStyles.AllowHexSpecifier
    );
    private static readonly Fp2[] FrobeniusCoeffC1 = BuildFrobeniusCoeff(1);
    private static readonly Fp2[] FrobeniusCoeffC2 = BuildFrobeniusCoeff(2);

    /// <summary>
    /// Returns a + b in Fp6. Performs component-wise Fp2 addition.
    /// Both inputs must be in Montgomery form.
    /// </summary>
    public static Fp6 Add(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Add(a.C0, b.C0),
            Fp2.Add(a.C1, b.C1),
            Fp2.Add(a.C2, b.C2)
            );
    }

    /// <summary>
    /// Returns a − b in Fp6. Performs component-wise Fp2 subtraction.
    /// Both inputs must be in Montgomery form.
    /// </summary>
    public static Fp6 Subtract(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Subtract(a.C0, b.C0),
            Fp2.Subtract(a.C1, b.C1),
            Fp2.Subtract(a.C2, b.C2)
        );
    }

    /// <summary>
    /// Returns −a in Fp6. Performs component-wise Fp2 negation.
    /// Input must be in Montgomery form.
    /// </summary>
    public static Fp6 Negate(Fp6 a)
    {
        return new Fp6(Fp2.Negate(a.C0), Fp2.Negate(a.C1), Fp2.Negate(a.C2));
    }

    /// <summary>
    /// Returns a · v in Fp6, where v is the generator of Fp6 over Fp2 (v³ = ξ = 1 + u).
    /// Implements the shift: (c0 + c1·v + c2·v²) · v = c2·ξ + c0·v + c1·v².
    /// Used in Fp12 arithmetic to multiply by the Fp12 non-residue.
    /// Input must be in Montgomery form.
    /// </summary>
    public static Fp6 MulByNonResidue(Fp6 a)
    {
        // (c0 + c1*v + c2*v^2) * v = (c2*xi) + c0*v + c1*v^2, xi = u+1
        return new Fp6(
            Fp2.MultiplyByNonResidue(a.C2),
            a.C0,
            a.C1
        );
    }

    /// <summary>
    /// Multiplies <paramref name="a"/> by the sparse Fp6 element (0, b1, 0).
    /// Exploits sparsity to save Fp2 multiplications; used by <see cref="Fp12.MulBy014"/>.
    /// Input must be in Montgomery form.
    /// </summary>
    /// <param name="a">Left operand (dense Fp6).</param>
    /// <param name="b1">The non-zero Fp2 coefficient at degree 1.</param>
    internal static Fp6 MulBy1(Fp6 a, Fp2 b1)
    {
        return new Fp6(
            Fp2.MultiplyByNonResidue(Fp2.Multiply(a.C2, b1)),
            Fp2.Multiply(a.C0, b1),
            Fp2.Multiply(a.C1, b1)
        );
    }

    /// <summary>
    /// Multiplies <paramref name="a"/> by the sparse Fp6 element (b0, b1, 0).
    /// Exploits sparsity to save Fp2 multiplications; used by <see cref="Fp12.MulBy014"/>.
    /// Input must be in Montgomery form.
    /// </summary>
    /// <param name="a">Left operand (dense Fp6).</param>
    /// <param name="b0">Non-zero Fp2 coefficient at degree 0.</param>
    /// <param name="b1">Non-zero Fp2 coefficient at degree 1.</param>
    internal static Fp6 MulBy01(Fp6 a, Fp2 b0, Fp2 b1)
    {
        var aa = Fp2.Multiply(a.C0, b0);
        var bb = Fp2.Multiply(a.C1, b1);
        var t1 = Fp2.Add(Fp2.MultiplyByNonResidue(Fp2.Multiply(a.C2, b1)), aa);
        var t2 = Fp2.Subtract(Fp2.Subtract(Fp2.Multiply(Fp2.Add(b0, b1), Fp2.Add(a.C0, a.C1)), aa), bb);
        var t3 = Fp2.Add(Fp2.Multiply(a.C2, b0), bb);
        return new Fp6(t1, t2, t3);
    }

    /// <summary>
    /// Returns a · b in Fp6 using Karatsuba-style multiplication with 6 Fp2 multiplications.
    /// Both inputs must be in Montgomery form.
    /// </summary>
    public static Fp6 Multiply(Fp6 a, Fp6 b)
    {
        Fp2 t0 = Fp2.Multiply(a.C0, b.C0);
        Fp2 t1 = Fp2.Multiply(a.C1, b.C1);
        Fp2 t2 = Fp2.Multiply(a.C2, b.C2);

        Fp2 c0 = Fp2.Add(t0, Fp2.MultiplyByNonResidue(Fp2.Subtract(Fp2.Subtract(Fp2.Multiply(Fp2.Add(a.C1, a.C2), Fp2.Add(b.C1, b.C2)), t1), t2)));
        Fp2 c1 = Fp2.Add(Fp2.Subtract(Fp2.Subtract(Fp2.Multiply(Fp2.Add(a.C0, a.C1), Fp2.Add(b.C0, b.C1)), t0), t1), Fp2.MultiplyByNonResidue(t2));
        Fp2 c2 = Fp2.Add(t1, Fp2.Subtract(Fp2.Subtract(Fp2.Multiply(Fp2.Add(a.C0, a.C2), Fp2.Add(b.C0, b.C2)), t0), t2));

        return new Fp6(c0, c1, c2);
    }

    /// <summary>
    /// Returns a² in Fp6. Delegates to <see cref="Multiply"/>(a, a).
    /// Input must be in Montgomery form.
    /// </summary>
    public static Fp6 Square(Fp6 a) => Multiply(a, a);

    /// <summary>
    /// Returns a⁻¹ in Fp6 using the standard formula for Fp6 = Fp2[v]/(v³ − ξ).
    /// Reduces to a single Fp2 inversion.
    /// </summary>
    /// <remarks>Behaviour for the zero element is determined by <see cref="Fp2.Invert"/>.</remarks>
    public static Fp6 Invert(Fp6 a)
    {
        // Standard inversion for Fp6 over Fp2 with v^3 = xi (xi = u+1)
        Fp2 c0 = Fp2.Subtract(Fp2.Square(a.C0), Fp2.MultiplyByNonResidue(Fp2.Multiply(a.C1, a.C2)));
        Fp2 c1 = Fp2.Subtract(Fp2.MultiplyByNonResidue(Fp2.Square(a.C2)), Fp2.Multiply(a.C0, a.C1));
        Fp2 c2 = Fp2.Subtract(Fp2.Square(a.C1), Fp2.Multiply(a.C0, a.C2));

        Fp2 t = Fp2.Add(Fp2.Multiply(a.C0, c0), Fp2.MultiplyByNonResidue(Fp2.Add(Fp2.Multiply(a.C2, c1), Fp2.Multiply(a.C1, c2))));
        Fp2 inv = Fp2.Invert(t);

        return new Fp6(
            Fp2.Multiply(c0, inv),
            Fp2.Multiply(c1, inv),
            Fp2.Multiply(c2, inv)
        );
    }

    /// <summary>
    /// Computes <paramref name="value"/>^<paramref name="exponent"/> in Fp6 using square-and-multiply (LSB-first).
    /// </summary>
    /// <param name="value">Base element in Montgomery form.</param>
    /// <param name="exponent">Non-negative exponent; negative values throw <see cref="ArgumentOutOfRangeException"/>.</param>
    /// <returns>value^exponent in Montgomery form.</returns>
    public static Fp6 Pow(Fp6 value, System.Numerics.BigInteger exponent)
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
    /// Applies the p^<paramref name="power"/>-power Frobenius endomorphism to <paramref name="a"/>.
    /// Uses precomputed Frobenius coefficients for C1 and C2; C0 is handled by <see cref="Fp2.FrobeniusMap"/>.
    /// </summary>
    /// <param name="a">Input element in Montgomery form.</param>
    /// <param name="power">The Frobenius power; reduced modulo 6 internally.</param>
    /// <returns>Frobenius(a, power) in Montgomery form.</returns>
    public static Fp6 FrobeniusMap(Fp6 a, int power)
    {
        var idx = ((power % 6) + 6) % 6;
        return new Fp6(
            Fp2.FrobeniusMap(a.C0, power),
            Fp2.Multiply(Fp2.FrobeniusMap(a.C1, power), FrobeniusCoeffC1[idx]),
            Fp2.Multiply(Fp2.FrobeniusMap(a.C2, power), FrobeniusCoeffC2[idx])
        );
    }

    /// <summary>
    /// Computes the Frobenius coefficients for component C<paramref name="factor"/> at powers 0..5.
    /// arr[i] = ξ^(factor·(p^i − 1)/3) where ξ = Fp2.NonResidue.
    /// Called once at static initialization; result stored in <see cref="FrobeniusCoeffC1"/> or <see cref="FrobeniusCoeffC2"/>.
    /// </summary>
    private static Fp2[] BuildFrobeniusCoeff(int factor)
    {
        var arr = new Fp2[6];
        arr[0] = Fp2.One;
        for (var i = 1; i < 6; i++)
        {
            var pPow = System.Numerics.BigInteger.Pow(P, i);
            var e = (factor * (pPow - 1)) / 3;
            arr[i] = Fp2.Pow(Fp2.NonResidue, e);
        }
        return arr;
    }
}
