using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors;

public class G2Tests
{
    private static Fp2 MakeFp2(ulong[] c0Limbs, ulong[] c1Limbs) =>
        new(new Fp(c0Limbs), new Fp(c1Limbs));

    private static readonly Fp2 Z = MakeFp2(
        [0xba7a_fa1f_9a6f_e250, 0xfa0f_5b59_5eaf_e731, 0x3bdc_4776_94c3_06e7,
         0x2149_be4b_3949_fa24, 0x64aa_6e06_49b2_078c, 0x12b1_08ac_3364_3c3e],
        [0x1253_25df_3d35_b5a8, 0xdc46_9ef5_555d_7fe3, 0x02d7_16d2_4431_06a9,
         0x05a1_db59_a6ff_37d0, 0x7cf7_784e_5300_bb8f, 0x16a8_8922_c7a5_e844]
    );

    [Fact]
    public void test_is_on_curve()
    {
        Assert.True(G2Affine.Infinity.IsOnCurve());
        Assert.True(G2Affine.Generator.IsOnCurve());
        Assert.True(G2Projective.Infinity.IsOnCurve());
        Assert.True(G2Projective.Generator.IsOnCurve());

        var gen = G2Affine.Generator;
        var test = new G2Projective(Fp2.Multiply(gen.X, Z), Fp2.Multiply(gen.Y, Z), Z);
        Assert.True(test.IsOnCurve());

        var bad = new G2Projective(Z, Fp2.Multiply(gen.Y, Z), Z);
        Assert.False(bad.IsOnCurve());
    }

    [Fact]
    public void test_affine_point_equality()
    {
        var a = G2Affine.Generator;
        var b = G2Affine.Infinity;

        Assert.True(a == a);
        Assert.True(b == b);
        Assert.True(a != b);
        Assert.True(b != a);
    }

    [Fact]
    public void test_projective_point_equality()
    {
        var a = G2Projective.Generator;
        var b = G2Projective.Infinity;

        Assert.True(a == a);
        Assert.True(b == b);
        Assert.True(a != b);
        Assert.True(b != a);

        var c = new G2Projective(Fp2.Multiply(a.X, Z), Fp2.Multiply(a.Y, Z), Z);
        Assert.True(c.IsOnCurve());

        Assert.True(a == c);
        Assert.True(b != c);
        Assert.True(c == a);
        Assert.True(c != b);

        var cNegY = new G2Projective(c.X, Fp2.Negate(c.Y), c.Z);
        Assert.True(cNegY.IsOnCurve());
        Assert.True(a != cNegY);
        Assert.True(b != cNegY);
        Assert.True(cNegY != a);
        Assert.True(cNegY != b);

        var cBadX = new G2Projective(Z, c.Y, c.Z);
        Assert.False(cBadX.IsOnCurve());
        Assert.True(a != b);
        Assert.True(a != cBadX);
        Assert.True(b != cBadX);
    }
    
    [Fact]
    public void test_conditionally_select_projective() {
        var a = G2Projective.Generator;
        var b = G2Projective.Infinity;

        Assert.Equal(
            G2Projective.ConditionalSelect(a, b, false),
            a
        );
        Assert.Equal(
            G2Projective.ConditionalSelect(a, b, true),
            b
        );
    }

    [Fact]
    public void test_projective_to_affine()
    {
        var a = G2Projective.Generator;
        var b = G2Projective.Infinity;

        Assert.True(a.ToAffine().IsOnCurve());
        Assert.False(a.ToAffine().IsInfinity);
        Assert.True(b.ToAffine().IsOnCurve());
        Assert.True(b.ToAffine().IsInfinity);

        var c = new G2Projective(Fp2.Multiply(a.X, Z), Fp2.Multiply(a.Y, Z), Z);
        Assert.Equal(G2Affine.Generator, c.ToAffine());
    }

    [Fact]
    public void test_affine_to_projective()
    {
        var a = G2Affine.Generator;
        var b = G2Affine.Infinity;

        Assert.True(a.ToProjective().IsOnCurve());
        Assert.False(a.ToProjective().IsInfinity);
        Assert.True(b.ToProjective().IsOnCurve());
        Assert.True(b.ToProjective().IsInfinity);
    }

    [Fact]
    public void test_doubling()
    {
        {
            var tmp = G2Projective.Double(G2Projective.Infinity);
            Assert.True(tmp.IsInfinity);
            Assert.True(tmp.IsOnCurve());
        }
        {
            var tmp = G2Projective.Double(G2Projective.Generator);
            Assert.False(tmp.IsInfinity);
            Assert.True(tmp.IsOnCurve());

            Assert.Equal(
                new G2Affine(
                    MakeFp2(
                        [0xe9d9_e2da_9620_f98b, 0x54f1_1993_46b9_7f36, 0x3db3_b820_376b_ed27,
                         0xcfdb_31c9_b0b6_4f4c, 0x41d7_c127_8635_4493, 0x0571_0794_c255_c064],
                        [0xd6c1_d3ca_6ea0_d06e, 0xda0c_bd90_5595_489f, 0x4f53_52d4_3479_221d,
                         0x8ade_5d73_6f8c_97e0, 0x48cc_8433_925e_f70e, 0x08d7_ea71_ea91_ef81]
                    ),
                    MakeFp2(
                        [0x15ba_26eb_4b0d_186f, 0x0d08_6d64_b7e9_e01e, 0xc8b8_48dd_652f_4c78,
                         0xeecf_46a6_123b_ae4f, 0x255e_8dd8_b6dc_812a, 0x1641_42af_21dc_f93f],
                        [0xf9b4_a1a8_9598_4db4, 0xd417_b114_cccf_f748, 0x6856_301f_c89f_086e,
                         0x41c7_7787_8931_e3da, 0x3556_b155_066a_2105, 0x00ac_f7d3_25cb_89cf]
                    )
                ),
                tmp.ToAffine()
            );
        }
    }

    [Fact]
    public void test_projective_addition()
    {
        {
            var c = G2Projective.Add(G2Projective.Infinity, G2Projective.Infinity);
            Assert.True(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
        {
            var gen = G2Projective.Generator;
            var b = new G2Projective(Fp2.Multiply(gen.X, Z), Fp2.Multiply(gen.Y, Z), Z);
            var c = G2Projective.Add(G2Projective.Infinity, b);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G2Projective.Generator);
        }
        {
            var gen = G2Projective.Generator;
            var b = new G2Projective(Fp2.Multiply(gen.X, Z), Fp2.Multiply(gen.Y, Z), Z);
            var c = G2Projective.Add(b, G2Projective.Infinity);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G2Projective.Generator);
        }
        {
            var a = G2Projective.Double(G2Projective.Double(G2Projective.Generator)); // 4P
            var b = G2Projective.Double(G2Projective.Generator); // 2P
            var c = G2Projective.Add(a, b);

            var d = G2Projective.Generator;
            for (int i = 0; i < 5; i++)
                d = G2Projective.Add(d, G2Projective.Generator);

            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.False(d.IsInfinity);
            Assert.True(d.IsOnCurve());
            Assert.Equal(c, d);
        }
        // Degenerate case
        {
            var betaFp2 = new Fp2(new Fp([
                0xcd03_c9e4_8671_f071, 0x5dab_2246_1fcd_a5d2, 0x5870_42af_d385_1b95,
                0x8eb6_0ebe_01ba_cb9e, 0x03f9_7d6e_83d0_50d2, 0x18f0_2065_5463_8741,
            ]), Fp.Zero);
            var beta = Fp2.Square(betaFp2);
            var a = G2Projective.Double(G2Projective.Double(G2Projective.Generator));
            var b = new G2Projective(Fp2.Multiply(a.X, beta), Fp2.Negate(a.Y), a.Z);
            Assert.True(a.IsOnCurve());
            Assert.True(b.IsOnCurve());

            var c = G2Projective.Add(a, b);
            Assert.Equal(
                new G2Projective(
                    MakeFp2(
                        [0x705a_bc79_9ca7_73d3, 0xfe13_2292_c1d4_bf08, 0xf37e_ce3e_07b2_b466,
                         0x887e_1c43_f447_e301, 0x1e09_70d0_33bc_77e8, 0x1985_c81e_20a6_93f2],
                        [0x1d79_b25d_b36a_b924, 0x2394_8e4d_5296_39d3, 0x471b_a7fb_0d00_6297,
                         0x2c36_d4b4_465d_c4c0, 0x82bb_c3cf_ec67_f538, 0x051d_2728_b67b_f952]
                    ),
                    MakeFp2(
                        [0x41b1_bbf6_576c_0abf, 0xb6cc_9371_3f7a_0f9a, 0x6b65_b43e_48f3_f01f,
                         0xfb7a_4cfc_af81_be4f, 0x3e32_dadc_6ec2_2cb6, 0x0bb0_fc49_d798_07e3],
                        [0x7d13_9778_8f5f_2ddf, 0xab29_0714_4ff0_d8e8, 0x5b75_73e0_cdb9_1f92,
                         0x4cb8_932d_d31d_af28, 0x62bb_fac6_db05_2a54, 0x11f9_5c16_d14c_3bbe]
                    ),
                    Fp2.One
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
            var c = G2Projective.Add(G2Affine.Infinity, G2Projective.Infinity);
            Assert.True(c.IsInfinity);
            Assert.True(c.IsOnCurve());
        }
        {
            var gen = G2Projective.Generator;
            var b = new G2Projective(Fp2.Multiply(gen.X, Z), Fp2.Multiply(gen.Y, Z), Z);
            var c = G2Projective.Add(G2Affine.Infinity, b);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G2Projective.Generator);
        }
        {
            var gen = G2Projective.Generator;
            var b = new G2Projective(Fp2.Multiply(gen.X, Z), Fp2.Multiply(gen.Y, Z), Z);
            var c = G2Projective.Add(b, G2Affine.Infinity);
            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.True(c == G2Projective.Generator);
        }
        {
            var a = G2Projective.Double(G2Projective.Double(G2Projective.Generator)); // 4P
            var b = G2Projective.Double(G2Projective.Generator); // 2P
            var c = G2Projective.Add(a, b);

            var d = G2Projective.Generator;
            for (int i = 0; i < 5; i++)
                d = G2Projective.Add(d, G2Affine.Generator);

            Assert.False(c.IsInfinity);
            Assert.True(c.IsOnCurve());
            Assert.False(d.IsInfinity);
            Assert.True(d.IsOnCurve());
            Assert.Equal(c, d);
        }
        // Degenerate case
        {
            var betaFp2 = new Fp2(new Fp([
                0xcd03_c9e4_8671_f071, 0x5dab_2246_1fcd_a5d2, 0x5870_42af_d385_1b95,
                0x8eb6_0ebe_01ba_cb9e, 0x03f9_7d6e_83d0_50d2, 0x18f0_2065_5463_8741,
            ]), Fp.Zero);
            var beta = Fp2.Square(betaFp2);
            var aProj = G2Projective.Double(G2Projective.Double(G2Projective.Generator));
            var b = new G2Projective(Fp2.Multiply(aProj.X, beta), Fp2.Negate(aProj.Y), aProj.Z);
            var a = aProj.ToAffine();
            Assert.True(a.IsOnCurve());
            Assert.True(b.IsOnCurve());

            var c = G2Projective.Add(a, b);
            Assert.Equal(
                new G2Projective(
                    MakeFp2(
                        [0x705a_bc79_9ca7_73d3, 0xfe13_2292_c1d4_bf08, 0xf37e_ce3e_07b2_b466,
                         0x887e_1c43_f447_e301, 0x1e09_70d0_33bc_77e8, 0x1985_c81e_20a6_93f2],
                        [0x1d79_b25d_b36a_b924, 0x2394_8e4d_5296_39d3, 0x471b_a7fb_0d00_6297,
                         0x2c36_d4b4_465d_c4c0, 0x82bb_c3cf_ec67_f538, 0x051d_2728_b67b_f952]
                    ),
                    MakeFp2(
                        [0x41b1_bbf6_576c_0abf, 0xb6cc_9371_3f7a_0f9a, 0x6b65_b43e_48f3_f01f,
                         0xfb7a_4cfc_af81_be4f, 0x3e32_dadc_6ec2_2cb6, 0x0bb0_fc49_d798_07e3],
                        [0x7d13_9778_8f5f_2ddf, 0xab29_0714_4ff0_d8e8, 0x5b75_73e0_cdb9_1f92,
                         0x4cb8_932d_d31d_af28, 0x62bb_fac6_db05_2a54, 0x11f9_5c16_d14c_3bbe]
                    ),
                    Fp2.One
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
        var a = G2Projective.Double(G2Projective.Generator);
        Assert.Equal(G2Projective.Infinity, G2Projective.Add(a, G2Projective.Negate(a)));
        Assert.Equal(G2Projective.Add(a, G2Projective.Negate(a)), G2Projective.Subtract(a, a));
    }

    // we don't have affine math also. let's keep the test vector too and test mixed subtraction
    [Fact]
    public void test_affine_negation_and_subtraction()
    {
        var a = G2Affine.Generator;
        var negA = new G2Affine(a.X, Fp2.Negate(a.Y));
        Assert.Equal(G2Projective.Infinity, G2Projective.Add(a.ToProjective(), negA.ToProjective()));
        Assert.Equal(G2Projective.Infinity, G2Projective.Subtract(a.ToProjective(), a));
        Assert.Equal(
            G2Projective.Add(a, negA.ToProjective()),
            G2Projective.Subtract(a.ToProjective(), a)
        );
        
        Assert.Equal(
            G2Projective.Add(a.ToProjective(), negA),
            G2Projective.Subtract(a, a.ToProjective())
        );
    }

    [Fact]
    public void test_projective_scalar_multiplication()
    {
        var g = G2Projective.Generator;
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
            G2Projective.ScalarMultiply(G2Projective.ScalarMultiply(g, a), b),
            G2Projective.ScalarMultiply(g, c)
        );
    }

    // same as b4, no math on affine :/ 
    [Fact]
    public void test_affine_scalar_multiplication()
    {
        var g = G2Affine.Generator;
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
            G2Projective.ScalarMultiply(
                G2Projective.ScalarMultiply(g.ToProjective(), a).ToAffine().ToProjective(),
                b
            ),
            G2Projective.ScalarMultiply(g.ToProjective(), c)
        );
    }

    [Fact]
    public void test_is_torsion_free()
    {
        var a = new G2Affine(
            MakeFp2(
                [0x89f5_50c8_13db_6431, 0xa50b_e8c4_56cd_8a1a, 0xa45b_3741_14ca_e851,
                 0xbb61_90f5_bf7f_ff63, 0x970c_a02c_3ba8_0bc7, 0x02b8_5d24_e840_fbac],
                [0x6888_bc53_d707_16dc, 0x3dea_6b41_1768_2d70, 0xd8f5_f930_500c_a354,
                 0x6b5e_cb65_56f5_c155, 0xc96b_ef04_3477_8ab0, 0x0508_1505_5150_06ad]
            ),
            MakeFp2(
                [0x3cf1_ea0d_434b_0f40, 0x1a0d_c610_e603_e333, 0x7f89_9561_60c7_2fa0,
                 0x25ee_03de_cf64_31c5, 0xeee8_e206_ec0f_e137, 0x0975_92b2_26df_ef28],
                [0x71e8_bb5f_2924_7367, 0xa5fe_049e_2118_31ce, 0x0ce6_b354_502a_3896,
                 0x93b0_1200_0997_314e, 0x6759_f3b6_aa5b_42ac, 0x1569_44c4_dfe9_2bbb]
            )
        );
        Assert.False(a.IsInSubgroup());

        Assert.True(G2Affine.Infinity.IsInSubgroup());
        Assert.True(G2Affine.Generator.IsInSubgroup());
    }

    [Fact]
    public void test_mul_by_x()
    {
        // BLS_X_IS_NEGATIVE = true, so mul_by_x() = [-BLS_X]P. Rust test: mul_by_x(P) == P * (-BLS_X).
        // Our MulByBLSX = [+BLS_X]P, so Negate(MulByBLSX(P)) == P * (-BLS_X).
        var generator = G2Projective.Generator;
        var x = Scalar.Negate(Scalar.From(0xd201_0000_0001_0000UL));
        Assert.Equal(G2Projective.Negate(G2Projective.MulByBLSX(generator)), G2Projective.ScalarMultiply(generator, x));

        var point = G2Projective.ScalarMultiply(G2Projective.Generator, Scalar.From(42));
        Assert.Equal(G2Projective.Negate(G2Projective.MulByBLSX(point)), G2Projective.ScalarMultiply(point, x));
    }

    [Fact]
    public void test_psi()
    {
        var generator = G2Projective.Generator;

        var z2 = MakeFp2(
            [0x0ef2ddffab187c0a, 0x2424522b7d5ecbfc, 0xc6f341a3398054f4,
             0x5523ddf409502df0, 0xd55c0b5a88e0dd97, 0x066428d704923e52],
            [0x538bbe0c95b4878d, 0xad04a50379522881, 0x6d5c05bf5c12fb64,
             0x4ce4a069a2d34787, 0x59ea6c8d0dffaeaf, 0x0d42a083a75bd6f3]
        );

        var pointX = MakeFp2(
            [0xee4c8cb7c047eaf2, 0x44ca22eee036b604, 0x33b3affb2aefe101,
             0x15d3e45bbafaeb02, 0x7bfc2154cd7419a4, 0x0a2d0c2b756e5edc],
            [0xfc224361029a8777, 0x4cbf2baab8740924, 0xc5008c6ec6592c89,
             0xecc2c57b472a9c2d, 0x8613eafd9d81ffb1, 0x10fe54daa2d3d495]
        );
        var pointY = MakeFp2(
            [0x7de7edc43953b75c, 0x58be1d2de35e87dc, 0x5731d30b0e337b40,
             0xbe93b60cfeaae4c9, 0x8b22c203764bedca, 0x01616c8d1033b771],
            [0xea126fe476b5733b, 0x85cee68b5dae1652, 0x98247779f7272b04,
             0xa649c8b468c6e808, 0xb5b9a62dff0c4e45, 0x1555b67fc7bbe73d]
        );
        var point = new G2Projective(
            Fp2.Multiply(pointX, z2),
            pointY,
            Fp2.Multiply(Fp2.Square(z2), z2)
        );
        Assert.True(point.IsOnCurve());

        // psi2(P) = psi(psi(P))
        Assert.Equal(G2Projective.Psi2(generator), G2Projective.Psi(G2Projective.Psi(generator)));
        Assert.Equal(G2Projective.Psi2(point), G2Projective.Psi(G2Projective.Psi(point)));
        // psi(P) is a morphism
        Assert.Equal(
            G2Projective.Psi(G2Projective.Double(generator)),
            G2Projective.Double(G2Projective.Psi(generator))
        );
        Assert.Equal(
            G2Projective.Add(G2Projective.Psi(point), G2Projective.Psi(generator)),
            G2Projective.Psi(G2Projective.Add(point, generator))
        );
        // psi(P) behaves the same on different projective representations of the same affine point
        var normalizedPoint = point.ToAffine().ToProjective();
        Assert.Equal(G2Projective.Psi(point), G2Projective.Psi(normalizedPoint));
        Assert.Equal(G2Projective.Psi2(point), G2Projective.Psi2(normalizedPoint));
    }

    [Fact]
    public void test_clear_cofactor()
    {
        var z2 = MakeFp2(
            [0x0ef2ddffab187c0a, 0x2424522b7d5ecbfc, 0xc6f341a3398054f4,
             0x5523ddf409502df0, 0xd55c0b5a88e0dd97, 0x066428d704923e52],
            [0x538bbe0c95b4878d, 0xad04a50379522881, 0x6d5c05bf5c12fb64,
             0x4ce4a069a2d34787, 0x59ea6c8d0dffaeaf, 0x0d42a083a75bd6f3]
        );

        var pointX = MakeFp2(
            [0xee4c8cb7c047eaf2, 0x44ca22eee036b604, 0x33b3affb2aefe101,
             0x15d3e45bbafaeb02, 0x7bfc2154cd7419a4, 0x0a2d0c2b756e5edc],
            [0xfc224361029a8777, 0x4cbf2baab8740924, 0xc5008c6ec6592c89,
             0xecc2c57b472a9c2d, 0x8613eafd9d81ffb1, 0x10fe54daa2d3d495]
        );
        var pointY = MakeFp2(
            [0x7de7edc43953b75c, 0x58be1d2de35e87dc, 0x5731d30b0e337b40,
             0xbe93b60cfeaae4c9, 0x8b22c203764bedca, 0x01616c8d1033b771],
            [0xea126fe476b5733b, 0x85cee68b5dae1652, 0x98247779f7272b04,
             0xa649c8b468c6e808, 0xb5b9a62dff0c4e45, 0x1555b67fc7bbe73d]
        );
        var point = new G2Projective(
            Fp2.Multiply(pointX, z2),
            pointY,
            Fp2.Multiply(Fp2.Square(z2), z2)
        );

        Assert.True(point.IsOnCurve());
        Assert.False(point.ToAffine().IsInSubgroup());
        var clearedPoint = point.ClearCofactor();
        Assert.True(clearedPoint.IsOnCurve());
        Assert.True(clearedPoint.ToAffine().IsInSubgroup());

        var generator = G2Projective.Generator;
        Assert.True(generator.ClearCofactor().IsOnCurve());
        Assert.True(G2Projective.Infinity.ClearCofactor().IsOnCurve());

        // h_eff % q = 0x2b116900400069009a40200040001ffff (little-endian bytes)
        byte[] hEffBytes =
        [
            0xff, 0xff, 0x01, 0x00, 0x04, 0x00, 0x02, 0xa4, 0x09, 0x90, 0x06, 0x00, 0x04, 0x90, 0x16,
            0xb1, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
        ];
        var hEff = Scalar.FromBytesLittleEndian(hEffBytes);
        Assert.Equal(generator.ClearCofactor(), G2Projective.ScalarMultiply(generator, hEff));
        Assert.Equal(clearedPoint.ClearCofactor(), G2Projective.ScalarMultiply(clearedPoint, hEff));
    }
}
