using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>Converts a <see cref="ulong"/> to its scalar field element, reducing modulo r.</summary>
    public static Scalar From(ulong n) => FromCanonical(new Scalar(n, 0UL, 0UL, 0UL));

    /// <summary>
    /// Reduces an arbitrary 512-bit integer (eight 64-bit little-endian limbs) modulo r
    /// and returns the result in Montgomery form.
    /// Equivalent to Rust's <c>Scalar::from_u512</c>: <c>lo·R + hi·R² mod r</c>.
    /// </summary>
    public static Scalar FromU512(ulong l0, ulong l1, ulong l2, ulong l3,
                                   ulong l4, ulong l5, ulong l6, ulong l7)
    {
        var lo = new Scalar(l0, l1, l2, l3);
        var hi = new Scalar(l4, l5, l6, l7);
        return Add(Mul(lo, R2), Mul(hi, R3));
    }

    /// <summary>
    /// Converts a non-negative <see cref="BigInteger"/> to a scalar by reducing modulo r and converting to Montgomery form.
    /// Not constant-time — use only for non-secret data (e.g. test vectors, public parameters).
    /// </summary>
    /// <param name="k">The integer value; must be non-negative and will be reduced mod r.</param>
    /// <returns>The scalar k mod r in Montgomery form.</returns>
    public static Scalar FromBigInteger(BigInteger k)
    {
        if (k.Sign < 0)
            throw new ArgumentOutOfRangeException(nameof(k), "Value must be non-negative.");

        // Reduce mod r before extracting bytes so that inputs >= 2^256 are handled
        // correctly. TryWriteBytes silently writes nothing when the buffer is too
        // small (k >= 2^256), which previously produced Scalar.Zero instead of
        // k mod r.
        Span<byte> rBuf = stackalloc byte[33]; // extra zero byte → positive unsigned
        BinaryPrimitives.WriteUInt64LittleEndian(rBuf,        GroupOrderR.L0);
        BinaryPrimitives.WriteUInt64LittleEndian(rBuf[8..],   GroupOrderR.L1);
        BinaryPrimitives.WriteUInt64LittleEndian(rBuf[16..],  GroupOrderR.L2);
        BinaryPrimitives.WriteUInt64LittleEndian(rBuf[24..],  GroupOrderR.L3);
        k %= new BigInteger(rBuf, isUnsigned: true, isBigEndian: false);

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

    public static string ToHexString(Scalar value)
    {
        var tmp = ToCanonical(value);
        return $"0x{tmp.L3:x16}" +
               $"{tmp.L2:x16}" +
               $"{tmp.L1:x16}" +
               $"{tmp.L0:x16}";
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
