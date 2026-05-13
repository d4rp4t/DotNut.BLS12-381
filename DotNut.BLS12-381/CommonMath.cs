using System.Runtime.CompilerServices;

namespace DotNut.BLS12_381;

internal static class CommonMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AddCarry(ulong a, ulong b, ulong carryIn, out ulong carryOut)
    {
        UInt128 sum = (UInt128)a + b + carryIn;
        carryOut = (ulong)(sum >> 64);
        return (ulong)sum;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SubBorrow(ulong a, ulong b, ulong borrowIn, out ulong borrowOut)
    {
        UInt128 diff = (UInt128)a - b - borrowIn;
        borrowOut = (ulong)(diff >> 64) & 1UL;
        return (ulong)diff;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mac(ulong a, ulong b, ulong t, ulong c, out ulong carry)
    {
        UInt128 r = (UInt128)a * b + t + c;
        carry = (ulong)(r >> 64);
        return (ulong)r;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SelectU64(ulong mask, ulong whenMaskAllOnes, ulong whenMaskZero)
    {
        return (whenMaskAllOnes & mask) | (whenMaskZero & ~mask);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CmpLimb(ulong a, ulong b, ref ulong gt, ref ulong lt)
    {
        ulong a_gt_b = (b - a) >> 63;
        ulong b_gt_a = (a - b) >> 63;
        ulong undecided = 1 - (gt | lt);
        gt |= undecided & a_gt_b;
        lt |= undecided & b_gt_a;
    }
}