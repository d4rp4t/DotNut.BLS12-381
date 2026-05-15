using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors;

public class G1Tests
{
    [Fact]
    public void test_beta()
    {
        var beta = G1Affine.Beta;
        // Canonical big-endian bytes for beta (from Rust Fp::from_bytes)
        var expected = Fp.FromBytesBigEndian([
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x5f,0x19,0x67,0x2f,0xdf,0x76,
            0xce,0x51,0xba,0x69,0xc6,0x07,0x6a,0x0f,0x77,0xea,0xdd,0xb3,0xa9,0x3b,
            0xe6,0xf8,0x96,0x88,0xde,0x17,0xd8,0x13,0x62,0x0a,0x00,0x02,0x2e,0x01,
            0xff,0xff,0xff,0xfe,0xff,0xfe,
        ]);
        Assert.Equal(expected, beta);
        Assert.NotEqual(Fp.One, beta);
        
        var betaSqr = Fp.Square(beta);
        Assert.NotEqual(Fp.One, betaSqr);
        Assert.Equal(Fp.One, Fp.Multiply(betaSqr, beta));
    }

    [Fact]
    public void test_is_on_curve()
    {
        Assert.True(G1Affine.Infinity.IsOnCurve());
        Assert.True(G1Affine.Generator.IsOnCurve());
        Assert.True(G1Projective.Infinity.IsOnCurve());
        Assert.True(G1Projective.Generator.IsOnCurve());

        var z = new Fp([
            0xba7a_fa1f_9a6f_e250,
            0xfa0f_5b59_5eaf_e731,
            0x3bdc_4776_94c3_06e7,
            0x2149_be4b_3949_fa24,
            0x64aa_6e06_49b2_078c,
            0x12b1_08ac_3364_3c3e,
        ]);

        var gen = G1Affine.Generator;
        var test = new G1Projective(Fp.Multiply(gen.X, z), Fp.Multiply(gen.Y, z), z);
        Assert.True(test.IsOnCurve());

        var bad = new G1Projective(z, Fp.Multiply(gen.Y, z), z);
        Assert.False(bad.IsOnCurve());
    }

    [Fact]
    public void test_affine_point_equality()
    {
        var a = G1Affine.Generator;
        var b = G1Affine.Infinity;

        Assert.True(a == a);
        Assert.True(b == b);
        Assert.True(a != b);
        Assert.True(b != a);
    }

    [Fact]
    public void test_projective_point_equality()
    {
        var a = G1Projective.Generator;
        var b = G1Projective.Infinity;

        Assert.True(a == a);
        Assert.True(b == b);
        Assert.True(a != b);
        Assert.True(b != a);

        var z = new Fp([
            0xba7a_fa1f_9a6f_e250,
            0xfa0f_5b59_5eaf_e731,
            0x3bdc_4776_94c3_06e7,
            0x2149_be4b_3949_fa24,
            0x64aa_6e06_49b2_078c,
            0x12b1_08ac_3364_3c3e,
        ]);

        var c = new G1Projective(
            Fp.Multiply(a.X, z), 
            Fp.Multiply(a.Y, z), 
            z
            );
        Assert.True(c.IsOnCurve());

        Assert.True(a == c);
        Assert.True(b != c);
        Assert.True(c == a);
        Assert.True(c != b);

        var cNegY = new G1Projective(c.X, Fp.Negate(c.Y), c.Z);
        Assert.True(cNegY.IsOnCurve());
        Assert.True(a != cNegY);
        Assert.True(b != cNegY);
        Assert.True(cNegY != a);
        Assert.True(cNegY != b);

        var cBadX = new G1Projective(z, c.Y, c.Z);
        Assert.False(cBadX.IsOnCurve());
        Assert.True(a != b);
        Assert.True(a != cBadX);
        Assert.True(b != cBadX);
    }
    [Fact]
    public void test_conditionally_select_affine()
    {
        var a = G1Affine.Generator;
        var b = G1Affine.Infinity;
        Assert.Equal(a, G1Affine.ConditionalSelect(a, b, false));
        Assert.Equal(b, G1Affine.ConditionalSelect(a, b, true));
    }

    [Fact]
    public void test_conditionally_select_projective()
    {
        var a = G1Projective.Generator;
        var b = G1Projective.Infinity;
        Assert.True(G1Projective.ConditionalSelect(a, b, false) == a);
        Assert.True(G1Projective.ConditionalSelect(a, b, true) == b);
    }

    [Fact]
    public void test_projective_to_affine()
    {
        var a = G1Projective.Generator;
        var b = G1Projective.Infinity;

        Assert.True(a.ToAffine().IsOnCurve());
        Assert.False(a.ToAffine().IsInfinity);
        Assert.True(b.ToAffine().IsOnCurve());
        Assert.True(b.ToAffine().IsInfinity);

        var z = new Fp([
            0xba7a_fa1f_9a6f_e250,
            0xfa0f_5b59_5eaf_e731,
            0x3bdc_4776_94c3_06e7,
            0x2149_be4b_3949_fa24,
            0x64aa_6e06_49b2_078c,
            0x12b1_08ac_3364_3c3e,
        ]);

        var c = new G1Projective(Fp.Multiply(a.X, z), Fp.Multiply(a.Y, z), z);
        Assert.Equal(G1Affine.Generator, c.ToAffine());
    }

    [Fact]
    public void test_affine_to_projective()
    {
        var a = G1Affine.Generator;
        var b = G1Affine.Infinity;

        Assert.True(a.ToProjective().IsOnCurve());
        Assert.False(a.ToProjective().IsInfinity);
        Assert.True(b.ToProjective().IsOnCurve());
        Assert.True(b.ToProjective().IsInfinity);
    }

    [Fact]
    public void test_doubling()
    {
        {
            var tmp = G1Projective.Double(G1Projective.Infinity);
            Assert.True(tmp.IsInfinity);
            Assert.True(tmp.IsOnCurve());
        }
        {
            var tmp = G1Projective.Double(G1Projective.Generator);
            Assert.False(tmp.IsInfinity);
            Assert.True(tmp.IsOnCurve());

            Assert.Equal(
                new G1Affine(
                    new Fp([
                        0x53e9_78ce_58a9_ba3c,
                        0x3ea0_583c_4f3d_65f9,
                        0x4d20_bb47_f001_2960,
                        0xa54c_664a_e5b2_b5d9,
                        0x26b5_52a3_9d7e_b21f,
                        0x0008_895d_26e6_8785,
                    ]),
                    new Fp([
                        0x7011_0b32_9829_3940,
                        0xda33_c539_3f1f_6afc,
                        0xb86e_dfd1_6a5a_a785,
                        0xaec6_d1c9_e7b1_c895,
                        0x25cf_c2b5_22d1_1720,
                        0x0636_1c83_f8d0_9b15,
                    ])
                ),
                tmp.ToAffine()
            );
        }
    }

    [Fact]
    public void test_projective_addition()
    {
        {
            var a = G1Projective.Infinity;
            var b = G1Projective.Infinity;
            var c = G1Projective.Add(a, b);
            Assert.True(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
        {
            var a = G1Projective.Infinity;
            var gen = G1Projective.Generator;
            var z = new Fp([
                0xba7a_fa1f_9a6f_e250,
                0xfa0f_5b59_5eaf_e731,
                0x3bdc_4776_94c3_06e7,
                0x2149_be4b_3949_fa24,
                0x64aa_6e06_49b2_078c,
                0x12b1_08ac_3364_3c3e,
            ]);
            var b = new G1Projective(Fp.Multiply(gen.X, z), Fp.Multiply(gen.Y, z), z);
            var c = G1Projective.Add(a, b);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G1Projective.Generator);
        }
        {
            var z = new Fp([
                0xba7a_fa1f_9a6f_e250,
                0xfa0f_5b59_5eaf_e731,
                0x3bdc_4776_94c3_06e7,
                0x2149_be4b_3949_fa24,
                0x64aa_6e06_49b2_078c,
                0x12b1_08ac_3364_3c3e,
            ]);
            var gen = G1Projective.Generator;
            var b = new G1Projective(Fp.Multiply(gen.X, z), Fp.Multiply(gen.Y, z), z);
            var c = G1Projective.Add(b, G1Projective.Infinity);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G1Projective.Generator);
        }
        {
            var a = G1Projective.Double(G1Projective.Double(G1Projective.Generator)); // 4P
            var b = G1Projective.Double(G1Projective.Generator); // 2P
            var c = G1Projective.Add(a, b);

            var d = G1Projective.Generator;
            for (int i = 0; i < 5; i++)
                d = G1Projective.Add(d, G1Projective.Generator);

            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.False(d.IsInfinity);
            Assert.True(d.IsOnCurve());
            Assert.Equal(c, d);
        }
        // Degenerate case
        {
            var beta = Fp.Square(new Fp([0xcd03_c9e4_8671_f071, 0x5dab_2246_1fcd_a5d2, 0x5870_42af_d385_1b95, 0x8eb6_0ebe_01ba_cb9e, 0x03f9_7d6e_83d0_50d2, 0x18f0_2065_5463_8741]));
            var a = G1Projective.Double(G1Projective.Double(G1Projective.Generator));
            var b = new G1Projective(Fp.Multiply(a.X, beta), Fp.Negate(a.Y), a.Z);
            Assert.True(a.IsOnCurve());
            Assert.True(b.IsOnCurve());

            var c = G1Projective.Add(a, b);
            Assert.Equal(
                new G1Projective(
                    new Fp([
                        0x29e1_e987_ef68_f2d0,
                        0xc5f3_ec53_1db0_3233,
                        0xacd6_c4b6_ca19_730f,
                        0x18ad_9e82_7bc2_bab7,
                        0x46e3_b2c5_785c_c7a9,
                        0x07e5_71d4_2d22_ddd6,
                    ]),
                    new Fp([
                        0x94d1_17a7_e5a5_39e7,
                        0x8e17_ef67_3d4b_5d22,
                        0x9d74_6aaf_508a_33ea,
                        0x8c6d_883d_2516_c9a2,
                        0x0bc3_b8d5_fb04_47f7,
                        0x07bf_a4c7_210f_4f44,
                    ]),
                    Fp.One
                ).ToAffine(),
                c.ToAffine()
            );
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
    }

    [Fact]
    public void test_mixed_addition()
    {
        {
            var a = G1Affine.Infinity;
            var b = G1Projective.Infinity;
            var c = G1Projective.Add(a, b);
            Assert.True(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
        {
            var z = new Fp([
                0xba7a_fa1f_9a6f_e250,
                0xfa0f_5b59_5eaf_e731,
                0x3bdc_4776_94c3_06e7,
                0x2149_be4b_3949_fa24,
                0x64aa_6e06_49b2_078c,
                0x12b1_08ac_3364_3c3e,
            ]);
            var gen = G1Projective.Generator;
            var b = new G1Projective(Fp.Multiply(gen.X, z), Fp.Multiply(gen.Y, z), z);
            var c = G1Projective.Add(G1Affine.Infinity, b);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G1Projective.Generator);
        }
        {
            var z = new Fp([
                0xba7a_fa1f_9a6f_e250,
                0xfa0f_5b59_5eaf_e731,
                0x3bdc_4776_94c3_06e7,
                0x2149_be4b_3949_fa24,
                0x64aa_6e06_49b2_078c,
                0x12b1_08ac_3364_3c3e,
            ]);
            var gen = G1Projective.Generator;
            var b = new G1Projective(Fp.Multiply(gen.X, z), Fp.Multiply(gen.Y, z), z);
            var c = G1Projective.Add(b, G1Affine.Infinity);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G1Projective.Generator);
        }
        {
            var a = G1Projective.Double(G1Projective.Double(G1Projective.Generator)); // 4P
            var b = G1Projective.Double(G1Projective.Generator); // 2P
            var c = G1Projective.Add(a, b);

            var d = G1Projective.Generator;
            for (int i = 0; i < 5; i++)
                d = G1Projective.Add(d, G1Affine.Generator);

            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.False(d.IsInfinity);
            Assert.True(d.IsOnCurve());
            Assert.Equal(c, d);
        }
        // Degenerate case
        {
            var beta = Fp.Square(new Fp([0xcd03_c9e4_8671_f071, 0x5dab_2246_1fcd_a5d2, 0x5870_42af_d385_1b95, 0x8eb6_0ebe_01ba_cb9e, 0x03f9_7d6e_83d0_50d2, 0x18f0_2065_5463_8741]));
            var aProj = G1Projective.Double(G1Projective.Double(G1Projective.Generator));
            var b = new G1Projective(Fp.Multiply(aProj.X, beta), Fp.Negate(aProj.Y), aProj.Z);
            var a = aProj.ToAffine();
            Assert.True(a.IsOnCurve());
            Assert.True(b.IsOnCurve());

            var c = G1Projective.Add(a, b);
            Assert.Equal(
                new G1Projective(
                    new Fp([
                        0x29e1_e987_ef68_f2d0,
                        0xc5f3_ec53_1db0_3233,
                        0xacd6_c4b6_ca19_730f,
                        0x18ad_9e82_7bc2_bab7,
                        0x46e3_b2c5_785c_c7a9,
                        0x07e5_71d4_2d22_ddd6,
                    ]),
                    new Fp([
                        0x94d1_17a7_e5a5_39e7,
                        0x8e17_ef67_3d4b_5d22,
                        0x9d74_6aaf_508a_33ea,
                        0x8c6d_883d_2516_c9a2,
                        0x0bc3_b8d5_fb04_47f7,
                        0x07bf_a4c7_210f_4f44,
                    ]),
                    Fp.One
                ).ToAffine(),
                c.ToAffine()
            );
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
    }

    [Fact]
    public void test_projective_negation_and_subtraction()
    {
        var a = G1Projective.Double(G1Projective.Generator);
        Assert.Equal(G1Projective.Infinity, G1Projective.Add(a, G1Projective.Negate(a)));
        Assert.Equal(G1Projective.Add(a, G1Projective.Negate(a)), G1Projective.Subtract(a, a));
    }

    // i haven't really implemented math on affine so it uses .ToProjective().
    // even though the test is valuable so im reusing it for mixed subtraction case
    [Fact]
    public void test_affine_negation_and_subtraction()
    {
        var a = G1Affine.Generator;
        var negA = new G1Affine(a.X, Fp.Negate(a.Y));
        Assert.Equal(G1Projective.Infinity, G1Projective.Add(a.ToProjective(), negA.ToProjective()));
        Assert.Equal(
            G1Projective.Add(a.ToProjective(), negA.ToProjective()),
            G1Projective.Subtract(a.ToProjective(), a.ToProjective())
        );
    }

    [Fact]
    public void test_projective_scalar_multiplication()
    {
        var g = G1Projective.Generator;
        var a = new Scalar(
            0x2b56_8297_a56d_a71c,
            0xd8c3_9ecb_0ef3_75d1,
            0x435c_38da_67bf_bf96,
            0x8088_a050_26b6_59b2
        );
        var b = new Scalar(
            0x785f_dd9b_26ef_8b85,
            0xc997_f258_3769_5c18,
            0x4c8d_bc39_e7b7_56c1,
            0x70d9_b6cc_6d87_df20
        );
        var c = Scalar.Mul(a, b);

        Assert.Equal(
            G1Projective.ScalarMultiply(G1Projective.ScalarMultiply(g, a), b),
            G1Projective.ScalarMultiply(g, c)
        );
    }

    // same as before. reusing test but for projective
    [Fact]
    public void test_affine_scalar_multiplication()
    {
        var g = G1Affine.Generator;
        var a = new Scalar(
            0x2b56_8297_a56d_a71c,
            0xd8c3_9ecb_0ef3_75d1,
            0x435c_38da_67bf_bf96,
            0x8088_a050_26b6_59b2
        );
        var b = new Scalar(
            0x785f_dd9b_26ef_8b85,
            0xc997_f258_3769_5c18,
            0x4c8d_bc39_e7b7_56c1,
            0x70d9_b6cc_6d87_df20
        );
        var c = Scalar.Mul(a, b);

        Assert.Equal(
            G1Projective.ScalarMultiply(
                G1Projective.ScalarMultiply(g.ToProjective(), a).ToAffine().ToProjective(),
                b
            ),
            G1Projective.ScalarMultiply(g.ToProjective(), c)
        );
    }

    [Fact]
    public void test_is_torsion_free()
    {
        var a = new G1Affine(
            new Fp([
                0x0aba_f895_b97e_43c8,
                0xba4c_6432_eb9b_61b0,
                0x1250_6f52_adfe_307f,
                0x7502_8c34_3933_6b72,
                0x8474_4f05_b8e9_bd71,
                0x113d_554f_b095_54f7,
            ]),
            new Fp([
                0x73e9_0e88_f5cf_01c0,
                0x3700_7b65_dd31_97e2,
                0x5cf9_a199_2f0d_7c78,
                0x4f83_c10b_9eb3_330d,
                0xf6a6_3f6f_07f6_0961,
                0x0c53_b5b9_7e63_4df3,
            ])
        );
        Assert.False(a.IsInSubgroup());

        Assert.True(G1Affine.Infinity.IsInSubgroup());
        Assert.True(G1Affine.Generator.IsInSubgroup());
    }

    [Fact]
    public void test_mul_by_x()
    {
        // BLS_X_IS_NEGATIVE = true, so mul_by_x() = [-BLS_X]P. Rust test: mul_by_x(P) == P * (-BLS_X).
        // Our MulByBLSX = [+BLS_X]P, so Negate(MulByBLSX(P)) == P * (-BLS_X).
        var generator = G1Projective.Generator;
        var x = Scalar.Negate(Scalar.From(0xd201_0000_0001_0000UL));
        Assert.Equal(G1Projective.Negate(G1Projective.MulByBLSX(generator)), G1Projective.ScalarMultiply(generator, x));

        var point = G1Projective.ScalarMultiply(G1Projective.Generator, Scalar.From(42));
        Assert.Equal(G1Projective.Negate(G1Projective.MulByBLSX(point)), G1Projective.ScalarMultiply(point, x));
    }

    [Fact]
    public void test_clear_cofactor()
    {
        var generator = G1Projective.Generator;
        Assert.True(generator.ClearCofactor().IsOnCurve());
        Assert.True(G1Projective.Infinity.ClearCofactor().IsOnCurve());

        var z = new Fp([
            0x3d2d1c67_0671394e,
            0x0ee3a800_a2f7c1ca,
            0x270f4f21_da2e5050,
            0xe02840a5_3f1be768,
            0x55debeb5_97512690,
            0x08bd2535_3dc8f791,
        ]);

        var pointX = new Fp([
            0x48af5ff5_40c817f0,
            0xd73893ac_af379d5a,
            0xe6c43584_e18e023c,
            0x1eda39c3_0f188b3e,
            0xf618c6d3_ccc0f8d8,
            0x0073542c_d671e16c,
        ]);
        var pointY = new Fp([
            0x57bf8be7_9461d0ba,
            0xfc61459c_ee3547c3,
            0x0d23567d_f1ef147b,
            0x0ee187bc_ce1d9b64,
            0xb0c8cfbe_9dc8fdc1,
            0x13286617_67ef368b,
        ]);
        var point = new G1Projective(
            Fp.Multiply(pointX, z),
            pointY,
            Fp.Multiply(Fp.Square(z), z)
        );

        Assert.True(point.IsOnCurve());
        Assert.False(point.ToAffine().IsInSubgroup());
        var clearedPoint = point.ClearCofactor();
        Assert.True(clearedPoint.IsOnCurve());
        Assert.True(clearedPoint.ToAffine().IsInSubgroup());

        var hEff = Scalar.Add(Scalar.From(1), Scalar.From(0xd201_0000_0001_0000UL));
        Assert.Equal(point.ClearCofactor(), G1Projective.ScalarMultiply(point, hEff));
    }
}
