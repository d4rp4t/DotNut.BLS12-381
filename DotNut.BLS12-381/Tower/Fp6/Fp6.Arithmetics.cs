namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6
{
    public static Fp6 Add(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Add(a.C0, b.C0),
            Fp2.Add(a.C1, b.C1),
            Fp2.Add(a.C2, b.C2)
            );
    }

    public static Fp6 Subtract(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Sub(a.C0, b.C0),
            Fp2.Sub(a.C1, b.C1),
            Fp2.Sub(a.C2, b.C2)
        );
    }

    public static Fp6 Negate(Fp6 a)
    {
        return new Fp6(Fp2.Negate(a.C0), Fp2.Negate(a.C1), Fp2.Negate(a.C2));
    }

    public static Fp6 MulByNonResidue(Fp6 a)
    {
        // (c0 + c1*v + c2*v^2) * v = (c2*xi) + c0*v + c1*v^2, xi = u+1
        return new Fp6(
            Fp2.MultiplyByNonResidue(a.C2),
            a.C0,
            a.C1
        );
    }

    public static Fp6 Multiply(Fp6 a, Fp6 b)
    {
        Fp2 t0 = Fp2.Multiply(a.C0, b.C0);
        Fp2 t1 = Fp2.Multiply(a.C1, b.C1);
        Fp2 t2 = Fp2.Multiply(a.C2, b.C2);

        Fp2 c0 = Fp2.Add(t0, Fp2.MultiplyByNonResidue(Fp2.Sub(Fp2.Sub(Fp2.Multiply(Fp2.Add(a.C1, a.C2), Fp2.Add(b.C1, b.C2)), t1), t2)));
        Fp2 c1 = Fp2.Add(Fp2.Sub(Fp2.Sub(Fp2.Multiply(Fp2.Add(a.C0, a.C1), Fp2.Add(b.C0, b.C1)), t0), t1), Fp2.MultiplyByNonResidue(t2));
        Fp2 c2 = Fp2.Add(t1, Fp2.Sub(Fp2.Sub(Fp2.Multiply(Fp2.Add(a.C0, a.C2), Fp2.Add(b.C0, b.C2)), t0), t2));

        return new Fp6(c0, c1, c2);
    }

    public static Fp6 Square(Fp6 a) => Multiply(a, a);

    public static Fp6 Invert(Fp6 a)
    {
        // Standard inversion for Fp6 over Fp2 with v^3 = xi (xi = u+1)
        Fp2 c0 = Fp2.Sub(Fp2.Square(a.C0), Fp2.MultiplyByNonResidue(Fp2.Multiply(a.C1, a.C2)));
        Fp2 c1 = Fp2.Sub(Fp2.MultiplyByNonResidue(Fp2.Square(a.C2)), Fp2.Multiply(a.C0, a.C1));
        Fp2 c2 = Fp2.Sub(Fp2.Square(a.C1), Fp2.Multiply(a.C0, a.C2));

        Fp2 t = Fp2.Add(Fp2.Multiply(a.C0, c0), Fp2.MultiplyByNonResidue(Fp2.Add(Fp2.Multiply(a.C2, c1), Fp2.Multiply(a.C1, c2))));
        Fp2 inv = Fp2.Invert(t);

        return new Fp6(
            Fp2.Multiply(c0, inv),
            Fp2.Multiply(c1, inv),
            Fp2.Multiply(c2, inv)
        );
    }

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

    public static Fp6 FrobeniusMap(Fp6 a, int power)
    {
        // Functional (not optimized): x -> x^(p^power)
        var p = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var exp = System.Numerics.BigInteger.One;
        for (var i = 0; i < power; i++) exp *= p;
        return Pow(a, exp);
    }
}
