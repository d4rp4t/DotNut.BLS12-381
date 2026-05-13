namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    /// <summary>
    /// Lexicographic comparison of two Fp2 elements: C1 is compared first, then C0 as a tiebreaker.
    /// Comparison is performed on the canonical (reduced) Fp representation of each component via <see cref="Fp.Compare"/>.
    /// </summary>
    /// <returns>Negative if a &lt; b, zero if a == b, positive if a &gt; b.</returns>
    public static int Compare(Fp2 a, Fp2 b)
    {
        int c1 = Fp.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;
        return Fp.Compare(a.C0, b.C0);
    }
    
    /// <summary>
    /// Constant-time equality. Returns 1 if <paramref name="a"/> and <paramref name="b"/>
    /// represent the same Fp2 element (raw limb comparison), 0 otherwise.
    /// Both inputs must be in reduced Montgomery form.
    /// </summary>
    internal static ulong CtEqual(Fp2 a, Fp2 b)
        => Fp.CtEqual(a.C0, b.C0) & Fp.CtEqual(a.C1, b.C1);
    
    /// <summary>
    /// Returns <see langword="true"/> if a and b represent the same element of Fp2.
    /// Delegates to <see cref="Fp.Equal"/> for each component, which compares raw Montgomery-form limbs.
    /// Two elements that are equal as field values but have different raw representations (e.g. not fully reduced)
    /// may compare as unequal — ensure inputs are in canonical form [0, p) before comparing.
    /// </summary>
    public static bool Equal(Fp2 a, Fp2 b)
    {
        return Fp.Equal(a.C0, b.C0)
               & Fp.Equal(a.C1, b.C1);
    }

    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp2 a, Fp2 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp2 a, Fp2 b) => !Equal(a, b);

    /// <inheritdoc cref="Equal"/>
    public override bool Equals(object? obj)
    {
        return obj is Fp2 other && Equal(this, other);
    }

    /// <summary>Returns a hash code based on the raw Montgomery-form limbs of both components.</summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1);
    }
}
