using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

/// <summary>
/// A point on the BLS12-381 G1 curve in projective (Jacobian) coordinates (X : Y : Z) over Fp.
/// The affine point (x, y) is represented as (X = x·Z², Y = y·Z³, Z ≠ 0).
/// The point at infinity is represented by Z = 0.
/// </summary>
public readonly struct G1Projective(Fp x, Fp y, Fp z)
{
    /// <summary>Projective X coordinate in Montgomery form.</summary>
    public Fp X { get; } = x;

    /// <summary>Projective Y coordinate in Montgomery form.</summary>
    public Fp Y { get; } = y;

    /// <summary>Projective Z coordinate in Montgomery form.</summary>
    public Fp Z { get; } = z;

    /// <summary>The point at infinity (Z = 0).</summary>
    public static readonly G1Projective Infinity = new(Fp.Zero, Fp.One, Fp.Zero);

    /// <summary>Returns <see langword="true"/> if Z = 0 (this is the point at infinity).</summary>
    public bool IsInfinity => Fp.Equal(Z, Fp.Zero);

    /// <summary>
    /// Returns P + Q using the EFD add-2007-bl formula for short Weierstrass curves in Jacobian coordinates.
    /// Handles the infinity case explicitly; degenerate cases (P = Q or P = −Q) are handled via <see cref="Double"/>.
    /// Both inputs must have all coordinates in Montgomery form.
    /// </summary>
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

    /// <summary>
    /// Returns 2·P using the EFD dbl-2009-l formula for a = 0 Weierstrass curves in Jacobian coordinates.
    /// Returns infinity if P is the point at infinity or if Y = 0 (a point of order 2, impossible in G1
    /// since r is prime and r > 2, but guarded defensively).
    /// Input must have all coordinates in Montgomery form.
    /// </summary>
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

    /// <summary>
    /// Computes [k]P using the Montgomery ladder scalar multiplication algorithm.
    /// Processes 255 bits of the scalar (bits 254 down to 0); the scalar must fit in 255 bits
    /// (all scalars in [0, r) satisfy this since r &lt; 2^255).
    /// The algorithm is constant-time with respect to the scalar.
    /// </summary>
    /// <param name="p">The base point. Must have coordinates in Montgomery form.</param>
    /// <param name="k">The scalar multiplier (in Montgomery form internally).</param>
    /// <returns>[k]P in projective coordinates.</returns>
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

    /// <summary>
    /// Branchless conditional swap of two projective points.
    /// When <paramref name="bit"/> = 1, swaps a and b; when 0, leaves them unchanged.
    /// Operates on raw limbs without field arithmetic.
    /// </summary>
    private static (G1Projective, G1Projective) CtSwap(ulong bit, G1Projective a, G1Projective b)
    {
        ulong mask = 0UL - bit;
        return (
            new G1Projective(CtSelect(mask, b.X, a.X), CtSelect(mask, b.Y, a.Y), CtSelect(mask, b.Z, a.Z)),
            new G1Projective(CtSelect(mask, a.X, b.X), CtSelect(mask, a.Y, b.Y), CtSelect(mask, a.Z, b.Z))
        );
    }

    /// <summary>
    /// Branchless conditional select of an Fp element.
    /// Returns x if mask is all-ones (0xFFFF...), y if mask is zero.
    /// Does not perform Montgomery reduction.
    /// </summary>
    private static Fp CtSelect(ulong mask, Fp x, Fp y) => new(
        (x.L0 & mask) | (y.L0 & ~mask),
        (x.L1 & mask) | (y.L1 & ~mask),
        (x.L2 & mask) | (y.L2 & ~mask),
        (x.L3 & mask) | (y.L3 & ~mask),
        (x.L4 & mask) | (y.L4 & ~mask),
        (x.L5 & mask) | (y.L5 & ~mask)
    );

    /// <summary>
    /// Converts this projective point to affine form by computing (X/Z², Y/Z³).
    /// Returns <see cref="G1Affine.Infinity"/> if this is the point at infinity.
    /// </summary>
    public G1Affine ToAffine()
    {
        if (IsInfinity) return G1Affine.Infinity;
        var zInv = Fp.Invert(Z);
        var z2 = Fp.Square(zInv);
        var z3 = Fp.Multiply(z2, zInv);
        return new G1Affine(Fp.Multiply(X, z2), Fp.Multiply(Y, z3));
    }

    /// <summary>Converts to affine and checks the curve equation. See <see cref="G1Affine.IsOnCurve"/>.</summary>
    public bool IsOnCurve() => ToAffine().IsOnCurve();

    /// <summary>
    /// Clears the E1(Fp) cofactor h1 = 1 + BLS_X so that the result is in the G1 prime-order subgroup.
    /// Uses h1 = 1 + BLS_X = 0xd201000000010001; computes [h1]P = P + [BLS_X]P.
    /// </summary>
    public G1Projective ClearCofactor() => Add(this, MulByBLSX(this));

    /// <summary>
    /// Computes [BLS_X]P via left-to-right binary double-and-add.
    /// BLS_X = 0xd201000000010000 (6 set bits; 64-bit scalar).
    /// Used by <see cref="ClearCofactor"/> and the G2 subgroup check.
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

    /// <summary>Returns 2·v as an Fp element via addition (no Montgomery overhead vs. a literal 2×).</summary>
    private static Fp DoubleFp(Fp v) => Fp.Add(v, v);

    /// <summary>Returns 3·v as an Fp element.</summary>
    private static Fp TripleFp(Fp v) => Fp.Add(v, DoubleFp(v));

    /// <summary>Returns 8·v as an Fp element via three doublings.</summary>
    private static Fp EightFp(Fp v) => DoubleFp(DoubleFp(DoubleFp(v)));
}
