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

    [Fact]
    public void Generator_ShouldBeOnCurve()
    {
        Assert.True(G1Affine.Generator.IsOnCurve());
    }

    [Fact]
    public void Infinity_Roundtrip_ProjectiveAffine()
    {
        var inf = G1Projective.Infinity.ToAffine();
        Assert.True(inf.IsInfinity);
    }

    [Fact]
    public void Double_ShouldMatch_AddSelf()
    {
        var g = G1Affine.Generator.ToProjective();
        var d = G1Projective.Double(g).ToAffine();
        var a = G1Projective.Add(g, g).ToAffine();
        Assert.True(AffineEqual(d, a));
    }

    [Fact]
    public void ScalarMul_SmallValues_ShouldBeConsistent()
    {
        var g = G1Affine.Generator.ToProjective();
        var one = G1Projective.ScalarMultiply(g, BigInteger.One).ToAffine();
        var two = G1Projective.ScalarMultiply(g, new BigInteger(2)).ToAffine();
        var three = G1Projective.ScalarMultiply(g, new BigInteger(3)).ToAffine();

        Assert.True(AffineEqual(one, G1Affine.Generator));
        Assert.True(AffineEqual(two, G1Projective.Double(g).ToAffine()));
        Assert.True(AffineEqual(three, G1Projective.Add(g, G1Projective.Double(g)).ToAffine()));
    }

    [Fact]
    public void ScalarMul_Zero_ShouldReturn_Infinity()
    {
        var g = G1Affine.Generator.ToProjective();
        var zero = G1Projective.ScalarMultiply(g, BigInteger.Zero).ToAffine();
        Assert.True(zero.IsInfinity);
    }

    [Fact]
    public void AffineProjective_Roundtrip_ShouldPreservePoint()
    {
        var g = G1Affine.Generator;
        var back = g.ToProjective().ToAffine();
        Assert.True(AffineEqual(g, back));
    }

    [Fact]
    public void Infinity_ShouldBehaveAsNeutralElement()
    {
        var g = G1Affine.Generator.ToProjective();
        Assert.True(AffineEqual(G1Projective.Add(g, G1Projective.Infinity).ToAffine(), g.ToAffine()));
        Assert.True(AffineEqual(G1Projective.Add(G1Projective.Infinity, g).ToAffine(), g.ToAffine()));
    }

    [Fact]
    public void Results_ShouldStayOnCurve()
    {
        var g = G1Affine.Generator.ToProjective();
        var p = g;
        for (var i = 0; i < 20; i++)
        {
            p = G1Projective.Add(p, g);
            Assert.True(p.IsOnCurve());
        }
    }

    [Fact]
    public void Jacobian_Add_And_Double_ShouldMatch_AffineReference()
    {
        var random = new Random(9001);
        var g = G1Affine.Generator;

        for (var i = 0; i < 40; i++)
        {
            var k1 = random.Next(0, 1 << 16);
            var k2 = random.Next(0, 1 << 16);

            var pJac = G1Projective.ScalarMultiply(g.ToProjective(), new BigInteger(k1));
            var qJac = G1Projective.ScalarMultiply(g.ToProjective(), new BigInteger(k2));
            var jacAdd = G1Projective.Add(pJac, qJac).ToAffine();
            var jacDbl = G1Projective.Double(pJac).ToAffine();

            var pAff = AffineScalarMultiply(g, k1);
            var qAff = AffineScalarMultiply(g, k2);
            var affAdd = AffineAdd(pAff, qAff);
            var affDbl = AffineDouble(pAff);

            Assert.True(AffineEqual(jacAdd, affAdd));
            Assert.True(AffineEqual(jacDbl, affDbl));
        }
    }

    [Fact]
    public void ExternalVector_Eip2537_G1Add_G1PlusG1_ShouldMatch()
    {
        // Source:
        // https://eips.ethereum.org/assets/eip-2537/add_G1_bls.json
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
        // Source:
        // https://eips.ethereum.org/assets/eip-2537/mul_G1_bls.json
        var g = G1Affine.Generator.ToProjective();
        var scalar = BigInteger.Parse("263dbd792f5b1be47ed85f8938c0f29586af0d3ac7b977f21c278fe1462040e3", System.Globalization.NumberStyles.AllowHexSpecifier);
        var result = G1Projective.ScalarMultiply(g, scalar).ToAffine();
        var expected = ParseG1FromEipOutput(
            "000000000000000000000000000000000491d1b0ecd9bb917989f0e74f0dea0422eac4a873e5e2644f368dffb9a6e20fd6e10c1b77654d067c0618f6e5a7f79a" +
            "0000000000000000000000000000000017cd7061575d3e8034fcea62adaa1a3bc38dca4b50e4c5c01d04dd78037c9cee914e17944ea99e7ad84278e5d49f36c4"
        );
        Assert.True(AffineEqual(result, expected));
    }

    [Fact]
    public void SubgroupCheck_RTimesGenerator_ShouldBeInfinity()
    {
        // Source for subgroup order r:
        // https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.4
        var g = G1Affine.Generator.ToProjective();
        var result = G1Projective.ScalarMultiply(g, GroupOrderR).ToAffine();
        Assert.True(result.IsInfinity);
    }

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

    private static G1Affine AffineAdd(G1Affine a, G1Affine b)
    {
        if (a.IsInfinity) return b;
        if (b.IsInfinity) return a;

        if (Fp.Equal(a.X, b.X))
            return Fp.Equal(a.Y, b.Y) ? AffineDouble(a) : G1Affine.Infinity;

        var lambda = Fp.Multiply(Fp.Subtract(b.Y, a.Y), Fp.Invert(Fp.Subtract(b.X, a.X)));
        var x3 = Fp.Subtract(Fp.Subtract(Fp.Square(lambda), a.X), b.X);
        var y3 = Fp.Subtract(Fp.Multiply(lambda, Fp.Subtract(a.X, x3)), a.Y);
        return new G1Affine(x3, y3);
    }

    private static G1Affine AffineDouble(G1Affine a)
    {
        if (a.IsInfinity || Fp.Equal(a.Y, Fp.Zero)) return G1Affine.Infinity;
        var x2 = Fp.Square(a.X);
        var threeX2 = Fp.Add(x2, Fp.Add(x2, x2));
        var twoY = Fp.Add(a.Y, a.Y);
        var lambda = Fp.Multiply(threeX2, Fp.Invert(twoY));
        var x3 = Fp.Subtract(Fp.Square(lambda), Fp.Add(a.X, a.X));
        var y3 = Fp.Subtract(Fp.Multiply(lambda, Fp.Subtract(a.X, x3)), a.Y);
        return new G1Affine(x3, y3);
    }

    private static G1Affine AffineScalarMultiply(G1Affine p, int k)
    {
        var result = G1Affine.Infinity;
        var cur = p;
        var e = k;
        while (e > 0)
        {
            if ((e & 1) != 0) result = AffineAdd(result, cur);
            cur = AffineDouble(cur);
            e >>= 1;
        }
        return result;
    }
}
