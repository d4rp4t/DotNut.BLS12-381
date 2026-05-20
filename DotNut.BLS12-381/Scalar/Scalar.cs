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
    internal readonly ulong L0, L1, L2, L3;
    
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
    internal Scalar(ulong l0, ulong l1, ulong l2, ulong l3)
    {
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
    }

    internal Scalar(ulong[] l5)
    {
        L0 = l5[0];
        L1 = l5[1];
        L2 = l5[2];
        L3 = l5[3];
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
