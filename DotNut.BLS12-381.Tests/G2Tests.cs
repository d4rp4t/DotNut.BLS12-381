using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;
using System.Numerics;

namespace DotNut.BLS12_381.Tests;

public sealed class G2Tests
{
    private static readonly BigInteger GroupOrderR = BigInteger.Parse(
        "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
        System.Globalization.NumberStyles.AllowHexSpecifier
    );

    #region ExternalVectors

    [Fact]
    public void ExternalVector_Eip2537_G2Add_G2PlusG2_ShouldMatch()
    {
        // Source: https://eips.ethereum.org/assets/eip-2537/add_G2_bls.json
        var g = G2Affine.Generator.ToProjective();
        var result = G2Projective.Add(g, g).ToAffine();
        var expected = ParseG2FromEipOutput(
            "000000000000000000000000000000001638533957d540a9d2370f17cc7ed5863bc0b995b8825e0ee1ea1e1e4d00dbae81f14b0bf3611b78c952aacab827a053" +
            "000000000000000000000000000000000a4edef9c1ed7f729f520e47730a124fd70662a904ba1074728114d1031e1572c6c886f6b57ec72a6178288c47c33577" +
            "000000000000000000000000000000000468fb440d82b0630aeb8dca2b5256789a66da69bf91009cbfe6bd221e47aa8ae88dece9764bf3bd999d95d71e4c9899" +
            "000000000000000000000000000000000f6d4552fa65dd2638b361543f887136a43253d9c66c411697003f7a13c308f5422e1aa0a59c8967acdefd8b6e36ccf3"
        );
        Assert.True(AffineEqual(result, expected));
    }

    [Fact]
    public void ExternalVector_Eip2537_G2Mul_RandomScalar_ShouldMatch()
    {
        // Source: https://eips.ethereum.org/assets/eip-2537/mul_G2_bls.json
        var g = G2Affine.Generator.ToProjective();
        var scalar = BigInteger.Parse("263dbd792f5b1be47ed85f8938c0f29586af0d3ac7b977f21c278fe1462040e3", System.Globalization.NumberStyles.AllowHexSpecifier);
        var result = G2Projective.ScalarMultiply(g, Scalar.FromBigInteger(scalar)).ToAffine();
        var expected = ParseG2FromEipOutput(
            "0000000000000000000000000000000014856c22d8cdb2967c720e963eedc999e738373b14172f06fc915769d3cc5ab7ae0a1b9c38f48b5585fb09d4bd2733bb" +
            "000000000000000000000000000000000c400b70f6f8cd35648f5c126cce5417f3be4d8eefbd42ceb4286a14df7e03135313fe5845e3a575faab3e8b949d2488" +
            "00000000000000000000000000000000149a0aacc34beba2beb2f2a19a440166e76e373194714f108e4ab1c3fd331e80f4e73e6b9ea65fe3ec96d7136de81544" +
            "000000000000000000000000000000000e4622fef26bdb9b1e8ef6591a7cc99f5b73164500c1ee224b6a761e676b8799b09a3fd4fa7e242645cc1a34708285e4"
        );
        Assert.True(AffineEqual(result, expected));
    }

    [Fact]
    public void SubgroupCheck_RTimesGenerator_ShouldBeInfinity()
    {
        var g = G2Affine.Generator.ToProjective();
        var result = G2Projective.ScalarMultiply(g, Scalar.FromBigInteger(GroupOrderR)).ToAffine();
        Assert.True(result.IsInfinity);
    }

    #endregion

    #region Helpers

    private static bool AffineEqual(G2Affine a, G2Affine b)
    {
        if (a.IsInfinity || b.IsInfinity) return a.IsInfinity == b.IsInfinity;
        return Fp2.Equal(a.X, b.X) && Fp2.Equal(a.Y, b.Y);
    }

    private static G2Affine ParseG2FromEipOutput(string hex)
    {
        if (hex.Length != 512) throw new ArgumentException("Expected 256-byte (512 hex chars) output.", nameof(hex));
        if (hex.AsSpan().Trim('0').Length == 0) return G2Affine.Infinity;

        var x0 = Fp.FromBytesBigEndian(Convert.FromHexString(hex.Substring(32, 96)));
        var x1 = Fp.FromBytesBigEndian(Convert.FromHexString(hex.Substring(160, 96)));
        var y0 = Fp.FromBytesBigEndian(Convert.FromHexString(hex.Substring(288, 96)));
        var y1 = Fp.FromBytesBigEndian(Convert.FromHexString(hex.Substring(416, 96)));
        return new G2Affine(new Fp2(x0, x1), new Fp2(y0, y1));
    }

    #endregion
}
