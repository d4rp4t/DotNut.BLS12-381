using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

public readonly partial struct G1Affine
{
    /// <summary>
    /// Serializes this point to the ZCash compressed format (48 bytes, big-endian x-coordinate).
    /// Byte 0 flag bits: bit 7 = C (compressed = 1), bit 6 = I (infinity), bit 5 = S (Y is lexicographically largest).
    /// The S flag encodes the sign of Y so that the point can be recovered from X alone.
    /// </summary>
    /// <returns>48-byte compressed representation.</returns>
    public byte[] ToCompressed()
    {
        var buf = new byte[48];
        Fp.ToBytesBigEndian(IsInfinity ? Fp.Zero : X, buf);
        buf[0] |= 0x80;
        if (IsInfinity)
            buf[0] |= 0x40;
        else if (Fp.LexicographicallyLargest(Y))
            buf[0] |= 0x20;
        return buf;
    }

    /// <summary>
    /// Serializes this point to the ZCash uncompressed format (96 bytes, big-endian x || y).
    /// Byte 0 flag bits: bit 7 = 0 (uncompressed), bit 6 = I (infinity), bit 5 = 0.
    /// </summary>
    /// <returns>96-byte uncompressed representation.</returns>
    public byte[] ToUncompressed()
    {
        var buf = new byte[96];
        Fp.ToBytesBigEndian(IsInfinity ? Fp.Zero : X, buf.AsSpan(0, 48));
        Fp.ToBytesBigEndian(IsInfinity ? Fp.Zero : Y, buf.AsSpan(48, 48));
        if (IsInfinity)
            buf[0] |= 0x40;
        return buf;
    }

    /// <summary>
    /// Attempts to deserialize a G1 point from the ZCash compressed format (48 bytes).
    /// Validates the compression flag, reconstructs Y from the curve equation y² = x³ + 4,
    /// and verifies that the point is in the G1 prime-order subgroup.
    /// </summary>
    /// <param name="bytes">48-byte compressed encoding.</param>
    /// <param name="point">The decoded point on success; <see cref="Infinity"/> on failure.</param>
    /// <returns><see langword="true"/> if decoding and validation succeeded.</returns>
    public static bool TryFromCompressed(ReadOnlySpan<byte> bytes, out G1Affine point)
    {
        point = Infinity;
        if (bytes.Length != 48) return false;

        var compressionFlag = (bytes[0] >> 7) & 1;
        var infinityFlag   = (bytes[0] >> 6) & 1;
        var sortFlag       = (bytes[0] >> 5) & 1;

        if (compressionFlag == 0) return false;

        Span<byte> xBytes = stackalloc byte[48];
        bytes.CopyTo(xBytes);
        xBytes[0] &= 0x1F;

        if (infinityFlag != 0)
        {
            if (sortFlag != 0) return false;
            foreach (var b in xBytes) if (b != 0) return false;
            point = Infinity;
            return true;
        }

        if (!Fp.TryFromBytesBigEndian(xBytes, out var x)) return false;

        // y^2 = x^3 + 4 (G1 curve equation)
        var curveB = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
        var rhs = Fp.Add(Fp.Multiply(Fp.Square(x), x), curveB);
        if (!Fp.TrySqrt(rhs, out var y)) return false;

        if (Fp.LexicographicallyLargest(y) != (sortFlag != 0))
            y = Fp.Negate(y);

        var candidate = new G1Affine(x, y);
        if (!candidate.IsInSubgroup()) return false;

        point = candidate;
        return true;
    }

    /// <summary>
    /// Attempts to deserialize a G1 point from the ZCash uncompressed format (96 bytes).
    /// Validates flags, parses X and Y directly, and checks curve membership and subgroup membership.
    /// </summary>
    /// <param name="bytes">96-byte uncompressed encoding.</param>
    /// <param name="point">The decoded point on success; <see cref="Infinity"/> on failure.</param>
    /// <returns><see langword="true"/> if decoding and validation succeeded.</returns>
    public static bool TryFromUncompressed(ReadOnlySpan<byte> bytes, out G1Affine point)
    {
        point = Infinity;
        if (bytes.Length != 96) return false;

        var compressionFlag = (bytes[0] >> 7) & 1;
        var infinityFlag   = (bytes[0] >> 6) & 1;
        var sortFlag       = (bytes[0] >> 5) & 1;

        if (compressionFlag != 0) return false;
        if (sortFlag != 0) return false;

        Span<byte> xBytes = stackalloc byte[48];
        bytes[..48].CopyTo(xBytes);
        xBytes[0] &= 0x1F;

        if (infinityFlag != 0)
        {
            foreach (var b in xBytes) if (b != 0) return false;
            foreach (var b in bytes[48..]) if (b != 0) return false;
            point = Infinity;
            return true;
        }

        if (!Fp.TryFromBytesBigEndian(xBytes, out var x)) return false;
        if (!Fp.TryFromBytesBigEndian(bytes[48..], out var y)) return false;

        var candidate = new G1Affine(x, y);
        if (!candidate.IsOnCurve()) return false;
        if (!candidate.IsInSubgroup()) return false;

        point = candidate;
        return true;
    }
}
