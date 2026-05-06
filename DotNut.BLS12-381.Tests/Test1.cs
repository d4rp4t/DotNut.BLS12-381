using System.Numerics;
using System.Reflection;
using DotNut.BLS12_381.Tower;
using Xunit;

namespace DotNut.BLS12_381.Tests;

public sealed class Test1
{
    private static readonly BigInteger Modulus = ToBigInteger(Fp.Modulus);

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
    public void Substract_Alias_ShouldEqualSubtract()
    {
        var a = new Fp(123, 456, 789, 101112, 131415, 161718);
        var b = new Fp(10, 20, 30, 40, 50, 60);

        var subtract = Fp.Subtract(a, b);
        var substract = Fp.Substract(a, b);

        Assert.Equal(ToBigInteger(subtract), ToBigInteger(substract));
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
        const string pHex = "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab";
        var expected = BigInteger.Parse(pHex, System.Globalization.NumberStyles.AllowHexSpecifier);
        Assert.Equal(expected, ToBigInteger(Fp.Modulus));
    }

    private static Fp RandomCanonicalFp(Random random)
    {
        return FromBigInteger(RandomBelow(random, Modulus));
    }

    private static BigInteger ToBigInteger(Fp value)
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
        ulong l0 = (ulong)(v & ulong.MaxValue);
        ulong l1 = (ulong)((v >> 64) & ulong.MaxValue);
        ulong l2 = (ulong)((v >> 128) & ulong.MaxValue);
        ulong l3 = (ulong)((v >> 192) & ulong.MaxValue);
        ulong l4 = (ulong)((v >> 256) & ulong.MaxValue);
        ulong l5 = (ulong)((v >> 320) & ulong.MaxValue);
        return new Fp(l0, l1, l2, l3, l4, l5);
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
