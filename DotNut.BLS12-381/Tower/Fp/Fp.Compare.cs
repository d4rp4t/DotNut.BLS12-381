namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    // returns -1, 0 or 1
    public static int Compare(Fp a, Fp b)
    {
        var ca = ToCanonical(a);
        var cb = ToCanonical(b);

        ulong gt = 0, lt = 0;

        CommonMath.CmpLimb(ca.L5, cb.L5, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L4, cb.L4, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L3, cb.L3, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L2, cb.L2, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L1, cb.L1, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L0, cb.L0, ref gt, ref lt);

        return (int)gt - (int)lt;
    }

    public static bool Equal(Fp a, Fp b) => CtEqual(a, b) == 1UL;
    
    internal static bool GreaterThanOrEqual(Fp a, Fp b)
    {
        return Compare(a, b) >= 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is Fp other && Equal(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(L0, L1, L2, L3, L4, L5);
    }
}
