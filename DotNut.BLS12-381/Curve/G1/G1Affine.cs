using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

public readonly struct G1Affine(Fp x, Fp y, bool isInfinity = false)
{
    public Fp X { get; } = x;
    public Fp Y { get; } = y;
    public bool IsInfinity { get; } = isInfinity;

    public static readonly G1Affine Infinity = new(Fp.Zero, Fp.Zero, true);

    // Source: IETF RFC 9380, Appendix J.9 (BLS12-381 G1 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.9
    public static readonly G1Affine Generator = new(
        ParseFpHex("17f1d3a73197d7942695638c4fa9ac0fc3688c4f9774b905a14e3a3f171bac586c55e83ff97a1aeffb3af00adb22c6bb"),
        ParseFpHex("08b3f481e3aaa0f1a09e30ed741d8ae4fcf5e095d5d00af600db18cb2c04b3edd03cc744a2888ae40caa232946c5e7e1")
    );

    public bool IsOnCurve()
    {
        if (IsInfinity) return true;

        var lhs = Fp.Square(Y);
        var four = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
        var rhs = Fp.Add(Fp.Multiply(Fp.Square(X), X), four);
        return Fp.Equal(lhs, rhs);
    }

    // verifies the point is in the G1 subgroup of order r.
    // IsOnCurve() alone is insufficient, points not in the subgroup enable small-subgroup attacks
    public bool IsInSubgroup()
    {
        if (!IsOnCurve()) return false;
        return G1Projective.ScalarMultiply(ToProjective(), Scalar.GroupOrderR).IsInfinity;
    }

    public G1Projective ToProjective()
    {
        return IsInfinity ? G1Projective.Infinity : new G1Projective(X, Y, Fp.One);
    }

    private static Fp ParseFpHex(string hex)
    {
        if (hex.Length != 96) throw new ArgumentException("Fp hex must be 96 chars.", nameof(hex));
        var bytes = Convert.FromHexString(hex);
        return Fp.FromBytesBigEndian(bytes);
    }
}
