using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Tower;
using System.Numerics;

namespace DotNut.BLS12_381.Tests;

public sealed class G1Tests
{
    private static readonly BigInteger GroupOrderR = BigInteger.Parse(
        "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
        System.Globalization.NumberStyles.AllowHexSpecifier
    );

    #region ExternalVectors

    [Fact]
    public void ExternalVector_Eip2537_G1Add_G1PlusG1_ShouldMatch()
    {
        // Source: https://eips.ethereum.org/assets/eip-2537/add_G1_bls.json
        var g = G1Affine.Generator.ToProjective();
        var result = G1Projective.Add(g, g).ToAffine();
        var expected = ParseG1FromEipOutput(
            "000000000000000000000000000000000572cbea904d67468808c8eb50a9450c9721db309128012543902d0ac358a62ae28f75bb8f1c7c42c39a8c5529bf0f4e" +
            "00000000000000000000000000000000166a9d8cabc673a322fda673779d8e3822ba3ecb8670e461f73bb9021d5fd76a4c56d9d4cd16bd1bba86881979749d28"
        );
        Assert.True(AffineEqual(result, expected));
    }

    [Fact]
    public void ExternalVector_Eip2537_G1Mul_RandomScalar_ShouldMatch()
    {
        // Source: https://eips.ethereum.org/assets/eip-2537/mul_G1_bls.json
        var g = G1Affine.Generator.ToProjective();
        var scalar = BigInteger.Parse("263dbd792f5b1be47ed85f8938c0f29586af0d3ac7b977f21c278fe1462040e3", System.Globalization.NumberStyles.AllowHexSpecifier);
        var result = G1Projective.ScalarMultiply(g, Scalar.FromBigInteger(scalar)).ToAffine();
        var expected = ParseG1FromEipOutput(
            "000000000000000000000000000000000491d1b0ecd9bb917989f0e74f0dea0422eac4a873e5e2644f368dffb9a6e20fd6e10c1b77654d067c0618f6e5a7f79a" +
            "0000000000000000000000000000000017cd7061575d3e8034fcea62adaa1a3bc38dca4b50e4c5c01d04dd78037c9cee914e17944ea99e7ad84278e5d49f36c4"
        );
        Assert.True(AffineEqual(result, expected));
    }

    [Fact]
    public void SubgroupCheck_RTimesGenerator_ShouldBeInfinity()
    {
        var g = G1Affine.Generator.ToProjective();
        var result = G1Projective.ScalarMultiply(g, Scalar.FromBigInteger(GroupOrderR)).ToAffine();
        Assert.True(result.IsInfinity);
    }

    #endregion

    #region Non-canonical infinity

    [Fact]
    public void NonCanonicalInfinity_PreservesAdditiveIdentityInProjectiveAdd()
    {
        // G1Affine(X, Y, isInfinity=true) with non-zero X/Y must still act as O
        // in projective addition; ToProjective must emit canonical (0:1:0).
        var malformed = new G1Affine(G1Affine.Generator.X, G1Affine.Generator.Y, isInfinity: true);
        var g = G1Projective.Generator;
        Assert.True(G1Projective.Add(malformed.ToProjective(), g) == g, "O + G should equal G");
        Assert.True(G1Projective.Add(g, malformed.ToProjective()) == g, "G + O should equal G");
    }

    #endregion

    #region Operator regression

    [Fact]
    public void OperatorMinus_AffineMinusProjective_ReturnsAMinusB()
    {
        // G - 2G should equal -G, not +G
        var gAff = G1Affine.Generator;
        var gProj = gAff.ToProjective();
        var twoG = G1Projective.Add(gProj, gProj);
        var result = gAff - twoG;
        var expected = G1Projective.Negate(gProj);
        Assert.True(result == expected);
    }

    #endregion

    #region Helpers

    private static bool AffineEqual(G1Affine a, G1Affine b)
    {
        if (a.IsInfinity || b.IsInfinity) return a.IsInfinity == b.IsInfinity;
        return Fp.Equal(a.X, b.X) && Fp.Equal(a.Y, b.Y);
    }

    private static G1Affine ParseG1FromEipOutput(string hex)
    {
        if (hex.Length != 256) throw new ArgumentException("Expected 128-byte (256 hex chars) output.", nameof(hex));
        if (hex.AsSpan().Trim('0').Length == 0) return G1Affine.Infinity;

        var xPadded = hex[..128];
        var yPadded = hex[128..];
        var x = Fp.FromBytesBigEndian(Convert.FromHexString(xPadded[32..]));
        var y = Fp.FromBytesBigEndian(Convert.FromHexString(yPadded[32..]));
        return new G1Affine(x, y);
    }

    #endregion
}
