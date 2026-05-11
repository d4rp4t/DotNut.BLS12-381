namespace DotNut.BLS12_381;

internal static class CommonMath
{
    public static ulong AddCarry(ulong a, ulong b, ulong carryIn, out ulong carryOut)
    {
        UInt128 sum = (UInt128)a + b + carryIn;
        carryOut = (ulong)(sum >> 64);
        return (ulong)sum;
    }
    
    public static ulong SubBorrow(ulong a, ulong b, ulong borrowIn, out ulong borrowOut)
    {
        UInt128 diff = (UInt128)a - b - borrowIn;
        borrowOut = (ulong)(diff >> 64) & 1UL;
        return (ulong)diff;
    }
    
    public static ulong Mac(ulong a, ulong b, ulong t, ulong c, out ulong carry)
    {
        UInt128 r = (UInt128)a * b + t + c;
        carry = (ulong)(r >> 64);
        return (ulong)r;
    }
    
    public static ulong SelectU64(ulong mask, ulong whenMaskAllOnes, ulong whenMaskZero)
    {
        return (whenMaskAllOnes & mask) | (whenMaskZero & ~mask);
    }
}