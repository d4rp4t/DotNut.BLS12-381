namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    /// <summary>
    /// Lexicographic comparison of two Fp12 elements: C1 is compared first, then C0.
    /// Each component is compared via <see cref="Fp6.Compare"/>.
    /// </summary>
    /// <returns>Negative if a &lt; b, zero if a == b, positive if a &gt; b.</returns>
    public static int Compare(Fp12 a, Fp12 b)
    {
        var c1 = Fp6.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;
        return Fp6.Compare(a.C0, b.C0);
    }

    /// <summary>
    /// Returns <see langword="true"/> if a and b represent the same element of Fp12.
    /// Delegates to <see cref="Fp6.Equal"/> for each component, which ultimately compares raw Montgomery-form limbs.
    /// Ensure all Fp components are fully reduced to [0, p) before comparing.
    /// </summary>
    public static bool Equal(Fp12 a, Fp12 b)
    {
        return Fp6.Equal(a.C0, b.C0)
               & Fp6.Equal(a.C1, b.C1);
    }

    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp12 a, Fp12 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp12 a, Fp12 b) => !Equal(a, b);

    /// <inheritdoc cref="Equal"/>
    public override bool Equals(object? obj)
    {
        return obj is Fp12 other && Equal(this, other);
    }

    /// <summary>Returns a hash code based on the raw Montgomery-form limbs of both Fp6 components.</summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1);
    }
}
