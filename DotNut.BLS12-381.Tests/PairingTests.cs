using DotNut.BLS12_381;
using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Pairing;
using DotNut.BLS12_381.Tower;
using Xunit;

namespace DotNut.BLS12_381.Tests;

public sealed class PairingTests
{
    [Fact]
    public void MillerLoop_WithInfinityInput_ShouldReturnOne()
    {
        var r1 = Bls12Pairing.MillerLoop(G1Affine.Infinity, G2Affine.Generator);
        var r2 = Bls12Pairing.MillerLoop(G1Affine.Generator, G2Affine.Infinity);
        Assert.True(Fp12.Equal(r1, Fp12.One));
        Assert.True(Fp12.Equal(r2, Fp12.One));
    }

    [Fact]
    public void Pair_WithInfinityInput_ShouldReturnOne()
    {
        var r1 = Bls12Pairing.Pair(G1Affine.Infinity, G2Affine.Generator);
        var r2 = Bls12Pairing.Pair(G1Affine.Generator, G2Affine.Infinity);
        Assert.True(Fp12.Equal(r1, Fp12.One));
        Assert.True(Fp12.Equal(r2, Fp12.One));
    }

    [Fact]
    public void MillerLoop_WithRegularPoints_ShouldReturnNonZero()
    {
        var f = Bls12Pairing.MillerLoop(G1Affine.Generator, G2Affine.Generator);
        Assert.False(Fp12.Equal(f, Fp12.Zero));
    }

    [Fact]
    public void Pairing_ShouldBeBilinear_ForSmallScalars()
    {
        var a = new System.Numerics.BigInteger(5);
        var b = new System.Numerics.BigInteger(7);

        var p = G1Projective.ScalarMultiply(G1Affine.Generator.ToProjective(), Scalar.FromBigInteger(a)).ToAffine();
        var q = G2Projective.ScalarMultiply(G2Affine.Generator.ToProjective(), Scalar.FromBigInteger(b)).ToAffine();

        var e1 = Bls12Pairing.Pair(p, q);
        var e = Bls12Pairing.Pair(G1Affine.Generator, G2Affine.Generator);
        var ab = new System.Numerics.BigInteger(35);
        var e2 = Fp12.Pow(e, ab);

        Assert.True(Fp12.Equal(e1, e2));
    }
}
