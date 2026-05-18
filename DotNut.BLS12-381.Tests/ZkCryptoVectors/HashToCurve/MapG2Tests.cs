using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.HashToCurve;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors.HashToCurve;

public class MapG2Tests
{
    private static Fp P_M1_OVER2 => TestConstants.P_M1_OVER2;

    bool check_g2_prime(G2Projective pt)
    {
        // (X : Y : Z)==(X/Z, Y/Z) is on E': y^2 = x^3 + A * x + B.
        // y^2 z = (x^3) + A (x z^2) + B z^3

        //z^2
        var zsq = Fp2.Square(pt.Z);

        //y^2 * z
        Fp2 y2z = Fp2.Multiply(Fp2.Square(pt.Y), pt.Z);

        //x^3
        Fp2 x3 = Fp2.Multiply(Fp2.Square(pt.X), pt.X);

        //A * x * z^2
        Fp2 z2Ax = Fp2.Multiply(Fp2.Multiply(HashToCurveMapper.G2_SSWU_ELLP_A, pt.X), zsq);

        //B * z^3)
        Fp2 z3B = Fp2.Multiply(Fp2.Multiply(HashToCurveMapper.G2_SSWU_ELLP_B, zsq), pt.Z);

        return y2z == Fp2.Add(Fp2.Add(x3, z2Ax), z3B);
    }

    [Fact]
    public void test_osswu_semirandom()
    {
        byte[] seed =
        [
            0x59,
            0x62,
            0xbe,
            0x5d,
            0x76,
            0x3d,
            0x31,
            0x8d,
            0x17,
            0xdb,
            0x37,
            0x32,
            0x54,
            0x06,
            0xbc,
            0xe5,
        ];
        XorShiftRng rng = new XorShiftRng(seed);
        for (int i = 0; i < 32; i++)
        {
            var input = rng.NextFp2();
            var p = HashToCurveMapper.SswuMapG2(input);
            Assert.True(check_g2_prime(p));

            var pIsp = HashToCurveMapper.IsoMapG2(p);
            Assert.True(pIsp.IsOnCurve());
        }
    }

    [Fact]
    public void test_sgn0()
    {
        Assert.Equal(0UL, Fp2.Sgn0(Fp2.Zero));
        Assert.Equal(1UL, Fp2.Sgn0(Fp2.One));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: P_M1_OVER2, c1: Fp.Zero)));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: P_M1_OVER2, c1: Fp.One)));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: Fp.Zero, c1: P_M1_OVER2)));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: Fp.One, c1: P_M1_OVER2)));

        var p_p1_over2 = Fp.Add(P_M1_OVER2, Fp.One);
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: p_p1_over2, c1: Fp.Zero)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: p_p1_over2, c1: Fp.One)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: Fp.Zero, c1: p_p1_over2)));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: Fp.One, c1: p_p1_over2)));

        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: P_M1_OVER2, c1: Fp.Negate(Fp.One))));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: p_p1_over2, c1: Fp.Negate(Fp.One))));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: Fp.Zero, c1: Fp.Negate(Fp.One))));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: P_M1_OVER2, c1: p_p1_over2)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: p_p1_over2, c1: P_M1_OVER2)));

        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: Fp.Negate(Fp.One), c1: P_M1_OVER2)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: Fp.Negate(Fp.One), c1: p_p1_over2)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: Fp.Negate(Fp.One), c1: Fp.Zero)));
        Assert.Equal(0UL, Fp2.Sgn0(new Fp2(c0: p_p1_over2, c1: P_M1_OVER2)));
        Assert.Equal(1UL, Fp2.Sgn0(new Fp2(c0: P_M1_OVER2, c1: p_p1_over2)));
    }
}
