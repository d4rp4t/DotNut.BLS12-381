using System.Runtime.CompilerServices;

namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{

    /// <summary>
    /// Computes <c>(a + b) mod p</c>.
    /// </summary>
    /// <returns>The sum reduced modulo the field modulus.</returns>
    public static Fp Add(Fp a, Fp b)
    {
        Fp r = AddUnchecked(a, b, out ulong carry);
        Fp rMinusP = SubtractUnchecked(r, Modulus, out ulong borrow);
        ulong shouldSubtract = carry | (borrow ^ 1UL);
        return ConditionalSelect(shouldSubtract, rMinusP, r);
    }

    /// <summary>
    /// Computes <c>(a - b) mod p</c>.
    /// </summary>
    /// <returns>The difference reduced modulo the field modulus.</returns>
    public static Fp Subtract(Fp a, Fp b)
    {
        Fp r = SubtractUnchecked(a, b, out ulong borrow);
        Fp rPlusP = AddUnchecked(r, Modulus, out _);
        return ConditionalSelect(borrow, rPlusP, r);
    }

    /// <summary>
    /// Computes <c>(a × b) mod p</c>.
    /// Both operands are assumed to be in Montgomery form.
    /// Uses a fully unrolled 6×6 schoolbook multiply to avoid heap allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp Multiply(Fp a, Fp b)
    {
        ulong c;
        ulong t0  = CommonMath.Mac(a.L0, b.L0, 0UL, 0UL, out c);
        ulong t1  = CommonMath.Mac(a.L0, b.L1, 0UL, c,   out c);
        ulong t2  = CommonMath.Mac(a.L0, b.L2, 0UL, c,   out c);
        ulong t3  = CommonMath.Mac(a.L0, b.L3, 0UL, c,   out c);
        ulong t4  = CommonMath.Mac(a.L0, b.L4, 0UL, c,   out c);
        ulong t5  = CommonMath.Mac(a.L0, b.L5, 0UL, c,   out ulong t6);
               t1 = CommonMath.Mac(a.L1, b.L0, t1,  0UL, out c);
               t2 = CommonMath.Mac(a.L1, b.L1, t2,  c,   out c);
               t3 = CommonMath.Mac(a.L1, b.L2, t3,  c,   out c);
               t4 = CommonMath.Mac(a.L1, b.L3, t4,  c,   out c);
               t5 = CommonMath.Mac(a.L1, b.L4, t5,  c,   out c);
               t6 = CommonMath.Mac(a.L1, b.L5, t6,  c,   out ulong t7);
               t2 = CommonMath.Mac(a.L2, b.L0, t2,  0UL, out c);
               t3 = CommonMath.Mac(a.L2, b.L1, t3,  c,   out c);
               t4 = CommonMath.Mac(a.L2, b.L2, t4,  c,   out c);
               t5 = CommonMath.Mac(a.L2, b.L3, t5,  c,   out c);
               t6 = CommonMath.Mac(a.L2, b.L4, t6,  c,   out c);
               t7 = CommonMath.Mac(a.L2, b.L5, t7,  c,   out ulong t8);
               t3 = CommonMath.Mac(a.L3, b.L0, t3,  0UL, out c);
               t4 = CommonMath.Mac(a.L3, b.L1, t4,  c,   out c);
               t5 = CommonMath.Mac(a.L3, b.L2, t5,  c,   out c);
               t6 = CommonMath.Mac(a.L3, b.L3, t6,  c,   out c);
               t7 = CommonMath.Mac(a.L3, b.L4, t7,  c,   out c);
               t8 = CommonMath.Mac(a.L3, b.L5, t8,  c,   out ulong t9);
               t4 = CommonMath.Mac(a.L4, b.L0, t4,  0UL, out c);
               t5 = CommonMath.Mac(a.L4, b.L1, t5,  c,   out c);
               t6 = CommonMath.Mac(a.L4, b.L2, t6,  c,   out c);
               t7 = CommonMath.Mac(a.L4, b.L3, t7,  c,   out c);
               t8 = CommonMath.Mac(a.L4, b.L4, t8,  c,   out c);
               t9 = CommonMath.Mac(a.L4, b.L5, t9,  c,   out ulong t10);
               t5 = CommonMath.Mac(a.L5, b.L0, t5,  0UL, out c);
               t6 = CommonMath.Mac(a.L5, b.L1, t6,  c,   out c);
               t7 = CommonMath.Mac(a.L5, b.L2, t7,  c,   out c);
               t8 = CommonMath.Mac(a.L5, b.L3, t8,  c,   out c);
               t9 = CommonMath.Mac(a.L5, b.L4, t9,  c,   out c);
              t10 = CommonMath.Mac(a.L5, b.L5, t10, c,   out ulong t11);
        return MontgomeryReduce(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
    }

    /// <summary>
    /// Computes <c>a² mod p</c> using the Comba squaring method:
    /// upper-triangle cross-products, double via bit-shift, add diagonal terms.
    /// No heap allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp Square(Fp a)
    {
        ulong c;
        // Phase 1: upper-triangle cross-products a[i]*a[j] for i < j
        ulong t1  = CommonMath.Mac(a.L0, a.L1, 0UL, 0UL, out c);
        ulong t2  = CommonMath.Mac(a.L0, a.L2, 0UL, c,   out c);
        ulong t3  = CommonMath.Mac(a.L0, a.L3, 0UL, c,   out c);
        ulong t4  = CommonMath.Mac(a.L0, a.L4, 0UL, c,   out c);
        ulong t5  = CommonMath.Mac(a.L0, a.L5, 0UL, c,   out ulong t6);
               t3 = CommonMath.Mac(a.L1, a.L2, t3,  0UL, out c);
               t4 = CommonMath.Mac(a.L1, a.L3, t4,  c,   out c);
               t5 = CommonMath.Mac(a.L1, a.L4, t5,  c,   out c);
               t6 = CommonMath.Mac(a.L1, a.L5, t6,  c,   out ulong t7);
               t5 = CommonMath.Mac(a.L2, a.L3, t5,  0UL, out c);
               t6 = CommonMath.Mac(a.L2, a.L4, t6,  c,   out c);
               t7 = CommonMath.Mac(a.L2, a.L5, t7,  c,   out ulong t8);
               t7 = CommonMath.Mac(a.L3, a.L4, t7,  0UL, out c);
               t8 = CommonMath.Mac(a.L3, a.L5, t8,  c,   out ulong t9);
               t9 = CommonMath.Mac(a.L4, a.L5, t9,  0UL, out ulong t10);
        // Phase 2: double the cross-products (left-shift the 640-bit value by 1)
        ulong t11 = t10 >> 63;
              t10 = (t10 << 1) | (t9  >> 63);
               t9 = (t9  << 1) | (t8  >> 63);
               t8 = (t8  << 1) | (t7  >> 63);
               t7 = (t7  << 1) | (t6  >> 63);
               t6 = (t6  << 1) | (t5  >> 63);
               t5 = (t5  << 1) | (t4  >> 63);
               t4 = (t4  << 1) | (t3  >> 63);
               t3 = (t3  << 1) | (t2  >> 63);
               t2 = (t2  << 1) | (t1  >> 63);
               t1 <<= 1;
        // Phase 3: add diagonal terms a[i]²
        ulong t0  = CommonMath.Mac(a.L0, a.L0, 0UL, 0UL, out c);
               t1 = CommonMath.AddCarry(t1,  c,   0UL, out c);
               t2 = CommonMath.Mac(a.L1, a.L1, t2,  c,   out c);
               t3 = CommonMath.AddCarry(t3,  c,   0UL, out c);
               t4 = CommonMath.Mac(a.L2, a.L2, t4,  c,   out c);
               t5 = CommonMath.AddCarry(t5,  c,   0UL, out c);
               t6 = CommonMath.Mac(a.L3, a.L3, t6,  c,   out c);
               t7 = CommonMath.AddCarry(t7,  c,   0UL, out c);
               t8 = CommonMath.Mac(a.L4, a.L4, t8,  c,   out c);
               t9 = CommonMath.AddCarry(t9,  c,   0UL, out c);
              t10 = CommonMath.Mac(a.L5, a.L5, t10, c,   out c);
              t11 = CommonMath.AddCarry(t11, c,   0UL, out _);
        return MontgomeryReduce(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
    }

    /// <summary>
    /// Computes the additive inverse <c>(-value) mod p</c>.
    /// </summary>
    public static Fp Negate(Fp value)
    {
        Fp neg = SubtractUnchecked(Modulus, value, out _);
        ulong isZero = IsZeroMask(value);
        return ConditionalSelect(isZero, Zero, neg);
    }

    /// <summary>
    /// Computes the multiplicative inverse of <paramref name="value"/>.
    /// </summary>
    /// <returns>
    /// <c>value^-1 mod p</c>.
    /// </returns>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="value"/> is zero.
    /// </exception>
    /// <remarks>
    /// Uses exponentiation by <c>p - 2</c> (Fermat's little theorem).
    /// </remarks>
    public static Fp Invert(Fp value)
    {
        if (Equal(value, Zero))
            throw new DivideByZeroException("Cannot invert zero in Fp.");

        // a^(p-2) in Montgomery domain
        ulong[] pMinus2 =
        [
            0xb9fe_ffff_ffff_aaa9UL,
            0x1eab_fffe_b153_ffffUL,
            0x6730_d2a0_f6b0_f624UL,
            0x6477_4b84_f385_12bfUL,
            0x4b1b_a7b6_434b_acd7UL,
            0x1a01_11ea_397f_e69aUL
        ];

        Fp result = One;
        Fp baseValue = value;

        for (int limb = 5; limb >= 0; limb--)
        {
            ulong e = pMinus2[limb];
            for (int bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                Fp multiplied = Multiply(result, baseValue);
                ulong b = (e >> bit) & 1UL;
                result = ConditionalSelect(b, multiplied, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a canonical field element into Montgomery form.
    /// </summary>
    internal static Fp FromCanonical(Fp canonical) => MontgomeryReduce(MultiplyWide(canonical, MontgomeryR2));

    /// <summary>
    /// Converts a Montgomery-form element into canonical representation.
    /// </summary>
    internal static Fp ToCanonical(Fp montgomery) => MontgomeryReduce(MultiplyWide(montgomery, RawOne));
    
    /// <summary>
    /// Adds two limbs without modular reduction.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fp AddUnchecked(Fp a, Fp b, out ulong carry)
    {
        ulong c = 0;
        ulong l0 = CommonMath.AddCarry(a.L0, b.L0, c, out c);
        ulong l1 = CommonMath.AddCarry(a.L1, b.L1, c, out c);
        ulong l2 = CommonMath.AddCarry(a.L2, b.L2, c, out c);
        ulong l3 = CommonMath.AddCarry(a.L3, b.L3, c, out c);
        ulong l4 = CommonMath.AddCarry(a.L4, b.L4, c, out c);
        ulong l5 = CommonMath.AddCarry(a.L5, b.L5, c, out carry);
        return new Fp(l0, l1, l2, l3, l4, l5);
    }

    /// <summary>
    /// Subtracts two limbs without modular reduction.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fp SubtractUnchecked(Fp a, Fp b, out ulong borrow)
    {
        ulong brrw = 0;
        ulong l0 = CommonMath.SubBorrow(a.L0, b.L0, brrw, out brrw);
        ulong l1 = CommonMath.SubBorrow(a.L1, b.L1, brrw, out brrw);
        ulong l2 = CommonMath.SubBorrow(a.L2, b.L2, brrw, out brrw);
        ulong l3 = CommonMath.SubBorrow(a.L3, b.L3, brrw, out brrw);
        ulong l4 = CommonMath.SubBorrow(a.L4, b.L4, brrw, out brrw);
        ulong l5 = CommonMath.SubBorrow(a.L5, b.L5, brrw, out borrow);
        return new Fp(l0, l1, l2, l3, l4, l5);
    }
    
    internal static ulong[] SquareWide(Fp a) => MultiplyWide(a, a);

    /// <summary>
    /// Computes the full 768-bit product before Montgomery reduction.
    /// </summary>
    internal static ulong[] MultiplyWide(Fp a, Fp b)
    {
        var t = new ulong[12];
        ulong[] x = [a.L0, a.L1, a.L2, a.L3, a.L4, a.L5];
        ulong[] y = [b.L0, b.L1, b.L2, b.L3, b.L4, b.L5];

        for (int i = 0; i < 6; i++)
        {
            ulong carry = 0;
            for (int j = 0; j < 6; j++)
                t[i + j] = CommonMath.Mac(x[i], y[j], t[i + j], carry, out carry);
            t[i + 6] = carry;
        }

        return t;
    }
    
    /// <summary>
    /// Fully unrolled Montgomery reduction on a 768-bit product given as 12 limbs.
    /// Called by <see cref="Multiply"/> and <see cref="Square"/>. No heap allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Fp MontgomeryReduce(
        ulong t0,  ulong t1,  ulong t2,  ulong t3,  ulong t4,  ulong t5,
        ulong t6,  ulong t7,  ulong t8,  ulong t9,  ulong t10, ulong t11)
    {
        ulong c, k;
        k  = unchecked(t0 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, t0,  0UL, out c);
        ulong r1 = CommonMath.Mac(k, Modulus.L1, t1,  c, out c);
        ulong r2 = CommonMath.Mac(k, Modulus.L2, t2,  c, out c);
        ulong r3 = CommonMath.Mac(k, Modulus.L3, t3,  c, out c);
        ulong r4 = CommonMath.Mac(k, Modulus.L4, t4,  c, out c);
        ulong r5 = CommonMath.Mac(k, Modulus.L5, t5,  c, out c);
        ulong r6 = CommonMath.AddCarry(t6,  c,   0UL, out ulong r7);
        k  = unchecked(r1 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, r1,  0UL, out c);
               r2 = CommonMath.Mac(k, Modulus.L1, r2,  c, out c);
               r3 = CommonMath.Mac(k, Modulus.L2, r3,  c, out c);
               r4 = CommonMath.Mac(k, Modulus.L3, r4,  c, out c);
               r5 = CommonMath.Mac(k, Modulus.L4, r5,  c, out c);
               r6 = CommonMath.Mac(k, Modulus.L5, r6,  c, out c);
               r7 = CommonMath.AddCarry(t7,  c,   r7,  out ulong r8);
        k  = unchecked(r2 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, r2,  0UL, out c);
               r3 = CommonMath.Mac(k, Modulus.L1, r3,  c, out c);
               r4 = CommonMath.Mac(k, Modulus.L2, r4,  c, out c);
               r5 = CommonMath.Mac(k, Modulus.L3, r5,  c, out c);
               r6 = CommonMath.Mac(k, Modulus.L4, r6,  c, out c);
               r7 = CommonMath.Mac(k, Modulus.L5, r7,  c, out c);
               r8 = CommonMath.AddCarry(t8,  c,   r8,  out ulong r9);
        k  = unchecked(r3 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, r3,  0UL, out c);
               r4 = CommonMath.Mac(k, Modulus.L1, r4,  c, out c);
               r5 = CommonMath.Mac(k, Modulus.L2, r5,  c, out c);
               r6 = CommonMath.Mac(k, Modulus.L3, r6,  c, out c);
               r7 = CommonMath.Mac(k, Modulus.L4, r7,  c, out c);
               r8 = CommonMath.Mac(k, Modulus.L5, r8,  c, out c);
               r9 = CommonMath.AddCarry(t9,  c,   r9,  out ulong r10);
        k  = unchecked(r4 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, r4,  0UL, out c);
               r5 = CommonMath.Mac(k, Modulus.L1, r5,  c, out c);
               r6 = CommonMath.Mac(k, Modulus.L2, r6,  c, out c);
               r7 = CommonMath.Mac(k, Modulus.L3, r7,  c, out c);
               r8 = CommonMath.Mac(k, Modulus.L4, r8,  c, out c);
               r9 = CommonMath.Mac(k, Modulus.L5, r9,  c, out c);
              r10 = CommonMath.AddCarry(t10, c,   r10, out ulong r11);
        k  = unchecked(r5 * MontgomeryInv);
        CommonMath.Mac(k, Modulus.L0, r5,  0UL, out c);
               r6 = CommonMath.Mac(k, Modulus.L1, r6,  c, out c);
               r7 = CommonMath.Mac(k, Modulus.L2, r7,  c, out c);
               r8 = CommonMath.Mac(k, Modulus.L3, r8,  c, out c);
               r9 = CommonMath.Mac(k, Modulus.L4, r9,  c, out c);
              r10 = CommonMath.Mac(k, Modulus.L5, r10, c, out c);
              r11 = CommonMath.AddCarry(t11, c,   r11, out _);
        Fp r = new(r6, r7, r8, r9, r10, r11);
        Fp rMinusP = SubtractUnchecked(r, Modulus, out ulong borrow);
        return ConditionalSelect(borrow ^ 1UL, rMinusP, r);
    }

    /// <summary>
    /// Performs Montgomery reduction on a 768-bit intermediate product.
    /// </summary>
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
        return ConditionalSelect(shouldSubtract, rMinusP, r);
    }

    /// <summary>
    /// "Constant-time" conditional selection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fp ConditionalSelect(ulong bit, Fp whenOne, Fp whenZero)
    {
        ulong mask = 0UL - bit;
        return new Fp(
            CommonMath.SelectU64(mask, whenOne.L0, whenZero.L0),
            CommonMath.SelectU64(mask, whenOne.L1, whenZero.L1),
            CommonMath.SelectU64(mask, whenOne.L2, whenZero.L2),
            CommonMath.SelectU64(mask, whenOne.L3, whenZero.L3),
            CommonMath.SelectU64(mask, whenOne.L4, whenZero.L4),
            CommonMath.SelectU64(mask, whenOne.L5, whenZero.L5)
        );
    }
    
    /// <summary>
    /// Returns 1 if the value is zero; otherwise 0.
    /// Operates on raw Montgomery limbs — does not canonicalize first.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong IsZeroMask(Fp value)
    {
        ulong x = value.L0 | value.L1 | value.L2 | value.L3 | value.L4 | value.L5;
        return ((x | (0UL - x)) >> 63) ^ 1UL;
    }

    /// <summary>
    /// Constant-time equality check. Returns 1 if <paramref name="a"/> and <paramref name="b"/>
    /// represent the same field element (raw limb comparison), 0 otherwise.
    /// Both inputs must be in reduced form (output of field arithmetic) for this to be correct.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CtEqual(Fp a, Fp b)
    {
        ulong diff = (a.L0 ^ b.L0) | (a.L1 ^ b.L1) | (a.L2 ^ b.L2)
                   | (a.L3 ^ b.L3) | (a.L4 ^ b.L4) | (a.L5 ^ b.L5);
        return ((diff | (0UL - diff)) >> 63) ^ 1UL;
    }

    /// <summary>
    /// Branchless select: returns <paramref name="a"/> when mask=0, <paramref name="b"/> when mask=~0UL.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fp ConditionalSelect(Fp a, Fp b, ulong mask) => new(
        (b.L0 & mask) | (a.L0 & ~mask),
        (b.L1 & mask) | (a.L1 & ~mask),
        (b.L2 & mask) | (a.L2 & ~mask),
        (b.L3 & mask) | (a.L3 & ~mask),
        (b.L4 & mask) | (a.L4 & ~mask),
        (b.L5 & mask) | (a.L5 & ~mask)
    );

    /// <summary>
    /// Returns <paramref name="a"/> if <paramref name="choice"/> is <see langword="false"/>,
    /// <paramref name="b"/> if <paramref name="choice"/> is <see langword="true"/>.
    /// Converts the bool to a mask using the CLI guarantee that false=0, true=1 as a byte.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp ConditionalSelect(Fp a, Fp b, bool choice)
    {
        ulong mask = 0UL - Unsafe.As<bool, byte>(ref Unsafe.AsRef(in choice));
        return ConditionalSelect(a, b, mask);
    }

    /// <summary>
    /// Returns the sign of <paramref name="value"/> as defined in RFC 9380 §4.1:
    /// 1 if the canonical (non-Montgomery) representation is odd, 0 if even.
    /// Canonicalizes via Montgomery reduction before extracting the LSB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Sgn0(Fp value) => ToCanonical(value).L0 & 1UL;
}
