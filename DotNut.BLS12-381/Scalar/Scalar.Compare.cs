using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    public static bool IsZero(Scalar a) => IsZeroMask(a) == 1UL;

    public static bool Equal(Scalar a, Scalar b)
    {
        var spanA = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref a, 1));
        var spanB = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref b, 1));
        return CryptographicOperations.FixedTimeEquals(spanA, spanB);
    }

    public static int Compare(Scalar a, Scalar b)
    {
        var ca = ToCanonical(a);
        var cb = ToCanonical(b);

        ulong gt = 0, lt = 0;

        CommonMath.CmpLimb(ca.L3, cb.L3, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L2, cb.L2, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L1, cb.L1, ref gt, ref lt);
        CommonMath.CmpLimb(ca.L0, cb.L0, ref gt, ref lt);

        return (int)gt - (int)lt;
    }

    public static bool operator ==(Scalar a, Scalar b) => Equal(a, b);
    public static bool operator !=(Scalar a, Scalar b) => !Equal(a, b);

    public override bool Equals(object? obj) => obj is Scalar other && Equal(this, other);

    public override int GetHashCode() => HashCode.Combine(L0, L1, L2, L3);
}
