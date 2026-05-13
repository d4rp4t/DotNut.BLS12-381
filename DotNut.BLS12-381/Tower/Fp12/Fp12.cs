namespace DotNut.BLS12_381.Tower;

/// <summary>
/// Element of Fp12 = Fp6[w]/(w² − v), the degree-12 extension of Fp completing the BLS12-381 tower.
/// An element is represented as C0 + C1·w where C0, C1 ∈ Fp6 and w² = v.
/// Both Fp6 components are stored in Montgomery form.
/// </summary>
public readonly partial struct Fp12
{
    /// <summary>Additive identity 0 + 0·w.</summary>
    public static readonly Fp12 Zero = new(Fp6.Zero, Fp6.Zero);

    /// <summary>Multiplicative identity 1 + 0·w.</summary>
    public static readonly Fp12 One = new(Fp6.One, Fp6.Zero);

    /// <summary>Constant component (coefficient of 1 in Fp6).</summary>
    public readonly Fp6 C0;

    /// <summary>Degree-1 component (coefficient of w where w² = v).</summary>
    public readonly Fp6 C1;

    /// <summary>
    /// Creates an Fp12 element from its two Fp6 components.
    /// Both components must already be in Montgomery form.
    /// </summary>
    /// <param name="c0">Constant Fp6 component.</param>
    /// <param name="c1">Degree-1 Fp6 component.</param>
    public Fp12(Fp6 c0, Fp6 c1)
    {
        C0 = c0;
        C1 = c1;
    }
}
