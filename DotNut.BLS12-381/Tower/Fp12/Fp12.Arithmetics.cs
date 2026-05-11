namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    private static readonly System.Numerics.BigInteger P = System.Numerics.BigInteger.Parse(
        "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
        System.Globalization.NumberStyles.AllowHexSpecifier
    );
    private static readonly Fp6[] FrobeniusCoeffC1 = BuildFrobeniusCoeffC1();

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

    public static Fp12 Square(Fp12 a)
    {
        // (a0 + a1*w)^2: uses 3 Fp6 muls instead of 4 in Multiply(a,a)
        var ab = Fp6.Multiply(a.C0, a.C1);
        var c0c1 = Fp6.Add(a.C0, a.C1);
        var c0 = Fp6.Multiply(Fp6.Add(Fp6.MulByNonResidue(a.C1), a.C0), c0c1);
        c0 = Fp6.Subtract(Fp6.Subtract(c0, ab), Fp6.MulByNonResidue(ab));
        var c1 = Fp6.Add(ab, ab);
        return new Fp12(c0, c1);
    }

    // Multiply f by a sparse Fp12 of the form (c0, c1, 0; 0, c4, 0).
    // Positions 0,1,4 are non-zero
    public static Fp12 MulBy014(Fp12 f, Fp2 c0, Fp2 c1, Fp2 c4)
    {
        var aa = Fp6.MulBy01(f.C0, c0, c1);
        var bb = Fp6.MulBy1(f.C1, c4);
        var o = Fp2.Add(c1, c4);
        var newC1 = Fp6.MulBy01(Fp6.Add(f.C1, f.C0), c0, o);
        newC1 = Fp6.Subtract(Fp6.Subtract(newC1, aa), bb);
        var newC0 = Fp6.Add(Fp6.MulByNonResidue(bb), aa);
        return new Fp12(newC0, newC1);
    }

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
        var idx = ((power % 12) + 12) % 12;
        return new Fp12(
            Fp6.FrobeniusMap(a.C0, power),
            Fp6.Multiply(Fp6.FrobeniusMap(a.C1, power), FrobeniusCoeffC1[idx])
        );
    }

    public static Fp12 Conjugate(Fp12 a)
    {
        return new Fp12(a.C0, Fp6.Negate(a.C1));
    }

    public static Fp12 CyclotomicSquare(Fp12 f)
    {
        // Algorithm 5.5.4 - only correct for elements in the cyclotomic subgroup
        // z-variable mapping from zkcrypto: z0=C0.C0, z4=C0.C1, z3=C0.C2, z2=C1.C0, z1=C1.C1, z5=C1.C2
        var z0 = f.C0.C0;
        var z4 = f.C0.C1;
        var z3 = f.C0.C2;
        var z2 = f.C1.C0;
        var z1 = f.C1.C1;
        var z5 = f.C1.C2;

        var (t0, t1) = Fp4Square(z0, z1);
        z0 = Fp2.Subtract(t0, z0);
        z0 = Fp2.Add(Fp2.Add(z0, z0), t0);
        z1 = Fp2.Add(t1, z1);
        z1 = Fp2.Add(Fp2.Add(z1, z1), t1);

        (t0, t1) = Fp4Square(z2, z3);
        var (t2, t3) = Fp4Square(z4, z5);

        z4 = Fp2.Subtract(t0, z4);
        z4 = Fp2.Add(Fp2.Add(z4, z4), t0);
        z5 = Fp2.Add(t1, z5);
        z5 = Fp2.Add(Fp2.Add(z5, z5), t1);

        t0 = Fp2.MultiplyByNonResidue(t3);
        z2 = Fp2.Add(t0, z2);
        z2 = Fp2.Add(Fp2.Add(z2, z2), t0);
        z3 = Fp2.Subtract(t2, z3);
        z3 = Fp2.Add(Fp2.Add(z3, z3), t2);

        return new Fp12(
            new Fp6(z0, z4, z3),
            new Fp6(z2, z1, z5)
        );
    }

    public static Fp12 CyclotomicExp(Fp12 a, System.Numerics.BigInteger exponent)
    {
        if (exponent.Sign < 0) throw new ArgumentOutOfRangeException(nameof(exponent));
        var result = One;
        var baseValue = a;
        var e = exponent;
        while (e > 0)
        {
            if (!e.IsEven)
                result = Multiply(result, baseValue);
            baseValue = CyclotomicSquare(baseValue);
            e >>= 1;
        }
        return result;
    }

    public static Fp12 FinalExponentiation(Fp12 a)
    {
        // Easy part: f^((p^6 - 1)(p^2 + 1))
        var t0 = Invert(a);
        var t1 = Conjugate(a);
        var f = Multiply(t1, t0);
        f = Multiply(FrobeniusMap(f, 2), f);

        // Hard part (Beuchat et al.)
        t1 = Conjugate(CyclotomicSquare(f));
        var t3 = CyclotomicExpBlsX(f);
        var t4 = CyclotomicSquare(t3);
        var t5 = Multiply(t1, t3);
        t1 = CyclotomicExpBlsX(t5);
        t0 = CyclotomicExpBlsX(t1);
        var t6 = CyclotomicExpBlsX(t0);
        t6 = Multiply(t6, t4);
        t4 = CyclotomicExpBlsX(t6);
        t5 = Conjugate(t5);
        t4 = Multiply(Multiply(t4, t5), f);
        t5 = Conjugate(f);
        t1 = Multiply(t1, f);
        t1 = FrobeniusMap(t1, 3);
        t6 = Multiply(t6, t5);
        t6 = FrobeniusMap(t6, 1);
        t3 = Multiply(t3, t0);
        t3 = FrobeniusMap(t3, 2);
        t3 = Multiply(t3, t1);
        t3 = Multiply(t3, t6);
        return Multiply(t3, t4);
    }

    private static (Fp2 c0, Fp2 c1) Fp4Square(Fp2 a, Fp2 b)
    {
        var t0 = Fp2.Square(a);
        var t1 = Fp2.Square(b);
        var t2 = Fp2.MultiplyByNonResidue(t1);
        var c0 = Fp2.Add(t2, t0);
        t2 = Fp2.Square(Fp2.Add(a, b));
        t2 = Fp2.Subtract(t2, t0);
        var c1 = Fp2.Subtract(t2, t1);
        return (c0, c1);
    }

    internal static Fp12 CyclotomicExpBlsX(Fp12 f)
    {
        // BLS_X = 0xd201000000010000, negative -> conjugate at end
        const ulong x = 0xd201000000010000UL;
        var tmp = One;
        var foundOne = false;
        for (var b = 63; b >= 0; b--)
        {
            var bit = ((x >> b) & 1) == 1;
            if (foundOne)
                tmp = CyclotomicSquare(tmp);
            else
                foundOne = bit;
            if (bit)
                tmp = Multiply(tmp, f);
        }
        return Conjugate(tmp);
    }

    private static Fp6[] BuildFrobeniusCoeffC1()
    {
        var arr = new Fp6[12];
        arr[0] = Fp6.One;
        var v = new Fp6(Fp2.Zero, Fp2.One, Fp2.Zero);
        for (var i = 1; i < 12; i++)
        {
            var pPow = System.Numerics.BigInteger.Pow(P, i);
            var e = (pPow - 1) / 2;
            arr[i] = Fp6.Pow(v, e);
        }
        return arr;
    }
}
