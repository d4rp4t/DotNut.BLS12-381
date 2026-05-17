using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Pairing;
using Fp12 = DotNut.BLS12_381.Tower.Fp12;
using G1Affine = DotNut.BLS12_381.Curve.G1.G1Affine;
using G2Prepared = DotNut.BLS12_381.Pairing.G2Prepared;
using MillerLoopResult = DotNut.BLS12_381.Pairing.MillerLoopResult;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors;

public class Pairings
{
    [Fact]
    public void test_gt_generator()
    {
        Assert.Equal(Gt.Generator, Bls12Pairing.Pair(G1Affine.Generator, G2Affine.Generator));
    }

    [Fact]
    public void test_bilinearity()
    {
        var a = Scalar.Square(Scalar.Invert(new Scalar([1, 2, 3, 4])));
        var b = Scalar.Square(Scalar.Invert(new Scalar([5, 6, 7, 8])));
        var c = Scalar.Mul(a, b);

        var g = G1Projective.ScalarMultiply(G1Projective.Generator, a).ToAffine();
        var h = G2Projective.ScalarMultiply(G2Projective.Generator, b).ToAffine();
        var p = Bls12Pairing.Pair(g, h);

        Assert.True(p != Gt.Identity);

        var expected = G1Projective.ScalarMultiply(G1Projective.Generator, c).ToAffine();

        Assert.Equal(p, Bls12Pairing.Pair(expected, G2Affine.Generator));
        Assert.Equal(p, Gt.Multiply(Bls12Pairing.Pair(G1Affine.Generator, G2Affine.Generator), c));
    }

    [Fact]
    public void test_unitary()
    {
        var g = G1Affine.Generator;
        var h = G2Affine.Generator;
        var p = Gt.Negate(Bls12Pairing.Pair(g, h));
        var q = Bls12Pairing.Pair(g, G2Affine.Negate(h));
        var r = Bls12Pairing.Pair(G1Affine.Negate(g), h);

        Assert.Equal(p, q);
        Assert.Equal(q, r);
    }

    [Fact]
    public void test_multi_miller_loop()
    {
        var a1 = G1Affine.Generator;
        var b1 = G2Affine.Generator;

        var a2 = G1Projective
            .ScalarMultiply(
                G1Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([1, 2, 3, 4])))
            )
            .ToAffine();

        var b2 = G2Projective
            .ScalarMultiply(
                G2Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([4, 2, 2, 4])))
            )
            .ToAffine();

        var a3 = G1Affine.Infinity;
        var b3 = G2Projective
            .ScalarMultiply(
                G2Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([9, 2, 2, 4])))
            )
            .ToAffine();

        var a4 = G1Projective
            .ScalarMultiply(
                G1Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([5, 5, 5, 5])))
            )
            .ToAffine();

        var b4 = G2Affine.Infinity;

        var a5 = G1Projective
            .ScalarMultiply(
                G1Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([323, 32, 3, 1])))
            )
            .ToAffine();

        var b5 = G2Projective
            .ScalarMultiply(
                G2Projective.Generator,
                Scalar.Square(Scalar.Invert(new Scalar([4, 2, 2, 9099])))
            )
            .ToAffine();

        var b1_prepared = G2Prepared.From(b1);
        var b2_prepared = G2Prepared.From(b2);
        var b3_prepared = G2Prepared.From(b3);
        var b4_prepared = G2Prepared.From(b4);
        var b5_prepared = G2Prepared.From(b5);

        var expected = Gt.Add(
            Gt.Add(
                Gt.Add(Bls12Pairing.Pair(a1, b1), Bls12Pairing.Pair(a2, b2)),
                Bls12Pairing.Pair(a3, b3)
            ),
            Gt.Add(Bls12Pairing.Pair(a4, b4), Bls12Pairing.Pair(a5, b5))
        );

        var test = Bls12Pairing
            .MultiMillerLoop([
                (a1, b1_prepared),
                (a2, b2_prepared),
                (a3, b3_prepared),
                (a4, b4_prepared),
                (a5, b5_prepared),
            ])
            .FinalExponentiation();

        Assert.Equal(expected, test);
    }

    [Fact]
    public void test_miller_loop_result_default()
    {
        Assert.Equal(MillerLoopResult.Default.FinalExponentiation(), Gt.Identity);
    }

    [Fact]
    public void tricking_miller_loop_result()
    {
        Assert.Equal(
            Bls12Pairing
                .MultiMillerLoop([(G1Affine.Infinity, G2Prepared.From(G2Affine.Generator))])
                .Value,
            Fp12.One
        );

        Assert.Equal(
            Bls12Pairing
                .MultiMillerLoop([(G1Affine.Generator, G2Prepared.From(G2Affine.Infinity))])
                .Value,
            Fp12.One
        );

        Assert.NotEqual(
            Bls12Pairing
                .MultiMillerLoop([
                    (G1Affine.Generator, G2Prepared.From(G2Affine.Generator)),
                    (G1Affine.Negate(G1Affine.Generator), G2Prepared.From(G2Affine.Generator)),
                ])
                .Value,
            Fp12.One
        );

        Assert.Equal(
            Bls12Pairing
                .MultiMillerLoop([
                    (G1Affine.Generator, G2Prepared.From(G2Affine.Generator)),
                    (G1Affine.Negate(G1Affine.Generator), G2Prepared.From(G2Affine.Generator)),
                ])
                .FinalExponentiation(),
            Gt.Identity
        );
    }
}
