using DotNut.BLS12_381.Tower;
using System.Numerics;

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

    public static G2Projective ScalarMultiply(G2Projective p, BigInteger k)
    {
        if (k.Sign < 0) throw new ArgumentOutOfRangeException(nameof(k));
        var acc = Infinity;
        var cur = p;
        var e = k;
        while (e > 0)
        {
            if (!e.IsEven) acc = Add(acc, cur);
            cur = Double(cur);
            e >>= 1;
        }
        return acc;
    }

    public G2Affine ToAffine()
    {
        if (IsInfinity) return G2Affine.Infinity;
        var zInv = Fp2.Invert(Z);
        var z2 = Fp2.Square(zInv);
        var z3 = Fp2.Multiply(z2, zInv);
        return new G2Affine(Fp2.Multiply(X, z2), Fp2.Multiply(Y, z3));
    }

    public bool IsOnCurve() => ToAffine().IsOnCurve();

    private static Fp2 DoubleFp(Fp2 v) => Fp2.Add(v, v);
    private static Fp2 TripleFp(Fp2 v) => Fp2.Add(v, DoubleFp(v));
    private static Fp2 EightFp(Fp2 v) => DoubleFp(DoubleFp(DoubleFp(v)));
}
