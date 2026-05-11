namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    public ulong GetBit(int i)
    {
        var c = ToCanonical(this);
        ulong limb = (i >> 6) switch { 0 => c.L0, 1 => c.L1, 2 => c.L2, _ => c.L3 };
        return (limb >> (i & 63)) & 1UL;
    }

    public static Scalar Add(Scalar a, Scalar b)
    {
        Scalar r = AddUnchecked(a, b, out ulong carry);
        Scalar rMinusP = SubUnchecked(r, GroupOrderR, out ulong borrow);
        ulong shouldSubtract = carry | (borrow ^ 1UL);
        return Select(shouldSubtract, rMinusP, r);
    }

    public static Scalar Sub(Scalar a, Scalar b)
    {
        Scalar r = SubUnchecked(a, b, out ulong borrow);
        Scalar rPlusP = AddUnchecked(r, GroupOrderR, out _);
        return Select(borrow, rPlusP, r);
    }

    public static Scalar Mul(Scalar a, Scalar b) => MontgomeryReduce(MultiplyWide(a, b));

    public static Scalar Square(Scalar a) => MontgomeryReduce(SquareWide(a));

    public static Scalar Negate(Scalar a)
    {
        Scalar neg = SubUnchecked(GroupOrderR, a, out _);
        return Select(IsZeroMask(a), Zero, neg);
    }

    public static Scalar Invert(Scalar a)
    {
        if (IsZero(a))
            throw new DivideByZeroException("Cannot invert zero in scalar field.");

        // a^(r-2) mod r, r-2 in little-endian limbs
        ulong[] rMinus2 =
        [
            0xfffffffeffffffffUL,
            0x53bda402fffe5bfeUL,
            0x3339d80809a1d805UL,
            0x73eda753299d7d48UL,
        ];

        Scalar result = One;
        for (int limb = 3; limb >= 0; limb--)
        {
            ulong e = rMinus2[limb];
            for (int bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                Scalar t = Mul(result, a);
                result = Select((e >> bit) & 1UL, t, result);
            }
        }
        return result;
    }

    internal static Scalar FromCanonical(Scalar a) => MontgomeryReduce(MultiplyWide(a, R2));

    internal static Scalar ToCanonical(Scalar a) =>
        MontgomeryReduce([a.L0, a.L1, a.L2, a.L3, 0UL, 0UL, 0UL, 0UL]);

    private static ulong[] SquareWide(Scalar a)
    {
        ulong carry;

        // phase 1: upper-triangle cross products (i < j), no 2x factor yet
        ulong r1 = CommonMath.Mac(a.L0, a.L1, 0, 0, out carry);
        ulong r2 = CommonMath.Mac(a.L0, a.L2, 0, carry, out carry);
        ulong r3 = CommonMath.Mac(a.L0, a.L3, 0, carry, out ulong r4);
        r3 = CommonMath.Mac(a.L1, a.L2, r3, 0, out carry);
        r4 = CommonMath.Mac(a.L1, a.L3, r4, carry, out ulong r5);
        r5 = CommonMath.Mac(a.L2, a.L3, r5, 0, out ulong r6);

        // phase 2: double cross terms (shift left 1 bit)
        ulong r7 = r6 >> 63;
        r6 = (r6 << 1) | (r5 >> 63);
        r5 = (r5 << 1) | (r4 >> 63);
        r4 = (r4 << 1) | (r3 >> 63);
        r3 = (r3 << 1) | (r2 >> 63);
        r2 = (r2 << 1) | (r1 >> 63);
        r1 <<= 1;

        // phase 3: add diagonal terms a[i]^2
        ulong r0 = CommonMath.Mac(a.L0, a.L0, 0, 0, out carry);
        r1 = CommonMath.AddCarry(r1, carry, 0, out carry);
        r2 = CommonMath.Mac(a.L1, a.L1, r2, carry, out carry);
        r3 = CommonMath.AddCarry(r3, carry, 0, out carry);
        r4 = CommonMath.Mac(a.L2, a.L2, r4, carry, out carry);
        r5 = CommonMath.AddCarry(r5, carry, 0, out carry);
        r6 = CommonMath.Mac(a.L3, a.L3, r6, carry, out carry);
        r7 = CommonMath.AddCarry(r7, carry, 0, out _);

        return [r0, r1, r2, r3, r4, r5, r6, r7];
    }

    private static ulong[] MultiplyWide(Scalar a, Scalar b)
    {
        var t = new ulong[8];
        ulong[] x = [a.L0, a.L1, a.L2, a.L3];
        ulong[] y = [b.L0, b.L1, b.L2, b.L3];
        for (int i = 0; i < 4; i++)
        {
            ulong carry = 0;
            for (int j = 0; j < 4; j++)
                t[i + j] = CommonMath.Mac(x[i], y[j], t[i + j], carry, out carry);
            t[i + 4] = carry;
        }
        return t;
    }

    private static Scalar MontgomeryReduce(ulong[] t8)
    {
        ulong[] t = [t8[0], t8[1], t8[2], t8[3], t8[4], t8[5], t8[6], t8[7], 0UL];
        ulong[] m = [GroupOrderR.L0, GroupOrderR.L1, GroupOrderR.L2, GroupOrderR.L3];
        for (int i = 0; i < 4; i++)
        {
            ulong u = unchecked(t[i] * MontgomeryInv);
            ulong carry = 0;
            for (int j = 0; j < 4; j++)
            {
                UInt128 z = (UInt128)u * m[j] + t[i + j] + carry;
                t[i + j] = (ulong)z;
                carry = (ulong)(z >> 64);
            }
            int k = i + 4;
            for (; k < 9; k++)
            {
                UInt128 z = (UInt128)t[k] + carry;
                t[k] = (ulong)z;
                carry = (ulong)(z >> 64);
            }
        }
        Scalar r = new(t[4], t[5], t[6], t[7]);
        Scalar rMinusM = SubUnchecked(r, GroupOrderR, out ulong borrow);
        return Select(borrow ^ 1UL, rMinusM, r);
    }

    private static Scalar Select(ulong bit, Scalar whenOne, Scalar whenZero)
    {
        ulong mask = 0UL - bit;
        return new Scalar(
            CommonMath.SelectU64(mask, whenOne.L0, whenZero.L0),
            CommonMath.SelectU64(mask, whenOne.L1, whenZero.L1),
            CommonMath.SelectU64(mask, whenOne.L2, whenZero.L2),
            CommonMath.SelectU64(mask, whenOne.L3, whenZero.L3)
        );
    }

    private static Scalar AddUnchecked(Scalar a, Scalar b, out ulong carry)
    {
        ulong c = 0;
        ulong l0 = CommonMath.AddCarry(a.L0, b.L0, c, out c);
        ulong l1 = CommonMath.AddCarry(a.L1, b.L1, c, out c);
        ulong l2 = CommonMath.AddCarry(a.L2, b.L2, c, out c);
        ulong l3 = CommonMath.AddCarry(a.L3, b.L3, c, out carry);
        return new Scalar(l0, l1, l2, l3);
    }

    private static Scalar SubUnchecked(Scalar a, Scalar b, out ulong borrow)
    {
        ulong brrw = 0;
        ulong l0 = CommonMath.SubBorrow(a.L0, b.L0, brrw, out brrw);
        ulong l1 = CommonMath.SubBorrow(a.L1, b.L1, brrw, out brrw);
        ulong l2 = CommonMath.SubBorrow(a.L2, b.L2, brrw, out brrw);
        ulong l3 = CommonMath.SubBorrow(a.L3, b.L3, brrw, out borrow);
        return new Scalar(l0, l1, l2, l3);
    }

    private static ulong IsZeroMask(Scalar a)
    {
        ulong x = a.L0 | a.L1 | a.L2 | a.L3;
        return ((x | (0UL - x)) >> 63) ^ 1UL;
    }
}
