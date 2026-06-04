using System.Globalization;
using System.Numerics;
using DotNut.BLS12_381.Tower;
using G2Projective = DotNut.BLS12_381.Curve.G2.G2Projective;

namespace DotNut.BLS12_381.Curve.G2;

/// <summary>
/// A point on the BLS12-381 G2 curve in homogeneous projective coordinates (X : Y : Z) over Fp2.
/// The affine point (x, y) is represented as (x·Z : y·Z : Z) for any Z ≠ 0.
/// The point at infinity uses the canonical representation (0 : 1 : 0).
/// Arithmetic uses Algorithm 7 (complete addition) and Algorithm 9 (doubling) from
/// https://eprint.iacr.org/2015/1060.pdf.
/// </summary>
public readonly partial struct G2Projective(Fp2 x, Fp2 y, Fp2 z)
{
    /// <summary>Projective X coordinate; each Fp component in Montgomery form.</summary>
    public Fp2 X { get; } = x;

    /// <summary>Projective Y coordinate; each Fp component in Montgomery form.</summary>
    public Fp2 Y { get; } = y;

    /// <summary>Projective Z coordinate; each Fp component in Montgomery form.</summary>
    public Fp2 Z { get; } = z;

    /// <summary>The point at infinity (canonical representation).</summary>
    public static readonly G2Projective Infinity = new(Fp2.Zero, Fp2.One, Fp2.Zero);

    /// <summary>The standard generator of G2, lifted from G2Affine.</summary>
    public static G2Projective Generator => G2Affine.Generator.ToProjective();

    /// <summary>Returns <see langword="true"/> if Z = 0 (this is the point at infinity).</summary>
    public bool IsInfinity => Fp2.Equal(Z, Fp2.Zero);

    /// <summary>
    /// Complete projective addition using Algorithm 7, https://eprint.iacr.org/2015/1060.pdf.
    /// Handles all cases including the point at infinity, P = Q, and P = −Q.
    /// </summary>
    public static G2Projective Add(G2Projective p, G2Projective q)
    {
        var t0 = Fp2.Multiply(p.X, q.X);
        var t1 = Fp2.Multiply(p.Y, q.Y);
        var t2 = Fp2.Multiply(p.Z, q.Z);
        var t3 = Fp2.Add(p.X, p.Y);
        var t4 = Fp2.Add(q.X, q.Y);
        t3 = Fp2.Multiply(t3, t4);
        t4 = Fp2.Add(t0, t1);
        t3 = Fp2.Subtract(t3, t4);
        t4 = Fp2.Add(p.Y, p.Z);
        var x3 = Fp2.Add(q.Y, q.Z);
        t4 = Fp2.Multiply(t4, x3);
        x3 = Fp2.Add(t1, t2);
        t4 = Fp2.Subtract(t4, x3);
        x3 = Fp2.Add(p.X, p.Z);
        var y3 = Fp2.Add(q.X, q.Z);
        x3 = Fp2.Multiply(x3, y3);
        y3 = Fp2.Add(t0, t2);
        y3 = Fp2.Subtract(x3, y3);
        x3 = Fp2.Add(t0, t0);
        t0 = Fp2.Add(x3, t0);
        t2 = MulBy3b(t2);
        var z3 = Fp2.Add(t1, t2);
        t1 = Fp2.Subtract(t1, t2);
        y3 = MulBy3b(y3);
        x3 = Fp2.Multiply(t4, y3);
        t2 = Fp2.Multiply(t3, t1);
        x3 = Fp2.Subtract(t2, x3);
        y3 = Fp2.Multiply(y3, t0);
        t1 = Fp2.Multiply(t1, z3);
        y3 = Fp2.Add(t1, y3);
        t0 = Fp2.Multiply(t0, t3);
        z3 = Fp2.Multiply(z3, t4);
        z3 = Fp2.Add(z3, t0);
        return new G2Projective(x3, y3, z3);
    }

    /// <summary>
    /// Mixed addition using Algorithm 8, https://eprint.iacr.org/2015/1060.pdf
    /// </summary>
    public static G2Projective Add(G2Projective p, G2Affine q)
    {
        var t0 = Fp2.Multiply(p.X, q.X);
        var t1 = Fp2.Multiply(p.Y, q.Y);
        var t3 = Fp2.Add(q.X, q.Y);
        var t4 = Fp2.Add(p.X, p.Y);
        t3 = Fp2.Multiply(t3, t4);
        t4 = Fp2.Add(t0, t1);
        t3 = Fp2.Subtract(t3, t4);
        t4 = Fp2.Multiply(q.Y, p.Z);
        t4 = Fp2.Add(t4, p.Y);
        var y3 = Fp2.Multiply(q.X, p.Z);
        y3 = Fp2.Add(y3, p.X);
        var x3 = Fp2.Add(t0, t0);
        t0 = Fp2.Add(x3, t0);
        var t2 = MulBy3b(p.Z);
        var z3 = Fp2.Add(t1, t2);
        t1 = Fp2.Subtract(t1, t2);
        y3 = MulBy3b(y3);
        x3 = Fp2.Multiply(t4, y3);
        t2 = Fp2.Multiply(t3, t1);
        x3 = Fp2.Subtract(t2, x3);
        y3 = Fp2.Multiply(y3, t0);
        t1 = Fp2.Multiply(t1, z3);
        y3 = Fp2.Add(t1, y3);
        t0 = Fp2.Multiply(t0, t3);
        z3 = Fp2.Multiply(z3, t4);
        z3 = Fp2.Add(z3, t0);

        var tmp = new G2Projective(x3, y3, z3);
        return ConditionalSelect(tmp, p, q.IsInfinity);
    }

    /// <summary>
    /// Mixed addition using Algorithm 8, https://eprint.iacr.org/2015/1060.pdf
    /// </summary>
    public static G2Projective Add(G2Affine p, G2Projective q) => Add(q, p);

    /// <summary>
    /// Projective doubling using Algorithm 9, https://eprint.iacr.org/2015/1060.pdf.
    /// Returns the identity if P is the point at infinity.
    /// </summary>
    public static G2Projective Double(G2Projective p)
    {
        var t0 = Fp2.Square(p.Y);
        var z3 = Fp2.Add(t0, t0);
        z3 = Fp2.Add(z3, z3);
        z3 = Fp2.Add(z3, z3);
        var t1 = Fp2.Multiply(p.Y, p.Z);
        var t2 = Fp2.Square(p.Z);
        t2 = MulBy3b(t2);
        var x3 = Fp2.Multiply(t2, z3);
        var y3 = Fp2.Add(t0, t2);
        z3 = Fp2.Multiply(t1, z3);
        t1 = Fp2.Add(t2, t2);
        t2 = Fp2.Add(t1, t2);
        t0 = Fp2.Subtract(t0, t2);
        y3 = Fp2.Multiply(t0, y3);
        y3 = Fp2.Add(x3, y3);
        t1 = Fp2.Multiply(p.X, p.Y);
        x3 = Fp2.Multiply(t0, t1);
        x3 = Fp2.Add(x3, x3);
        return ConditionalSelect(new G2Projective(x3, y3, z3), Infinity, p.IsInfinity);
    }

    /// <summary>
    /// Computes [k]P using the Montgomery ladder scalar multiplication algorithm.
    /// Processes 255 bits of the scalar; constant-time with respect to the scalar.
    /// </summary>
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
            new G2Projective(Fp2.ConditionalSelect(a.X, b.X, mask), Fp2.ConditionalSelect(a.Y, b.Y, mask), Fp2.ConditionalSelect(a.Z, b.Z, mask)),
            new G2Projective(Fp2.ConditionalSelect(b.X, a.X, mask), Fp2.ConditionalSelect(b.Y, a.Y, mask), Fp2.ConditionalSelect(b.Z, a.Z, mask))
        );
    }

    /// <summary>
    /// Converts this homogeneous projective point to affine form by computing (X/Z, Y/Z).
    /// Returns <see cref="G2Affine.Infinity"/> if this is the point at infinity.
    /// </summary>
    public G2Affine ToAffine()
    {
        var zInv = Fp2.Invert(Z);
        return G2Affine.ConditionalSelect(
            new G2Affine(Fp2.Multiply(X, zInv), Fp2.Multiply(Y, zInv)),
            G2Affine.Infinity,
            IsInfinity);
    }

    /// <summary>Converts to affine and checks the G2 curve equation. See <see cref="G2Affine.IsOnCurve"/>.</summary>
    public bool IsOnCurve() => ToAffine().IsOnCurve();

    // PSI constants: psi(X:Y:Z) = (PSI_X · Frob(X) : PSI_Y · Frob(Y) : Frob(Z))
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

    /// <summary>
    /// Applies the Untwist-Frobenius-Twist (ψ) endomorphism on E2(Fp2).
    /// Computes ψ(X:Y:Z) = (PSI_X · Frob(X) : PSI_Y · Frob(Y) : Frob(Z)).
    /// </summary>
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

    /// <summary>
    /// Computes ψ²(P) = ψ(ψ(P)) using precomputed combined constants.
    /// The Z coordinate is unchanged since applying Frobenius twice is the identity.
    /// </summary>
    public static G2Projective Psi2(G2Projective p) =>
        new(Fp2.Multiply(Psi2X, p.X), Fp2.Multiply(Psi2Y, p.Y), p.Z);

    /// <summary>
    /// Computes [BLS_X]P via left-to-right binary double-and-add.
    /// BLS_X = 0xd201000000010000.
    /// </summary>
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

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// </summary>
    public static G2Projective ConditionalSelect(G2Projective a, G2Projective b, bool choice) => new(
        Fp2.ConditionalSelect(a.X, b.X, choice),
        Fp2.ConditionalSelect(a.Y, b.Y, choice),
        Fp2.ConditionalSelect(a.Z, b.Z, choice)
    );

    /// <summary>Returns −P by negating the Y coordinate.</summary>
    public static G2Projective Negate(G2Projective p) =>
        new(p.X, Fp2.Negate(p.Y), p.Z);

    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G2Projective Subtract(G2Projective a, G2Projective b) =>
        Add(a, Negate(b));
    
    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G2Projective Subtract(G2Projective a, G2Affine b) => Add(a, G2Affine.Negate(b));
    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G2Projective Subtract(G2Affine a, G2Projective b) => Add(G2Affine.Negate(a), b);

    /// <summary>
    /// Clears the G2 cofactor h2 so that the result is in the G2 prime-order subgroup.
    /// Uses the Wahby-Boneh 2019 formula (https://eprint.iacr.org/2019/403, Appendix G.2).
    /// </summary>
    public G2Projective ClearCofactor()
    {
        var t1 = Negate(MulByBLSX(this));
        var t2 = Psi(this);

        var result = Psi2(Double(this));
        result = Subtract(result, this);

        var inner = Negate(MulByBLSX(Add(t1, t2)));
        result = Add(result, inner);
        result = Subtract(result, t1);
        result = Subtract(result, t2);

        return result;
    }

    public override bool Equals(object? obj) => obj is G2Projective other && this == other;

    public override int GetHashCode() => ToAffine().GetHashCode();

    // B3 = 3 * B where B = (4+4u) for G2 curve y² = x³ + (4+4u)
    private static readonly Fp Four = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
    private static readonly Fp2 B2 = new(Four, Four);
    private static readonly Fp2 B3G2 = Fp2.Add(Fp2.Add(B2, B2), B2);

    private static Fp2 MulBy3b(Fp2 x) => Fp2.Multiply(B3G2, x);
}
