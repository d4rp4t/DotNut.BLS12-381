using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

public readonly struct G2Affine(Fp2 x, Fp2 y, bool isInfinity = false)
{
    public Fp2 X { get; } = x;
    public Fp2 Y { get; } = y;
    public bool IsInfinity { get; } = isInfinity;

    public static readonly G2Affine Infinity = new(Fp2.Zero, Fp2.Zero, true);

    // Source: RFC 9380, Appendix J.10 (BLS12-381 G2 generator)
    // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.10
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

    public bool IsOnCurve()
    {
        if (IsInfinity) return true;
        var lhs = Fp2.Square(Y);
        var rhs = Fp2.Add(Fp2.Multiply(Fp2.Square(X), X), CurveB);
        return Fp2.Equal(lhs, rhs);
    }

    public G2Projective ToProjective()
    {
        return IsInfinity ? G2Projective.Infinity : new G2Projective(X, Y, Fp2.One);
    }

    private static readonly Fp2 CurveB = new(
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One)),
        Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One))
    );

    private static Fp ParseFpHex(string hex)
    {
        if (hex.Length != 96) throw new ArgumentException("Fp hex must be 96 chars.", nameof(hex));
        return Fp.FromBytesBigEndian(Convert.FromHexString(hex));
    }
}
