using System.Numerics;
using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Pairing;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public sealed class SecurityFixTests
{
    private static readonly BigInteger R = BigInteger.Parse(
        "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
        System.Globalization.NumberStyles.AllowHexSpecifier);

    #region CmpLimb — fix 1

    [Fact]
    public void ScalarCompare_LimbDiffExceedsHalfUlong_CorrectlyOrdersAscending()
    {
        // L0 of a = 0xffffffff00000001, L0 of b = 1 — same upper limbs, difference in L0
        // is 0xffffffff00000000 > 2^63. The old (b-a)>>63 trick would flip the ordering.
        var a = Scalar.FromBigInteger(0xffffffff00000001UL);
        var b = Scalar.FromBigInteger(1UL);
        Assert.True(Scalar.Compare(a, b) > 0);
        Assert.True(Scalar.Compare(b, a) < 0);
        Assert.Equal(0, Scalar.Compare(a, a));
    }

    #endregion

    #region MultiMillerLoop / G2Prepared — fix 2

    [Fact]
    public void MultiMillerLoop_OffCurveG1_ThrowsArgumentException()
    {
        var badG1 = new G1Affine(Fp.One, Fp.One); // not on curve → not in subgroup
        var q = G2Prepared.From(G2Affine.Generator);
        Assert.Throws<ArgumentException>(() =>
            Bls12Pairing.MultiMillerLoop([(badG1, q)]));
    }

    [Fact]
    public void MultiMillerLoop_ValidG1_DoesNotThrow()
    {
        var p = G1Affine.Generator;
        var q = G2Prepared.From(G2Affine.Generator);
        var _ = Bls12Pairing.MultiMillerLoop([(p, q)]);
    }

    [Fact]
    public void G2Prepared_From_OffCurveG2_ThrowsArgumentException()
    {
        var badG2 = new G2Affine(Fp2.One, Fp2.One); // not on curve → not in subgroup
        Assert.Throws<ArgumentException>(() => G2Prepared.From(badG2));
    }

    [Fact]
    public void G2Prepared_From_InfinityG2_DoesNotThrow()
    {
        var _ = G2Prepared.From(G2Affine.Infinity);
    }

    #endregion

    #region Scalar.FromBigInteger — fix 3

    [Fact]
    public void FromBigInteger_InputOver256Bits_ReducesCorrectly()
    {
        // 2^256 requires 33 bytes; TryWriteBytes used to silently return zero.
        var huge = BigInteger.One << 256;       // = 2^256, fits in 33 bytes only
        var expected = Scalar.FromBigInteger(huge % R); // huge % R < R, safe recursion-free
        var result = Scalar.FromBigInteger(huge);
        Assert.True(Scalar.Equal(result, expected));
        Assert.False(Scalar.IsZero(result));    // 2^256 mod r ≠ 0
    }

    [Fact]
    public void FromBigInteger_VeryLargeInput_ReducesCorrectly()
    {
        var huge = (BigInteger.One << 512) + 42;
        var expected = Scalar.FromBigInteger(huge % R);
        var result = Scalar.FromBigInteger(huge);
        Assert.True(Scalar.Equal(result, expected));
    }

    [Fact]
    public void FromBigInteger_NegativeInput_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Scalar.FromBigInteger(BigInteger.MinusOne));
    }

    #endregion

    #region HashToCurve empty DST — fix 4

    [Fact]
    public void HashToG1_EmptyDst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            HashToCurve.HashToCurve.HashToG1("msg"u8, []));
    }

    [Fact]
    public void EncodeToG1_EmptyDst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            HashToCurve.HashToCurve.EncodeToG1("msg"u8, []));
    }

    [Fact]
    public void HashToG2_EmptyDst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            HashToCurve.HashToCurve.HashToG2("msg"u8, []));
    }

    [Fact]
    public void EncodeToG2_EmptyDst_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            HashToCurve.HashToCurve.EncodeToG2("msg"u8, []));
    }

    #endregion
}
