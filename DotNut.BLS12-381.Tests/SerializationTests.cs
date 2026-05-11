using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public sealed class SerializationTests
{
    #region G1 Round-trip

    [Fact]
    public void G1_Compressed_Roundtrip_Generator()
    {
        var g = G1Affine.Generator;
        var bytes = g.ToCompressed();
        Assert.Equal(48, bytes.Length);
        Assert.True(G1Affine.TryFromCompressed(bytes, out var recovered));
        Assert.True(Fp.Equal(g.X, recovered.X));
        Assert.True(Fp.Equal(g.Y, recovered.Y));
    }

    [Fact]
    public void G1_Uncompressed_Roundtrip_Generator()
    {
        var g = G1Affine.Generator;
        var bytes = g.ToUncompressed();
        Assert.Equal(96, bytes.Length);
        Assert.True(G1Affine.TryFromUncompressed(bytes, out var recovered));
        Assert.True(Fp.Equal(g.X, recovered.X));
        Assert.True(Fp.Equal(g.Y, recovered.Y));
    }

    [Fact]
    public void G1_Compressed_Roundtrip_Infinity()
    {
        var inf = G1Affine.Infinity;
        var bytes = inf.ToCompressed();
        Assert.Equal(48, bytes.Length);
        Assert.Equal(0xC0, bytes[0]); // C=1, I=1, S=0
        Assert.True(G1Affine.TryFromCompressed(bytes, out var recovered));
        Assert.True(recovered.IsInfinity);
    }

    [Fact]
    public void G1_Uncompressed_Roundtrip_Infinity()
    {
        var inf = G1Affine.Infinity;
        var bytes = inf.ToUncompressed();
        Assert.Equal(96, bytes.Length);
        Assert.Equal(0x40, bytes[0]); // C=0, I=1, S=0
        Assert.True(G1Affine.TryFromUncompressed(bytes, out var recovered));
        Assert.True(recovered.IsInfinity);
    }

    [Fact]
    public void G1_Compressed_FlagsByte_Generator()
    {
        var bytes = G1Affine.Generator.ToCompressed();
        // C flag must be set; I flag must be clear
        Assert.True((bytes[0] & 0x80) != 0);
        Assert.True((bytes[0] & 0x40) == 0);
    }

    [Fact]
    public void G1_Compressed_Roundtrip_ArbitraryPoint()
    {
        var g = G1Affine.Generator.ToProjective();
        var p = G1Projective.Add(G1Projective.Double(g), g).ToAffine(); // 3G
        var bytes = p.ToCompressed();
        Assert.True(G1Affine.TryFromCompressed(bytes, out var recovered));
        Assert.True(Fp.Equal(p.X, recovered.X));
        Assert.True(Fp.Equal(p.Y, recovered.Y));
    }

    [Fact]
    public void G1_TryFromCompressed_RejectsUncompressedFlag()
    {
        var bytes = new byte[48]; // all zeros, no C flag
        Assert.False(G1Affine.TryFromCompressed(bytes, out _));
    }

    [Fact]
    public void G1_TryFromUncompressed_RejectsCompressedFlag()
    {
        var bytes = new byte[96];
        bytes[0] = 0x80; // C flag set
        Assert.False(G1Affine.TryFromUncompressed(bytes, out _));
    }

    [Fact]
    public void G1_TryFromCompressed_RejectsWrongLength()
    {
        Assert.False(G1Affine.TryFromCompressed(new byte[47], out _));
        Assert.False(G1Affine.TryFromCompressed(new byte[49], out _));
    }

    [Fact]
    public void G1_TryFromUncompressed_RejectsWrongLength()
    {
        Assert.False(G1Affine.TryFromUncompressed(new byte[95], out _));
        Assert.False(G1Affine.TryFromUncompressed(new byte[97], out _));
    }

    #endregion

    #region G2 Round-trip

    [Fact]
    public void G2_Compressed_Roundtrip_Generator()
    {
        var g = G2Affine.Generator;
        var bytes = g.ToCompressed();
        Assert.Equal(96, bytes.Length);
        Assert.True(G2Affine.TryFromCompressed(bytes, out var recovered));
        Assert.True(Fp2.Equal(g.X, recovered.X));
        Assert.True(Fp2.Equal(g.Y, recovered.Y));
    }

    [Fact]
    public void G2_Uncompressed_Roundtrip_Generator()
    {
        var g = G2Affine.Generator;
        var bytes = g.ToUncompressed();
        Assert.Equal(192, bytes.Length);
        Assert.True(G2Affine.TryFromUncompressed(bytes, out var recovered));
        Assert.True(Fp2.Equal(g.X, recovered.X));
        Assert.True(Fp2.Equal(g.Y, recovered.Y));
    }

    [Fact]
    public void G2_Compressed_Roundtrip_Infinity()
    {
        var inf = G2Affine.Infinity;
        var bytes = inf.ToCompressed();
        Assert.Equal(96, bytes.Length);
        Assert.Equal(0xC0, bytes[0]); // C=1, I=1, S=0
        Assert.True(G2Affine.TryFromCompressed(bytes, out var recovered));
        Assert.True(recovered.IsInfinity);
    }

    [Fact]
    public void G2_Uncompressed_Roundtrip_Infinity()
    {
        var inf = G2Affine.Infinity;
        var bytes = inf.ToUncompressed();
        Assert.Equal(192, bytes.Length);
        Assert.Equal(0x40, bytes[0]);
        Assert.True(G2Affine.TryFromUncompressed(bytes, out var recovered));
        Assert.True(recovered.IsInfinity);
    }

    [Fact]
    public void G2_Compressed_FlagsByte_Generator()
    {
        var bytes = G2Affine.Generator.ToCompressed();
        Assert.True((bytes[0] & 0x80) != 0);
        Assert.True((bytes[0] & 0x40) == 0);
    }

    [Fact]
    public void G2_TryFromCompressed_RejectsUncompressedFlag()
    {
        var bytes = new byte[96];
        Assert.False(G2Affine.TryFromCompressed(bytes, out _));
    }

    [Fact]
    public void G2_TryFromUncompressed_RejectsCompressedFlag()
    {
        var bytes = new byte[192];
        bytes[0] = 0x80;
        Assert.False(G2Affine.TryFromUncompressed(bytes, out _));
    }

    [Fact]
    public void G2_TryFromCompressed_RejectsWrongLength()
    {
        Assert.False(G2Affine.TryFromCompressed(new byte[95], out _));
        Assert.False(G2Affine.TryFromCompressed(new byte[97], out _));
    }

    [Fact]
    public void G2_TryFromUncompressed_RejectsWrongLength()
    {
        Assert.False(G2Affine.TryFromUncompressed(new byte[191], out _));
        Assert.False(G2Affine.TryFromUncompressed(new byte[193], out _));
    }

    #endregion

    #region Fp TrySqrt

    [Fact]
    public void Fp_TrySqrt_PerfectSquare_ReturnsTrue()
    {
        var x = Fp.Add(Fp.One, Fp.Add(Fp.One, Fp.One)); // 3
        var x2 = Fp.Square(x);
        Assert.True(Fp.TrySqrt(x2, out var s));
        Assert.True(Fp.Equal(Fp.Square(s), x2));
    }

    [Fact]
    public void Fp_TrySqrt_NonResidue_ReturnsFalse()
    {
        // -1 is not a QR in Fp (p ≡ 3 mod 4 means -1 is a non-residue)
        var minusOne = Fp.Negate(Fp.One);
        Assert.False(Fp.TrySqrt(minusOne, out _));
    }

    [Fact]
    public void Fp_LexicographicallyLargest_NegationInverse()
    {
        // exactly one of (v, -v) should be lex-largest (unless v == 0)
        var v = Fp.Add(Fp.One, Fp.One);
        var neg = Fp.Negate(v);
        Assert.True(Fp.LexicographicallyLargest(v) != Fp.LexicographicallyLargest(neg));
    }

    #endregion
}
