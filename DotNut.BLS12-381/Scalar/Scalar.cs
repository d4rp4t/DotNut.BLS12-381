using System.Numerics;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
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

    // not CT-safe, use ONLY for non-secret data
    public static Scalar FromBigInteger(BigInteger k)
    {
        var bytes = new byte[32];
        k.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: false);
        return FromCanonical(new Scalar(
            ReadUInt64LE(bytes, 0),
            ReadUInt64LE(bytes, 8),
            ReadUInt64LE(bytes, 16),
            ReadUInt64LE(bytes, 24)
        ));
    }

    public static implicit operator BigInteger(Scalar scalar)
    {
        var c = ToCanonical(scalar);
        var bytes = new byte[33]; // extra zero byte = positive sign
        WriteUInt64LE(bytes, 0,  c.L0);
        WriteUInt64LE(bytes, 8,  c.L1);
        WriteUInt64LE(bytes, 16, c.L2);
        WriteUInt64LE(bytes, 24, c.L3);
        return new BigInteger(bytes);
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
