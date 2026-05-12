using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public sealed class CofactorTests
{
#region G1 ClearCofactor
    [Fact]
    public void G1_ClearCofactor_Generator_IsInSubgroup()
    {
        // Generator is already in G1; h1*G is still in G1.
        var result = G1Affine.Generator.ToProjective().ClearCofactor().ToAffine();
        Assert.True(result.IsOnCurve());
        Assert.True(result.IsInSubgroup());
    }

    [Fact]
    public void G1_ClearCofactor_Infinity_IsInfinity()
    {
        var result = G1Projective.Infinity.ClearCofactor();
        Assert.True(result.IsInfinity);
    }

    [Fact]
    public void G1_ClearCofactor_TwoG_IsInSubgroup()
    {
        // [2]G is in G1; h1*[2]G should still be in G1.
        var twoG = G1Projective.Double(G1Affine.Generator.ToProjective());
        var result = twoG.ClearCofactor().ToAffine();
        Assert.True(result.IsOnCurve());
        Assert.True(result.IsInSubgroup());
    }

    [Fact]
    public void G1_ClearCofactor_OutputIsOnCurve()
    {
        var p = G1Projective.Double(G1Projective.Double(G1Affine.Generator.ToProjective()));
        Assert.True(p.ClearCofactor().IsOnCurve());
    }
#endregion
    
#region G2 ClearCofactor

    [Fact]
    public void G2_ClearCofactor_Generator_IsInSubgroup()
    {
        var result = G2Affine.Generator.ToProjective().ClearCofactor().ToAffine();
        Assert.True(result.IsOnCurve());
        Assert.True(result.IsInSubgroup());
    }

    [Fact]
    public void G2_ClearCofactor_Infinity_IsInfinity()
    {
        var result = G2Projective.Infinity.ClearCofactor();
        Assert.True(result.IsInfinity);
    }

    [Fact]
    public void G2_ClearCofactor_TwoG_IsInSubgroup()
    {
        var twoG = G2Projective.Double(G2Affine.Generator.ToProjective());
        var result = twoG.ClearCofactor().ToAffine();
        Assert.True(result.IsOnCurve());
        Assert.True(result.IsInSubgroup());
    }

    [Fact]
    public void G2_ClearCofactor_OutputIsOnCurve()
    {
        var p = G2Projective.Double(G2Projective.Double(G2Affine.Generator.ToProjective()));
        Assert.True(p.ClearCofactor().IsOnCurve());
    }
#endregion
    
#region G2 Psi endomorphism
    
    [Fact]
    public void G2_Psi_Infinity_IsInfinity()
    {
        var result = G2Projective.Psi(G2Projective.Infinity);
        Assert.True(result.IsInfinity);
    }

    [Fact]
    public void G2_Psi_Generator_IsOnCurve()
    {
        var result = G2Projective.Psi(G2Affine.Generator.ToProjective());
        Assert.True(result.IsOnCurve());
    }

    [Fact]
    public void G2_Psi2_Generator_IsOnCurve()
    {
        var result = G2Projective.Psi2(G2Affine.Generator.ToProjective());
        Assert.True(result.IsOnCurve());
    }

    // psi is a group homomorphism: psi(P+Q) = psi(P) + psi(Q).
    [Fact]
    public void G2_Psi_IsHomomorphism()
    {
        var g = G2Affine.Generator.ToProjective();
        var g2 = G2Projective.Double(g);

        var lhs = G2Projective.Psi(G2Projective.Add(g, g2)).ToAffine();
        var rhs = G2Projective.Add(G2Projective.Psi(g), G2Projective.Psi(g2)).ToAffine();
        Assert.True(G2AffineEqual(lhs, rhs));
    }

    // psi^2 = psi*psi composed two ways must agree.
    [Fact]
    public void G2_Psi2_MatchesDoublePsi()
    {
        var g = G2Affine.Generator.ToProjective();
        var via2 = G2Projective.Psi2(g).ToAffine();
        var viaPsi = G2Projective.Psi(G2Projective.Psi(g)).ToAffine();
        Assert.True(G2AffineEqual(via2, viaPsi));
    }
#endregion
#region G2 fast IsInSubgroup

    [Fact]
    public void G2_IsInSubgroup_Generator_ReturnsTrue()
    {
        Assert.True(G2Affine.Generator.IsInSubgroup());
    }

    [Fact]
    public void G2_IsInSubgroup_Infinity_ReturnsTrue()
    {
        Assert.True(G2Affine.Infinity.IsInSubgroup());
    }

    [Fact]
    public void G2_IsInSubgroup_DoubledGenerator_ReturnsTrue()
    {
        var g2 = G2Projective.Double(G2Affine.Generator.ToProjective()).ToAffine();
        Assert.True(g2.IsInSubgroup());
    }

    [Fact]
    public void G2_IsInSubgroup_PointNotOnCurve_ReturnsFalse()
    {
        // Deliberately wrong y, not on E2.
        var bad = new G2Affine(G2Affine.Generator.X, Fp2.One);
        Assert.False(bad.IsInSubgroup());
    }

    // Fast psi-based check must agree with the slow [r]P = O check for G2 generator.
    [Fact]
    public void G2_IsInSubgroup_FastMatchesSlow_ForGenerator()
    {
        var g = G2Affine.Generator;
        bool slow = G2Projective.ScalarMultiply(g.ToProjective(), Scalar.GroupOrderR).IsInfinity;
        bool fast  = g.IsInSubgroup();
        Assert.Equal(slow, fast);
    }

    // Fast check must agree with slow check for [2]G.
    [Fact]
    public void G2_IsInSubgroup_FastMatchesSlow_For2G()
    {
        var g2 = G2Projective.Double(G2Affine.Generator.ToProjective()).ToAffine();
        bool slow = G2Projective.ScalarMultiply(g2.ToProjective(), Scalar.GroupOrderR).IsInfinity;
        bool fast  = g2.IsInSubgroup();
        Assert.Equal(slow, fast);
    }
#endregion
#region helpers

    private static bool G2AffineEqual(G2Affine a, G2Affine b)
    {
        if (a.IsInfinity && b.IsInfinity) return true;
        if (a.IsInfinity != b.IsInfinity) return false;
        return Fp2.Equal(a.X, b.X) && Fp2.Equal(a.Y, b.Y);
    }
#endregion
}

