namespace DotNut.BLS12_381.Tower;

/// <summary>
/// Element of Fp2 = Fp[u]/(u² + 1), the degree-2 extension of the BLS12-381 base field.
/// An element is represented as C0 + C1·u where C0, C1 ∈ Fp and u² = −1.
/// Both components are stored in Montgomery form (multiplied by R = 2^384 mod p).
/// </summary>
public readonly partial struct Fp2
{
    /// <summary>Additive identity 0 + 0·u.</summary>
    public static readonly Fp2 Zero = new(Fp.Zero, Fp.Zero);

    /// <summary>Multiplicative identity 1 + 0·u.</summary>
    public static readonly Fp2 One = new(Fp.One, Fp.Zero);

    /// <summary>
    /// The Fp6 cubic non-residue ξ = 1 + u used to define Fp6 = Fp2[v]/(v³ − ξ) in the BLS12-381 tower.
    /// </summary>
    public static readonly Fp2 NonResidue = new(Fp.One, Fp.One);

    /// <summary>Constant component of the element (the Fp coefficient of 1).</summary>
    public readonly Fp C0;

    /// <summary>Imaginary component of the element (the Fp coefficient of u where u² = −1).</summary>
    public readonly Fp C1;

    /// <summary>
    /// Creates an Fp2 element from its two Fp components.
    /// Both components must already be in Montgomery form (i.e. in [0, p)).
    /// </summary>
    /// <param name="c0">Constant component (coefficient of 1).</param>
    /// <param name="c1">Imaginary component (coefficient of u).</param>
    public Fp2(Fp c0, Fp c1)
    {
        C0 = c0;
        C1 = c1;
    }

    /// <summary>
    /// Branchless select: returns <paramref name="a"/> when mask=0, <paramref name="b"/> when mask=~0UL.
    /// </summary>
    internal static Fp2 ConditionalSelect(Fp2 a, Fp2 b, ulong mask) => new(
        Fp.ConditionalSelect(a.C0, b.C0, mask),
        Fp.ConditionalSelect(a.C1, b.C1, mask)
    );

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// </summary>
    public static Fp2 ConditionalSelect(Fp2 a, Fp2 b, bool choice) => new(
        Fp.ConditionalSelect(a.C0, b.C0, choice),
        Fp.ConditionalSelect(a.C1, b.C1, choice)
    );
}
