namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    public static Fp12 Add(Fp12 a, Fp12 b)
    {
        return new Fp12(
            Fp6.Add(a.C0, b.C0),
            Fp6.Add(a.C1, b.C1)
        );
    }

    public static Fp12 Subtract(Fp12 a, Fp12 b)
    {
        return new Fp12(
            Fp6.Subtract(a.C0, b.C0),
            Fp6.Subtract(a.C1, b.C1)
        );
    }

    public static Fp12 Negate(Fp12 a)
    {
        return new Fp12(Fp6.Negate(a.C0), Fp6.Negate(a.C1));
    }

    public static Fp12 Multiply(Fp12 a, Fp12 b)
    {
        // (a0 + a1*w)(b0 + b1*w), w^2 = v
        var t0 = Fp6.Multiply(a.C0, b.C0);
        var t1 = Fp6.Multiply(a.C1, b.C1);
        var c0 = Fp6.Add(t0, Fp6.MulByNonResidue(t1));
        var c1 = Fp6.Subtract(Fp6.Subtract(Fp6.Multiply(Fp6.Add(a.C0, a.C1), Fp6.Add(b.C0, b.C1)), t0), t1);
        return new Fp12(c0, c1);
    }

    public static Fp12 Square(Fp12 a) => Multiply(a, a);

    public static Fp12 Invert(Fp12 a)
    {
        // (a0 + a1*w)^-1 = (a0 - a1*w)/(a0^2 - v*a1^2)
        var t0 = Fp6.Square(a.C0);
        var t1 = Fp6.MulByNonResidue(Fp6.Square(a.C1));
        var t = Fp6.Subtract(t0, t1);
        var tInv = Fp6.Invert(t);
        return new Fp12(
            Fp6.Multiply(a.C0, tInv),
            Fp6.Negate(Fp6.Multiply(a.C1, tInv))
        );
    }

    public static Fp12 Pow(Fp12 value, System.Numerics.BigInteger exponent)
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

    public static Fp12 FrobeniusMap(Fp12 a, int power)
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

    public static Fp12 CyclotomicSquare(Fp12 a) => Square(a);

    public static Fp12 CyclotomicExp(Fp12 a, System.Numerics.BigInteger exponent) => Pow(a, exponent);

    public static Fp12 FinalExponentiation(Fp12 a)
    {
        // Generic final exponent for GT: (p^12 - 1) / r
        // p, r per BLS12-381 parameters (RFC 9380 / EIP-2537)
        var p = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var r = System.Numerics.BigInteger.Parse(
            "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var exp = (System.Numerics.BigInteger.Pow(p, 12) - 1) / r;
        return Pow(a, exp);
    }
}
