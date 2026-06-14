using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

/// <summary>
/// A point on the BLS12-381 G2 curve in affine coordinates (X, Y) over Fp2.
/// The curve equation is y² = x³ + 4·(1 + u) (where u² = −1 in Fp2).
/// The point at infinity is represented by <see cref="IsInfinity"/> = true with X = Y = 0.
/// </summary>
public readonly partial struct G2Affine(Fp2 x, Fp2 y, bool isInfinity = false)
{
    /// <summary>X coordinate of the point; each Fp component in Montgomery form.</summary>
    public Fp2 X { get; } = x;

    /// <summary>Y coordinate of the point; each Fp component in Montgomery form.</summary>
    public Fp2 Y { get; } = y;

    /// <summary>Returns <see langword="true"/> if this is the point at infinity (additive identity of G2).</summary>
    public bool IsInfinity { get; } = isInfinity;

    /// <summary>The point at infinity (additive identity).</summary>
    public static readonly G2Affine Infinity = new(Fp2.Zero, Fp2.One, true);

    // Source: RFC 9380, Appendix J.10 (BLS12-381 G2 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.10
    /// <summary>
    /// The standard generator of G2, as defined in IETF RFC 9380, Appendix J.10.
    /// This point has prime order r and is in the correct G2 subgroup.
    /// </summary>
    public static readonly G2Affine Generator = new(
        new Fp2(
            Fp.ParseHex("024aa2b2f08f0a91260805272dc51051c6e47ad4fa403b02b4510b647ae3d1770bac0326a805bbefd48056c8c121bdb8"),
            Fp.ParseHex("13e02b6052719f607dacd3a088274f65596bd0d09920b61ab5da61bbdc7f5049334cf11213945d57e5ac7d055d042b7e")
        ),
        new Fp2(
            Fp.ParseHex("0ce5d527727d6e118cc9cdc6da2e351aadfd9baa8cbdd3a76d429a695160d12c923ac9cc3baca289e193548608b82801"),
            Fp.ParseHex("0606c4a02ea734cc32acd2b02bc28b99cb3e287e85a763af267492ab572e99ab3f370d275cec1da1aaa9075ff05f79be")
        )
    );

    /// <summary>
    /// Returns <see langword="true"/> if the point satisfies the G2 curve equation y² = x³ + 4·(1 + u).
    /// The point at infinity always passes this check.
    /// Does not verify subgroup membership; use <see cref="IsInSubgroup"/> for that.
    /// </summary>
    public bool IsOnCurve()
    {
        var lhs = Fp2.Square(Y);
        var rhs = Fp2.Add(Fp2.Multiply(Fp2.Square(X), X), CurveB);
        return Fp2.Equal(lhs, rhs) | IsInfinity;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the point is in the G2 prime-order subgroup.
    /// Uses the fast check: P ∈ G2 iff ψ(P) + [BLS_X]P = O,
    /// which replaces the slow 255-bit [r]P = O verification.
    /// Checks curve membership first.
    /// </summary>
    public bool IsInSubgroup()
    {
        if (!IsOnCurve()) return false;
        var p = ToProjective();
        return G2Projective.Add(G2Projective.Psi(p), G2Projective.MulByBLSX(p)).IsInfinity;
    }

    /// <summary>
    /// Converts this affine point to homogeneous projective form without branching.
    /// Non-infinity: (X, Y, 1). Infinity: (0, 1, 0) = G2Projective.Infinity.
    /// X and Y are forced to their canonical values (0 and 1) when IsInfinity is true
    /// so that non-canonical affine infinities (arbitrary X, Y, IsInfinity=true) do
    /// not corrupt the projective addition algorithm which requires exactly (0:1:0).
    /// </summary>
    public G2Projective ToProjective() => new(
        Fp2.ConditionalSelect(X, Fp2.Zero, IsInfinity),
        Fp2.ConditionalSelect(Y, Fp2.One,  IsInfinity),
        Fp2.ConditionalSelect(Fp2.One, Fp2.Zero, IsInfinity));

    /// <summary>The G2 curve constant B = 4 + 4u (coefficient of the constant term in y² = x³ + B).</summary>
    private static readonly Fp2 CurveB = new(
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One)),
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One))
    );

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// </summary>
    public static G2Affine ConditionalSelect(G2Affine a, G2Affine b, bool choice) => new(
        Fp2.ConditionalSelect(a.X, b.X, choice),
        Fp2.ConditionalSelect(a.Y, b.Y, choice),
        (!choice & a.IsInfinity) | (choice & b.IsInfinity)
    );

    /// <summary>
    /// Returns −P: same X, negated Y (or Fp2.One for the canonical infinity representation).
    /// </summary>
    public static G2Affine Negate(G2Affine p) => new(
        p.X,
        Fp2.ConditionalSelect(Fp2.Negate(p.Y), Fp2.One, p.IsInfinity),
        p.IsInfinity
    );

    public override bool Equals(object? obj) => obj is G2Affine other && this == other;

    public override int GetHashCode() => IsInfinity ? HashCode.Combine(true) : HashCode.Combine(X, Y);
}
