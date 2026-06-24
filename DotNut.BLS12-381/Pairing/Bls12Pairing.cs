using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

/// <summary>
/// BLS12-381 optimal Ate pairing e: G1 × G2 → GT.
/// Implements the Miller loop using the BLS seed x = 0xd201000000010000 (negative)
/// and the Beuchat-et-al. final exponentiation.
/// </summary>
public static class Bls12Pairing
{
    // BLS_X = 0xd201000000010000 (negative parameter for BLS12-381)
    // BLS_X >> 1 — used as the binary Miller loop scalar
    private const ulong BlsXHalf = 0x6900800000008000UL;

    /// <summary>
    /// Computes the full BLS12-381 pairing e(P, Q) = MillerLoop(P, Q).FinalExponentiation().
    /// Both inputs must be in the correct prime-order subgroup.
    /// Returns the identity element of GT if either input is the point at infinity.
    /// </summary>
    /// <param name="p">G1 point; must be in the G1 prime-order subgroup.</param>
    /// <param name="q">G2 point; must be in the G2 prime-order subgroup.</param>
    public static Gt Pair(G1Affine p, G2Affine q) => MillerLoop(p, q).FinalExponentiation();

    /// <summary>
    /// Runs the optimal Ate Miller loop for a single (P, Q) pair.
    /// Iterates over the bits of BLS_X/2 (MSB to LSB) performing doubling and conditional addition steps.
    /// At the end the result is conjugated because BLS_X is negative.
    /// Does NOT apply the final exponentiation; call <see cref="MillerLoopResult.FinalExponentiation"/> on the result.
    /// </summary>
    /// <param name="p">G1 affine point; must be in the correct subgroup. Returns default if infinity.</param>
    /// <param name="q">G2 affine point; must be in the correct subgroup. Returns default if infinity.</param>
    /// <returns>Raw Miller loop Fp12 value wrapped in <see cref="MillerLoopResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if either point is not in the correct prime-order subgroup.</exception>
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

    /// <summary>
    /// Runs the optimal Ate Miller loop for multiple (P_i, Q_i) pairs simultaneously,
    /// accumulating line evaluations into a single Fp12 product.
    /// More efficient than calling <see cref="MillerLoop"/> separately for each pair
    /// because the squarings are shared.
    /// Q values should be precomputed via <see cref="G2Prepared.From"/> to avoid redundant G2 steps.
    /// Does NOT apply final exponentiation.
    /// </summary>
    /// <param name="terms">Pairs of (G1 point, precomputed G2 coefficients).</param>
    /// <returns>Accumulated Miller loop result; call <see cref="MillerLoopResult.FinalExponentiation"/> to get GT.</returns>
    public static MillerLoopResult MultiMillerLoop(IEnumerable<(G1Affine P, G2Prepared Q)> terms)
    {
        var termList = terms.ToList();

        foreach (var (p, _) in termList)
            if (!p.IsInfinity && !p.IsInSubgroup())
                throw new ArgumentException("G1 point is not in the correct prime-order subgroup.");

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

    /// <summary>
    /// Runs the full G2 Miller loop preprocessing for <paramref name="q"/> and stores the 68 coefficient triples
    /// needed by <see cref="MultiMillerLoop"/>.
    /// If <paramref name="q"/> is infinity, the generator is used as a stand-in for valid arithmetic,
    /// and the <see cref="G2Prepared.IsInfinity"/> flag is set so that <see cref="MultiMillerLoop"/> skips it.
    /// </summary>
    internal static G2Prepared BuildG2Prepared(G2Affine q)
    {
        if (!q.IsInfinity && !q.IsInSubgroup())
            throw new ArgumentException("G2 point is not in the correct prime-order subgroup.", nameof(q));

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

    /// <summary>
    /// Performs one Miller loop doubling step (Algorithm 26 from https://eprint.iacr.org/2010/354.pdf).
    /// Updates the running G2 projective accumulator (rx, ry, rz) in place and returns the line coefficients (c0, c1, c2).
    /// </summary>
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

    /// <summary>
    /// Performs one Miller loop addition step (Algorithm 27 from https://eprint.iacr.org/2010/354.pdf).
    /// Updates the running G2 projective accumulator (rx, ry, rz) in place and returns the line coefficients (c0, c1, c2).
    /// </summary>
    /// <param name="rx">Running accumulator X (projective, modified in place).</param>
    /// <param name="ry">Running accumulator Y (projective, modified in place).</param>
    /// <param name="rz">Running accumulator Z (projective, modified in place).</param>
    /// <param name="qx">X coordinate of the fixed G2 affine point.</param>
    /// <param name="qy">Y coordinate of the fixed G2 affine point.</param>
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

    /// <summary>
    /// Applies the precomputed line evaluation coefficients (c0, c1, c2) to the current Miller loop accumulator f,
    /// scaling c0 by the G1 Y coordinate and c1 by the G1 X coordinate, then calling <see cref="Fp12.MulBy014"/>.
    /// </summary>
    /// <param name="f">Current Miller loop Fp12 accumulator.</param>
    /// <param name="coeffs">Line coefficient triple from a doubling or addition step.</param>
    /// <param name="px">X coordinate of the G1 affine point (scales c1).</param>
    /// <param name="py">Y coordinate of the G1 affine point (scales c0).</param>
    private static Fp12 Ell(Fp12 f, (Fp2 c0, Fp2 c1, Fp2 c2) coeffs, Fp px, Fp py)
    {
        var ell4 = MulByFp(coeffs.c0, py);
        var ell1 = MulByFp(coeffs.c1, px);
        return Fp12.MulBy014(f, coeffs.c2, ell1, ell4);
    }

    /// <summary>
    /// Scales an Fp2 element by an Fp scalar (component-wise multiplication).
    /// Used to fold G1 coordinates into the line evaluation coefficients.
    /// </summary>
    private static Fp2 MulByFp(Fp2 value, Fp scalar)
        => new(Fp.Multiply(value.C0, scalar), Fp.Multiply(value.C1, scalar));
}
