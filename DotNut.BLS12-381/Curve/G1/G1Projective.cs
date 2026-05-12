using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

public readonly struct G1Projective(Fp x, Fp y, Fp z)
{
    public Fp X { get; } = x;
    public Fp Y { get; } = y;
    public Fp Z { get; } = z;

    public static readonly G1Projective Infinity = new(Fp.Zero, Fp.One, Fp.Zero);

    public bool IsInfinity => Fp.Equal(Z, Fp.Zero);

    public static G1Projective Add(G1Projective p, G1Projective q)
    {
        if (p.IsInfinity) return q;
        if (q.IsInfinity) return p;

        // EFD: add-2007-bl for short Weierstrass in Jacobian coordinates.
        var z1z1 = Fp.Square(p.Z);
        var z2z2 = Fp.Square(q.Z);
        var u1 = Fp.Multiply(p.X, z2z2);
        var u2 = Fp.Multiply(q.X, z1z1);
        var s1 = Fp.Multiply(p.Y, Fp.Multiply(q.Z, z2z2));
        var s2 = Fp.Multiply(q.Y, Fp.Multiply(p.Z, z1z1));

        if (Fp.Equal(u1, u2))
            return Fp.Equal(s1, s2) ? Double(p) : Infinity;

        var h = Fp.Subtract(u2, u1);
        var i = Fp.Square(DoubleFp(h));
        var j = Fp.Multiply(h, i);
        var r = DoubleFp(Fp.Subtract(s2, s1));
        var v = Fp.Multiply(u1, i);

        var x3 = Fp.Subtract(Fp.Subtract(Fp.Square(r), j), DoubleFp(v));
        var y3 = Fp.Subtract(Fp.Multiply(r, Fp.Subtract(v, x3)), Fp.Multiply(DoubleFp(s1), j));
        var z3 = Fp.Multiply(Fp.Subtract(Fp.Subtract(Fp.Square(Fp.Add(p.Z, q.Z)), z1z1), z2z2), h);
        return new G1Projective(x3, y3, z3);
    }

    public static G1Projective Double(G1Projective p)
    {
        if (p.IsInfinity || Fp.Equal(p.Y, Fp.Zero)) return Infinity;

        // EFD: dbl-2009-l for a=0 curve in Jacobian coordinates.
        var a = Fp.Square(p.X);
        var b = Fp.Square(p.Y);
        var c = Fp.Square(b);
        var d = DoubleFp(Fp.Subtract(Fp.Subtract(Fp.Square(Fp.Add(p.X, b)), a), c));
        var e = TripleFp(a);
        var f = Fp.Square(e);
        var x3 = Fp.Subtract(f, DoubleFp(d));
        var y3 = Fp.Subtract(Fp.Multiply(e, Fp.Subtract(d, x3)), EightFp(c));
        var z3 = DoubleFp(Fp.Multiply(p.Y, p.Z));
        return new G1Projective(x3, y3, z3);
    }

    public static G1Projective ScalarMultiply(G1Projective p, Scalar k)
    {
        var r0 = Infinity;
        var r1 = p;

        for (int i = 254; i >= 0; i--)
        {
            ulong bit = k.GetBit(i);
            (r0, r1) = CtSwap(bit, r0, r1);
            r1 = Add(r0, r1);
            r0 = Double(r0);
            (r0, r1) = CtSwap(bit, r0, r1);
        }

        return r0;
    }

    private static (G1Projective, G1Projective) CtSwap(ulong bit, G1Projective a, G1Projective b)
    {
        ulong mask = 0UL - bit;
        return (
            new G1Projective(CtSelect(mask, b.X, a.X), CtSelect(mask, b.Y, a.Y), CtSelect(mask, b.Z, a.Z)),
            new G1Projective(CtSelect(mask, a.X, b.X), CtSelect(mask, a.Y, b.Y), CtSelect(mask, a.Z, b.Z))
        );
    }

    private static Fp CtSelect(ulong mask, Fp x, Fp y) => new(
        (x.L0 & mask) | (y.L0 & ~mask),
        (x.L1 & mask) | (y.L1 & ~mask),
        (x.L2 & mask) | (y.L2 & ~mask),
        (x.L3 & mask) | (y.L3 & ~mask),
        (x.L4 & mask) | (y.L4 & ~mask),
        (x.L5 & mask) | (y.L5 & ~mask)
    );

    public G1Affine ToAffine()
    {
        if (IsInfinity) return G1Affine.Infinity;
        var zInv = Fp.Invert(Z);
        var z2 = Fp.Square(zInv);
        var z3 = Fp.Multiply(z2, zInv);
        return new G1Affine(Fp.Multiply(X, z2), Fp.Multiply(Y, z3));
    }

    public bool IsOnCurve() => ToAffine().IsOnCurve();

    // h1 = 0xd201000000010001 = 1 + BLS_X; clears the E1(Fp) cofactor so the result is in G1.
    public G1Projective ClearCofactor() => Add(this, MulByBLSX(this));

    // [BLS_X]P via left-to-right binary double-and-add (BLS_X = 0xd201000000010000, 6 set bits).
    internal static G1Projective MulByBLSX(G1Projective p)
    {
        const ulong BlsX = 0xd201_0000_0001_0000UL;
        var acc = Infinity;
        var foundOne = false;
        for (var i = 63; i >= 0; i--)
        {
            var bit = ((BlsX >> i) & 1UL) != 0;
            if (foundOne)
                acc = Double(acc);
            else
                foundOne = bit;
            if (bit)
                acc = Add(acc, p);
        }
        return acc;
    }

    private static Fp DoubleFp(Fp v) => Fp.Add(v, v);
    private static Fp TripleFp(Fp v) => Fp.Add(v, DoubleFp(v));
    private static Fp EightFp(Fp v) => DoubleFp(DoubleFp(DoubleFp(v)));
}
