using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

public static class Bls12Pairing
{
    // BLS_X = 0xd201000000010000 (negative parameter for BLS12-381)
    // BLS_X >> 1 — used as the binary Miller loop scalar
    private const ulong BlsXHalf = 0x6900800000008000UL;

    public static Gt Pair(G1Affine p, G2Affine q) => MillerLoop(p, q).FinalExponentiation();

    public static MillerLoopResult MillerLoop(G1Affine p, G2Affine q)
    {
        if (p.IsInfinity || q.IsInfinity) return MillerLoopResult.Default;
        if (!p.IsInSubgroup() || !q.IsInSubgroup())
            throw new ArgumentException("Input point is not in the correct subgroup.");

        var rx = q.X; var ry = q.Y; var rz = Fp2.One;
        var f = Fp12.One;
        var foundOne = false;

        for (var b = 63; b >= 0; b--)
        {
            var bit = ((BlsXHalf >> b) & 1) == 1;
            if (!foundOne) { foundOne = bit; continue; }

            f = Ell(f, DoublingStep(ref rx, ref ry, ref rz), p.X, p.Y);
            if (bit)
                f = Ell(f, AdditionStep(ref rx, ref ry, ref rz, q.X, q.Y), p.X, p.Y);
            f = Fp12.Square(f);
        }

        f = Ell(f, DoublingStep(ref rx, ref ry, ref rz), p.X, p.Y);

        // BLS_X is negative
        return new MillerLoopResult(Fp12.Conjugate(f));
    }

    public static MillerLoopResult MultiMillerLoop(IEnumerable<(G1Affine P, G2Prepared Q)> terms)
    {
        var termList = terms.ToList();
        var f = Fp12.One;
        var coeffIdx = 0;
        var foundOne = false;

        for (var b = 63; b >= 0; b--)
        {
            var bit = ((BlsXHalf >> b) & 1) == 1;
            if (!foundOne) { foundOne = bit; continue; }

            foreach (var (p, qPrep) in termList)
                if (!p.IsInfinity && !qPrep.IsInfinity)
                    f = Ell(f, qPrep.Coeffs[coeffIdx], p.X, p.Y);
            coeffIdx++;

            if (bit)
            {
                foreach (var (p, qPrep) in termList)
                    if (!p.IsInfinity && !qPrep.IsInfinity)
                        f = Ell(f, qPrep.Coeffs[coeffIdx], p.X, p.Y);
                coeffIdx++;
            }

            f = Fp12.Square(f);
        }

        foreach (var (p, qPrep) in termList)
            if (!p.IsInfinity && !qPrep.IsInfinity)
                f = Ell(f, qPrep.Coeffs[coeffIdx], p.X, p.Y);

        return new MillerLoopResult(Fp12.Conjugate(f));
    }

    internal static G2Prepared BuildG2Prepared(G2Affine q)
    {
        var isInfinity = q.IsInfinity;
        // Use generator as stand-in for infinity to keep valid arithmetic
        var qq = isInfinity ? G2Affine.Generator : q;

        var rx = qq.X; var ry = qq.Y; var rz = Fp2.One;
        var coeffs = new (Fp2 c0, Fp2 c1, Fp2 c2)[68];
        var idx = 0;
        var foundOne = false;

        for (var b = 63; b >= 0; b--)
        {
            var bit = ((BlsXHalf >> b) & 1) == 1;
            if (!foundOne) { foundOne = bit; continue; }

            coeffs[idx++] = DoublingStep(ref rx, ref ry, ref rz);
            if (bit)
                coeffs[idx++] = AdditionStep(ref rx, ref ry, ref rz, qq.X, qq.Y);
        }

        coeffs[idx++] = DoublingStep(ref rx, ref ry, ref rz);
        System.Diagnostics.Debug.Assert(idx == 68);

        return new G2Prepared(isInfinity, coeffs);
    }

    // Algorithm 26, https://eprint.iacr.org/2010/354.pdf
    private static (Fp2 c0, Fp2 c1, Fp2 c2) DoublingStep(ref Fp2 rx, ref Fp2 ry, ref Fp2 rz)
    {
        var tmp0 = Fp2.Square(rx);
        var tmp1 = Fp2.Square(ry);
        var tmp2 = Fp2.Square(tmp1);
        var tmp3 = Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(tmp1, rx)), tmp0), tmp2);
        tmp3 = Fp2.Add(tmp3, tmp3);
        var tmp4 = Fp2.Add(Fp2.Add(tmp0, tmp0), tmp0);
        var tmp6 = Fp2.Add(rx, tmp4);
        var tmp5 = Fp2.Square(tmp4);
        var zSq = Fp2.Square(rz);

        rx = Fp2.Subtract(Fp2.Subtract(tmp5, tmp3), tmp3);
        rz = Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(rz, ry)), tmp1), zSq);
        ry = Fp2.Multiply(Fp2.Subtract(tmp3, rx), tmp4);
        tmp2 = Fp2.Add(tmp2, tmp2);
        tmp2 = Fp2.Add(tmp2, tmp2);
        tmp2 = Fp2.Add(tmp2, tmp2);
        ry = Fp2.Subtract(ry, tmp2);

        var c1 = Fp2.Multiply(tmp4, zSq);
        c1 = Fp2.Negate(Fp2.Add(c1, c1));

        var c2 = Fp2.Subtract(Fp2.Subtract(Fp2.Square(tmp6), tmp0), tmp5);
        var t1 = Fp2.Add(Fp2.Add(tmp1, tmp1), Fp2.Add(tmp1, tmp1));
        c2 = Fp2.Subtract(c2, t1);

        var c0 = Fp2.Multiply(rz, zSq);
        c0 = Fp2.Add(c0, c0);

        return (c0, c1, c2);
    }

    // Algorithm 27, https://eprint.iacr.org/2010/354.pdf
    private static (Fp2 c0, Fp2 c1, Fp2 c2) AdditionStep(ref Fp2 rx, ref Fp2 ry, ref Fp2 rz, Fp2 qx, Fp2 qy)
    {
        var zSq = Fp2.Square(rz);
        var ySq = Fp2.Square(qy);
        var t0 = Fp2.Multiply(zSq, qx);
        var t1 = Fp2.Multiply(
            Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(qy, rz)), ySq), zSq),
            zSq);
        var t2 = Fp2.Subtract(t0, rx);
        var t3 = Fp2.Square(t2);
        var t4 = Fp2.Add(t3, t3);
        t4 = Fp2.Add(t4, t4);
        var t5 = Fp2.Multiply(t4, t2);
        var t6 = Fp2.Subtract(Fp2.Subtract(t1, ry), ry);
        var t9 = Fp2.Multiply(t6, qx);
        var t7 = Fp2.Multiply(t4, rx);

        rx = Fp2.Subtract(Fp2.Subtract(Fp2.Subtract(Fp2.Square(t6), t5), t7), t7);
        rz = Fp2.Subtract(Fp2.Subtract(Fp2.Square(Fp2.Add(rz, t2)), zSq), t3);

        var t10 = Fp2.Add(qy, rz);
        var t8 = Fp2.Multiply(Fp2.Subtract(t7, rx), t6);
        var t0b = Fp2.Multiply(ry, t5);
        t0b = Fp2.Add(t0b, t0b);
        ry = Fp2.Subtract(t8, t0b);

        t10 = Fp2.Subtract(Fp2.Square(t10), ySq);
        var ztSq = Fp2.Square(rz);
        t10 = Fp2.Subtract(t10, ztSq);
        t9 = Fp2.Subtract(Fp2.Add(t9, t9), t10);
        var c0 = Fp2.Add(rz, rz);
        t6 = Fp2.Negate(t6);
        var c1 = Fp2.Add(t6, t6);

        return (c0, c1, t9);
    }

    // Maps precomputed (c0,c1,c2) line coefficients + G1 point into Fp12 line-evaluation.
    // c0 is scaled by py, c1 by px; c2 is the unscaled ell_0 term.
    private static Fp12 Ell(Fp12 f, (Fp2 c0, Fp2 c1, Fp2 c2) coeffs, Fp px, Fp py)
    {
        var ell4 = MulByFp(coeffs.c0, py);
        var ell1 = MulByFp(coeffs.c1, px);
        return Fp12.MulBy014(f, coeffs.c2, ell1, ell4);
    }

    private static Fp2 MulByFp(Fp2 value, Fp scalar)
        => new(Fp.Multiply(value.C0, scalar), Fp.Multiply(value.C1, scalar));
}
