using DotNut.BLS12_381.Tower;
using Xunit;

namespace DotNut.BLS12_381.Tests;

public sealed class TowerFieldTests
{
    // Tower parameters source:
    // EIP-2537 (BLS12-381 extension tower):
    // https://eips.ethereum.org/EIPS/eip-2537
    // Fp2: u^2 = -1, Fp6: v^3 = u + 1, Fp12: w^2 = v

    [Fact]
    public void Fp2_Invert_ShouldSatisfy_a_times_inv_a_is_one()
    {
        var random = new Random(101);
        for (var i = 0; i < 120; i++)
        {
            var a = RandomFp2NonZero(random);
            var inv = Fp2.Invert(a);
            Assert.True(Fp2.Equal(Fp2.Multiply(a, inv), Fp2.One));
        }
    }

    [Fact]
    public void Fp2_Square_ShouldMatch_MultiplySelf()
    {
        var random = new Random(102);
        for (var i = 0; i < 120; i++)
        {
            var a = RandomFp2(random);
            Assert.True(Fp2.Equal(Fp2.Square(a), Fp2.Multiply(a, a)));
        }
    }

    [Fact]
    public void Fp2_FrobeniusPowerOne_ShouldConjugate()
    {
        var random = new Random(103);
        for (var i = 0; i < 120; i++)
        {
            var a = RandomFp2(random);
            var expected = new Fp2(a.C0, Fp.Negate(a.C1));
            Assert.True(Fp2.Equal(expected, Fp2.FrobeniusMap(a, 1)));
        }
    }

    [Fact]
    public void Fp6_Invert_ShouldSatisfy_a_times_inv_a_is_one()
    {
        var random = new Random(201);
        for (var i = 0; i < 80; i++)
        {
            var a = RandomFp6NonZero(random);
            var inv = Fp6.Invert(a);
            Assert.True(Fp6.Equal(Fp6.Multiply(a, inv), Fp6.One));
        }
    }

    [Fact]
    public void Fp6_Square_ShouldMatch_MultiplySelf()
    {
        var random = new Random(202);
        for (var i = 0; i < 80; i++)
        {
            var a = RandomFp6(random);
            Assert.True(Fp6.Equal(Fp6.Square(a), Fp6.Multiply(a, a)));
        }
    }

    [Fact]
    public void Fp6_MulByNonResidue_ShouldMatch_MultiplyByV()
    {
        var random = new Random(203);
        var v = new Fp6(Fp2.Zero, Fp2.One, Fp2.Zero);
        for (var i = 0; i < 80; i++)
        {
            var a = RandomFp6(random);
            Assert.True(Fp6.Equal(Fp6.MulByNonResidue(a), Fp6.Multiply(a, v)));
        }
    }

    [Fact]
    public void Fp12_Invert_ShouldSatisfy_a_times_inv_a_is_one()
    {
        var random = new Random(301);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12NonZero(random);
            var inv = Fp12.Invert(a);
            Assert.True(Fp12.Equal(Fp12.Multiply(a, inv), Fp12.One));
        }
    }

    [Fact]
    public void Fp12_Square_ShouldMatch_MultiplySelf()
    {
        var random = new Random(302);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12(random);
            Assert.True(Fp12.Equal(Fp12.Square(a), Fp12.Multiply(a, a)));
        }
    }

    [Fact]
    public void Fp12_CyclotomicSquare_ShouldMatch_Square()
    {
        var random = new Random(303);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12(random);
            Assert.True(Fp12.Equal(Fp12.CyclotomicSquare(a), Fp12.Square(a)));
        }
    }

    [Fact]
    public void Fp12_FinalExponentiation_One_ShouldStayOne()
    {
        Assert.True(Fp12.Equal(Fp12.One, Fp12.FinalExponentiation(Fp12.One)));
    }

    private static Fp RandomFp(Random random)
    {
        var bytes = new byte[48];
        while (true)
        {
            random.NextBytes(bytes);
            if (Fp.TryFromBytesBigEndian(bytes, out var v))
                return v;
        }
    }

    private static Fp2 RandomFp2(Random random) => new(RandomFp(random), RandomFp(random));

    private static Fp2 RandomFp2NonZero(Random random)
    {
        while (true)
        {
            var x = RandomFp2(random);
            if (!Fp2.Equal(x, Fp2.Zero)) return x;
        }
    }

    private static Fp6 RandomFp6(Random random) => new(RandomFp2(random), RandomFp2(random), RandomFp2(random));

    private static Fp6 RandomFp6NonZero(Random random)
    {
        while (true)
        {
            var x = RandomFp6(random);
            if (!Fp6.Equal(x, Fp6.Zero)) return x;
        }
    }

    private static Fp12 RandomFp12(Random random) => new(RandomFp6(random), RandomFp6(random));

    private static Fp12 RandomFp12NonZero(Random random)
    {
        while (true)
        {
            var x = RandomFp12(random);
            if (!Fp12.Equal(x, Fp12.Zero)) return x;
        }
    }
}
