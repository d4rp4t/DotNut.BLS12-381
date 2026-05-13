using System.Runtime.CompilerServices;

namespace DotNut.BLS12_381;

internal static class CommonMath
{
    /// <summary>
    /// Computes a + b + carryIn and returns the low 64 bits. Sets <paramref name="carryOut"/> to 1 on overflow, 0 otherwise.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="carryIn">Carry in; must be 0 or 1.</param>
    /// <param name="carryOut">Carry out; set to 1 if the 128-bit sum overflows 64 bits.</param>
    /// <returns>Low 64 bits of a + b + carryIn.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AddCarry(ulong a, ulong b, ulong carryIn, out ulong carryOut)
    {
        UInt128 sum = (UInt128)a + b + carryIn;
        carryOut = (ulong)(sum >> 64);
        return (ulong)sum;
    }

    /// <summary>
    /// Computes a - b - borrowIn and returns the low 64 bits. Sets <paramref name="borrowOut"/> to 1 on underflow, 0 otherwise.
    /// </summary>
    /// <param name="a">Minuend.</param>
    /// <param name="b">Subtrahend.</param>
    /// <param name="borrowIn">Borrow in; must be 0 or 1.</param>
    /// <param name="borrowOut">Borrow out; set to 1 if the result underflowed (i.e. a - b - borrowIn &lt; 0).</param>
    /// <returns>Low 64 bits of a - b - borrowIn.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SubBorrow(ulong a, ulong b, ulong borrowIn, out ulong borrowOut)
    {
        UInt128 diff = (UInt128)a - b - borrowIn;
        borrowOut = (ulong)(diff >> 64) & 1UL;
        return (ulong)diff;
    }

    /// <summary>
    /// Multiply-accumulate: returns the low 64 bits of a*b + t + c and sets <paramref name="carry"/> to the high bits.
    /// Does not perform any field arithmetic; operates on raw 64-bit limbs.
    /// </summary>
    /// <param name="a">Multiplicand.</param>
    /// <param name="b">Multiplier.</param>
    /// <param name="t">Accumulator term (added to a*b).</param>
    /// <param name="c">Carry term (added after a*b + t).</param>
    /// <param name="carry">High 64 bits of a*b + t + c.</param>
    /// <returns>Low 64 bits of a*b + t + c.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Mac(ulong a, ulong b, ulong t, ulong c, out ulong carry)
    {
        UInt128 r = (UInt128)a * b + t + c;
        carry = (ulong)(r >> 64);
        return (ulong)r;
    }

    /// <summary>
    /// Branchless conditional select on 64-bit values.
    /// Returns <paramref name="whenMaskAllOnes"/> if mask is 0xFFFFFFFFFFFFFFFF, or <paramref name="whenMaskZero"/> if mask is 0.
    /// </summary>
    /// <param name="mask">Selection mask; must be either 0 (all bits zero) or 0xFFFFFFFFFFFFFFFF (all bits one).</param>
    /// <param name="whenMaskAllOnes">Value returned when mask is all-ones.</param>
    /// <param name="whenMaskZero">Value returned when mask is zero.</param>
    /// <returns>Branchless selection of one of the two values based on mask.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SelectU64(ulong mask, ulong whenMaskAllOnes, ulong whenMaskZero)
    {
        return (whenMaskAllOnes & mask) | (whenMaskZero & ~mask);
    }

    /// <summary>
    /// Updates the running greater-than/less-than flags for a multi-limb big-integer comparison.
    /// Call MSB-first for each corresponding pair of limbs. Once either flag is set, subsequent limbs are ignored.
    /// </summary>
    /// <param name="a">Limb from the left operand.</param>
    /// <param name="b">Limb from the right operand.</param>
    /// <param name="gt">Set to 1 if a &gt; b and no earlier limb pair was decisive. Otherwise unchanged.</param>
    /// <param name="lt">Set to 1 if a &lt; b and no earlier limb pair was decisive. Otherwise unchanged.</param>
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
