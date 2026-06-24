using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public sealed class AffineEqualityTests
{
    [Fact]
    public void G2Affine_RogueInfinity_SharesHashCodeWithCanonicalInfinity()
    {
        var canonical = G2Affine.Infinity;
        var rogue = new G2Affine(new Fp2(Fp.One, Fp.One), new Fp2(Fp.One, Fp.One), isInfinity: true);

        Assert.True(canonical == rogue);
        Assert.True(canonical.Equals(rogue));
        Assert.Equal(canonical.GetHashCode(), rogue.GetHashCode());
    }

    [Fact]
    public void G2Affine_RogueInfinity_WorksCorrectlyInHashSet()
    {
        var canonical = G2Affine.Infinity;
        var rogue = new G2Affine(new Fp2(Fp.One, Fp.One), new Fp2(Fp.One, Fp.One), isInfinity: true);
        var set = new HashSet<G2Affine> { canonical };
        Assert.Contains(rogue, set);
    }

    [Fact]
    public void G1Affine_RogueInfinity_SharesHashCodeWithCanonicalInfinity()
    {
        var canonical = G1Affine.Infinity;
        var rogue = new G1Affine(Fp.One, Fp.One, isInfinity: true);

        Assert.True(canonical == rogue);
        Assert.True(canonical.Equals(rogue));
        Assert.Equal(canonical.GetHashCode(), rogue.GetHashCode());
    }

    [Fact]
    public void G1Affine_RogueInfinity_WorksCorrectlyInHashSet()
    {
        var canonical = G1Affine.Infinity;
        var rogue = new G1Affine(Fp.One, Fp.One, isInfinity: true);
        var set = new HashSet<G1Affine> { canonical };
        Assert.Contains(rogue, set);
    }
}
