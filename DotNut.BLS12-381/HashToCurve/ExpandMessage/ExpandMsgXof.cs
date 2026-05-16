namespace DotNut.BLS12_381.HashToCurve;

/// <summary>
/// RFC 9380 §5.3.2 <c>expand_message_xof</c> using SHAKE-128 or SHAKE-256.
/// Feeds <c>msg || I2OSP(len_in_bytes, 2) || DST_prime</c> to the XOF and eagerly captures
/// exactly <c>len_in_bytes</c> of output. <see cref="ReadInto"/> returns successive slices of
/// that buffer. (SHAKE has no streaming reader in .NET 8, so the full output is computed upfront.)
/// </summary>
internal sealed class ExpandMsgXof
{
    private readonly byte[] _output;
    private int _offs;

    /// <summary>Number of bytes that have not yet been consumed by <see cref="ReadInto"/>.</summary>
    public int Remain => _output.Length - _offs;

    private ExpandMsgXof(byte[] output) => _output = output;

    /// <summary>
    /// Initializes an expand_message_xof generator (RFC 9380 §5.3.2).
    /// Computes <c>msg_prime = msg || I2OSP(len_in_bytes, 2) || DST_prime</c> and squeezes
    /// <paramref name="lenInBytes"/> bytes from the XOF.
    /// </summary>
    /// <param name="message">Input message.</param>
    /// <param name="dst">Domain separation tag. If longer than 255 bytes it is reduced to
    /// <paramref name="oversizeDstLength"/> bytes via the same XOF with the oversize salt
    /// prefix (RFC 9380 §5.3.3).</param>
    /// <param name="lenInBytes">Number of pseudorandom bytes to produce. Must be ≤ 65535.</param>
    /// <param name="algorithm">SHAKE variant to use.</param>
    /// <param name="oversizeDstLength">Output size when an oversize DST must be hashed down;
    /// should be <c>ceil(2k / 8)</c> for security parameter k. Defaults to 32 (k = 128).</param>
    public static ExpandMsgXof Create(
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> dst,
        int lenInBytes,
        XofAlgorithm algorithm,
        int oversizeDstLength = 32)
    {
        if ((uint)lenInBytes > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(lenInBytes));
        }

        ExpandMessageDST dstObj = ExpandMessageDST.ForXof(dst, oversizeDstLength, algorithm);
        byte[] output = new byte[lenInBytes];
        byte[] lenBe = [(byte)(lenInBytes >> 8), (byte)lenInBytes];
        byte[] dstLenByte = [(byte)dstObj.Length];

        Shake xof = Shake.Create(algorithm);
        xof.AppendData(message);
        xof.AppendData(lenBe);
        xof.AppendData(dstObj.Data);
        xof.AppendData(dstLenByte);
        xof.GetHashAndReset(output);

        return new ExpandMsgXof(output);
    }

    /// <summary>
    /// Copies up to <c>min(Remain, output.Length)</c> bytes into <paramref name="output"/>,
    /// advances the read position, and returns the number of bytes written.
    /// </summary>
    public int ReadInto(Span<byte> output)
    {
        int len = Math.Min(Remain, output.Length);
        _output.AsSpan(_offs, len).CopyTo(output[..len]);
        _offs += len;
        return len;
    }
}
