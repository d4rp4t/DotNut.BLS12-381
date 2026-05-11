using System.Numerics;

namespace DotNut.BLS12_381;

public readonly struct Scalar
{
    public readonly ulong L0, L1, L2, L3;

    // r = 0x73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001
    internal static readonly Scalar GroupOrderR = new(
        0xffffffff00000001UL,
        0x53bda402fffe5bfeUL,
        0x3339d80809a1d805UL,
        0x73eda753299d7d48UL
    );

    // -r^{-1} mod 2^64
    private const ulong MontgomeryInv = 0xfffffffeffffffffUL;

    // R^2 mod r, where R = 2^256
    private static readonly Scalar R2 = new(
        0xc999e990f3f29c6dUL,
        0x2b6cedcb87925c23UL,
        0x05d314967254398fUL,
        0x0748d9d99f59ff11UL
    );

    public Scalar(ulong l0, ulong l1, ulong l2, ulong l3)
    {
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
    }

    public ulong GetBit(int i)
    {
        ulong limb = (i >> 6) switch { 0 => L0, 1 => L1, 2 => L2, _ => L3 };
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

    // Mul(a, b) for canonical a, b:
    // convert b to Montgomery form (b*R mod r), then MontReduce(a * b_m) = a*b mod r
    public static Scalar Mul(Scalar a, Scalar b)
    {
        var bm = MontgomeryReduce(MultiplyWide(b, R2));
        return MontgomeryReduce(MultiplyWide(a, bm));
    }

    // not CT-safe, use ONLY for non-secret data (tests or basically anything before entering CT code).
    public static Scalar FromBigInteger(BigInteger k)
    {
        var bytes = new byte[32];
        k.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: false);
        return new Scalar(
            ReadUInt64LE(bytes, 0),
            ReadUInt64LE(bytes, 8),
            ReadUInt64LE(bytes, 16),
            ReadUInt64LE(bytes, 24)
        );
    }

    public static implicit operator BigInteger(Scalar scalar)
    {
        var bytes = new byte[33]; // extra zero byte = positive sign
        WriteUInt64LE(bytes, 0, scalar.L0);
        WriteUInt64LE(bytes, 8, scalar.L1);
        WriteUInt64LE(bytes, 16, scalar.L2);
        WriteUInt64LE(bytes, 24, scalar.L3);
        return new BigInteger(bytes);
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

    private static ulong ReadUInt64LE(byte[] buf, int offset) =>
        (ulong)buf[offset]             |
        ((ulong)buf[offset + 1] << 8)  |
        ((ulong)buf[offset + 2] << 16) |
        ((ulong)buf[offset + 3] << 24) |
        ((ulong)buf[offset + 4] << 32) |
        ((ulong)buf[offset + 5] << 40) |
        ((ulong)buf[offset + 6] << 48) |
        ((ulong)buf[offset + 7] << 56);

    private static void WriteUInt64LE(byte[] buf, int offset, ulong val)
    {
        buf[offset + 0] = (byte)val;
        buf[offset + 1] = (byte)(val >> 8);
        buf[offset + 2] = (byte)(val >> 16);
        buf[offset + 3] = (byte)(val >> 24);
        buf[offset + 4] = (byte)(val >> 32);
        buf[offset + 5] = (byte)(val >> 40);
        buf[offset + 6] = (byte)(val >> 48);
        buf[offset + 7] = (byte)(val >> 56);
    }
}
