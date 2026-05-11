
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    // returns -1, 0 or 1
    public static int Compare(Fp a, Fp b)
    {
        var ca = ToCanonical(a);
        var cb = ToCanonical(b);

        ulong gt = 0, lt = 0;

        CmpLimb(ca.L5, cb.L5, ref gt, ref lt);
        CmpLimb(ca.L4, cb.L4, ref gt, ref lt);
        CmpLimb(ca.L3, cb.L3, ref gt, ref lt);
        CmpLimb(ca.L2, cb.L2, ref gt, ref lt);
        CmpLimb(ca.L1, cb.L1, ref gt, ref lt);
        CmpLimb(ca.L0, cb.L0, ref gt, ref lt);

        return (int)gt - (int)lt;
    }

    public static bool Equal(Fp a, Fp b)
    {
        var spanA = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a, 1));
        var spanB = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref b, 1));
        return CryptographicOperations.FixedTimeEquals(spanA, spanB);
    }
    
    internal static bool GreaterThanOrEqual(Fp a, Fp b)
    {
        return Compare(a, b) >= 0;
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void CmpLimb(ulong a, ulong b, ref ulong gt, ref ulong lt)
    {
        ulong a_gt_b = (b - a) >> 63;
        ulong b_gt_a = (a - b) >> 63;
        ulong undecided = 1 - (gt | lt);
        gt |= undecided & a_gt_b;
        lt |= undecided & b_gt_a;
    }
}
