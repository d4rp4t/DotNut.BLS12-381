using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    /// <summary>Creates an <see cref="Fp"/> from its canonical big-endian byte representation.</summary>
    /// <param name="bytes">A 48-byte canonical big-endian encoding of the field element.</param>
    /// <returns>The corresponding <see cref="Fp"/> in Montgomery form.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="bytes"/> is not exactly 48 bytes long.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the encoded value is not canonical (i.e. greater than or equal to the field modulus).
    /// </exception>
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

        Fp canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Fp encoding is not canonical (must be < p).");
        }

        return FromCanonical(canonical);
    }

    /// <summary> Attempts to create an <see cref="Fp"/> from its canonical big-endian byte representation. </summary>
    /// <param name="bytes"> A 48-byte canonical big-endian encoding of the field element. </param>
    /// <param name="value"> When this method returns <see langword="true"/>, contains the parsed <see cref="Fp"/> in Montgomery form; otherwise contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryFromBytesBigEndian(ReadOnlySpan<byte> bytes, out Fp value)
    {
        value = Zero;
        if (bytes.Length != 48)
        {
            return false;
        }

        ulong l5 = BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]);
        ulong l4 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8));
        ulong l1 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(32, 8));
        ulong l0 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(40, 8));

        Fp canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
        {
            return false;
        }

        value = FromCanonical(canonical);
        return true;
    }

    /// <summary>
    /// Writes the canonical big-endian byte representation of an <see cref="Fp"/>
    /// into the specified destination span.
    /// </summary>
    /// <param name="value">
    /// The <see cref="Fp"/> value in Montgomery form.
    /// </param>
    /// <param name="destination">
    /// The destination span to which the 48-byte encoding will be written.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is smaller than 48 bytes.
    /// </exception>
    public static void ToBytesBigEndian(Fp value, Span<byte> destination)
    {
        if (destination.Length < 48)
        {
            throw new ArgumentException("Destination must be at least 48 bytes.");
        }
        Fp canonical = ToCanonical(value);
        BinaryPrimitives.WriteUInt64BigEndian(destination, canonical.L5);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..], canonical.L4);
        BinaryPrimitives.WriteUInt64BigEndian(destination[16..], canonical.L3);
        BinaryPrimitives.WriteUInt64BigEndian(destination[24..], canonical.L2);
        BinaryPrimitives.WriteUInt64BigEndian(destination[32..], canonical.L1);
        BinaryPrimitives.WriteUInt64BigEndian(destination[40..], canonical.L0);
    }

    /// <summary>Creates an <see cref="Fp"/> from its canonical little-endian byte representation.</summary>
    /// <param name="bytes">A 48-byte canonical little-endian encoding of the field element.</param>
    /// <returns>The corresponding <see cref="Fp"/> in Montgomery form.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="bytes"/> is not exactly 48 bytes long.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the encoded value is not canonical (i.e. greater than or equal to the field modulus).
    /// </exception>
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

        Fp canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Fp encoding is not canonical (must be < p).");
        }

        return FromCanonical(canonical);
    }

    /// <summary> Attempts to create an <see cref="Fp"/> from its canonical little-endian byte representation. </summary>
    /// <param name="bytes"> A 48-byte canonical little-endian encoding of the field element. </param>
    /// <param name="value"> When this method returns <see langword="true"/>, contains the parsed <see cref="Fp"/> in Montgomery form; otherwise contains <see cref="Zero"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeded; otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryFromBytesLittleEndian(ReadOnlySpan<byte> bytes, [MaybeNullWhen(false)] out Fp value)
    {
        value = Zero;
        if (bytes.Length != 48)
        {
            return false;
        }

        ulong l0 = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        ulong l1 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8, 8));
        ulong l2 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8));
        ulong l3 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8));
        ulong l4 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(32, 8));
        ulong l5 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(40, 8));

        Fp canonical = new Fp(l0, l1, l2, l3, l4, l5);
        if (GreaterThanOrEqualCanonical(canonical, Modulus))
        {
            return false;
        }

        value = FromCanonical(canonical);
        return true;
    }

    /// <summary>
    /// Writes the canonical little-endian byte representation of an <see cref="Fp"/>
    /// into the specified destination span.
    /// </summary>
    /// <param name="value">
    /// The <see cref="Fp"/> value in Montgomery form.
    /// </param>
    /// <param name="destination">
    /// The destination span to which the 48-byte encoding will be written.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="destination"/> is smaller than 48 bytes.
    /// </exception>
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
    

    /// <summary>
    /// Converts an <see cref="Fp"/> value to its canonical hexadecimal string representation.
    /// </summary>
    /// <param name="fp">
    /// The <see cref="Fp"/> value in Montgomery form.
    /// </param>
    /// <returns>
    /// A hexadecimal string in canonical big-endian form, prefixed with <c>0x</c>.
    /// </returns>
    public static string ToHexString(Fp fp)
    {
        var tmp = ToCanonical(fp);
        return $"0x{tmp.L5:x16}" +
               $"{tmp.L4:x16}" +
               $"{tmp.L3:x16}" +
               $"{tmp.L2:x16}" +
               $"{tmp.L1:x16}" +
               $"{tmp.L0:x16}";
    }

    /// <summary>
    /// Determines whether one canonical <see cref="Fp"/> value is greater than
    /// or equal to another.
    /// </summary>
    private static bool GreaterThanOrEqualCanonical(Fp a, Fp b)
    {
        SubtractUnchecked(a, b, out ulong borrow);
        return borrow == 0;
    }
}
