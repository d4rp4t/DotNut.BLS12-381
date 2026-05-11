using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    public static Scalar FromBytesLittleEndian(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 32)
            throw new ArgumentException("Scalar encoding must be exactly 32 bytes.", nameof(bytes));

        var s = new Scalar(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8,  8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8))
        );
        if (IsGeR(s))
            throw new ArgumentOutOfRangeException(nameof(bytes), "Scalar encoding is not canonical (must be < r).");
        return FromCanonical(s);
    }

    public static bool TryFromBytesLittleEndian(ReadOnlySpan<byte> bytes, [MaybeNullWhen(false)] out Scalar value)
    {
        value = Zero;
        if (bytes.Length != 32) return false;
        var s = new Scalar(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8,  8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8))
        );
        if (IsGeR(s)) return false;
        value = FromCanonical(s);
        return true;
    }

    public static void ToBytesLittleEndian(Scalar value, Span<byte> dest)
    {
        if (dest.Length < 32)
            throw new ArgumentException("Destination must be at least 32 bytes.");
        var c = ToCanonical(value);
        BinaryPrimitives.WriteUInt64LittleEndian(dest,        c.L0);
        BinaryPrimitives.WriteUInt64LittleEndian(dest[8..],   c.L1);
        BinaryPrimitives.WriteUInt64LittleEndian(dest[16..],  c.L2);
        BinaryPrimitives.WriteUInt64LittleEndian(dest[24..],  c.L3);
    }

    public static Scalar FromBytesBigEndian(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 32)
            throw new ArgumentException("Scalar encoding must be exactly 32 bytes.", nameof(bytes));

        var s = new Scalar(
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8,  8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes[..8])
        );
        if (IsGeR(s))
            throw new ArgumentOutOfRangeException(nameof(bytes), "Scalar encoding is not canonical (must be < r).");
        return FromCanonical(s);
    }

    public static bool TryFromBytesBigEndian(ReadOnlySpan<byte> bytes, [MaybeNullWhen(false)] out Scalar value)
    {
        value = Zero;
        if (bytes.Length != 32) return false;
        var s = new Scalar(
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8,  8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes[..8])
        );
        if (IsGeR(s)) return false;
        value = FromCanonical(s);
        return true;
    }

    public static void ToBytesBigEndian(Scalar value, Span<byte> dest)
    {
        if (dest.Length < 32)
            throw new ArgumentException("Destination must be at least 32 bytes.");
        var c = ToCanonical(value);
        BinaryPrimitives.WriteUInt64BigEndian(dest,        c.L3);
        BinaryPrimitives.WriteUInt64BigEndian(dest[8..],   c.L2);
        BinaryPrimitives.WriteUInt64BigEndian(dest[16..],  c.L1);
        BinaryPrimitives.WriteUInt64BigEndian(dest[24..],  c.L0);
    }

    // Reduces a 64-byte (512-bit) value mod r. Used for hash-to-scalar.
    // (lo + hi·2^256) mod r - FromCanonical handles reduction implicitly via Montgomery
    public static Scalar FromBytesWide(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 64)
            throw new ArgumentException("Expected exactly 64 bytes.", nameof(bytes));

        var lo = new Scalar(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8,  8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8))
        );
        var hi = new Scalar(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(32, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(40, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(48, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(56, 8))
        );
        return Add(FromCanonical(lo), Mul(FromCanonical(hi), R2));
    }

    private static bool IsGeR(Scalar a)
    {
        SubUnchecked(a, GroupOrderR, out ulong borrow);
        return borrow == 0;
    }
}
