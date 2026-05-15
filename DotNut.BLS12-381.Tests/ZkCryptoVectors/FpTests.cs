using DotNut.BLS12_381.Tower;
using Xunit.Abstractions;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors;

public class FpTests
{

    [Fact]
    public void test_conditional_selection()
    {
        Fp a = new Fp([1, 2, 3, 4, 5, 6]);
        Fp b = new Fp([7, 8, 9, 10, 11, 12]);

        Assert.Equal(b, Fp.ConditionalSelect(0, a, b));
        Assert.Equal(a, Fp.ConditionalSelect(1, a, b));
    }

    public static IEnumerable<object[]> FpTestData()
    {
        yield return new object[]
        {
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            true,
        };
        yield return new object[]
        {
            new ulong[] { 7, 2, 3, 4, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 7, 2, 3, 4, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 1, 7, 3, 4, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 1, 2, 7, 4, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 1, 2, 3, 7, 5, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 1, 2, 3, 4, 7, 6 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
        yield return new object[]
        {
            new ulong[] { 1, 2, 3, 4, 5, 7 },
            new ulong[] { 1, 2, 3, 4, 5, 6 },
            false,
        };
    }

    [Theory]
    [MemberData(nameof(FpTestData))]
    public void test_equality(ulong[] limbsA, ulong[] limbsB, bool expected)
    {
        var Fp1 = new Fp(limbsA[0], limbsA[1], limbsA[2], limbsA[3], limbsA[4], limbsA[5]);
        var Fp2 = new Fp(limbsB[0], limbsB[1], limbsB[2], limbsB[3], limbsB[4], limbsB[5]);
        Assert.Equal(Fp1.Equals(Fp2), expected);
        Assert.Equal(Fp2.Equals(Fp1), expected);
        Assert.Equal(Fp1 == Fp2, expected);
        Assert.Equal(Equals(Fp1, Fp2), expected);
    }

    [Fact]
    public void test_squaring()
    {
        var a = new Fp([
            0xd215_d276_8e83_191b,
            0x5085_d80f_8fb2_8261,
            0xce9a_032d_df39_3a56,
            0x3e9c_4fff_2ca0_c4bb,
            0x6436_b6f7_f4d9_5dfb,
            0x1060_6628_ad4a_4d90,
        ]);
        var b = new Fp([
            0x33d9_c42a_3cb3_e235,
            0xdad1_1a09_4c4c_d455,
            0xa2f1_44bd_729a_aeba,
            0xd415_0932_be9f_feac,
            0xe27b_c7c4_7d44_ee50,
            0x14b6_a78d_3ec7_a560,
        ]);

        Assert.Equal(Fp.Square(a), b);
    }

    [Fact]
    public void test_multiplication()
    {
        var a = new Fp([
            0x0397_a383_2017_0cd4,
            0x734c_1b2c_9e76_1d30,
            0x5ed2_55ad_9a48_beb5,
            0x095a_3c6b_22a7_fcfc,
            0x2294_ce75_d4e2_6a27,
            0x1333_8bd8_7001_1ebb,
        ]);
        var b = new Fp([
            0xb9c3_c7c5_b119_6af7,
            0x2580_e208_6ce3_35c1,
            0xf49a_ed3d_8a57_ef42,
            0x41f2_81e4_9846_e878,
            0xe076_2346_c384_52ce,
            0x0652_e893_26e5_7dc0,
        ]);
        var c = new Fp([
            0xf96e_f3d7_11ab_5355,
            0xe8d4_59ea_00f1_48dd,
            0x53f7_354a_5f00_fa78,
            0x9e34_a4f3_125c_5f83,
            0x3fbe_0c47_ca74_c19e,
            0x01b0_6a8b_bd4a_dfe4,
        ]);

        Assert.Equal(Fp.Multiply(a, b), c);
    }

    [Fact]
    public void test_addition()
    {
        var a = new Fp([
            0x5360_bb59_7867_8032,
            0x7dd2_75ae_799e_128e,
            0x5c5b_5071_ce4f_4dcf,
            0xcdb2_1f93_078d_bb3e,
            0xc323_65c5_e73f_474a,
            0x115a_2a54_89ba_be5b,
        ]);
        var b = new Fp([
            0x9fd2_8773_3d23_dda0,
            0xb16b_f2af_738b_3554,
            0x3e57_a75b_d3cc_6d1d,
            0x900b_c0bd_627f_d6d6,
            0xd319_a080_efb2_45fe,
            0x15fd_caa4_e4bb_2091,
        ]);
        var c = new Fp([
            0x3934_42cc_b58b_b327,
            0x1092_685f_3bd5_47e3,
            0x3382_252c_ab6a_c4c9,
            0xf946_94cb_7688_7f55,
            0x4b21_5e90_93a5_e071,
            0x0d56_e30f_34f5_f853,
        ]);

        Assert.Equal(Fp.Add(a, b), c);
    }

    [Fact]
    public void test_subtraction()
    {
        var a = new Fp([
            0x5360_bb59_7867_8032,
            0x7dd2_75ae_799e_128e,
            0x5c5b_5071_ce4f_4dcf,
            0xcdb2_1f93_078d_bb3e,
            0xc323_65c5_e73f_474a,
            0x115a_2a54_89ba_be5b,
        ]);
        var b = new Fp([
            0x9fd2_8773_3d23_dda0,
            0xb16b_f2af_738b_3554,
            0x3e57_a75b_d3cc_6d1d,
            0x900b_c0bd_627f_d6d6,
            0xd319_a080_efb2_45fe,
            0x15fd_caa4_e4bb_2091,
        ]);
        var c = new Fp([
            0x6d8d_33e6_3b43_4d3d,
            0xeb12_82fd_b766_dd39,
            0x8534_7bb6_f133_d6d5,
            0xa21d_aa5a_9892_f727,
            0x3b25_6cfb_3ad8_ae23,
            0x155d_7199_de7f_8464,
        ]);

        Assert.Equal(Fp.Subtract(a, b), c);
    }

    [Fact]
    public void test_negation()
    {
        var a = new Fp([
            0x5360_bb59_7867_8032,
            0x7dd2_75ae_799e_128e,
            0x5c5b_5071_ce4f_4dcf,
            0xcdb2_1f93_078d_bb3e,
            0xc323_65c5_e73f_474a,
            0x115a_2a54_89ba_be5b,
        ]);
        var b = new Fp([
            0x669e_44a6_8798_2a79,
            0xa0d9_8a50_37b5_ed71,
            0x0ad5_822f_2861_a854,
            0x96c5_2bf1_ebf7_5781,
            0x87f8_41f0_5c0c_658c,
            0x08a6_e795_afc5_283e,
        ]);

        Assert.Equal(Fp.Negate(a), b);
    }

    [Fact]
    public void test_debug()
    {
        Assert.Equal(
            "0x104bf052ad3bc99bcb176c24a06a6c3aad4eaf2308fc4d282e106c84a757d061052630515305e59bdddf8111bfdeb704",
            Fp.ToHexString(
                new Fp([
                    0x5360_bb59_7867_8032,
                    0x7dd2_75ae_799e_128e,
                    0x5c5b_5071_ce4f_4dcf,
                    0xcdb2_1f93_078d_bb3e,
                    0xc323_65c5_e73f_474a,
                    0x115a_2a54_89ba_be5b,
                ])
            )
        );
    }

    [Fact]
    public void test_from_bytes()
    {
        var a = new Fp([
            0xdc90_6d9b_e3f9_5dc8,
            0x8755_caf7_4596_91a1,
            0xcff1_a7f4_e958_3ab3,
            0x9b43_821f_849e_2284,
            0xf575_54f3_a297_4f3f,
            0x085d_bea8_4ed4_7f79,
        ]);

        for (int i = 0; i < 100; i++)
        {
            a = Fp.Square(a);
            var tmp = new byte[48];
            Fp.ToBytesLittleEndian(a, tmp);
            var b = Fp.FromBytesLittleEndian(tmp);
            Assert.Equal(a, b);
            Fp.ToBytesBigEndian(a, tmp);
            b = Fp.FromBytesBigEndian(tmp);
            Assert.Equal(a, b);
        }

        Assert.Equal(
            Fp.Negate(Fp.One),
            Fp.FromBytesBigEndian([
                26, 1, 17, 234, 
                57, 127, 230, 154, 
                75, 27, 167, 182, 
                67, 75, 172, 215, 
                100, 119, 75, 132, 
                243, 133, 18, 191, 
                103, 48, 210, 160, 
                246, 176, 246, 36, 
                30, 171, 255, 254, 
                177, 83, 255, 255, 
                185, 254, 255, 255, 
                255, 255, 170, 170,
            ])
        );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Fp.FromBytesLittleEndian([ 
                27, 1, 17, 234, 
                57, 127, 230, 154, 
                75, 27, 167, 182, 
                67, 75, 172, 215, 
                100, 119, 75, 132, 
                243, 133, 18, 191, 
                103, 48, 210, 160, 
                246, 176, 246, 36, 
                30, 171, 255, 254, 
                177, 83, 255, 255, 
                185, 254, 255, 255, 
                255, 255, 170, 170,
            ])
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Fp.FromBytesBigEndian([ 
                27, 1, 17, 234, 
                57, 127, 230, 154, 
                75, 27, 167, 182, 
                67, 75, 172, 215, 
                100, 119, 75, 132, 
                243, 133, 18, 191, 
                103, 48, 210, 160, 
                246, 176, 246, 36, 
                30, 171, 255, 254, 
                177, 83, 255, 255, 
                185, 254, 255, 255, 
                255, 255, 170, 170,
            ])
        );
        Assert.False(
            Fp.TryFromBytesBigEndian([
                    27, 1, 17, 234, 
                    57, 127, 230, 154, 
                    75, 27, 167, 182, 
                    67, 75, 172, 215, 
                    100, 119, 75, 132, 
                    243, 133, 18, 191, 
                    103, 48, 210, 160, 
                    246, 176, 246, 36,
                    30, 171, 255, 254, 
                    177, 83, 255, 255, 
                    185, 254, 255, 255, 
                    255, 255, 170, 170,
                ],
                out _
            )
        );

        Assert.False(
            Fp.TryFromBytesLittleEndian([ 
                    27, 1, 17, 234, 
                    57, 127, 230, 154, 
                    75, 27, 167, 182, 
                    67, 75, 172, 215, 
                    100, 119, 75, 132, 
                    243, 133, 18, 191, 
                    103, 48, 210, 160, 
                    246, 176, 246, 36, 
                    30, 171, 255, 254, 
                    177, 83, 255, 255, 
                    185, 254, 255, 255, 
                    255, 255, 170, 170,
                ],
                out _
            )
        );
    }

    [Fact]
    public void test_sqrt() {
        // a = 4
        var a = new Fp(
            [
                0xaa27_0000_000c_fff3,
                0x53cc_0032_fc34_000a,
                0x478f_e97a_6b0a_807f,
                0xb1d3_7ebe_e6ba_24d7,
                0x8ec9_733b_bf78_ab2f,
                0x09d6_4551_3d83_de7e
            ]
        );
        
        // sqrt(4) = -2
        Assert.True(Fp.TrySqrt(a, out var fp));
    
        Assert.Equal(
                fp, 
                // negate so we'll have 2
                Fp.Negate(
                    new Fp(
                0x3213_0000_0006_554f,
                0xb93c_0018_d6c4_0005,
                0x5760_5e0d_b0dd_bb51,
                0x8b25_6521_ed1f_9bcb,
                0x6cf2_8d79_0162_2c03,
                0x11eb_ab9d_bb81_e28c)
            ));
    }

    [Fact]
    public void test_inversion()
    {
        var a = new Fp(
            0x43b4_3a50_78ac_2076,
            0x1ce0_7630_46f8_962b,
            0x724a_5276_486d_735c,
            0x6f05_c2a6_282d_48fd,
            0x2095_bd5b_b4ca_9331,
            0x03b3_5b38_94b0_f7da
        );
        var b = new Fp(
            0x69ec_d704_0952_148f,
            0x985c_cc20_2219_0f55,
            0xe19b_ba36_a9ad_2f41,
            0x19bb_16c9_5219_dbd8,
            0x14dc_acfd_fb47_8693,
            0x115f_f58a_fff9_a8e1
        );

        Assert.Equal(Fp.Invert(a), b);
        Assert.Throws<DivideByZeroException>(() => Fp.Invert(Fp.Zero));
    }

    [Fact]
    public void test_lexicographic_largest()
    {
        Assert.False(Fp.LexicographicallyLargest(Fp.Zero));
        Assert.False(Fp.LexicographicallyLargest(Fp.One));
        Assert.False(
            Fp.LexicographicallyLargest(
                new Fp([
                    0xa1fa_ffff_fffe_5557,
                    0x995b_fff9_76a3_fffe,
                    0x03f4_1d24_d174_ceb4,
                    0xf654_7998_c199_5dbd,
                    0x778a_468f_507a_6034,
                    0x0205_5993_1f7f_8103,
                ])
            )
        );
        Assert.True(
            Fp.LexicographicallyLargest(
                new Fp([
                    0x1804_0000_0001_5554,
                    0x8550_0005_3ab0_0001,
                    0x633c_b57c_253c_276f,
                    0x6e22_d1ec_31eb_b502,
                    0xd391_6126_f2d1_4ca2,
                    0x17fb_b857_1a00_6596,
                ])
            )
        );
        Assert.True(
            Fp.LexicographicallyLargest(
                new Fp([
                    0x43f5_ffff_fffc_aaae,
                    0x32b7_fff2_ed47_fffd,
                    0x07e8_3a49_a2e9_9d69,
                    0xeca8_f331_8332_bb7a,
                    0xef14_8d1e_a0f4_c069,
                    0x040a_b326_3eff_0206,
                ])
            )
        );
    }
}
