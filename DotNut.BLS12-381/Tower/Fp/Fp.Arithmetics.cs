namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static Fp Add(Fp a, Fp b)
    {
        Fp r = AddUnchecked(a, b, out ulong carry);
        Fp rMinusP = SubtractUnchecked(r, Modulus, out ulong borrow);
        ulong shouldSubtract = carry | (borrow ^ 1UL);
        return Select(shouldSubtract, rMinusP, r);
    }

    public static Fp Subtract(Fp a, Fp b)
    {
        Fp r = SubtractUnchecked(a, b, out ulong borrow);
        Fp rPlusP = AddUnchecked(r, Modulus, out _);
        return Select(borrow, rPlusP, r);
    }

    public static Fp Multiply(Fp a, Fp b) => MontgomeryReduce(MultiplyWide(a, b));

    public static Fp Square(Fp a) => MontgomeryReduce(SquareWide(a));

    public static Fp Negate(Fp value)
    {
        Fp neg = SubtractUnchecked(Modulus, value, out _);
        ulong isZero = IsZeroMask(value);
        return Select(isZero, Zero, neg);
    }

    public static Fp Invert(Fp value)
    {
        if (Equal(value, Zero))
            throw new DivideByZeroException("Cannot invert zero in Fp.");

        // a^(p-2) in Montgomery domain
        ulong[] pMinus2 =
        [
            0xb9feffffffffaaa9UL,
            0x1eabfffeb153ffffUL,
            0x6730d2a0f6b0f624UL,
            0x64774b84f38512bfUL,
            0x4b1ba7b6434bacd7UL,
            0x1a0111ea397fe69aUL
        ];

        Fp result = One;
        Fp baseValue = value;

        for (int limb = 5; limb >= 0; limb--)
        {
            ulong e = pMinus2[limb];
            for (int bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                if (((e >> bit) & 1UL) != 0)
                    result = Multiply(result, baseValue);
            }
        }

        return result;
    }

    internal static Fp FromCanonical(Fp canonical) => MontgomeryReduce(MultiplyWide(canonical, MontgomeryR2));

    internal static Fp ToCanonical(Fp montgomery) => MontgomeryReduce(MultiplyWide(montgomery, RawOne));

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

    private static ulong AddCarry(ulong a, ulong b, ulong carryIn, out ulong carryOut)
    {
        UInt128 sum = (UInt128)a + b + carryIn;
        carryOut = (ulong)(sum >> 64);
        return (ulong)sum;
    }

    private static ulong SubBorrow(ulong a, ulong b, ulong borrowIn, out ulong borrowOut)
    {
        UInt128 subtrahend = (UInt128)b + borrowIn;
        borrowOut = a < subtrahend ? 1UL : 0UL;
        return (ulong)((UInt128)a - subtrahend);
    }

    internal static ulong[] SquareWide(Fp a)
    {
        var acc = new UInt128[13];
        ulong[] x = [a.L0, a.L1, a.L2, a.L3, a.L4, a.L5];

        for (int i = 0; i < 6; i++)
            MultiplyAccumulate(acc, i + i, x[i], x[i]);

        for (int i = 0; i < 6; i++)
        {
            for (int j = i + 1; j < 6; j++)
            {
                MultiplyAccumulate(acc, i + j, x[i], x[j]);
                MultiplyAccumulate(acc, i + j, x[i], x[j]);
            }
        }

        return
        [
            (ulong)acc[0], (ulong)acc[1], (ulong)acc[2], (ulong)acc[3], (ulong)acc[4], (ulong)acc[5],
            (ulong)acc[6], (ulong)acc[7], (ulong)acc[8], (ulong)acc[9], (ulong)acc[10], (ulong)acc[11]
        ];
    }

    internal static ulong[] MultiplyWide(Fp a, Fp b)
    {
        var acc = new UInt128[13];
        ulong[] x = [a.L0, a.L1, a.L2, a.L3, a.L4, a.L5];
        ulong[] y = [b.L0, b.L1, b.L2, b.L3, b.L4, b.L5];

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
                MultiplyAccumulate(acc, i + j, x[i], y[j]);
        }

        return
        [
            (ulong)acc[0], (ulong)acc[1], (ulong)acc[2], (ulong)acc[3], (ulong)acc[4], (ulong)acc[5],
            (ulong)acc[6], (ulong)acc[7], (ulong)acc[8], (ulong)acc[9], (ulong)acc[10], (ulong)acc[11]
        ];
    }

    private static Fp MontgomeryReduce(ulong[] t12)
    {
        ulong[] t = [t12[0], t12[1], t12[2], t12[3], t12[4], t12[5], t12[6], t12[7], t12[8], t12[9], t12[10], t12[11], 0UL];
        ulong[] m = [Modulus.L0, Modulus.L1, Modulus.L2, Modulus.L3, Modulus.L4, Modulus.L5];

        for (int i = 0; i < 6; i++)
        {
            ulong u = unchecked(t[i] * MontgomeryInv);
            ulong carry = 0;

            for (int j = 0; j < 6; j++)
            {
                UInt128 z = (UInt128)u * m[j] + t[i + j] + carry;
                t[i + j] = (ulong)z;
                carry = (ulong)(z >> 64);
            }

            int k = i + 6;
            for (; k < 13; k++)
            {
                UInt128 z = (UInt128)t[k] + carry;
                t[k] = (ulong)z;
                carry = (ulong)(z >> 64);
            }
        }

        Fp r = new(t[6], t[7], t[8], t[9], t[10], t[11]);
        Fp rMinusP = SubtractUnchecked(r, Modulus, out ulong borrow);
        ulong shouldSubtract = borrow ^ 1UL;
        return Select(shouldSubtract, rMinusP, r);
    }

    private static void MultiplyAccumulate(UInt128[] acc, int index, ulong x, ulong y)
    {
        UInt128 product = (UInt128)x * y;
        acc[index] += product;

        int k = index;
        while ((acc[k] >> 64) != 0)
        {
            UInt128 carry = acc[k] >> 64;
            acc[k] &= ulong.MaxValue;
            k++;
            acc[k] += carry;
        }
    }

    private static bool GreaterThanOrEqualByLimbs(Fp a, Fp b)
    {
        if (a.L5 != b.L5) return a.L5 > b.L5;
        if (a.L4 != b.L4) return a.L4 > b.L4;
        if (a.L3 != b.L3) return a.L3 > b.L3;
        if (a.L2 != b.L2) return a.L2 > b.L2;
        if (a.L1 != b.L1) return a.L1 > b.L1;
        if (a.L0 != b.L0) return a.L0 > b.L0;
        return true;
    }

    private static Fp Select(ulong bit, Fp whenOne, Fp whenZero)
    {
        ulong mask = 0UL - bit;
        return new Fp(
            SelectU64(mask, whenOne.L0, whenZero.L0),
            SelectU64(mask, whenOne.L1, whenZero.L1),
            SelectU64(mask, whenOne.L2, whenZero.L2),
            SelectU64(mask, whenOne.L3, whenZero.L3),
            SelectU64(mask, whenOne.L4, whenZero.L4),
            SelectU64(mask, whenOne.L5, whenZero.L5)
        );
    }

    private static ulong SelectU64(ulong mask, ulong whenMaskAllOnes, ulong whenMaskZero)
    {
        return (whenMaskAllOnes & mask) | (whenMaskZero & ~mask);
    }

    private static ulong IsZeroMask(Fp value)
    {
        ulong x = value.L0 | value.L1 | value.L2 | value.L3 | value.L4 | value.L5;
        return ((x | (0UL - x)) >> 63) ^ 1UL;
    }
}
