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

    public static byte[] ToBytesBigEndian(Fp value)
    {
        var canonical = ToCanonical(value);
        var bytes = new byte[48];

        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(0, 8), canonical.L5);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(8, 8), canonical.L4);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(16, 8), canonical.L3);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(24, 8), canonical.L2);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(32, 8), canonical.L1);
        BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(40, 8), canonical.L0);

        return bytes;
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

    public static byte[] ToBytesLittleEndian(Fp value)
    {
        var canonical = ToCanonical(value);
        var bytes = new byte[48];

        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(0, 8), canonical.L0);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(8, 8), canonical.L1);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(16, 8), canonical.L2);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(24, 8), canonical.L3);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(32, 8), canonical.L4);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40, 8), canonical.L5);

        return bytes;
    }

    private static bool GreaterThanOrEqualCanonical(Fp a, Fp b)
    {
        if (a.L5 != b.L5) return a.L5 > b.L5;
        if (a.L4 != b.L4) return a.L4 > b.L4;
        if (a.L3 != b.L3) return a.L3 > b.L3;
        if (a.L2 != b.L2) return a.L2 > b.L2;
        if (a.L1 != b.L1) return a.L1 > b.L1;
        if (a.L0 != b.L0) return a.L0 > b.L0;
        return true;
    }
}
