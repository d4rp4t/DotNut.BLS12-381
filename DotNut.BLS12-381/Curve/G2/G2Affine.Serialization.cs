using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

public readonly partial struct G2Affine
{
    // ZCash compressed format: 96 bytes.
    // Layout: x.C1 (48 bytes) || x.C0 (48 bytes), big-endian.
    // Byte 0: bit7=C (compressed=1), bit6=I (infinity), bit5=S (lex-largest y)
    public byte[] ToCompressed()
    {
        var buf = new byte[96];
        var x = IsInfinity ? Fp2.Zero : X;
        Fp.ToBytesBigEndian(x.C1, buf.AsSpan(0, 48));
        Fp.ToBytesBigEndian(x.C0, buf.AsSpan(48, 48));
        buf[0] |= 0x80;
        if (IsInfinity)
            buf[0] |= 0x40;
        else if (Fp2.LexicographicallyLargest(Y))
            buf[0] |= 0x20;
        return buf;
    }

    // ZCash uncompressed format: 192 bytes.
    // Layout: x.C1 || x.C0 || y.C1 || y.C0, big-endian.
    // Byte 0: bit7=0, bit6=I (infinity), bit5=0
    public byte[] ToUncompressed()
    {
        var buf = new byte[192];
        var x = IsInfinity ? Fp2.Zero : X;
        var y = IsInfinity ? Fp2.Zero : Y;
        Fp.ToBytesBigEndian(x.C1, buf.AsSpan(0, 48));
        Fp.ToBytesBigEndian(x.C0, buf.AsSpan(48, 48));
        Fp.ToBytesBigEndian(y.C1, buf.AsSpan(96, 48));
        Fp.ToBytesBigEndian(y.C0, buf.AsSpan(144, 48));
        if (IsInfinity)
            buf[0] |= 0x40;
        return buf;
    }

    public static bool TryFromCompressed(ReadOnlySpan<byte> bytes, out G2Affine point)
    {
        point = Infinity;
        if (bytes.Length != 96) return false;

        var compressionFlag = (bytes[0] >> 7) & 1;
        var infinityFlag   = (bytes[0] >> 6) & 1;
        var sortFlag       = (bytes[0] >> 5) & 1;

        if (compressionFlag == 0) return false;

        // Copy x bytes with top 3 flag bits cleared; layout: c1 first, then c0
        Span<byte> c1Bytes = stackalloc byte[48];
        Span<byte> c0Bytes = stackalloc byte[48];
        bytes[..48].CopyTo(c1Bytes);
        bytes[48..].CopyTo(c0Bytes);
        c1Bytes[0] &= 0x1F;

        if (infinityFlag != 0)
        {
            if (sortFlag != 0) return false;
            foreach (var b in c1Bytes) if (b != 0) return false;
            foreach (var b in c0Bytes) if (b != 0) return false;
            point = Infinity;
            return true;
        }

        if (!Fp.TryFromBytesBigEndian(c1Bytes, out var xC1)) return false;
        if (!Fp.TryFromBytesBigEndian(c0Bytes, out var xC0)) return false;
        var x = new Fp2(xC0, xC1);

        // y^2 = x^3 + 4*(u+1) (G2 curve equation)
        var four = Fp.Add(Fp.Add(Fp.One, Fp.One), Fp.Add(Fp.One, Fp.One));
        var curveB = new Fp2(four, four);
        var rhs = Fp2.Add(Fp2.Multiply(Fp2.Square(x), x), curveB);
        if (!Fp2.TrySqrt(rhs, out var y)) return false;

        if (Fp2.LexicographicallyLargest(y) != (sortFlag != 0))
            y = Fp2.Negate(y);

        var candidate = new G2Affine(x, y);
        if (!candidate.IsInSubgroup()) return false;

        point = candidate;
        return true;
    }

    public static bool TryFromUncompressed(ReadOnlySpan<byte> bytes, out G2Affine point)
    {
        point = Infinity;
        if (bytes.Length != 192) return false;

        var compressionFlag = (bytes[0] >> 7) & 1;
        var infinityFlag   = (bytes[0] >> 6) & 1;
        var sortFlag       = (bytes[0] >> 5) & 1;

        if (compressionFlag != 0) return false;
        if (sortFlag != 0) return false;

        Span<byte> xC1Bytes = stackalloc byte[48];
        Span<byte> xC0Bytes = stackalloc byte[48];
        bytes[..48].CopyTo(xC1Bytes);
        bytes[48..96].CopyTo(xC0Bytes);
        xC1Bytes[0] &= 0x1F;

        if (infinityFlag != 0)
        {
            foreach (var b in xC1Bytes) if (b != 0) return false;
            foreach (var b in xC0Bytes) if (b != 0) return false;
            foreach (var b in bytes[96..]) if (b != 0) return false;
            point = Infinity;
            return true;
        }

        if (!Fp.TryFromBytesBigEndian(xC1Bytes, out var xC1)) return false;
        if (!Fp.TryFromBytesBigEndian(xC0Bytes, out var xC0)) return false;
        if (!Fp.TryFromBytesBigEndian(bytes[96..144], out var yC1)) return false;
        if (!Fp.TryFromBytesBigEndian(bytes[144..192], out var yC0)) return false;

        var x = new Fp2(xC0, xC1);
        var y = new Fp2(yC0, yC1);

        var candidate = new G2Affine(x, y);
        if (!candidate.IsOnCurve()) return false;
        if (!candidate.IsInSubgroup()) return false;

        point = candidate;
        return true;
    }
}
