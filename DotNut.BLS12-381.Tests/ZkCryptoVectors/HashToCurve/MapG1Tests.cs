using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.HashToCurve;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors.HashToCurve;

public class MapG1Tests
{
    public bool check_g1_prime(G1Projective pt)
    {
        // (X : Y : Z)==(X/Z, Y/Z) is on E': y^2 = x^3 + A * x + B.
        // y^2 z = (x^3) + A (x z^2) + B z^3
        var zsq = Fp.Square(pt.Z);
        
        // y^2 * z 
        var y2z = Fp.Multiply(Fp.Square(pt.Y), pt.Z);
        //x^3
        var x3 =  Fp.Multiply(Fp.Square(pt.X), pt.X);
        // z^2 * x * A
        var z2xA = Fp.Multiply(Fp.Multiply(HashToCurveMapper.SSWU_ELLP_A, pt.X), zsq);
        
        // z^3 * B
        var z3B = Fp.Multiply(Fp.Multiply(HashToCurveMapper.SSWU_ELLP_B, zsq), pt.Z);
        
        return y2z == Fp.Add(Fp.Add(x3, z2xA), z3B);
    }

    public static TheoryData<Fp, Fp, Fp, Fp> SswuTestData() =>
        new()
        {
            // exceptional case: zero
            {
                Fp.Zero,
                new Fp([
                    0xfb99_6971_fe22_a1e0,
                    0x9aa9_3eb3_5b74_2d6f,
                    0x8c47_6013_de99_c5c4,
                    0x873e_27c3_a221_e571,
                    0xca72_b5e4_5a52_d888,
                    0x0682_4061_418a_386b,
                ]),
                new Fp([
                    0xfd6f_ced8_7a7f_11a3,
                    0x9a6b_314b_03c8_db31,
                    0x41f8_5416_e0ea_b593,
                    0xfeeb_089f_7e6e_c4d7,
                    0x85a1_34c3_7ed1_278f,
                    0x0575_c525_bb9f_74bb,
                ]),
                new Fp([
                    0x7f67_4ea0_a891_5178,
                    0xb0f9_45fc_13b8_fa65,
                    0x4b46_759a_38e8_7d76,
                    0x2e7a_9296_41bb_b6a1,
                    0x1668_ddfa_462b_f6b6,
                    0x0096_0e2e_d1cf_294c,
                ])
            },
            // exceptional case: sqrt(-1/XI) (positive)
            {
                new Fp([
                    0x00f3_d047_7e91_edbf,
                    0x08d6_621e_4ca8_dc69,
                    0xb9cf_7927_b19b_9726,
                    0xba13_3c99_6caf_a2ec,
                    0xed2a_5ccd_5ca7_bb68,
                    0x19cb_022f_8ee9_d73b,
                ]),
                new Fp([
                    0xfb99_6971_fe22_a1e0,
                    0x9aa9_3eb3_5b74_2d6f,
                    0x8c47_6013_de99_c5c4,
                    0x873e_27c3_a221_e571,
                    0xca72_b5e4_5a52_d888,
                    0x0682_4061_418a_386b,
                ]),
                new Fp([
                    0xfd6f_ced8_7a7f_11a3,
                    0x9a6b_314b_03c8_db31,
                    0x41f8_5416_e0ea_b593,
                    0xfeeb_089f_7e6e_c4d7,
                    0x85a1_34c3_7ed1_278f,
                    0x0575_c525_bb9f_74bb,
                ]),
                new Fp([
                    0x7f67_4ea0_a891_5178,
                    0xb0f9_45fc_13b8_fa65,
                    0x4b46_759a_38e8_7d76,
                    0x2e7a_9296_41bb_b6a1,
                    0x1668_ddfa_462b_f6b6,
                    0x0096_0e2e_d1cf_294c,
                ])
            },
            // exceptional case: sqrt(-1/XI) (negative)
            {
                new Fp([
                    0xb90b_2fb8_816d_bcec,
                    0x15d5_9de0_64ab_2396,
                    0xad61_5979_4515_5efe,
                    0xaa64_0eeb_86d5_6fd2,
                    0x5df1_4ae8_e6a3_f16e,
                    0x0036_0fba_aa96_0f5e,
                ]),
                new Fp([
                    0xfb99_6971_fe22_a1e0,
                    0x9aa9_3eb3_5b74_2d6f,
                    0x8c47_6013_de99_c5c4,
                    0x873e_27c3_a221_e571,
                    0xca72_b5e4_5a52_d888,
                    0x0682_4061_418a_386b,
                ]),
                Fp.Negate(
                    new Fp([
                        0xfd6f_ced8_7a7f_11a3,
                        0x9a6b_314b_03c8_db31,
                        0x41f8_5416_e0ea_b593,
                        0xfeeb_089f_7e6e_c4d7,
                        0x85a1_34c3_7ed1_278f,
                        0x0575_c525_bb9f_74bb,
                    ])
                ),
                new Fp([
                    0x7f67_4ea0_a891_5178,
                    0xb0f9_45fc_13b8_fa65,
                    0x4b46_759a_38e8_7d76,
                    0x2e7a_9296_41bb_b6a1,
                    0x1668_ddfa_462b_f6b6,
                    0x0096_0e2e_d1cf_294c,
                ])
            },
            {
                new Fp([
                    0xa618_fa19_f7e2_eadc,
                    0x93c7_f1fc_876b_a245,
                    0xe2ed_4cc4_7b5c_0ae0,
                    0xd49e_fa74_e4a8_d000,
                    0xa0b2_3ba6_92b5_431c,
                    0x0d15_51f2_d7d8_d193,
                ]),
                new Fp([
                    0x2197_ca55_fab3_ba48,
                    0x591d_eb39_f434_949a,
                    0xf9df_7fb4_f1fa_6a08,
                    0x59e3_c16a_9dfa_8fa5,
                    0xe592_9b19_4aad_5f7a,
                    0x130a_46a4_c61b_44ed,
                ]),
                new Fp([
                    0xf721_5b58_c720_0ad0,
                    0x8905_1631_3a4e_66bf,
                    0xc903_1acc_8a36_19a8,
                    0xea1f_9978_fde3_ffec,
                    0x0548_f02d_6cfb_f472,
                    0x1693_7557_3529_163f,
                ]),
                new Fp([
                    0xf36f_eb2e_1128_ade0,
                    0x42e2_2214_250b_cd94,
                    0xb94f_6ba2_dddf_62d6,
                    0xf56d_4392_782b_f0a2,
                    0xb2d7_ce1e_c263_09e7,
                    0x182b_57ed_6b99_f0a1,
                ])
            },
        };

    [Theory]
    [MemberData(nameof(SswuTestData))]
    public void test_simple_swu_expected(Fp u, Fp expectedX, Fp expectedY, Fp expectedZ)
    {
        var p = HashToCurveMapper.SswuMapG1(u);
        Assert.Equal(p.X, expectedX);
        Assert.Equal(p.Y, expectedY);
        Assert.Equal(p.Z, expectedZ);
        Assert.True(check_g1_prime(p));
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
            var input = rng.NextFp();
            var p = HashToCurveMapper.SswuMapG1(input);
            Assert.True(check_g1_prime(p));
            var pIso = HashToCurveMapper.IsoMapG1(p);
            Assert.True(pIso.IsOnCurve());
        }
    }

    [Fact]
    public void test_sgn0()
    {
        var p = TestConstants.P_M1_OVER2;
        Assert.Equal(0UL, Fp.Sgn0(Fp.Zero));
        Assert.Equal(1UL, Fp.Sgn0(Fp.One));
        Assert.Equal(0UL, Fp.Sgn0(Fp.Negate(Fp.One)));
        Assert.Equal(0UL, Fp.Sgn0(Fp.Negate(Fp.Zero)));
        Assert.Equal(1UL, Fp.Sgn0(p));

        var pP1Over2 = Fp.Add(p, Fp.One);
        Assert.Equal(0UL, Fp.Sgn0(pP1Over2));

        var NegPP1Over2 = Fp.Negate(pP1Over2);
        Assert.Equal(NegPP1Over2, p);
    }
}
