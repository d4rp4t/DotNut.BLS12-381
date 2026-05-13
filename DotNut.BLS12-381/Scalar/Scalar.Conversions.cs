using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>
    /// Decodes a scalar from 32 bytes in little-endian order and converts to Montgomery form.
    /// The input must represent a canonical integer in [0, r); values ≥ r are rejected.
    /// </summary>
    /// <param name="bytes">Exactly 32 bytes in little-endian order.</param>
    /// <returns>The scalar in Montgomery form.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is not 32 bytes.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the encoded value is ≥ r.</exception>
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

    /// <summary>
    /// Attempts to decode a scalar from 32 bytes in little-endian order and convert to Montgomery form.
    /// Returns <see langword="false"/> if the input is not 32 bytes or if the value is ≥ r.
    /// </summary>
    /// <param name="bytes">Candidate 32-byte encoding (little-endian).</param>
    /// <param name="value">Set to the decoded scalar in Montgomery form on success, or <see cref="Zero"/> on failure.</param>
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

    /// <summary>
    /// Encodes the scalar as its canonical integer value in 32 bytes, little-endian order.
    /// Converts from Montgomery form first via <see cref="ToCanonical"/>.
    /// </summary>
    /// <param name="value">The scalar to encode (in Montgomery form).</param>
    /// <param name="dest">Destination span; must be at least 32 bytes.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dest"/> is shorter than 32 bytes.</exception>
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

    /// <summary>
    /// Decodes a scalar from 32 bytes in big-endian order and converts to Montgomery form.
    /// The input must represent a canonical integer in [0, r); values ≥ r are rejected.
    /// </summary>
    /// <param name="bytes">Exactly 32 bytes in big-endian order.</param>
    /// <returns>The scalar in Montgomery form.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is not 32 bytes.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the encoded value is ≥ r.</exception>
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

    /// <summary>
    /// Attempts to decode a scalar from 32 bytes in big-endian order and convert to Montgomery form.
    /// Returns <see langword="false"/> if the input is not 32 bytes or if the value is ≥ r.
    /// </summary>
    /// <param name="bytes">Candidate 32-byte encoding (big-endian).</param>
    /// <param name="value">Set to the decoded scalar in Montgomery form on success, or <see cref="Zero"/> on failure.</param>
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

    /// <summary>
    /// Encodes the scalar as its canonical integer value in 32 bytes, big-endian order.
    /// Converts from Montgomery form first via <see cref="ToCanonical"/>.
    /// </summary>
    /// <param name="value">The scalar to encode (in Montgomery form).</param>
    /// <param name="dest">Destination span; must be at least 32 bytes.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dest"/> is shorter than 32 bytes.</exception>
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

    /// <summary>
    /// Reduces a 64-byte (512-bit) little-endian value modulo r. Used for hash-to-scalar.
    /// Splits the input into lo (bytes 0–31) and hi (bytes 32–63) and computes
    /// FromCanonical(lo) + FromCanonical(hi) · R² mod r.
    /// </summary>
    /// <param name="bytes">Exactly 64 bytes in little-endian order.</param>
    /// <returns>The reduced scalar in Montgomery form.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="bytes"/> is not 64 bytes.</exception>
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

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="a"/> (treated as a canonical integer, not Montgomery form)
    /// is greater than or equal to the group order r.
    /// Used as a validity check in the byte-decoding methods.
    /// </summary>
    private static bool IsGeR(Scalar a)
    {
        SubUnchecked(a, GroupOrderR, out ulong borrow);
        return borrow == 0;
    }
}
