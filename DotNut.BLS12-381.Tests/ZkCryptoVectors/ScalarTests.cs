namespace DotNut.BLS12_381.Tests.ZkCryptoVectors;

public class ScalarTests
{
    private static readonly Scalar R = Scalar.One;
    private static readonly Scalar R2 = Scalar.R2;
    private static readonly Scalar R3 = Scalar.R3;
    private static readonly Scalar Largest = new Scalar(
        0xffff_ffff_0000_0000UL, 0x53bd_a402_fffe_5bfeUL,
        0x3339_d808_09a1_d805UL, 0x73ed_a753_299d_7d48UL);

    [Fact]
    public void test_constants()
    {
        Assert.Equal(
            "0x73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            $"0x{Scalar.GroupOrderR.L3:x16}{Scalar.GroupOrderR.L2:x16}{Scalar.GroupOrderR.L1:x16}{Scalar.GroupOrderR.L0:x16}");

        Assert.Equal(Scalar.One, Scalar.Mul(Scalar.From(2), Scalar.TwoInv));

        Assert.Equal(Scalar.One, Scalar.Mul(Scalar.RootOfUnity, Scalar.RootOfUnityInv));

        // ROOT_OF_UNITY^{2^s} mod m == 1
        Assert.Equal(
            Scalar.One,
            Scalar.Pow(Scalar.RootOfUnity, [1UL << Scalar.TwoAdicity, 0UL, 0UL, 0UL]));

        // DELTA^{t} mod m == 1
        Assert.Equal(
            Scalar.One,
            Scalar.Pow(Scalar.Delta, [
                0xfffe_5bfe_ffff_ffffUL,
                0x09a1_d805_53bd_a402UL,
                0x299d_7d48_3339_d808UL,
                0x0000_0000_73ed_a753UL,
            ]));
    }

    [Fact]
    public void test_inv()
    {
        // Compute -(q^{-1} mod 2^64) mod 2^64 by exponentiating
        // by totient(2**64) - 1
        ulong inv = 1UL;
        ulong modL0 = Scalar.GroupOrderR.L0;
        for (int i = 0; i < 63; i++)
        {
            inv = unchecked(inv * inv);
            inv = unchecked(inv * modL0);
        }
        inv = unchecked(0UL - inv);
        Assert.Equal(Scalar.MontgomeryInv, inv);
    }

    [Fact]
    public void test_debug()
    {
        // there's no debug here. we're testing ToHexString()
        Assert.Equal(
            "0x0000000000000000000000000000000000000000000000000000000000000000",
            Scalar.ToHexString(Scalar.Zero));
        Assert.Equal(
            "0x0000000000000000000000000000000000000000000000000000000000000001",
            Scalar.ToHexString(Scalar.One));
        Assert.Equal(
            "0x1824b159acc5056f998c4fefecbc4ff55884b7fa0003480200000001fffffffe",
            Scalar.ToHexString(R2));
    }

    public static IEnumerable<object[]> EqTestData()
    {
        yield return new object[]
        {
            Scalar.Zero, Scalar.Zero, true
        };
        yield return new object[]
        {
            Scalar.One, Scalar.One, true
        };
        yield return new object[]
        {
            R2, R2, true
        };
        yield return new object[]
        {
            Scalar.Zero, Scalar.One, false
        };
        yield return new object[]
        {
            Scalar.One, Scalar.R2, false
        };
    }
    [Theory]
    [MemberData(nameof(EqTestData))]
    public void test_equality(Scalar a, Scalar b, bool shouldEq)
    {
        if (shouldEq)
        {
            Assert.Equal(a, b);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
        }
        else
        {
            Assert.NotEqual(a, b);
            Assert.False(a.Equals(b));
            Assert.False(a == b);
            Assert.True(a != b);
        }
    }

    [Fact]
    public void test_to_bytes()
    {
        var buf = new byte[32];

        Scalar.ToBytesLittleEndian(Scalar.Zero, buf);
        Assert.Equal(new byte[32], buf);

        Scalar.ToBytesLittleEndian(Scalar.One, buf);
        var expectedOne = new byte[32]; expectedOne[0] = 1;
        Assert.Equal(expectedOne, buf);

        Scalar.ToBytesLittleEndian(R2, buf);
        Assert.Equal(new byte[]
        {
            254, 255, 255, 255, 1, 0, 0, 0, 2, 72, 3, 0, 250, 183, 132, 88, 245, 79, 188, 236, 239,
            79, 140, 153, 111, 5, 197, 172, 89, 177, 36, 24
        }, buf);

        Scalar.ToBytesLittleEndian(Scalar.Negate(Scalar.One), buf);
        Assert.Equal(new byte[]
        {
            0, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 115
        }, buf);
    }

    [Fact]
    public void test_from_bytes()
    {
        Assert.Equal(Scalar.Zero, Scalar.FromBytesLittleEndian(new byte[32]));

        var oneBytes = new byte[32]; oneBytes[0] = 1;
        Assert.Equal(Scalar.One, Scalar.FromBytesLittleEndian(oneBytes));

        Assert.Equal(R2, Scalar.FromBytesLittleEndian(new byte[]
        {
            254, 255, 255, 255, 1, 0, 0, 0, 2, 72, 3, 0, 250, 183, 132, 88, 245, 79, 188, 236, 239,
            79, 140, 153, 111, 5, 197, 172, 89, 177, 36, 24
        }));
        
        // -1 should work
        Assert.True(Scalar.TryFromBytesLittleEndian(new byte[]
        {
            0, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 115
        }, out _));

        // modulus is invalid
        Assert.False(Scalar.TryFromBytesLittleEndian(new byte[]
        {
            1, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 115
        }, out _));
        // Anything larger than the modulus is invalid
        Assert.False(Scalar.TryFromBytesLittleEndian(new byte[]
        {
            2, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 115
        }, out _));

        Assert.False(Scalar.TryFromBytesLittleEndian(new byte[]
        {
            1, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 58, 51, 72, 125, 157, 41, 83, 167, 237, 115
        }, out _));

        Assert.False(Scalar.TryFromBytesLittleEndian(new byte[]
        {
            1, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
            216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 116
        }, out _));
    }

    [Fact]
    public void test_from_u512_zero()
    {
        Assert.Equal(
            Scalar.Zero,
            Scalar.FromU512(
                Scalar.GroupOrderR.L0, Scalar.GroupOrderR.L1,
                Scalar.GroupOrderR.L2, Scalar.GroupOrderR.L3,
                0, 0, 0, 0));
    }

    [Fact]
    public void test_from_u512_r()
    {
        Assert.Equal(R, Scalar.FromU512(1, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void test_from_u512_r2()
    {
        Assert.Equal(R2, Scalar.FromU512(0, 0, 0, 0, 1, 0, 0, 0));
    }

    [Fact]
    public void test_from_u512_max()
    {
        const ulong maxU64 = 0xffff_ffff_ffff_ffffUL;
        Assert.Equal(
            Scalar.Sub(R3, R),
            Scalar.FromU512(maxU64, maxU64, maxU64, maxU64, maxU64, maxU64, maxU64, maxU64));
    }

    [Fact]
    public void test_from_bytes_wide_r2()
    {
        var bytes = new byte[64];
        Scalar.ToBytesLittleEndian(R2, bytes.AsSpan(0, 32));
        Assert.Equal(R2, Scalar.FromBytesWide(bytes));
    }

    [Fact]
    public void test_from_bytes_wide_negative_one()
    {
        Assert.Equal(
            Scalar.Negate(Scalar.One),
            Scalar.FromBytesWide(new byte[]
            {
                0, 0, 0, 0, 255, 255, 255, 255, 254, 91, 254, 255, 2, 164, 189, 83, 5, 216, 161, 9, 8,
                216, 57, 51, 72, 125, 157, 41, 83, 167, 237, 115, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }));
    }

    [Fact]
    public void test_from_bytes_wide_maximum()
    {
        var allFF = new byte[64];
        Array.Fill(allFF, (byte)0xff);
        Assert.Equal(
            new Scalar(0xc62c_1805_439b_73b1UL, 0xc2b9_551e_8ced_218eUL,
                       0xda44_ec81_daf9_a422UL, 0x5605_aa60_1c16_2e79UL),
            Scalar.FromBytesWide(allFF));
    }

    [Fact]
    public void test_zero()
    {
        Assert.Equal(Scalar.Zero, Scalar.Negate(Scalar.Zero));
        Assert.Equal(Scalar.Zero, Scalar.Add(Scalar.Zero, Scalar.Zero));
        Assert.Equal(Scalar.Zero, Scalar.Sub(Scalar.Zero, Scalar.Zero));
        Assert.Equal(Scalar.Zero, Scalar.Mul(Scalar.Zero, Scalar.Zero));
    }

    [Fact]
    public void test_addition()
    {
        Assert.Equal(
            new Scalar(0xffff_fffe_ffff_ffffUL, 0x53bd_a402_fffe_5bfeUL,
                       0x3339_d808_09a1_d805UL, 0x73ed_a753_299d_7d48UL),
            Scalar.Add(Largest, Largest));

        Assert.Equal(Scalar.Zero, Scalar.Add(Largest, new Scalar(1UL, 0UL, 0UL, 0UL)));
    }

    [Fact]
    public void test_negation()
    {
        Assert.Equal(new Scalar(1UL, 0UL, 0UL, 0UL), Scalar.Negate(Largest));
        Assert.Equal(Scalar.Zero, Scalar.Negate(Scalar.Zero));
        Assert.Equal(Largest, Scalar.Negate(new Scalar(1UL, 0UL, 0UL, 0UL)));
    }

    [Fact]
    public void test_subtraction()
    {
        Assert.Equal(Scalar.Zero, Scalar.Sub(Largest, Largest));

        var tmp  = Scalar.Sub(Scalar.Zero, Largest);
        var tmp2 = Scalar.Sub(Scalar.GroupOrderR, Largest);
        Assert.Equal(tmp, tmp2);
    }

    [Fact]
    public void test_multiplication()
    {
        var cur = Largest;
        for (int iter = 0; iter < 100; iter++)
        {
            var tmp = Scalar.Mul(cur, cur);

            var tmp2  = Scalar.Zero;
            var bytes = new byte[32];
            Scalar.ToBytesLittleEndian(cur, bytes);
            for (int b = 31; b >= 0; b--)
                for (int bit = 7; bit >= 0; bit--)
                {
                    tmp2 = Scalar.Add(tmp2, tmp2);
                    if (((bytes[b] >> bit) & 1) != 0)
                        tmp2 = Scalar.Add(tmp2, cur);
                }

            Assert.Equal(tmp, tmp2);
            cur = Scalar.Add(cur, Largest);
        }
    }

    [Fact]
    public void test_squaring()
    {
        var cur = Largest;
        for (int iter = 0; iter < 100; iter++)
        {
            var tmp = Scalar.Square(cur);

            var tmp2  = Scalar.Zero;
            var bytes = new byte[32];
            Scalar.ToBytesLittleEndian(cur, bytes);
            for (int b = 31; b >= 0; b--)
                for (int bit = 7; bit >= 0; bit--)
                {
                    tmp2 = Scalar.Add(tmp2, tmp2);
                    if (((bytes[b] >> bit) & 1) != 0)
                        tmp2 = Scalar.Add(tmp2, cur);
                }

            Assert.Equal(tmp, tmp2);
            cur = Scalar.Add(cur, Largest);
        }
    }

    [Fact]
    public void test_inversion()
    {
        Assert.Throws<DivideByZeroException>(() => Scalar.Invert(Scalar.Zero));
        Assert.Equal(Scalar.One, Scalar.Invert(Scalar.One));
        Assert.Equal(Scalar.Negate(Scalar.One), Scalar.Invert(Scalar.Negate(Scalar.One)));

        var tmp = R2;
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(Scalar.One, Scalar.Mul(Scalar.Invert(tmp), tmp));
            tmp = Scalar.Add(tmp, R2);
        }
    }

    [Fact]
    public void test_invert_is_pow()
    {
        ulong[] qMinus2 = [
            0xffff_fffe_ffff_ffffUL,
            0x53bd_a402_fffe_5bfeUL,
            0x3339_d808_09a1_d805UL,
            0x73ed_a753_299d_7d48UL,
        ];

        var r1 = R;
        var r2 = R;
        for (int i = 0; i < 100; i++)
        {
            r1 = Scalar.Invert(r1);
            r2 = Scalar.Pow(r2, qMinus2);
            Assert.Equal(r1, r2);
            r1 = Scalar.Add(r1, R);
            r2 = r1;
        }
    }

    [Fact]
    public void test_sqrt()
    {
        Assert.Equal(Scalar.Zero, Scalar.Sqrt(Scalar.Zero)!.Value);

        var square = new Scalar(
            0x46cd_85a5_f273_077eUL, 0x1d30_c47d_d68f_c735UL,
            0x77f6_56f6_0bec_a0ebUL, 0x494a_a01b_df32_468dUL);

        int noneCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var sr = Scalar.Sqrt(square);
            if (sr is null)
                noneCount++;
            else
                Assert.Equal(square, Scalar.Mul(sr.Value, sr.Value));
            square = Scalar.Sub(square, Scalar.One);
        }
        Assert.Equal(49, noneCount);
    }

    [Fact]
    public void test_from_raw()
    {
        Assert.Equal(
            Scalar.FromCanonical(new Scalar(0x0001_ffff_fffdUL, 0x5884_b7fa_0003_4802UL,
                                            0x998c_4fef_ecbc_4ff5UL, 0x1824_b159_acc5_056fUL)),
            Scalar.FromCanonical(new Scalar(0xffff_ffff_ffff_ffffUL, 0xffff_ffff_ffff_ffffUL,
                                            0xffff_ffff_ffff_ffffUL, 0xffff_ffff_ffff_ffffUL)));

        Assert.Equal(
            Scalar.Zero,
            Scalar.FromCanonical(new Scalar(Scalar.GroupOrderR.L0, Scalar.GroupOrderR.L1,
                                            Scalar.GroupOrderR.L2, Scalar.GroupOrderR.L3)));

        Assert.Equal(R, Scalar.FromCanonical(new Scalar(1UL, 0UL, 0UL, 0UL)));
    }

    [Fact]
    public void test_double()
    {
        var a = new Scalar(
            0x1fff_3231_233f_fffdUL, 0x4884_b7fa_0003_4802UL,
            0x998c_4fef_ecbc_4ff3UL, 0x1824_b159_acc5_0562UL);
        Assert.Equal(Scalar.Add(a, a), Scalar.Add(a, a));
    }
}
