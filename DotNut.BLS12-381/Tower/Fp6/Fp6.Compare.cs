namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6
{
    /// <summary>
    /// Lexicographic comparison of two Fp6 elements: C2 is compared first, then C1, then C0.
    /// Each component is compared via <see cref="Fp2.Compare"/>.
    /// </summary>
    /// <returns>Negative if a &lt; b, zero if a == b, positive if a &gt; b.</returns>
    public static int Compare(Fp6 a, Fp6 b)
    {
        var c2 = Fp2.Compare(a.C2, b.C2);
        if (c2 != 0) return c2;

        var c1 = Fp2.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;

        return Fp2.Compare(a.C0, b.C0);
    }

    /// <summary>
    /// Returns <see langword="true"/> if a and b represent the same element of Fp6.
    /// Delegates to <see cref="Fp2.Equal"/> for each component, which compares raw Montgomery-form limbs.
    /// Ensure all Fp components are fully reduced to [0, p) before comparing.
    /// </summary>
    public static bool Equal(Fp6 a, Fp6 b)
    {
        return Fp2.Equal(a.C0, b.C0)
               & Fp2.Equal(a.C1, b.C1)
               & Fp2.Equal(a.C2, b.C2);
    }

    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp6 a, Fp6 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp6 a, Fp6 b) => !Equal(a, b);

    /// <inheritdoc cref="Equal"/>
    public override bool Equals(object? obj)
    {
        return obj is Fp6 other && Equal(this, other);
    }

    /// <summary>Returns a hash code based on the raw Montgomery-form limbs of all three components.</summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1, C2);
    }
}
