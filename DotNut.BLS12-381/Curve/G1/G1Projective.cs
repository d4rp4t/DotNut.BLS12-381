using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

/// <summary>
/// A point on the BLS12-381 G1 curve in homogeneous projective coordinates (X : Y : Z) over Fp.
/// The affine point (x, y) is represented as (x·Z : y·Z : Z) for any Z ≠ 0.
/// The point at infinity uses the canonical representation (0 : 1 : 0).
/// Arithmetic uses Algorithm 7 (compvare addition) and Algorithm 9 (doubling) from
/// https://eprint.iacr.org/2015/1060.pdf.
/// </summary>
public readonly partial struct G1Projective(Fp x, Fp y, Fp z)
{
    /// <summary>Projective X coordinate in Montgomery form.</summary>
    public Fp X { get; } = x;

    /// <summary>Projective Y coordinate in Montgomery form.</summary>
    public Fp Y { get; } = y;

    /// <summary>Projective Z coordinate in Montgomery form.</summary>
    public Fp Z { get; } = z;

    /// <summary>The point at infinity (canonical representation).</summary>
    public static readonly G1Projective Infinity = new(Fp.Zero, Fp.One, Fp.Zero);

    /// <summary>The standard generator of G1, lifted from G1Affine.</summary>
    public static G1Projective Generator => G1Affine.Generator.ToProjective();

    /// <summary>Returns <see langword="true"/> if Z = 0 (this is the point at infinity).</summary>
    public bool IsInfinity => Fp.Equal(Z, Fp.Zero);

    /// <summary>
    /// Complete projective addition using Algorithm 7, https://eprint.iacr.org/2015/1060.pdf.
    /// Handles all cases including the point at infinity, P = Q, and P = −Q.
    /// </summary>
    public static G1Projective Add(G1Projective p, G1Projective q)
    {
        var t0 = Fp.Multiply(p.X, q.X);
        var t1 = Fp.Multiply(p.Y, q.Y);
        var t2 = Fp.Multiply(p.Z, q.Z);
        var t3 = Fp.Add(p.X, p.Y);
        var t4 = Fp.Add(q.X, q.Y);
        t3 = Fp.Multiply(t3, t4);
        t4 = Fp.Add(t0, t1);
        t3 = Fp.Subtract(t3, t4);
        t4 = Fp.Add(p.Y, p.Z);
        var x3 = Fp.Add(q.Y, q.Z);
        t4 = Fp.Multiply(t4, x3);
        x3 = Fp.Add(t1, t2);
        t4 = Fp.Subtract(t4, x3);
        x3 = Fp.Add(p.X, p.Z);
        var y3 = Fp.Add(q.X, q.Z);
        x3 = Fp.Multiply(x3, y3);
        y3 = Fp.Add(t0, t2);
        y3 = Fp.Subtract(x3, y3);
        x3 = Fp.Add(t0, t0);
        t0 = Fp.Add(x3, t0);
        t2 = MulBy3b(t2);
        var z3 = Fp.Add(t1, t2);
        t1 = Fp.Subtract(t1, t2);
        y3 = MulBy3b(y3);
        x3 = Fp.Multiply(t4, y3);
        t2 = Fp.Multiply(t3, t1);
        x3 = Fp.Subtract(t2, x3);
        y3 = Fp.Multiply(y3, t0);
        t1 = Fp.Multiply(t1, z3);
        y3 = Fp.Add(t1, y3);
        t0 = Fp.Multiply(t0, t3);
        z3 = Fp.Multiply(z3, t4);
        z3 = Fp.Add(z3, t0);
        return new G1Projective(x3, y3, z3);
    }

    public static G1Projective Add(G1Projective p, G1Affine q)
    {
        // Algorithm 8, https://eprint.iacr.org/2015/1060.pdf

        var t0 = Fp.Multiply(p.X, q.X);
        var t1 = Fp.Multiply(p.Y, q.Y);
        var t3 = Fp.Add(q.X, q.Y);
        var t4 = Fp.Add(p.X, p.Y);
        t3 = Fp.Multiply(t3, t4);
        t4 = Fp.Add(t0, t1);
        t3 = Fp.Subtract(t3, t4);
        t4 = Fp.Multiply(q.Y, p.Z);
        t4 = Fp.Add(t4, p.Y);
        var y3 = Fp.Multiply(q.X, p.Z);
        y3 = Fp.Add(y3, p.X);
        var x3 = Fp.Add(t0, t0);
        t0 = Fp.Add(x3, t0);
        var t2 = MulBy3b(p.Z);
        var z3 = Fp.Add(t1, t2);
        t1 = Fp.Subtract(t1, t2);
        y3 = MulBy3b(y3);
        x3 = Fp.Multiply(t4, y3);
        t2 = Fp.Multiply(t3, t1);
        x3 = Fp.Subtract(t2, x3);
        y3 = Fp.Multiply(y3, t0);
        t1 = Fp.Multiply(t1, z3);
        y3 = Fp.Add(t1, y3);
        t0 = Fp.Multiply(t0, t3);
        z3 = Fp.Multiply(z3, t4);
        z3 = Fp.Add(z3, t0);

        var tmp = new G1Projective(x3, y3, z3);

        return ConditionalSelect(tmp, p, q.IsInfinity);
    }
    
    public static G1Projective Add(G1Affine p, G1Projective q) => Add(q, p);

    /// <summary>
    /// Projective doubling using Algorithm 9, https://eprint.iacr.org/2015/1060.pdf.
    /// Returns the identity if P is the point at infinity.
    /// </summary>
    public static G1Projective Double(G1Projective p)
    {
        var t0 = Fp.Square(p.Y);
        var z3 = Fp.Add(t0, t0);
        z3 = Fp.Add(z3, z3);
        z3 = Fp.Add(z3, z3);
        var t1 = Fp.Multiply(p.Y, p.Z);
        var t2 = Fp.Square(p.Z);
        t2 = MulBy3b(t2);
        var x3 = Fp.Multiply(t2, z3);
        var y3 = Fp.Add(t0, t2);
        z3 = Fp.Multiply(t1, z3);
        t1 = Fp.Add(t2, t2);
        t2 = Fp.Add(t1, t2);
        t0 = Fp.Subtract(t0, t2);
        y3 = Fp.Multiply(t0, y3);
        y3 = Fp.Add(x3, y3);
        t1 = Fp.Multiply(p.X, p.Y);
        x3 = Fp.Multiply(t0, t1);
        x3 = Fp.Add(x3, x3);
        return ConditionalSelect(new G1Projective(x3, y3, z3), Infinity, p.IsInfinity);
    }

    /// <summary>
    /// Computes [k]P using the Montgomery ladder scalar multiplication algorithm.
    /// Processes 255 bits of the scalar; constant-time with respect to the scalar.
    /// </summary>
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
            new G1Projective(Fp.ConditionalSelect(a.X, b.X, mask), Fp.ConditionalSelect(a.Y, b.Y, mask), Fp.ConditionalSelect(a.Z, b.Z, mask)),
            new G1Projective(Fp.ConditionalSelect(b.X, a.X, mask), Fp.ConditionalSelect(b.Y, a.Y, mask), Fp.ConditionalSelect(b.Z, a.Z, mask))
        );
    }

    /// <summary>
    /// Converts this homogeneous projective point to affine form by computing (X/Z, Y/Z).
    /// Returns <see cref="G1Affine.Infinity"/> if this is the point at infinity.
    /// </summary>
    public G1Affine ToAffine()
    {
        // For the point at infinity (Z=0), substitute One so Invert doesn't throw;
        // the computed affine coords are discarded by ConditionalSelect anyway.
        var safeZ = Fp.ConditionalSelect(Z, Fp.One, IsInfinity);
        var zInv = Fp.Invert(safeZ);
        return G1Affine.ConditionalSelect(
            new G1Affine(Fp.Multiply(X, zInv), Fp.Multiply(Y, zInv)),
            G1Affine.Infinity,
            IsInfinity);
    }

    /// <summary>Converts to affine and checks the curve equation. See <see cref="G1Affine.IsOnCurve"/>.</summary>
    public bool IsOnCurve() => ToAffine().IsOnCurve();

    /// <summary>
    /// Clears the E1(Fp) cofactor h1 = 1 + BLS_X so that the result is in the G1 prime-order subgroup.
    /// </summary>
    public G1Projective ClearCofactor() => Add(this, MulByBLSX(this));

    /// <summary>
    /// Computes [BLS_X]P via left-to-right binary double-and-add.
    /// BLS_X = 0xd201000000010000.
    /// </summary>
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

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// </summary>
    public static G1Projective ConditionalSelect(G1Projective a, G1Projective b, bool choice) => new(
        Fp.ConditionalSelect(a.X, b.X, choice),
        Fp.ConditionalSelect(a.Y, b.Y, choice),
        Fp.ConditionalSelect(a.Z, b.Z, choice)
    );

    /// <summary>Returns −P by negating the Y coordinate.</summary>
    public static G1Projective Negate(G1Projective p) =>
        new(p.X, Fp.Negate(p.Y), p.Z);

    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G1Projective Subtract(G1Projective a, G1Projective b) =>
        Add(a, Negate(b));
    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G1Projective Subtract(G1Projective a, G1Affine b) => Add(a, G1Affine.Negate(b));
    /// <summary>Returns P − Q as P + (−Q).</summary>
    public static G1Projective Subtract(G1Affine a, G1Projective b) => Add(G1Affine.Negate(a), b);
    

    public override bool Equals(object? obj) => obj is G1Projective other && this == other;

    public override int GetHashCode() => ToAffine().GetHashCode();

    // 3B = 12 (B = 4 for G1 curve y² = x³ + 4)
    private static Fp MulBy3b(Fp a)
    {
        a = Fp.Add(a, a); // 2a
        a = Fp.Add(a, a); // 4a
        return Fp.Add(Fp.Add(a, a), a); // 12a
    }
}
