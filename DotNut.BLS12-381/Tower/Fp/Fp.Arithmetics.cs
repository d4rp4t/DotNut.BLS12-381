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
        return Select(shouldSubtract, rMinusP, r);
    }

    /// <summary>
    /// Computes <c>(a - b) mod p</c>.
    /// </summary>
    /// <returns>The difference reduced modulo the field modulus.</returns>
    public static Fp Subtract(Fp a, Fp b)
    {
        Fp r = SubtractUnchecked(a, b, out ulong borrow);
        Fp rPlusP = AddUnchecked(r, Modulus, out _);
        return Select(borrow, rPlusP, r);
    }

    /// <summary>
    /// Computes <c>(a × b) mod p</c>.
    /// </summary>
    /// <remarks>
    /// Both operands are assumed to be in Montgomery form.
    /// </remarks>
    public static Fp Multiply(Fp a, Fp b) => MontgomeryReduce(MultiplyWide(a, b));

    /// <summary>
    /// Computes <c>a^2 mod p</c>.
    /// </summary>
    public static Fp Square(Fp a) => MontgomeryReduce(SquareWide(a));

    /// <summary>
    /// Computes the additive inverse <c>(-value) mod p</c>.
    /// </summary>
    public static Fp Negate(Fp value)
    {
        Fp neg = SubtractUnchecked(Modulus, value, out _);
        ulong isZero = IsZeroMask(value);
        return Select(isZero, Zero, neg);
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
                result = Select(b, multiplied, result);
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
        return Select(shouldSubtract, rMinusP, r);
    }

    /// <summary>
    /// "Constant-time" conditional selection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fp Select(ulong bit, Fp whenOne, Fp whenZero)
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
    /// Returns the sign of <paramref name="value"/> as defined in RFC 9380 §4.1:
    /// 1 if the canonical (non-Montgomery) representation is odd, 0 if even.
    /// Canonicalizes via Montgomery reduction before extracting the LSB.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Sgn0(Fp value) => ToCanonical(value).L0 & 1UL;
}
