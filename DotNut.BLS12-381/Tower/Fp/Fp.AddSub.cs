namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{

    public static Fp Add(Fp a, Fp b)
    {
        Fp r = AddUnchecked(a, b, out ulong carry);
        if (carry != 0 || GreaterThanOrEqual(r, Modulus))
        {
            r = SubtractUnchecked(r, Modulus, out _);
        }

        return r;
    }

    public static Fp Subtract(Fp a, Fp b)
    {
        Fp r = SubtractUnchecked(a, b, out ulong borrow);

        if (borrow != 0)
            r = AddUnchecked(r, Modulus, out _);

        return r;
    }
    
    internal static Fp AddUnchecked(Fp a, Fp b, out ulong carry)
    {
        ulong c = 0;
        ulong l0 = AddCarry(a.L0, b.L0, c, out c);
        ulong l1 = AddCarry(a.L1, b.L1, c, out c);
        ulong l2 = AddCarry(a.L2, b.L2, c, out c);
        ulong l3 = AddCarry(a.L3, b.L3, c, out c);
        ulong l4 = AddCarry(a.L4, b.L4, c, out c);
        ulong l5 = AddCarry(a.L5, b.L5, c, out carry);

        return new Fp(l0, l1, l2, l3, l4, l5);
    }

    internal static Fp SubtractUnchecked(Fp a, Fp b, out ulong borrow)
    {
        ulong brrw = 0;
        ulong l0 = SubBorrow(a.L0, b.L0, brrw, out brrw);
        ulong l1 = SubBorrow(a.L1, b.L1, brrw, out brrw);
        ulong l2 = SubBorrow(a.L2, b.L2, brrw, out brrw);
        ulong l3 = SubBorrow(a.L3, b.L3, brrw, out brrw);
        ulong l4 = SubBorrow(a.L4, b.L4, brrw, out brrw);
        ulong l5 = SubBorrow(a.L5, b.L5, brrw, out borrow);

        return new Fp(l0, l1, l2, l3, l4, l5);
    }

    public static Fp Substract(Fp a, Fp b) => Subtract(a, b);

    private static ulong AddCarry(
        ulong a,
        ulong b,
        ulong carryIn,
        out ulong carryOut)
    {
        UInt128 sum = (UInt128)a + b + carryIn;

        carryOut = (ulong)(sum >> 64);

        return (ulong)sum;
    }

    private static ulong SubBorrow(
        ulong a,
        ulong b,
        ulong borrowIn,
        out ulong borrowOut)
    {
        UInt128 subtrahend = (UInt128)b + borrowIn;
        borrowOut = a < subtrahend ? 1UL : 0UL;

        return (ulong)((UInt128)a - subtrahend);
    }
}
