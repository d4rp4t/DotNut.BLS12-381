namespace DotNut.BLS12_381.Tower;

/// <summary>
/// Element of Fp6 = Fp2[v]/(v³ − ξ), the degree-3 extension of Fp2 in the BLS12-381 tower.
/// An element is represented as C0 + C1·v + C2·v² where C0, C1, C2 ∈ Fp2 and v³ = ξ = 1 + u.
/// All three Fp2 components are stored in Montgomery form.
/// </summary>
public readonly partial struct Fp6(Fp2 c0, Fp2 c1, Fp2 c2)
{
    /// <summary>Additive identity 0 + 0·v + 0·v².</summary>
    public static readonly Fp6 Zero = new(Fp2.Zero, Fp2.Zero, Fp2.Zero);

    /// <summary>Multiplicative identity 1 + 0·v + 0·v².</summary>
    public static readonly Fp6 One = new(Fp2.One, Fp2.Zero, Fp2.Zero);

    /// <summary>Constant component (coefficient of 1).</summary>
    public readonly Fp2 C0 = c0;

    /// <summary>Degree-1 component (coefficient of v where v³ = ξ = 1 + u).</summary>
    public readonly Fp2 C1 = c1;

    /// <summary>Degree-2 component (coefficient of v²).</summary>
    public readonly Fp2 C2 = c2;
}
