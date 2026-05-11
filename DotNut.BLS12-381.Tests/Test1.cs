using System.Numerics;
using System.Reflection;
using DotNut.BLS12_381.Tower;
using Xunit;

namespace DotNut.BLS12_381.Tests;

public sealed class Test1
{
    // Source (official modulus p for BLS12-381 Fp):
    // RFC 9380, section 8.8.1
    // https://www.rfc-editor.org/rfc/rfc9380#section-8.8.1
    // Cross-check same constant in noble-curves:
    // https://github.com/paulmillr/noble-curves/blob/main/src/bls12-381.ts
    private static readonly BigInteger Modulus = BigInteger.Parse(
        "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
        System.Globalization.NumberStyles.AllowHexSpecifier
    );

    [Fact]
    public void Add_ShouldMatchBigInteger_ModP()
    {
        var a = FromBigInteger(Modulus - 1);
        var b = Fp.One;

        var result = Fp.Add(a, b);

        var expected = (ToBigInteger(a) + ToBigInteger(b)) % Modulus;
        Assert.Equal(expected, ToBigInteger(result));
    }

    [Fact]
    public void Subtract_ShouldMatchBigInteger_ModP()
    {
        var a = Fp.Zero;
        var b = Fp.One;

        var result = Fp.Subtract(a, b);

        var expected = PositiveMod(ToBigInteger(a) - ToBigInteger(b), Modulus);
        Assert.Equal(expected, ToBigInteger(result));
    }

    [Fact]
    public void Subtract_ShouldWork_ForSimpleCase()
    {
        var a = FromBigInteger(123) ;
        var b = FromBigInteger(10);

        var subtract = Fp.Subtract(a, b);
        Assert.Equal(new BigInteger(113), ToBigInteger(subtract));
    }

    [Fact]
    public void Add_And_Subtract_Randomized_ShouldMatchBigInteger_ModP()
    {
        var random = new Random(12345);

        for (var i = 0; i < 250; i++)
        {
            var a = RandomCanonicalFp(random);
            var b = RandomCanonicalFp(random);

            var addExpected = (ToBigInteger(a) + ToBigInteger(b)) % Modulus;
            var subExpected = PositiveMod(ToBigInteger(a) - ToBigInteger(b), Modulus);

            Assert.Equal(addExpected, ToBigInteger(Fp.Add(a, b)));
            Assert.Equal(subExpected, ToBigInteger(Fp.Subtract(a, b)));
        }
    }

    [Fact]
    public void Multiply_ShouldMatchBigInteger_ModP()
    {
        var a = FromBigInteger(Modulus - 1);
        var b = FromBigInteger(Modulus - 1);

        var expected = (ToBigInteger(a) * ToBigInteger(b)) % Modulus;
        var actual = ToBigInteger(Fp.Multiply(a, b));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Multiply_Randomized_ShouldMatchBigInteger_ModP()
    {
        var random = new Random(54321);

        for (var i = 0; i < 250; i++)
        {
            var a = RandomCanonicalFp(random);
            var b = RandomCanonicalFp(random);

            var expected = (ToBigInteger(a) * ToBigInteger(b)) % Modulus;
            var actual = ToBigInteger(Fp.Multiply(a, b));

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Square_Randomized_ShouldMatchBigInteger_ModP()
    {
        var random = new Random(98765);

        for (var i = 0; i < 250; i++)
        {
            var a = RandomCanonicalFp(random);
            var expected = (ToBigInteger(a) * ToBigInteger(a)) % Modulus;
            var actual = ToBigInteger(Fp.Square(a));
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Negate_ShouldMatchBigInteger_ModP()
    {
        var random = new Random(24680);
        for (var i = 0; i < 200; i++)
        {
            var a = RandomCanonicalFp(random);
            var expected = PositiveMod(-ToBigInteger(a), Modulus);
            Assert.Equal(expected, ToBigInteger(Fp.Negate(a)));
        }
        Assert.Equal(BigInteger.Zero, ToBigInteger(Fp.Negate(Fp.Zero)));
    }

    [Fact]
    public void Invert_Randomized_ShouldMatchBigInteger_ModP()
    {
        var random = new Random(112233);
        for (var i = 0; i < 120; i++)
        {
            var a = RandomNonZeroCanonicalFp(random);
            var inv = Fp.Invert(a);
            Assert.Equal(BigInteger.One, ToBigInteger(Fp.Multiply(a, inv)));
        }
    }

    [Fact]
    public void Invert_Zero_ShouldThrow()
    {
        Assert.Throws<DivideByZeroException>(() => Fp.Invert(Fp.Zero));
    }

    [Fact]
    public void Compare_Fp2_Fp6_Fp12_ShouldBeLexicographic()
    {
        var z = Fp.Zero;
        var o = Fp.One;
        var t = FromBigInteger(2);

        var fp2a = new Fp2(o, z);
        var fp2b = new Fp2(z, o);
        Assert.True(Fp2.Compare(fp2a, fp2b) < 0);

        var fp6a = new Fp6(fp2a, fp2a, fp2a);
        var fp6b = new Fp6(fp2a, fp2a, fp2b);
        Assert.True(Fp6.Compare(fp6a, fp6b) < 0);

        var fp12a = new Fp12(fp6a, fp6a);
        var fp12b = new Fp12(fp6a, new Fp6(fp2a, fp2a, new Fp2(t, z)));
        Assert.True(Fp12.Compare(fp12a, fp12b) < 0);
    }

    [Fact]
    public void Modulus_ShouldMatch_Rfc9380_Bls12_381_P()
    {
        // Source:
        // https://www.rfc-editor.org/rfc/rfc9380#section-8.8.1
        const string pHex = "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab";
        var expected = BigInteger.Parse(pHex, System.Globalization.NumberStyles.AllowHexSpecifier);
        Assert.Equal(expected, RawToBigInteger(Fp.Modulus));
    }

    [Fact]
    public void ToBytes_And_FromBytes_BigEndian_RoundTrip()
    {
        var random = new Random(777);
        for (var i = 0; i < 100; i++)
        {
            var a = RandomCanonicalFp(random);
            var bytes = new byte[48];
            Fp.ToBytesBigEndian(a, bytes);
            var b = Fp.FromBytesBigEndian(bytes);
            Assert.Equal(ToBigInteger(a), ToBigInteger(b));
        }
    }

    [Fact]
    public void FromBytesBigEndian_ShouldReject_NonCanonical_Modulus()
    {
        // Canonical encoding must be strictly < p (reject p itself).
        // Source for p:
        // https://www.rfc-editor.org/rfc/rfc9380#section-8.8.1
        const string pHex = "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab";
        byte[] bytes = Convert.FromHexString(pHex);
        Assert.Throws<ArgumentOutOfRangeException>(() => Fp.FromBytesBigEndian(bytes));
        Assert.False(Fp.TryFromBytesBigEndian(bytes, out _));
    }

    [Fact]
    public void FromBytesBigEndian_ShouldParse_One()
    {
        var bytes = new byte[48];
        bytes[47] = 1;
        var one = Fp.FromBytesBigEndian(bytes);
        Assert.Equal(BigInteger.One, ToBigInteger(one));
    }

    [Fact]
    public void ToBytes_And_FromBytes_LittleEndian_RoundTrip()
    {
        var random = new Random(778);
        for (var i = 0; i < 100; i++)
        {
            var a = RandomCanonicalFp(random);
            var bytes = new byte[48];
            Fp.ToBytesLittleEndian(a, bytes);
            var b = Fp.FromBytesLittleEndian(bytes);
            Assert.Equal(ToBigInteger(a), ToBigInteger(b));
        }
    }

    [Fact]
    public void FromBytesLittleEndian_ShouldReject_NonCanonical_Modulus()
    {
        // Same canonical rejection test as BE, but LE byte order.
        // Source for p:
        // https://www.rfc-editor.org/rfc/rfc9380#section-8.8.1
        const string pHex = "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab";
        byte[] be = Convert.FromHexString(pHex);
        byte[] bytes = new byte[48];
        for (int i = 0; i < 48; i++) bytes[i] = be[47 - i];

        Assert.Throws<ArgumentOutOfRangeException>(() => Fp.FromBytesLittleEndian(bytes));
        Assert.False(Fp.TryFromBytesLittleEndian(bytes, out _));
    }

    private static Fp RandomCanonicalFp(Random random)
    {
        return FromBigInteger(RandomBelow(random, Modulus));
    }

    private static Fp RandomNonZeroCanonicalFp(Random random)
    {
        while (true)
        {
            var v = RandomCanonicalFp(random);
            if (!Fp.Equal(v, Fp.Zero))
                return v;
        }
    }

    private static BigInteger ToBigInteger(Fp value)
    {
        var bytes = new byte[48];
        Fp.ToBytesBigEndian(value, bytes);
        return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
    }

    private static BigInteger RawToBigInteger(Fp value)
    {
        var l0 = ReadLimb(value, "L0");
        var l1 = ReadLimb(value, "L1");
        var l2 = ReadLimb(value, "L2");
        var l3 = ReadLimb(value, "L3");
        var l4 = ReadLimb(value, "L4");
        var l5 = ReadLimb(value, "L5");

        BigInteger result = l0;
        result += (BigInteger)l1 << 64;
        result += (BigInteger)l2 << 128;
        result += (BigInteger)l3 << 192;
        result += (BigInteger)l4 << 256;
        result += (BigInteger)l5 << 320;
        return result;
    }

    private static Fp FromBigInteger(BigInteger value)
    {
        var v = PositiveMod(value, Modulus);
        var bytes = v.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (bytes.Length > 48)
            throw new InvalidOperationException("Value does not fit into 48 bytes.");

        var padded = new byte[48];
        Buffer.BlockCopy(bytes, 0, padded, 48 - bytes.Length, bytes.Length);
        return Fp.FromBytesBigEndian(padded);
    }

    private static BigInteger RandomBelow(Random random, BigInteger modulus)
    {
        Span<byte> bytes = stackalloc byte[48];
        random.NextBytes(bytes);
        var candidate = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        return candidate % modulus;
    }

    private static BigInteger PositiveMod(BigInteger value, BigInteger modulus)
    {
        var r = value % modulus;
        return r.Sign < 0 ? r + modulus : r;
    }

    private static ulong ReadLimb(Fp value, string name)
    {
        var field = typeof(Fp).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var raw = field.GetValue(value);
        Assert.NotNull(raw);

        return (ulong)raw;
    }
}
