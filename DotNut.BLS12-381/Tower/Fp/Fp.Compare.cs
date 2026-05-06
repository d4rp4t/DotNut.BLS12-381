namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static int Compare(Fp a, Fp b)
    {
        if (a.L5 != b.L5) return a.L5 > b.L5 ? 1 : -1;
        if (a.L4 != b.L4) return a.L4 > b.L4 ? 1 : -1;
        if (a.L3 != b.L3) return a.L3 > b.L3 ? 1 : -1;
        if (a.L2 != b.L2) return a.L2 > b.L2 ? 1 : -1;
        if (a.L1 != b.L1) return a.L1 > b.L1 ? 1 : -1;
        if (a.L0 != b.L0) return a.L0 > b.L0 ? 1 : -1;
        return 0;
    }

    public static bool Equals(Fp first, Fp other)
    {
        return first.L0 == other.L0
               && first.L1 == other.L1
               && first.L2 == other.L2
               && first.L3 == other.L3
               && first.L4 == other.L4
               && first.L5 == other.L5;
    }
    
    internal static bool GreaterThanOrEqual(Fp a, Fp b)
    {
        return Compare(a, b) >= 0;
    }

    public static bool Equal(Fp a, Fp b)
    {
        return a.L0 == b.L0
               && a.L1 == b.L1
               && a.L2 == b.L2
               && a.L3 == b.L3
               && a.L4 == b.L4
               && a.L5 == b.L5;
    }

    public static bool operator ==(Fp a, Fp b) => Equal(a, b);

    public static bool operator !=(Fp a, Fp b) => !Equal(a, b);

    public override bool Equals(object? obj)
    {
        return obj is Fp other && Equal(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(L0, L1, L2, L3, L4, L5);
    }
}