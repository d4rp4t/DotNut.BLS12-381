using System.Security.Cryptography;

namespace DotNut.BLS12_381.HashToCurve;

/// <summary>
/// Processed domain separation tag (DST) as defined in RFC 9380 §5.3.3.
/// If the raw DST exceeds 255 bytes it is hashed down to a shorter value;
/// otherwise it is stored as-is. The processed tag is always ≤ 255 bytes.
/// </summary>
internal sealed class ExpandMessageDST
{
    public const int MaxDstLength = 255;
    private static readonly byte[] OverSizeSalt = "H2C-OVERSIZE-DST-"u8.ToArray();

    private readonly byte[] _data;

    /// <summary>Processed DST bytes.</summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>Length of the processed DST in bytes.</summary>
    public int Length => _data.Length;

    private ExpandMessageDST(byte[] data) => _data = data;

    /// <summary>
    /// Builds a DST for use with <see cref="ExpandMsgXmd"/>.
    /// DSTs longer than 255 bytes are hashed with <paramref name="hashName"/> and
    /// the oversize salt <c>"H2C-OVERSIZE-DST-"</c> prepended (RFC 9380 §5.3.3).
    /// </summary>
    public static ExpandMessageDST ForXmd(ReadOnlySpan<byte> dst, HashAlgorithmName hashName)
    {
        if (dst.Length <= MaxDstLength)
            return new ExpandMessageDST(dst.ToArray());

        using var hash = IncrementalHash.CreateHash(hashName);
        hash.AppendData(OverSizeSalt);
        hash.AppendData(dst);
        return new ExpandMessageDST(hash.GetHashAndReset());
    }

    /// <summary>
    /// Builds a DST for use with <see cref="ExpandMsgXof"/>.
    /// DSTs longer than 255 bytes are squeezed to <paramref name="outputLength"/> bytes
    /// using the specified XOF with the oversize salt prepended (RFC 9380 §5.3.3).
    /// <paramref name="outputLength"/> should be <c>ceil(2 * k / 8)</c> for security
    /// parameter k (e.g. 32 for k = 128).
    /// </summary>
    public static ExpandMessageDST ForXof(ReadOnlySpan<byte> dst, int outputLength, XofAlgorithm algorithm)
    {
        if (dst.Length <= MaxDstLength)
            return new ExpandMessageDST(dst.ToArray());

        byte[] buf = new byte[outputLength];
        if (algorithm == XofAlgorithm.Shake128)
        {
            using var xof = new Shake128();
            xof.AppendData(OverSizeSalt);
            xof.AppendData(dst);
            xof.GetHashAndReset(buf);
        }
        else
        {
            using var xof = new Shake256();
            xof.AppendData(OverSizeSalt);
            xof.AppendData(dst);
            xof.GetHashAndReset(buf);
        }
        return new ExpandMessageDST(buf);
    }
}
