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
    public static readonly G1Affine Infinity = new(Fp.Zero, Fp.Zero, true);

    // Source: IETF RFC 9380, Appendix J.9 (BLS12-381 G1 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.9
    /// <summary>
    /// The standard generator of G1, as defined in IETF RFC 9380, Appendix J.9.
    /// This point has prime order r and is in the correct G1 subgroup.
    /// </summary>
    public static readonly G1Affine Generator = new(
        ParseFpHex("17f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb"),
        ParseFpHex("08b3f481e3aaa0f1a09e30ed741d8ae4fcf5e095d5d00af600db18cb2c04b3edd03cc744a2888ae40caa232946c5e7e1")
    );

    /// <summary>
    /// Returns <see langword="true"/> if the point satisfies the G1 curve equation y² = x³ + 4.
    /// The point at infinity always passes this check.
    /// Does not verify subgroup membership; use <see cref="IsInSubgroup"/> for that.
    /// </summary>
    public bool IsOnCurve()
    {
        if (IsInfinity) return true;

        var lhs = Fp.Square(Y);
        var four = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
        var rhs = Fp.Add(Fp.Multiply(Fp.Square(X), X), four);
        return Fp.Equal(lhs, rhs);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the point is in the G1 subgroup of prime order r.
    /// Checks curve membership first, then verifies [r]P = O via scalar multiplication.
    /// This is slower than the fast G2 subgroup check; for G1 the slow check is unavoidable.
    /// Call <see cref="IsOnCurve"/> alone only when subgroup attacks are not a concern.
    /// </summary>
    public bool IsInSubgroup()
    {
        if (!IsOnCurve()) return false;
        return G1Projective.ScalarMultiply(ToProjective(), Scalar.GroupOrderR).IsInfinity;
    }

    /// <summary>
    /// Converts this affine point to projective (Jacobian) form.
    /// The infinity point maps to <see cref="G1Projective.Infinity"/>; otherwise sets Z = 1.
    /// </summary>
    public G1Projective ToProjective()
    {
        return IsInfinity ? G1Projective.Infinity : new G1Projective(X, Y, Fp.One);
    }

    /// <summary>
    /// Parses a 48-byte (96 hex-char) big-endian Fp value from hex for use in the generator constant.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <paramref name="hex"/> is not exactly 96 characters.</exception>
    private static Fp ParseFpHex(string hex)
    {
        if (hex.Length != 96) throw new ArgumentException("Fp hex must be 96 chars.", nameof(hex));
        var bytes = Convert.FromHexString(hex);
        return Fp.FromBytesBigEndian(bytes);
    }
}
