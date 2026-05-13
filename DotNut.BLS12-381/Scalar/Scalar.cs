using System.Numerics;

namespace DotNut.BLS12_381;

/// <summary>
/// An element of the BLS12-381 scalar field Fr = Z/rZ where r is the prime group order.
/// r = 0x73eda753299d7d483339d80809a1d80553bda402fffe5bfeffffffff00000001.
/// Internally stored in Montgomery form: a raw Scalar value represents a·R mod r, R = 2^256.
/// Arithmetic operations preserve Montgomery form; use <see cref="FromCanonical"/> / <see cref="ToCanonical"/>
/// to convert between canonical integer representation and Montgomery form.
/// </summary>
public readonly partial struct Scalar
{
    /// <summary>Limb 0 (least significant 64 bits) of the scalar in Montgomery form.</summary>
    public readonly ulong L0, L1, L2, L3;

    /// <summary>The prime group order r of BLS12-381.</summary>
    internal static readonly Scalar GroupOrderR = new(
        0xffffffff00000001UL,
        0x53bda402fffe5bfeUL,
        0x3339d80809a1d805UL,
        0x73eda753299d7d48UL
    );

    // -r^{-1} mod 2^64
    private const ulong MontgomeryInv = 0xfffffffeffffffffUL;

    /// <summary>R² mod r where R = 2^256. Used to convert integers into Montgomery form via MontgomeryReduce(a · R²) = a·R mod r.</summary>
    private static readonly Scalar R2 = new(
        0xc999e990f3f29c6dUL,
        0x2b6cedcb87925c23UL,
        0x05d314967254398fUL,
        0x0748d9d99f59ff11UL
    );

    /// <summary>
    /// Creates a scalar from its four little-endian 64-bit limbs.
    /// The caller is responsible for ensuring the value is already in Montgomery form
    /// (i.e. the raw limbs represent a·R mod r for some integer a).
    /// Does not perform any reduction.
    /// </summary>
    /// <param name="l0">Least-significant limb.</param>
    /// <param name="l1">Second limb.</param>
    /// <param name="l2">Third limb.</param>
    /// <param name="l3">Most-significant limb.</param>
    public Scalar(ulong l0, ulong l1, ulong l2, ulong l3)
    {
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
    }

    /// <summary>
    /// Converts a non-negative <see cref="BigInteger"/> to a scalar by reducing modulo r and converting to Montgomery form.
    /// Not constant-time — use only for non-secret data (e.g. test vectors, public parameters).
    /// </summary>
    /// <param name="k">The integer value; must be non-negative and will be reduced mod r.</param>
    /// <returns>The scalar k mod r in Montgomery form.</returns>
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

    /// <summary>
    /// Implicit conversion to <see cref="BigInteger"/>: converts the scalar to its canonical integer value in [0, r).
    /// Extracts the canonical representation via <see cref="ToCanonical"/>.
    /// Not constant-time.
    /// </summary>
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

    /// <summary>Reads a little-endian uint64 from <paramref name="buf"/> starting at <paramref name="offset"/>.</summary>
    private static ulong ReadUInt64LE(byte[] buf, int offset) =>
        (ulong)buf[offset]             |
        ((ulong)buf[offset + 1] << 8)  |
        ((ulong)buf[offset + 2] << 16) |
        ((ulong)buf[offset + 3] << 24) |
        ((ulong)buf[offset + 4] << 32) |
        ((ulong)buf[offset + 5] << 40) |
        ((ulong)buf[offset + 6] << 48) |
        ((ulong)buf[offset + 7] << 56);

    /// <summary>Writes a uint64 in little-endian order into <paramref name="buf"/> at <paramref name="offset"/>.</summary>
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
