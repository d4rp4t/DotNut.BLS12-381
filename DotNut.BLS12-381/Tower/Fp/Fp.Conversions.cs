using System.Buffers.Binary;

namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static Fp FromBytesBigEndian(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 48)
        {
            throw new ArgumentException("Fp encoding must be exactly 48 bytes.", nameof(bytes));
        }

        ulong l5 = BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]);
        ulong l4 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8));
        ulong l1 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(32, 8));
        ulong l0 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(40, 8));

        var canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
            throw new ArgumentOutOfRangeException(nameof(bytes), "Fp encoding is not canonical (must be < p).");

        return FromCanonical(canonical);
    }

    public static bool TryFromBytesBigEndian(ReadOnlySpan<byte> bytes, out Fp value)
    {
        value = Zero;
        if (bytes.Length != 48)
            return false;

        ulong l5 = BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]);
        ulong l4 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8));
        ulong l1 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(32, 8));
        ulong l0 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(40, 8));

        var canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
            return false;

        value = FromCanonical(canonical);
        return true;
    }

    public static void ToBytesBigEndian(Fp value, Span<byte> destination)
    {
        if (destination.Length < 48)
        {
            throw new ArgumentException("Destination must be at least 48 bytes.");
        }
        var canonical = ToCanonical(value);
        BinaryPrimitives.WriteUInt64BigEndian(destination, canonical.L5);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..], canonical.L4);
        BinaryPrimitives.WriteUInt64BigEndian(destination[16..], canonical.L3);
        BinaryPrimitives.WriteUInt64BigEndian(destination[24..], canonical.L2);
        BinaryPrimitives.WriteUInt64BigEndian(destination[32..], canonical.L1);
        BinaryPrimitives.WriteUInt64BigEndian(destination[40..], canonical.L0);
    }

    public static Fp FromBytesLittleEndian(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 48)
        {
            throw new ArgumentException("Fp encoding must be exactly 48 bytes.", nameof(bytes));
        }

        ulong l0 = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        ulong l1 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8));
        ulong l4 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(32, 8));
        ulong l5 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(40, 8));

        var canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Fp encoding is not canonical (must be < p).");
        }

        return FromCanonical(canonical);
    }

    public static bool TryFromBytesLittleEndian(ReadOnlySpan<byte> bytes, out Fp value)
    {
        value = Zero;
        if (bytes.Length != 48)
            return false;

        ulong l0 = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        ulong l1 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8));
        ulong l4 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(32, 8));
        ulong l5 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(40, 8));

        var canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
            return false;

        value = FromCanonical(canonical);
        return true;
    }

    public static void ToBytesLittleEndian(Fp value, Span<byte> destination)
    {
        if (destination.Length < 48)
        {
            throw new ArgumentException("Destination must be at least 48 bytes.");
        }
        
        var canonical = ToCanonical(value);
        BinaryPrimitives.WriteUInt64LittleEndian(destination,       canonical.L0);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..],  canonical.L1);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], canonical.L2);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], canonical.L3);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[32..], canonical.L4);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[40..], canonical.L5);
    }

    private static bool GreaterThanOrEqualCanonical(Fp a, Fp b)
    {
        SubtractUnchecked(a, b, out ulong borrow);
        return borrow == 0;
    }
}
