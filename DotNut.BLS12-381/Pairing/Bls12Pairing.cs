using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;
using System.Globalization;
using System.Numerics;

namespace DotNut.BLS12_381.Pairing;

public static class Bls12Pairing
{
    // Leading "0" required: AllowHexSpecifier treats high bit as sign (two's complement).
    private static readonly BigInteger AteLoopSize = BigInteger.Parse("0d201000000010000", NumberStyles.AllowHexSpecifier);
    private static readonly Fp2 Fp2Div2 = DivideBy2(Fp2.One);

    public static Fp12 Pair(G1Affine p, G2Affine q)
    {
        var f = MillerLoop(p, q);
        return Fp12.FinalExponentiation(f);
    }

    public static Fp12 MillerLoop(G1Affine p, G2Affine q)
    {
        if (p.IsInfinity || q.IsInfinity)
            return Fp12.One;
        if (!p.IsInSubgroup() || !q.IsInSubgroup())
            throw new ArgumentException("Input point is not in the correct subgroup.");

        var ell = CalcPairingPrecomputes(q);
        var f12 = Fp12.One;

        for (var i = 0; i < ell.Count; i++)
        {
            f12 = Fp12.Square(f12);
            var steps = ell[i];
            for (var j = 0; j < steps.Count; j++)
            {
                var (c0, c1, c2) = steps[j];
                f12 = LineFunction(c0, c1, c2, f12, p.X, p.Y);
            }
        }

        // x is negative for BLS12-381.
        return Fp12.Conjugate(f12);
    }

    private static List<List<(Fp2 c0, Fp2 c1, Fp2 c2)>> CalcPairingPrecomputes(G2Affine q)
    {
        var qx = q.X;
        var qy = q.Y;
        var negQy = Fp2.Negate(qy);

        var rx = qx;
        var ry = qy;
        var rz = Fp2.One;

        var ell = new List<List<(Fp2, Fp2, Fp2)>>();
        foreach (var bit in NafDecomposition(AteLoopSize))
        {
            var cur = new List<(Fp2, Fp2, Fp2)>(2);
            (rx, ry, rz) = PointDouble(cur, rx, ry, rz);
            if (bit != 0)
                (rx, ry, rz) = PointAdd(cur, rx, ry, rz, qx, bit == -1 ? negQy : qy);
            ell.Add(cur);
        }

        return ell;
    }

    private static (Fp2 rx, Fp2 ry, Fp2 rz) PointDouble(List<(Fp2 c0, Fp2 c1, Fp2 c2)> ell, Fp2 rx, Fp2 ry, Fp2 rz)
    {
        var t0 = Fp2.Square(ry);
        var t1 = Fp2.Square(rz);
        var t2 = MulByB(MulBy3(t1));
        var t3 = MulBy3(t2);
        var t4 = Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(ry, rz)), t1), t0);
        var c0 = Fp2.Subtract(t2, t0);
        var c1 = MulBy3(Fp2.Square(rx));
        var c2 = Fp2.Negate(t4);
        ell.Add((c0, c1, c2));

        rx = Fp2.Multiply(Fp2.Multiply(Fp2.Multiply(Fp2.Subtract(t0, t3), rx), ry), Fp2Div2);
        ry = Fp2.Subtract(Fp2.Square(Fp2.Multiply(Fp2.Add(t0, t3), Fp2Div2)), MulBy3(Fp2.Square(t2)));
        rz = Fp2.Multiply(t0, t4);
        return (rx, ry, rz);
    }

    private static (Fp2 rx, Fp2 ry, Fp2 rz) PointAdd(List<(Fp2 c0, Fp2 c1, Fp2 c2)> ell, Fp2 rx, Fp2 ry, Fp2 rz, Fp2 qx, Fp2 qy)
    {
        var t0 = Fp2.Subtract(ry, Fp2.Multiply(qy, rz));
        var t1 = Fp2.Subtract(rx, Fp2.Multiply(qx, rz));
        var c0 = Fp2.Subtract(Fp2.Multiply(t0, qx), Fp2.Multiply(t1, qy));
        var c1 = Fp2.Negate(t0);
        var c2 = t1;
        ell.Add((c0, c1, c2));

        var t2 = Fp2.Square(t1);
        var t3 = Fp2.Multiply(t2, t1);
        var t4 = Fp2.Multiply(t2, rx);
        var t5 = Fp2.Add(Fp2.Subtract(t3, MulBy2(t4)), Fp2.Multiply(Fp2.Square(t0), rz));
        rx = Fp2.Multiply(t1, t5);
        ry = Fp2.Subtract(Fp2.Multiply(Fp2.Subtract(t4, t5), t0), Fp2.Multiply(t3, ry));
        rz = Fp2.Multiply(rz, t3);
        return (rx, ry, rz);
    }

    private static Fp12 LineFunction(Fp2 c0, Fp2 c1, Fp2 c2, Fp12 f, Fp px, Fp py)
    {
        var o1 = MulByFp(c1, px);
        var o4 = MulByFp(c2, py);
        return Fp12.MulBy014(f, c0, o1, o4);
    }

    private static List<int> NafDecomposition(BigInteger a)
    {
        var res = new List<int>();
        while (a > 1)
        {
            if (a.IsEven) res.Insert(0, 0);
            else if ((a & 3) == 3)
            {
                res.Insert(0, -1);
                a += 1;
            }
            else res.Insert(0, 1);
            a >>= 1;
        }
        return res;
    }

    private static Fp2 MulByFp(Fp2 value, Fp scalar) => new(Fp.Multiply(value.C0, scalar), Fp.Multiply(value.C1, scalar));
    private static Fp2 MulBy2(Fp2 value) => Fp2.Add(value, value);
    private static Fp2 MulBy3(Fp2 value) => Fp2.Add(value, MulBy2(value));
    private static Fp2 MulBy4(Fp2 value) => MulBy2(MulBy2(value));

    private static Fp2 MulByB(Fp2 value)
    {
        var fourC0 = MulBy4Fp(value.C0);
        var fourC1 = MulBy4Fp(value.C1);
        // 4*(c0 + c1*u)*(1+u) = (4c0-4c1) + (4c0+4c1)u
        var c0 = Fp.Subtract(fourC0, fourC1);
        var c1 = Fp.Add(fourC0, fourC1);
        return new Fp2(c0, c1);
    }

    private static Fp2 DivideBy2(Fp2 value)
    {
        var inv2 = Fp.Invert(Fp.Add(Fp.One, Fp.One));
        return new Fp2(Fp.Multiply(value.C0, inv2), Fp.Multiply(value.C1, inv2));
    }

    private static Fp MulBy4Fp(Fp value) => Fp.Add(Fp.Add(value, value), Fp.Add(value, value));
}
