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
    public static readonly G2Affine Infinity = new(Fp2.Zero, Fp2.Zero, true);

    // Source: RFC 9380, Appendix J.10 (BLS12-381 G2 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.10
    /// <summary>
    /// The standard generator of G2, as defined in IETF RFC 9380, Appendix J.10.
    /// This point has prime order r and is in the correct G2 subgroup.
    /// </summary>
    public static readonly G2Affine Generator = new(
        new Fp2(
            ParseFpHex("024aa2b2f08f0a91260805272dc51051c6e47ad4fa403b02b4510b647ae3d1770bac0326a805bbefd48056c8c121bdb8"),
            ParseFpHex("13e02b6052719f607dacd3a088274f65596bd0d09920b61ab5da61bbdc7f5049334cf11213945d57e5ac7d055d042b7e")
        ),
        new Fp2(
            ParseFpHex("0ce5d527727d6e118cc9cdc6da2e351aadfd9baa8cbdd3a76d429a695160d12c923ac9cc3baca289e193548608b82801"),
            ParseFpHex("0606c4a02ea734cc32acd2b02bc28b99cb3e287e85a763af267492ab572e99ab3f370d275cec1da1aaa9075ff05f79be")
        )
    );

    /// <summary>
    /// Returns <see langword="true"/> if the point satisfies the G2 curve equation y² = x³ + 4·(1 + u).
    /// The point at infinity always passes this check.
    /// Does not verify subgroup membership; use <see cref="IsInSubgroup"/> for that.
    /// </summary>
    public bool IsOnCurve()
    {
        if (IsInfinity) return true;
        var lhs = Fp2.Square(Y);
        var rhs = Fp2.Add(Fp2.Multiply(Fp2.Square(X), X), CurveB);
        return Fp2.Equal(lhs, rhs);
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
    /// Converts this affine point to projective (Jacobian) form.
    /// The infinity point maps to <see cref="G2Projective.Infinity"/>; otherwise sets Z = 1.
    /// </summary>
    public G2Projective ToProjective()
    {
        return IsInfinity ? G2Projective.Infinity : new G2Projective(X, Y, Fp2.One);
    }

    /// <summary>The G2 curve constant B = 4 + 4u (coefficient of the constant term in y² = x³ + B).</summary>
    private static readonly Fp2 CurveB = new(
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One)),
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One))
    );

    /// <summary>
    /// Parses a 48-byte (96 hex-char) big-endian Fp value from hex for use in the generator constant.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <paramref name="hex"/> is not exactly 96 characters.</exception>
    private static Fp ParseFpHex(string hex)
    {
        if (hex.Length != 96) throw new ArgumentException("Fp hex must be 96 chars.", nameof(hex));
        return Fp.FromBytesBigEndian(Convert.FromHexString(hex));
    }
}
