namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public static Fp2 Add(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Add(a.C0, b.C0),
            Fp.Add(a.C1, b.C1)
        );
    }

    public static Fp2 Sub(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Subtract(a.C0, b.C0),
            Fp.Subtract(a.C1, b.C1)
        );
    }

    public static Fp2 Negate(Fp2 a)
    {
        return new Fp2(Fp.Negate(a.C0), Fp.Negate(a.C1));
    }

    public static Fp2 Multiply(Fp2 a, Fp2 b)
    {
        // (a0 + a1*u)(b0 + b1*u), u^2 = -1
        Fp t0 = Fp.Multiply(a.C0, b.C0);
        Fp t1 = Fp.Multiply(a.C1, b.C1);
        Fp c0 = Fp.Subtract(t0, t1);
        Fp c1 = Fp.Add(Fp.Multiply(a.C0, b.C1), Fp.Multiply(a.C1, b.C0));
        return new Fp2(c0, c1);
    }

    public static Fp2 Square(Fp2 a)
    {
        // (a0 + a1*u)^2 = (a0^2 - a1^2) + 2*a0*a1*u
        Fp a0a0 = Fp.Square(a.C0);
        Fp a1a1 = Fp.Square(a.C1);
        Fp c0 = Fp.Subtract(a0a0, a1a1);
        Fp c1 = Fp.Add(Fp.Multiply(a.C0, a.C1), Fp.Multiply(a.C0, a.C1));
        return new Fp2(c0, c1);
    }

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

    public static Fp2 MultiplyByNonResidue(Fp2 a)
    {
        // (a0 + a1*u) * (1 + u) = (a0 - a1) + (a0 + a1)u
        return new Fp2(
            Fp.Subtract(a.C0, a.C1),
            Fp.Add(a.C0, a.C1)
        );
    }

    public static Fp2 FrobeniusMap(Fp2 a, int power)
    {
        // For BLS12-381 base field p, u^p = -u
        return (power & 1) == 0 ? a : new Fp2(a.C0, Fp.Negate(a.C1));
    }
}
