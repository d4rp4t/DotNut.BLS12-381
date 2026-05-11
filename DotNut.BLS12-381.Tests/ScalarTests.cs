using System.Numerics;

namespace DotNut.BLS12_381.Tests;

// Source: https://github.com/zkcrypto/bls12_381/blob/main/src/scalar.rs
public sealed class ScalarTests
{
    private static readonly BigInteger R = BigInteger.Parse(
        "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
        System.Globalization.NumberStyles.AllowHexSpecifier);

    // LE bytes of r-1 (= largest canonical Scalar, = Negate(One))
    private static readonly byte[] RMinus1Le =
    [
        0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
        0xfe, 0x5b, 0xfe, 0xff, 0x02, 0xa4, 0xbd, 0x53,
        0x05, 0xd8, 0xa1, 0x09, 0x08, 0xd8, 0x39, 0x33,
        0x48, 0x7d, 0x9d, 0x29, 0x53, 0xa7, 0xed, 0x73,
    ];

    #region Serialization

    [Fact]
    public void ToBytesLE_Zero_AllZeros()
    {
        var buf = new byte[32];
        Scalar.ToBytesLittleEndian(Scalar.Zero, buf);
        Assert.Equal(new byte[32], buf);
    }

    [Fact]
    public void ToBytesLE_One_CanonicalOne()
    {
        var expected = new byte[32];
        expected[0] = 1;
        var buf = new byte[32];
        Scalar.ToBytesLittleEndian(Scalar.One, buf);
        Assert.Equal(expected, buf);
    }

    [Fact]
    public void ToBytesLE_NegateOne_CanonicalRMinusOne()
    {
        var buf = new byte[32];
        Scalar.ToBytesLittleEndian(Scalar.Negate(Scalar.One), buf);
        Assert.Equal(RMinus1Le, buf);
    }

    [Fact]
    public void ToBytesLE_FortyTwo_CorrectEncoding()
    {
        var expected = new byte[32];
        expected[0] = 42;
        var buf = new byte[32];
        Scalar.ToBytesLittleEndian(Scalar.FromBigInteger(42), buf);
        Assert.Equal(expected, buf);
    }

    [Fact]
    public void ToBE_FromBE_Roundtrip()
    {
        var buf = new byte[32];
        var s = Scalar.FromBigInteger(12345678901234567890UL);
        Scalar.ToBytesBigEndian(s, buf);
        var back = Scalar.FromBytesBigEndian(buf);
        Assert.True(Scalar.Equal(s, back));
    }

    [Fact]
    public void FromBytesLE_Roundtrip_One()
    {
        var oneBuf = new byte[32];
        oneBuf[0] = 1;
        Assert.True(Scalar.Equal(Scalar.FromBytesLittleEndian(oneBuf), Scalar.One));
    }

    [Fact]
    public void FromBytesLE_Roundtrip_RMinusOne()
    {
        Assert.True(Scalar.Equal(Scalar.FromBytesLittleEndian(RMinus1Le), Scalar.Negate(Scalar.One)));
    }

    [Fact]
    public void FromBytesLE_Modulus_Throws()
    {
        var rLE = new byte[]
        {
            0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
            0xfe, 0x5b, 0xfe, 0xff, 0x02, 0xa4, 0xbd, 0x53,
            0x05, 0xd8, 0xa1, 0x09, 0x08, 0xd8, 0x39, 0x33,
            0x48, 0x7d, 0x9d, 0x29, 0x53, 0xa7, 0xed, 0x73,
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => Scalar.FromBytesLittleEndian(rLE));
    }

    [Fact]
    public void TryFromBytesLE_Modulus_ReturnsFalse()
    {
        var rLE = new byte[]
        {
            0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
            0xfe, 0x5b, 0xfe, 0xff, 0x02, 0xa4, 0xbd, 0x53,
            0x05, 0xd8, 0xa1, 0x09, 0x08, 0xd8, 0x39, 0x33,
            0x48, 0x7d, 0x9d, 0x29, 0x53, 0xa7, 0xed, 0x73,
        };
        Assert.False(Scalar.TryFromBytesLittleEndian(rLE, out _));
    }

    #endregion

    #region Addition

    [Fact]
    public void Add_Zero_Zero_IsZero()
    {
        Assert.True(Scalar.Equal(Scalar.Add(Scalar.Zero, Scalar.Zero), Scalar.Zero));
    }

    [Fact]
    public void Add_LARGEST_PlusOne_IsZero()
    {
        // (r-1) + 1 = 0 (mod r)
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Add(largest, Scalar.One), Scalar.Zero));
    }

    [Fact]
    public void Add_LARGEST_PlusLARGEST_IsRMinusTwo()
    {
        // (r-1) + (r-1) = 2r-2 = r-2 (mod r)
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        var expected = Scalar.FromBigInteger(R - 2);
        Assert.True(Scalar.Equal(Scalar.Add(largest, largest), expected));
    }

    #endregion

    #region Subtraction

    [Fact]
    public void Sub_SelfSelf_IsZero()
    {
        var a = Scalar.FromBigInteger(12345);
        Assert.True(Scalar.Equal(Scalar.Sub(a, a), Scalar.Zero));
    }

    [Fact]
    public void Sub_ZeroMinusOne_IsRMinusOne()
    {
        // 0 - 1 = r-1 (mod r)
        var expected = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Sub(Scalar.Zero, Scalar.One), expected));
    }

    #endregion

    #region Negation

    [Fact]
    public void Negate_Zero_IsZero()
    {
        Assert.True(Scalar.Equal(Scalar.Negate(Scalar.Zero), Scalar.Zero));
    }

    [Fact]
    public void Negate_One_IsRMinusOne()
    {
        var expected = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Negate(Scalar.One), expected));
    }

    [Fact]
    public void Negate_RMinusOne_IsOne()
    {
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Negate(largest), Scalar.One));
    }

    [Fact]
    public void Negate_Involutive()
    {
        // -(-a) = a
        var a = Scalar.FromBigInteger(987654321);
        Assert.True(Scalar.Equal(Scalar.Negate(Scalar.Negate(a)), a));
    }

    #endregion

    #region Multiplication

    [Fact]
    public void Mul_Zero_IsZero()
    {
        var a = Scalar.FromBigInteger(999);
        Assert.True(Scalar.Equal(Scalar.Mul(a, Scalar.Zero), Scalar.Zero));
        Assert.True(Scalar.Equal(Scalar.Mul(Scalar.Zero, a), Scalar.Zero));
    }

    [Fact]
    public void Mul_One_IsIdentity()
    {
        var a = Scalar.FromBigInteger(0x1234567890ABCDEFul);
        Assert.True(Scalar.Equal(Scalar.Mul(a, Scalar.One), a));
        Assert.True(Scalar.Equal(Scalar.Mul(Scalar.One, a), a));
    }

    [Fact]
    public void Mul_NegOneSquared_IsOne()
    {
        // (-1)^2 = 1
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Mul(largest, largest), Scalar.One));
    }

    [Fact]
    public void Mul_Commutative()
    {
        var a = Scalar.FromBigInteger(0xDEADBEEFCAFEBABEul);
        var b = Scalar.FromBigInteger(0x0102030405060708ul);
        Assert.True(Scalar.Equal(Scalar.Mul(a, b), Scalar.Mul(b, a)));
    }

    #endregion

    #region Squaring

    [Fact]
    public void Square_MatchesMul()
    {
        var rng = new Random(42);
        for (int i = 0; i < 32; i++)
        {
            var n = (ulong)rng.NextInt64() & 0x7FFF_FFFF_FFFF_FFFFul;
            var a = Scalar.FromBigInteger(n);
            Assert.True(Scalar.Equal(Scalar.Square(a), Scalar.Mul(a, a)));
        }
    }

    [Fact]
    public void Square_RMinusOne_IsOne()
    {
        // (r-1)^2 = (-1)^2 = 1
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Square(largest), Scalar.One));
    }

    #endregion

    #region Inversion

    [Fact]
    public void Invert_Zero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Scalar.Invert(Scalar.Zero));
    }

    [Fact]
    public void Invert_One_IsOne()
    {
        Assert.True(Scalar.Equal(Scalar.Invert(Scalar.One), Scalar.One));
    }

    [Fact]
    public void Invert_NegOne_IsNegOne()
    {
        // (-1)^(-1) = -1
        var largest = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(Scalar.Invert(largest), largest));
    }

    [Fact]
    public void Invert_Iterative_MulEqualsOne()
    {
        // a * a^(-1) = 1 for 100 consecutive values; mirrors zkcrypto test_inversion
        var step = Scalar.FromBigInteger(2);
        var cur = step;
        for (int i = 0; i < 100; i++)
        {
            var inv = Scalar.Invert(cur);
            Assert.True(Scalar.Equal(Scalar.Mul(cur, inv), Scalar.One));
            cur = Scalar.Add(cur, step);
        }
    }

    #endregion

    #region FromBytesWide

    [Fact]
    public void FromBytesWide_AllZeros_IsZero()
    {
        Assert.True(Scalar.Equal(Scalar.FromBytesWide(new byte[64]), Scalar.Zero));
    }

    [Fact]
    public void FromBytesWide_NegativeOne_IsNegateOne()
    {
        // Source: zkcrypto test_from_bytes_wide_negative_one
        // Input = LE(r-1) || 00...0 -> canonical r-1 mod r = Negate(One)
        var input = new byte[64];
        RMinus1Le.CopyTo(input, 0);
        Assert.True(Scalar.Equal(Scalar.FromBytesWide(input), Scalar.Negate(Scalar.One)));
    }

    [Fact]
    public void FromBytesWide_AllFf_SpecificVector()
    {
        // Source: zkcrypto test_from_bytes_wide_maximum
        // Verified Montgomery-form limbs: Scalar([0xc62c1805439b73b1, ...])
        var input = new byte[64];
        Array.Fill(input, (byte)0xff);
        var result = Scalar.FromBytesWide(input);
        var expected = new Scalar(
            0xc62c_1805_439b_73b1UL,
            0xc2b9_551e_8ced_218eUL,
            0xda44_ec81_daf9_a422UL,
            0x5605_aa60_1c16_2e79UL);
        Assert.True(Scalar.Equal(result, expected));
    }

    [Fact]
    public void FromBytesWide_R2Vector_SpecificVector()
    {
        // Source: zkcrypto test_from_bytes_wide_r2
        // Input = LE(R mod r) || 00...0  ->  R^2 mod r  (the R2 helper constant)
        var input = new byte[64];
        new byte[]
        {
            0xfe, 0xff, 0xff, 0xff, 0x01, 0x00, 0x00, 0x00,
            0x02, 0x48, 0x03, 0x00, 0xfa, 0xb7, 0x84, 0x58,
            0xf5, 0x4f, 0xbc, 0xec, 0xef, 0x4f, 0x8c, 0x99,
            0x6f, 0x05, 0xc5, 0xac, 0x59, 0xb1, 0x24, 0x18,
        }.CopyTo(input, 0);
        var result = Scalar.FromBytesWide(input);
        var expected = new Scalar(
            0xc999_e990_f3f2_9c6dUL,
            0x2b6c_edcb_8792_5c23UL,
            0x05d3_1496_7254_398fUL,
            0x0748_d9d9_9f59_ff11UL);
        Assert.True(Scalar.Equal(result, expected));
    }

    #endregion

    #region IsZero_Equal

    [Fact]
    public void IsZero_OnlyTrueForZero()
    {
        Assert.True(Scalar.IsZero(Scalar.Zero));
        Assert.False(Scalar.IsZero(Scalar.One));
        Assert.False(Scalar.IsZero(Scalar.FromBytesLittleEndian(RMinus1Le)));
    }

    [Fact]
    public void Equal_SameCanonicalValue_IsTrue()
    {
        var a = Scalar.FromBigInteger(R - 1);
        var b = Scalar.FromBytesLittleEndian(RMinus1Le);
        Assert.True(Scalar.Equal(a, b));
    }

    [Fact]
    public void Equal_DifferentValues_IsFalse()
    {
        Assert.False(Scalar.Equal(Scalar.One, Scalar.Zero));
        Assert.False(Scalar.Equal(Scalar.One, Scalar.FromBigInteger(2)));
    }

    #endregion
}
