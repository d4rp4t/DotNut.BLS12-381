using System.Globalization;
using System.Numerics;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

public readonly struct G2Projective(Fp2 x, Fp2 y, Fp2 z)
{
    public Fp2 X { get; } = x;
    public Fp2 Y { get; } = y;
    public Fp2 Z { get; } = z;

    public static readonly G2Projective Infinity = new(Fp2.Zero, Fp2.One, Fp2.Zero);

    public bool IsInfinity => Fp2.Equal(Z, Fp2.Zero);

    public static G2Projective Add(G2Projective p, G2Projective q)
    {
        if (p.IsInfinity) return q;
        if (q.IsInfinity) return p;

        var z1z1 = Fp2.Square(p.Z);
        var z2z2 = Fp2.Square(q.Z);
        var u1 = Fp2.Multiply(p.X, z2z2);
        var u2 = Fp2.Multiply(q.X, z1z1);
        var s1 = Fp2.Multiply(p.Y, Fp2.Multiply(q.Z, z2z2));
        var s2 = Fp2.Multiply(q.Y, Fp2.Multiply(p.Z, z1z1));

        if (Fp2.Equal(u1, u2))
            return Fp2.Equal(s1, s2) ? Double(p) : Infinity;

        var h = Fp2.Subtract(u2, u1);
        var i = Fp2.Square(DoubleFp(h));
        var j = Fp2.Multiply(h, i);
        var r = DoubleFp(Fp2.Subtract(s2, s1));
        var v = Fp2.Multiply(u1, i);

        var x3 = Fp2.Subtract(Fp2.Subtract(Fp2.Square(r), j), DoubleFp(v));
        var y3 = Fp2.Subtract(Fp2.Multiply(r, Fp2.Subtract(v, x3)), Fp2.Multiply(DoubleFp(s1), j));
        var z3 = Fp2.Multiply(Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(p.Z, q.Z)), z1z1), z2z2), h);
        return new G2Projective(x3, y3, z3);
    }

    public static G2Projective Double(G2Projective p)
    {
        if (p.IsInfinity || Fp2.Equal(p.Y, Fp2.Zero)) return Infinity;

        var a = Fp2.Square(p.X);
        var b = Fp2.Square(p.Y);
        var c = Fp2.Square(b);
        var d = DoubleFp(Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(p.X, b)), a), c));
        var e = TripleFp(a);
        var f = Fp2.Square(e);
        var x3 = Fp2.Subtract(f, DoubleFp(d));
        var y3 = Fp2.Subtract(Fp2.Multiply(e, Fp2.Subtract(d, x3)), EightFp(c));
        var z3 = DoubleFp(Fp2.Multiply(p.Y, p.Z));
        return new G2Projective(x3, y3, z3);
    }

    public static G2Projective ScalarMultiply(G2Projective p, Scalar k)
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

    private static (G2Projective, G2Projective) CtSwap(ulong bit, G2Projective a, G2Projective b)
    {
        ulong mask = 0UL - bit;
        return (
            new G2Projective(CtSelect(mask, b.X, a.X), CtSelect(mask, b.Y, a.Y), CtSelect(mask, b.Z, a.Z)),
            new G2Projective(CtSelect(mask, a.X, b.X), CtSelect(mask, a.Y, b.Y), CtSelect(mask, a.Z, b.Z))
        );
    }

    private static Fp2 CtSelect(ulong mask, Fp2 x, Fp2 y) => new(
        CtSelectFp(mask, x.C0, y.C0),
        CtSelectFp(mask, x.C1, y.C1)
    );

    private static Fp CtSelectFp(ulong mask, Fp x, Fp y) => new(
        (x.L0 & mask) | (y.L0 & ~mask),
        (x.L1 & mask) | (y.L1 & ~mask),
        (x.L2 & mask) | (y.L2 & ~mask),
        (x.L3 & mask) | (y.L3 & ~mask),
        (x.L4 & mask) | (y.L4 & ~mask),
        (x.L5 & mask) | (y.L5 & ~mask)
    );

    public G2Affine ToAffine()
    {
        if (IsInfinity) return G2Affine.Infinity;
        var zInv = Fp2.Invert(Z);
        var z2 = Fp2.Square(zInv);
        var z3 = Fp2.Multiply(z2, zInv);
        return new G2Affine(Fp2.Multiply(X, z2), Fp2.Multiply(Y, z3));
    }

    public bool IsOnCurve() => ToAffine().IsOnCurve();

    // Cofactor clearing

    // PSI constants: psi(X:Y:Z) = (PSI_X * Frob(X) : PSI_Y * Frob(Y) : Frob(Z))
    // where Frob(a+bu) = a-bu is the p-power Frobenius in Fp2.
    // PSI_X = xi^(-(p-1)/3),  PSI_Y = xi^(-(p-1)/2),  xi = Fp2.NonResidue = 1+u.
    private static readonly BigInteger ModP = BigInteger.Parse(
        "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
        NumberStyles.AllowHexSpecifier);

    private static readonly Fp2 PsiX  = Fp2.Invert(Fp2.Pow(Fp2.NonResidue, (ModP - 1) / 3));
    private static readonly Fp2 PsiY  = Fp2.Invert(Fp2.Pow(Fp2.NonResidue, (ModP - 1) / 2));
    // psi^2(X:Y:Z) = (PSI2_X * X : PSI2_Y * Y : Z);  PSI2_V = PSI_V * Frob(PSI_V).
    private static readonly Fp2 Psi2X = Fp2.Multiply(PsiX, new Fp2(PsiX.C0, Fp.Negate(PsiX.C1)));
    private static readonly Fp2 Psi2Y = Fp2.Multiply(PsiY, new Fp2(PsiY.C0, Fp.Negate(PsiY.C1)));

    // Untwist-Frobenius-Twist endomorphism on E2.
    public static G2Projective Psi(G2Projective p)
    {
        var frobX = new Fp2(p.X.C0, Fp.Negate(p.X.C1));
        var frobY = new Fp2(p.Y.C0, Fp.Negate(p.Y.C1));
        var frobZ = new Fp2(p.Z.C0, Fp.Negate(p.Z.C1));
        return new G2Projective(
            Fp2.Multiply(PsiX, frobX),
            Fp2.Multiply(PsiY, frobY),
            frobZ);
    }

    // psi^2 = psi(psi(.)); applying Frobenius twice is the identity so Z is unchanged.
    public static G2Projective Psi2(G2Projective p) =>
        new(Fp2.Multiply(Psi2X, p.X), Fp2.Multiply(Psi2Y, p.Y), p.Z);

    // [BLS_X]P via left-to-right binary double-and-add (BLS_X = 0xd201000000010000).
    internal static G2Projective MulByBLSX(G2Projective p)
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

    public static G2Projective Negate(G2Projective p) =>
        new(p.X, Fp2.Negate(p.Y), p.Z);

    private static G2Projective Subtract(G2Projective a, G2Projective b) =>
        Add(a, Negate(b));

    // Clears the h2 cofactor so the result lands in G2.
    // Formula from Wahby-Boneh 2019 (https://eprint.iacr.org/2019/403) Appendix G.2:
    //   [h2_eff]P = psi^2(2P) + (x^2 - x - 1)P + (x - 1)psi(P)
    // where x = -BLS_X (the negative BLS seed).  Expanded with t1=[x]P, t2=psi(P):
    //   = psi^2(2P) - P + [x](t1+t2) - t1 - t2
    public G2Projective ClearCofactor()
    {
        var t1 = Negate(MulByBLSX(this));          // [x]P  (x is negative)
        var t2 = Psi(this);                          // psi(P)

        var result = Psi2(Double(this));             // psi^2(2P)
        result = Subtract(result, this);              // psi^2(2P) - P

        var inner = Negate(MulByBLSX(Add(t1, t2))); // [x]([x]P + psiP) = [x^2]P + [x]psiP
        result = Add(result, inner);                   // + [x^2]P + [x]psiP
        result = Subtract(result, t1);                 // - [x]P
        result = Subtract(result, t2);                 // - psiP

        return result;
    }

    private static Fp2 DoubleFp(Fp2 v) => Fp2.Add(v, v);
    private static Fp2 TripleFp(Fp2 v) => Fp2.Add(v, DoubleFp(v));
    private static Fp2 EightFp(Fp2 v) => DoubleFp(DoubleFp(DoubleFp(v)));
}
