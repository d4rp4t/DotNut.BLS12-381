using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

/// <summary>
/// Element of GT, the target group of the BLS12-381 pairing.
/// Written additively; internally Fp12 multiplication in the cyclotomic subgroup.
/// </summary>
public readonly struct Gt
{
    internal readonly Fp12 Value;

    internal Gt(Fp12 value) => Value = value;

    public static readonly Gt Identity = new(Fp12.One);

    // pairing(G1Affine.Generator, G2Affine.Generator) — hardcoded from zkcrypto/bls12_381
    public static readonly Gt Generator = new(new Fp12(
        new Fp6(
            new Fp2(
                new Fp(0x1972e433a01f85c5UL, 0x97d32b76fd772538UL, 0xc8ce546fc96bcdf9UL,
                       0xcef63e7366d40614UL, 0xa611342781843780UL, 0x13f3448a3fc6d825UL),
                new Fp(0xd26331b02e9d6995UL, 0x9d68a482f7797e7dUL, 0x9c9b29248d39ea92UL,
                       0xf4801ca2e13107aaUL, 0xa16c0732bdbcb066UL, 0x083ca4afba360478UL)
            ),
            new Fp2(
                new Fp(0x59e261db0916b641UL, 0x2716b6f4b23e960dUL, 0xc8e55b10a0bd9c45UL,
                       0x0bdb0bd99c4deda8UL, 0x8cf89ebf57fdaac5UL, 0x12d6b7929e777a5eUL),
                new Fp(0x5fc85188b0e15f35UL, 0x34a06e3a8f096365UL, 0xdb3126a6e02ad62cUL,
                       0xfc6f5aa97d9a990bUL, 0xa12f55f5eb89c210UL, 0x1723703a926f8889UL)
            ),
            new Fp2(
                new Fp(0x93588f2971828778UL, 0x43f65b8611ab7585UL, 0x3183aaf5ec279fdfUL,
                       0xfa73d7e18ac99df6UL, 0x64e176a6a64c99b0UL, 0x179fa78c58388f1fUL),
                new Fp(0x672a0a11ca2aef12UL, 0x0d11b9b52aa3f16bUL, 0xa44412d0699d056eUL,
                       0xc01d0177221a5ba5UL, 0x66e0cede6c735529UL, 0x05f5a71e9fddc339UL)
            )
        ),
        new Fp6(
            new Fp2(
                new Fp(0xd30a88a1b062c679UL, 0x5ac56a5d35fc8304UL, 0xd0c834a6a81f290dUL,
                       0xcd5430c2da3707c7UL, 0xf0c27ff780500af0UL, 0x09245da6e2d72eaeUL),
                new Fp(0x9f2e0676791b5156UL, 0xe2d1c8234918fe13UL, 0x4c9e459f3c561bf4UL,
                       0xa3e85e53b9d3e3c1UL, 0x820a121e21a70020UL, 0x15af618341c59accUL)
            ),
            new Fp2(
                new Fp(0x7c95658c24993ab1UL, 0x73eb38721ca886b9UL, 0x5256d749477434bcUL,
                       0x8ba41902ea504a8bUL, 0x04a3d3f80c86ce6dUL, 0x18a64a87fb686eaaUL),
                new Fp(0xbb83e71bb920cf26UL, 0x2a5277ac92a73945UL, 0xfc0ee59f94f046a0UL,
                       0x7158cdf3786058f7UL, 0x7cc1061b82f945f6UL, 0x03f847aa9fdbe567UL)
            ),
            new Fp2(
                new Fp(0x8078dba56134e657UL, 0x1cd7ec9a43998a6eUL, 0xb1aa599a1a993766UL,
                       0xc9a0f62f0842ee44UL, 0x8e159be3b605dffaUL, 0x0c86ba0d4af13fc2UL),
                new Fp(0xe80ff2a06a52ffb1UL, 0x7694ca48721a906cUL, 0x7583183e03b08514UL,
                       0xf567afdd40cee4e2UL, 0x9a6d96d2e526a5fcUL, 0x197e9f49861f2242UL)
            )
        )
    ));

    public Gt Double() => new(Fp12.Square(Value));

    public static Gt Negate(Gt a) => new(Fp12.Conjugate(a.Value));

    public static Gt Add(Gt a, Gt b) => new(Fp12.Multiply(a.Value, b.Value));

    public static Gt Subtract(Gt a, Gt b) => Add(a, Negate(b));

    public static Gt Multiply(Gt g, Scalar scalar)
    {
        // Double-and-add MSB-to-LSB, skipping the leading bit (always 0 for valid scalars < r)
        Span<byte> bytes = stackalloc byte[32];
        Scalar.ToBytesLittleEndian(scalar, bytes);

        var acc = Identity;
        var first = true;
        for (var i = 31; i >= 0; i--)
        {
            for (var b = 7; b >= 0; b--)
            {
                if (first) { first = false; continue; }
                var bit = ((bytes[i] >> b) & 1) != 0;
                acc = acc.Double();
                acc = ConditionalSelect(acc, Add(acc, g), bit);
            }
        }
        return acc;
    }

    public static bool Equal(Gt a, Gt b) => Fp12.Equal(a.Value, b.Value);

    public static Gt ConditionalSelect(Gt a, Gt b, bool chooseB) => chooseB ? b : a;
}
