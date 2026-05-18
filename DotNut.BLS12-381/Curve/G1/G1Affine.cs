using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

/// <summary>
/// A point on the BLS12-381 G1 curve in affine coordinates (X, Y) over Fp.
/// The curve equation is y² = x³ + 4.
/// The point at infinity is represented by <see cref="IsInfinity"/> = true with X = Y = 0.
/// </summary>
public readonly partial struct G1Affine(Fp x, Fp y, bool isInfinity = false)
{
    /// <summary>X coordinate of the point in Montgomery form.</summary>
    public Fp X { get; } = x;

    /// <summary>Y coordinate of the point in Montgomery form.</summary>
    public Fp Y { get; } = y;

    /// <summary>Returns <see langword="true"/> if this is the point at infinity (additive identity of G1).</summary>
    public bool IsInfinity { get; } = isInfinity;

    /// <summary>The point at infinity (additive identity).</summary>
    public static readonly G1Affine Infinity = new(Fp.Zero, Fp.One, true);

    // Source: IETF RFC 9380, Appendix J.9 (BLS12-381 G1 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.9
    /// <summary>
    /// The standard generator of G1, as defined in IETF RFC 9380, Appendix J.9.
    /// This point has prime order r and is in the correct G1 subgroup.
    /// </summary>
    public static readonly G1Affine Generator = new(
        Fp.ParseHex("17f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb"),
        Fp.ParseHex("08b3f481e3aaa0f1a09e30ed741d8ae4fcf5e095d5d00af600db18cb2c04b3edd03cc744a2888ae40caa232946c5e7e1")
    );

    /// <summary>
    /// Returns <see langword="true"/> if the point satisfies the G1 curve equation y² = x³ + 4.
    /// The point at infinity always passes this check.
    /// Does not verify subgroup membership; use <see cref="IsInSubgroup"/> for that.
    /// </summary>
    public bool IsOnCurve()
    {
        var lhs = Fp.Square(Y);
        var rhs = Fp.Add(Fp.Multiply(Fp.Square(X), X), CurveB);
        return Fp.Equal(lhs, rhs) | IsInfinity;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the point is in the G1 prime-order subgroup.
    /// Uses the fast endomorphism check: P ∈ G1 iff φ²(P) = −[x²]P,
    /// where φ(P) = (β·Px, Py) and x = BLS_X.
    /// Checks curve membership first.
    /// </summary>
    public bool IsInSubgroup()
    {
        if (!IsOnCurve()) return false;
        if (IsInfinity) return true;
        var p = ToProjective();
        var endoP = new G1Projective(Fp.Multiply(Beta, X), Y, Fp.One);
        var minusX2P = G1Projective.Negate(G1Projective.MulByBLSX(G1Projective.MulByBLSX(p)));
        return minusX2P == endoP;
    }

    /// <summary>
    /// Converts this affine point to homogeneous projective form without branching.
    /// Non-infinity: (X, Y, 1). Infinity (X=0, Y=1): (0, 1, 0) = G1Projective.Infinity.
    /// </summary>
    public G1Projective ToProjective() => new(X, Y, Fp.ConditionalSelect(Fp.One, Fp.Zero, IsInfinity));

    /// <summary>The non-trivial cube root of unity in Fp; β³ = 1, β ≠ 1.</summary>
    public static readonly Fp Beta = new([
        0x30f1_361b_798a_64e8,
        0xf3b8_ddab_7ece_5a2a,
        0x16a8_ca3a_c615_77f7,
        0xc26a_2ff8_74fd_029b,
        0x3636_b766_6070_1c6e,
        0x051b_a4ab_241b_6160,
    ]);

    public static bool operator ==(G1Affine a, G1Affine b)
    {
        if (a.IsInfinity && b.IsInfinity) return true;
        if (a.IsInfinity || b.IsInfinity) return false;
        return Fp.Equal(a.X, b.X) && Fp.Equal(a.Y, b.Y);
    }

    public static bool operator !=(G1Affine a, G1Affine b) => !(a == b);

    public override bool Equals(object? obj) => obj is G1Affine other && this == other;

    public override int GetHashCode() => HashCode.Combine(X, Y, IsInfinity);

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// </summary>
    public static G1Affine ConditionalSelect(G1Affine a, G1Affine b, bool choice) => new(
        Fp.ConditionalSelect(a.X, b.X, choice),
        Fp.ConditionalSelect(a.Y, b.Y, choice),
        (!choice & a.IsInfinity) | (choice & b.IsInfinity)
    );

    /// <summary>
    /// Returns −P: same X, negated Y (or Fp.One for the canonical infinity representation).
    /// </summary>
    public static G1Affine Negate(G1Affine p) => new(
        p.X,
        Fp.ConditionalSelect(Fp.Negate(p.Y), Fp.One, p.IsInfinity),
        p.IsInfinity
    );

    private static readonly Fp CurveB = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
}
