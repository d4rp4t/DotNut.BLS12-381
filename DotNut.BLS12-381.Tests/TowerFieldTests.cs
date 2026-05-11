using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public sealed class TowerFieldTests
{
    // Tower parameters source:
    // EIP-2537 (BLS12-381 extension tower): https://eips.ethereum.org/EIPS/eip-2537
    // Fp2: u^2 = -1, Fp6: v^3 = u + 1, Fp12: w^2 = v

    #region Fp2

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
    public void Fp2_Compare_ShouldBeConsistentWith_Equality()
    {
        var random = new Random(104);
        for (var i = 0; i < 120; i++)
        {
            var a = RandomFp2(random);
            var b = RandomFp2(random);
            var cmp = Fp2.Compare(a, b);
            Assert.Equal(Fp2.Equal(a, b), cmp == 0);
            Assert.Equal(-cmp, Fp2.Compare(b, a));
        }
    }

    #endregion

    #region Fp6

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
    public void Fp6_Compare_ShouldBeConsistentWith_Equality()
    {
        var random = new Random(204);
        for (var i = 0; i < 80; i++)
        {
            var a = RandomFp6(random);
            var b = RandomFp6(random);
            var cmp = Fp6.Compare(a, b);
            Assert.Equal(Fp6.Equal(a, b), cmp == 0);
            Assert.Equal(-cmp, Fp6.Compare(b, a));
        }
    }

    [Fact]
    public void Fp6_FrobeniusPowerSix_ShouldBeIdentity()
    {
        var random = new Random(205);
        for (var i = 0; i < 80; i++)
        {
            var a = RandomFp6(random);
            Assert.True(Fp6.Equal(a, Fp6.FrobeniusMap(a, 6)));
        }
    }

    #endregion

    #region Fp12

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
        // CyclotomicSquare is equivalent to Square only for cyclotomic subgroup elements.
        // Generate them via the easy part: f^((p^6-1)(p^2+1)).
        var random = new Random(303);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12NonZero(random);
            var f = Fp12.Multiply(Fp12.Conjugate(a), Fp12.Invert(a));
            f = Fp12.Multiply(Fp12.FrobeniusMap(f, 2), f);
            Assert.True(Fp12.Equal(Fp12.CyclotomicSquare(f), Fp12.Square(f)));
        }
    }

    [Fact]
    public void Fp12_FinalExponentiation_One_ShouldStayOne()
    {
        Assert.True(Fp12.Equal(Fp12.One, Fp12.FinalExponentiation(Fp12.One)));
    }

    [Fact]
    public void Fp12_Compare_ShouldBeConsistentWith_Equality()
    {
        var random = new Random(304);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12(random);
            var b = RandomFp12(random);
            var cmp = Fp12.Compare(a, b);
            Assert.Equal(Fp12.Equal(a, b), cmp == 0);
            Assert.Equal(-cmp, Fp12.Compare(b, a));
        }
    }

    [Fact]
    public void Fp12_FrobeniusPowerTwelve_ShouldBeIdentity()
    {
        var random = new Random(305);
        for (var i = 0; i < 60; i++)
        {
            var a = RandomFp12(random);
            Assert.True(Fp12.Equal(a, Fp12.FrobeniusMap(a, 12)));
        }
    }

    [Fact]
    public void Fp12_CyclotomicExp_ShouldMatchPow_ForCyclotomicElements()
    {
        // Leading "0" required: AllowHexSpecifier treats high bit as sign bit (two's complement).
        var blsX = System.Numerics.BigInteger.Parse(
            "0d201000000010000",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var random = new Random(602);
        for (var i = 0; i < 5; i++)
        {
            var a = RandomFp12NonZero(random);
            var f = Fp12.Multiply(Fp12.Conjugate(a), Fp12.Invert(a));
            f = Fp12.Multiply(Fp12.FrobeniusMap(f, 2), f);
            var expected = Fp12.Pow(f, blsX);
            var actual = Fp12.CyclotomicExp(f, blsX);
            Assert.True(Fp12.Equal(expected, actual));
        }
    }

    [Fact]
    public void Fp12_CyclotomicExpBlsX_ShouldMatch_PowAndConjugate()
    {
        var blsXPositive = System.Numerics.BigInteger.Parse(
            "0d201000000010000",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var random = new Random(701);
        for (var i = 0; i < 3; i++)
        {
            var a = RandomFp12NonZero(random);
            var f = Fp12.Multiply(Fp12.Conjugate(a), Fp12.Invert(a));
            f = Fp12.Multiply(Fp12.FrobeniusMap(f, 2), f);
            var expected = Fp12.Conjugate(Fp12.Pow(f, blsXPositive));
            var actual = Fp12.CyclotomicExpBlsX(f);
            Assert.True(Fp12.Equal(expected, actual));
        }
    }

    [Fact]
    public void Fp12_Beuchat_PolynomialForm_ShouldEqual_HardExp()
    {
        // Verify the Beuchat decomposition formula symbolically.
        // result = g^[λ_0 + λ_1*p + λ_2*p^2 + λ_3*p^3]
        // where λ_i are polynomials in u, the BLS parameter.
        var p = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var r = System.Numerics.BigInteger.Parse(
            "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var hardExp = (System.Numerics.BigInteger.Pow(p, 4) - System.Numerics.BigInteger.Pow(p, 2) + 1) / r;

        // u = -0xd201000000010000 (negative BLS parameter)
        var blsXPos = System.Numerics.BigInteger.Parse(
            "0d201000000010000",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var u = -blsXPos;

        var u2 = u * u;
        var u3 = u2 * u;
        var u4 = u3 * u;
        var u5 = u4 * u;

        var lambda0 = u5 - 2 * u4 + 2 * u2 - u + 3;
        var lambda1 = u4 - 2 * u3 + 2 * u - 1;
        var lambda2 = u3 - 2 * u2 + u;
        var lambda3 = u2 - 2 * u + 1;

        var pBig = System.Numerics.BigInteger.Pow(p, 1);
        var p2 = System.Numerics.BigInteger.Pow(p, 2);
        var p3 = System.Numerics.BigInteger.Pow(p, 3);

        var polyExp = lambda0 + lambda1 * pBig + lambda2 * p2 + lambda3 * p3;

        // The Beuchat decomposition for BLS12 equals 3·(p^4-p^2+1)/r exactly.
        // The factor 3 absorbs the 1/3 in p(u) = (u-1)^2(u^4-u^2+1)/3 + u so the
        // coefficients λ_i(u) stay integer-valued.
        Assert.Equal(3 * hardExp, polyExp);
    }

    [Fact]
    public void Fp12_FrobeniusMap_Single_ShouldMatch_PowByP()
    {
        var p = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var random = new Random(702);
        for (var i = 0; i < 3; i++)
        {
            var a = RandomFp12NonZero(random);
            Assert.True(Fp12.Equal(Fp12.FrobeniusMap(a, 1), Fp12.Pow(a, p)));
            Assert.True(Fp12.Equal(Fp12.FrobeniusMap(a, 3), Fp12.Pow(a, System.Numerics.BigInteger.Pow(p, 3))));
        }
    }

    [Fact]
    public void Fp12_HardPart_Beuchat_ShouldMatchPow_Seed404()
    {
        var p_bi = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var r_bi = System.Numerics.BigInteger.Parse(
            "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var hardExp = 3 * (System.Numerics.BigInteger.Pow(p_bi, 4) - System.Numerics.BigInteger.Pow(p_bi, 2) + 1) / r_bi;

        var random = new Random(404);
        var a = RandomFp12NonZero(random);

        var easy = Fp12.Multiply(Fp12.Conjugate(a), Fp12.Invert(a));
        easy = Fp12.Multiply(Fp12.FrobeniusMap(easy, 2), easy);

        var expected = Fp12.Pow(easy, hardExp);
        var actual = Fp12.FinalExponentiation(a);

        Assert.True(Fp12.Equal(expected, actual));
    }

    [Fact]
    public void Fp12_FrobeniusMap_ShouldBeCompositional()
    {
        var random = new Random(601);
        for (var i = 0; i < 20; i++)
        {
            var a = RandomFp12(random);
            Assert.True(Fp12.Equal(Fp12.FrobeniusMap(a, 2), Fp12.FrobeniusMap(Fp12.FrobeniusMap(a, 1), 1)));
            Assert.True(Fp12.Equal(Fp12.FrobeniusMap(a, 3), Fp12.FrobeniusMap(Fp12.FrobeniusMap(a, 2), 1)));
            Assert.True(Fp12.Equal(Fp12.FrobeniusMap(a, 6), Fp12.FrobeniusMap(Fp12.FrobeniusMap(a, 3), 3)));
        }
    }

    [Fact]
    public void Fp12_FinalExponentiation_ShouldMatch_GenericExponent_Reference()
    {
        var p = System.Numerics.BigInteger.Parse(
            "1a0111ea397fe69a4b1ba7b6434bacd764774b84f38512bf6730d2a0f6b0f6241eabfffeb153ffffb9feffffffffaaab",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var r = System.Numerics.BigInteger.Parse(
            "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );
        var exp = 3 * (System.Numerics.BigInteger.Pow(p, 12) - 1) / r;

        var random = new Random(404);
        var a = RandomFp12NonZero(random);
        var optimized = Fp12.FinalExponentiation(a);
        var generic = Fp12.Pow(a, exp);
        Assert.True(Fp12.Equal(optimized, generic));
    }

    [Fact]
    public void Fp12_FinalExponentiation_Result_ShouldLieIn_R_Order_Subgroup()
    {
        // GT subgroup condition: z^r = 1 after final exponentiation.
        // Source: https://www.rfc-editor.org/rfc/rfc9380.html#appendix-J.4
        var r = System.Numerics.BigInteger.Parse(
            "73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001",
            System.Globalization.NumberStyles.AllowHexSpecifier
        );

        var random = new Random(405);
        for (var i = 0; i < 20; i++)
        {
            var a = RandomFp12NonZero(random);
            var z = Fp12.FinalExponentiation(a);
            Assert.True(Fp12.Equal(Fp12.Pow(z, r), Fp12.One));
        }
    }

    #endregion

    #region Helpers

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

    #endregion
}
