namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>
    /// Returns bit <paramref name="i"/> (0-indexed, LSB = 0) of the scalar's canonical value.
    /// Converts to canonical form first, then extracts the bit from the appropriate limb.
    /// </summary>
    /// <returns>0 or 1 as a <see cref="ulong"/>.</returns>
    internal ulong GetBit(int i)
    {
        var c = ToCanonical(this);
        ulong limb = (i >> 6) switch { 0 => c.L0, 1 => c.L1, 2 => c.L2, _ => c.L3 };
        return (limb >> (i & 63)) & 1UL;
    }

    /// <summary>
    /// Returns a + b in Fr. Adds the raw limbs, then conditionally subtracts r to keep the result in [0, r).
    /// Inputs must be in Montgomery form (i.e. both are valid scalar field elements).
    /// </summary>
    public static Scalar Add(Scalar a, Scalar b)
    {
        Scalar r = AddUnchecked(a, b, out ulong carry);
        Scalar rMinusP = SubUnchecked(r, GroupOrderR, out ulong borrow);
        ulong shouldSubtract = carry | (borrow ^ 1UL);
        return ConditionalSelect(shouldSubtract, rMinusP, r);
    }

    /// <summary>
    /// Returns a − b in Fr. Subtracts the raw limbs, then conditionally adds r on underflow.
    /// Inputs must be in Montgomery form.
    /// </summary>
    public static Scalar Sub(Scalar a, Scalar b)
    {
        Scalar r = SubUnchecked(a, b, out ulong borrow);
        Scalar rPlusP = AddUnchecked(r, GroupOrderR, out _);
        return ConditionalSelect(borrow, rPlusP, r);
    }

    /// <summary>
    /// Returns a · b in Fr using a fully unrolled 4×4 schoolbook multiply. No heap allocation.
    /// Both inputs must be in Montgomery form.
    /// </summary>
    public static Scalar Mul(Scalar a, Scalar b)
    {
        ulong c;
        ulong t0 = CommonMath.Mac(a.L0, b.L0, 0UL, 0UL, out c);
        ulong t1 = CommonMath.Mac(a.L0, b.L1, 0UL, c,   out c);
        ulong t2 = CommonMath.Mac(a.L0, b.L2, 0UL, c,   out c);
        ulong t3 = CommonMath.Mac(a.L0, b.L3, 0UL, c,   out ulong t4);
              t1 = CommonMath.Mac(a.L1, b.L0, t1,  0UL, out c);
              t2 = CommonMath.Mac(a.L1, b.L1, t2,  c,   out c);
              t3 = CommonMath.Mac(a.L1, b.L2, t3,  c,   out c);
              t4 = CommonMath.Mac(a.L1, b.L3, t4,  c,   out ulong t5);
              t2 = CommonMath.Mac(a.L2, b.L0, t2,  0UL, out c);
              t3 = CommonMath.Mac(a.L2, b.L1, t3,  c,   out c);
              t4 = CommonMath.Mac(a.L2, b.L2, t4,  c,   out c);
              t5 = CommonMath.Mac(a.L2, b.L3, t5,  c,   out ulong t6);
              t3 = CommonMath.Mac(a.L3, b.L0, t3,  0UL, out c);
              t4 = CommonMath.Mac(a.L3, b.L1, t4,  c,   out c);
              t5 = CommonMath.Mac(a.L3, b.L2, t5,  c,   out c);
              t6 = CommonMath.Mac(a.L3, b.L3, t6,  c,   out ulong t7);
        return MontgomeryReduce(t0, t1, t2, t3, t4, t5, t6, t7);
    }

    /// <summary>
    /// Returns a² in Fr using the Comba squaring method (upper-triangle + doubling + diagonal).
    /// No heap allocation. Input must be in Montgomery form.
    /// </summary>
    public static Scalar Square(Scalar a)
    {
        ulong c;
        // Phase 1: upper-triangle cross-products
        ulong r1 = CommonMath.Mac(a.L0, a.L1, 0UL, 0UL, out c);
        ulong r2 = CommonMath.Mac(a.L0, a.L2, 0UL, c,   out c);
        ulong r3 = CommonMath.Mac(a.L0, a.L3, 0UL, c,   out ulong r4);
               r3 = CommonMath.Mac(a.L1, a.L2, r3,  0UL, out c);
               r4 = CommonMath.Mac(a.L1, a.L3, r4,  c,   out ulong r5);
               r5 = CommonMath.Mac(a.L2, a.L3, r5,  0UL, out ulong r6);
        // Phase 2: double the cross-products
        ulong r7 = r6 >> 63;
              r6 = (r6 << 1) | (r5 >> 63);
              r5 = (r5 << 1) | (r4 >> 63);
              r4 = (r4 << 1) | (r3 >> 63);
              r3 = (r3 << 1) | (r2 >> 63);
              r2 = (r2 << 1) | (r1 >> 63);
              r1 <<= 1;
        // Phase 3: add diagonal terms a[i]²
        ulong r0 = CommonMath.Mac(a.L0, a.L0, 0UL, 0UL, out c);
               r1 = CommonMath.AddCarry(r1, c,   0UL, out c);
               r2 = CommonMath.Mac(a.L1, a.L1, r2,  c,   out c);
               r3 = CommonMath.AddCarry(r3, c,   0UL, out c);
               r4 = CommonMath.Mac(a.L2, a.L2, r4,  c,   out c);
               r5 = CommonMath.AddCarry(r5, c,   0UL, out c);
               r6 = CommonMath.Mac(a.L3, a.L3, r6,  c,   out c);
               r7 = CommonMath.AddCarry(r7, c,   0UL, out _);
        return MontgomeryReduce(r0, r1, r2, r3, r4, r5, r6, r7);
    }

    /// <summary>
    /// Returns −a in Fr. Returns zero if a is zero (branchless).
    /// Input must be in Montgomery form.
    /// </summary>
    public static Scalar Negate(Scalar a)
    {
        Scalar neg = SubUnchecked(GroupOrderR, a, out _);
        return ConditionalSelect(IsZeroMask(a), Zero, neg);
    }

    /// <summary>
    /// Returns a⁻¹ in Fr using Fermat's little theorem: a^(r−2) mod r.
    /// Implemented as a constant-time square-and-multiply over a fixed 256-bit exponent.
    /// </summary>
    /// <exception cref="DivideByZeroException">Thrown if <paramref name="a"/> is zero.</exception>
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
                result = ConditionalSelect((e >> bit) & 1UL, t, result);
            }
        }
        return result;
    }

    /// <summary>
    /// Converts an integer a in [0, r) to Montgomery form by computing MontgomeryReduce(a · R²) = a·R mod r.
    /// Input raw limbs must represent a canonical integer in [0, r); they are NOT already in Montgomery form.
    /// </summary>
    internal static Scalar FromCanonical(Scalar a) => MontgomeryReduce(MultiplyWide(a, R2));

    /// <summary>
    /// Converts a scalar from Montgomery form back to its canonical integer in [0, r).
    /// Computes MontgomeryReduce([a_limbs, 0, 0, 0, 0]) = a·R⁻¹ mod r.
    /// Result raw limbs represent the canonical integer, NOT in Montgomery form.
    /// </summary>
    internal static Scalar ToCanonical(Scalar a) =>
        MontgomeryReduce([a.L0, a.L1, a.L2, a.L3, 0UL, 0UL, 0UL, 0UL]);

    /// <summary>
    /// Computes the 512-bit product of <paramref name="a"/> × <paramref name="a"/> using the
    /// Comba squaring method. Exploits symmetry: off-diagonal terms are computed once then doubled.
    /// Returns 8 limbs in little-endian order. Does not perform any modular reduction.
    /// </summary>
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

    /// <summary>
    /// Computes the 512-bit schoolbook product <paramref name="a"/> × <paramref name="b"/>.
    /// Returns 8 limbs in little-endian order. Does not perform any modular reduction.
    /// </summary>
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

    /// <summary>
    /// Fully unrolled Montgomery reduction on a 512-bit product given as 8 limbs.
    /// Called by <see cref="Mul"/> and <see cref="Square"/>. No heap allocation.
    /// </summary>
    private static Scalar MontgomeryReduce(
        ulong t0, ulong t1, ulong t2, ulong t3,
        ulong t4, ulong t5, ulong t6, ulong t7)
    {
        ulong c, k;
        k  = unchecked(t0 * MontgomeryInv);
        CommonMath.Mac(k, GroupOrderR.L0, t0, 0UL, out c);
        ulong r1 = CommonMath.Mac(k, GroupOrderR.L1, t1, c, out c);
        ulong r2 = CommonMath.Mac(k, GroupOrderR.L2, t2, c, out c);
        ulong r3 = CommonMath.Mac(k, GroupOrderR.L3, t3, c, out c);
        ulong r4 = CommonMath.AddCarry(t4, c, 0UL, out ulong r5);
        k  = unchecked(r1 * MontgomeryInv);
        CommonMath.Mac(k, GroupOrderR.L0, r1, 0UL, out c);
               r2 = CommonMath.Mac(k, GroupOrderR.L1, r2, c, out c);
               r3 = CommonMath.Mac(k, GroupOrderR.L2, r3, c, out c);
               r4 = CommonMath.Mac(k, GroupOrderR.L3, r4, c, out c);
               r5 = CommonMath.AddCarry(t5, c, r5, out ulong r6);
        k  = unchecked(r2 * MontgomeryInv);
        CommonMath.Mac(k, GroupOrderR.L0, r2, 0UL, out c);
               r3 = CommonMath.Mac(k, GroupOrderR.L1, r3, c, out c);
               r4 = CommonMath.Mac(k, GroupOrderR.L2, r4, c, out c);
               r5 = CommonMath.Mac(k, GroupOrderR.L3, r5, c, out c);
               r6 = CommonMath.AddCarry(t6, c, r6, out ulong r7);
        k  = unchecked(r3 * MontgomeryInv);
        CommonMath.Mac(k, GroupOrderR.L0, r3, 0UL, out c);
               r4 = CommonMath.Mac(k, GroupOrderR.L1, r4, c, out c);
               r5 = CommonMath.Mac(k, GroupOrderR.L2, r5, c, out c);
               r6 = CommonMath.Mac(k, GroupOrderR.L3, r6, c, out c);
               r7 = CommonMath.AddCarry(t7, c, r7, out _);
        Scalar r = new(r4, r5, r6, r7);
        Scalar rMinusM = SubUnchecked(r, GroupOrderR, out ulong borrow);
        return ConditionalSelect(borrow ^ 1UL, rMinusM, r);
    }

    /// <summary>
    /// Performs Montgomery reduction on the 512-bit value <paramref name="t8"/>.
    /// Computes t · R⁻¹ mod r and returns the result in [0, r).
    /// Uses MontgomeryInv = −r⁻¹ mod 2^64.
    /// </summary>
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
        return ConditionalSelect(borrow ^ 1UL, rMinusM, r);
    }

    /// <summary>
    /// Branchless conditional select: returns <paramref name="whenOne"/> if <paramref name="bit"/> = 1,
    /// <paramref name="whenZero"/> if <paramref name="bit"/> = 0.
    /// Does not perform any field arithmetic.
    /// </summary>
    private static Scalar ConditionalSelect(ulong bit, Scalar whenOne, Scalar whenZero)
    {
        ulong mask = 0UL - bit;
        return new Scalar(
            CommonMath.SelectU64(mask, whenOne.L0, whenZero.L0),
            CommonMath.SelectU64(mask, whenOne.L1, whenZero.L1),
            CommonMath.SelectU64(mask, whenOne.L2, whenZero.L2),
            CommonMath.SelectU64(mask, whenOne.L3, whenZero.L3)
        );
    }

    /// <summary>
    /// Adds the raw limbs of a and b, propagating carries. Does NOT reduce modulo r.
    /// Sets <paramref name="carry"/> to 1 if the result overflowed 256 bits.
    /// </summary>
    private static Scalar AddUnchecked(Scalar a, Scalar b, out ulong carry)
    {
        ulong c = 0;
        ulong l0 = CommonMath.AddCarry(a.L0, b.L0, c, out c);
        ulong l1 = CommonMath.AddCarry(a.L1, b.L1, c, out c);
        ulong l2 = CommonMath.AddCarry(a.L2, b.L2, c, out c);
        ulong l3 = CommonMath.AddCarry(a.L3, b.L3, c, out carry);
        return new Scalar(l0, l1, l2, l3);
    }

    /// <summary>
    /// Subtracts the raw limbs of b from a, propagating borrows. Does NOT reduce modulo r.
    /// Sets <paramref name="borrow"/> to 1 if the result underflowed.
    /// </summary>
    private static Scalar SubUnchecked(Scalar a, Scalar b, out ulong borrow)
    {
        ulong brrw = 0;
        ulong l0 = CommonMath.SubBorrow(a.L0, b.L0, brrw, out brrw);
        ulong l1 = CommonMath.SubBorrow(a.L1, b.L1, brrw, out brrw);
        ulong l2 = CommonMath.SubBorrow(a.L2, b.L2, brrw, out brrw);
        ulong l3 = CommonMath.SubBorrow(a.L3, b.L3, brrw, out borrow);
        return new Scalar(l0, l1, l2, l3);
    }

    /// <summary>
    /// Square-and-multiply exponentiation. <paramref name="exp"/> is a little-endian
    /// array of 64-bit limbs; bits are processed from MSB to LSB (variable-time).
    /// </summary>
    internal static Scalar Pow(Scalar value, ReadOnlySpan<ulong> exp)
    {
        Scalar result = One;
        for (int limb = exp.Length - 1; limb >= 0; limb--)
        {
            for (int bit = 63; bit >= 0; bit--)
            {
                result = Square(result);
                if (((exp[limb] >> bit) & 1UL) != 0UL)
                    result = Mul(result, value);
            }
        }
        return result;
    }

    /// <summary>
    /// Computes a square root of <paramref name="value"/> in Fr using Tonelli-Shanks.
    /// Returns <see langword="null"/> if <paramref name="value"/> is a non-residue.
    /// </summary>
    public static Scalar? Sqrt(Scalar value)
    {
        if (IsZero(value)) return Zero;

        // (t-1)/2 where r-1 = 2^32 * t
        ReadOnlySpan<ulong> exp = [
            0x7fff_2dff_7fff_ffffUL,
            0x04d0_ec02_a9de_d201UL,
            0x94ce_bea4_199c_ec04UL,
            0x0000_0000_39f6_d3a9UL,
        ];
        Scalar w = Pow(value, exp);
        int v = TwoAdicity;
        Scalar x = Mul(value, w);
        Scalar b = Mul(x, w);
        Scalar z = RootOfUnity;

        for (;;)
        {
            if (Equal(b, One)) return x;

            int i = 0;
            Scalar b2i = b;
            do { b2i = Square(b2i); i++; }
            while (!Equal(b2i, One) && i < v);

            if (i >= v) return null;

            Scalar w2 = z;
            for (int j = 0; j < v - i - 1; j++)
                w2 = Square(w2);

            z = Square(w2);
            x = Mul(x, w2);
            b = Mul(b, z);
            v = i;
        }
    }
    
    /// <summary>
    /// Returns an all-ones mask (0xFFFFFFFFFFFFFFFF) if <paramref name="a"/> is zero, or 0 otherwise.
    /// Operates on raw limbs; does not perform any field conversion.
    /// </summary>
    private static ulong IsZeroMask(Scalar a)
    {
        ulong x = a.L0 | a.L1 | a.L2 | a.L3;
        return ((x | (0UL - x)) >> 63) ^ 1UL;
    }
}
