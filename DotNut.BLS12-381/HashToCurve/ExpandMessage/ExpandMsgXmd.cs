using System.Security.Cryptography;

namespace DotNut.BLS12_381.HashToCurve;

/// <summary>
/// RFC 9380 §5.3.1 <c>expand_message_xmd</c> using a fixed-output hash (SHA-256 or SHA-512).
/// Generates a stream of pseudorandom bytes by chaining uniformly distributed hash blocks.
/// <see cref="ReadInto"/> yields successive chunks without buffering the entire output.
/// </summary>
internal sealed class ExpandMsgXmd
{
    private readonly ExpandMessageDST _dst;
    private readonly byte[] _b0;
    private byte[] _bi;
    private int _i;
    private int _bOffs;
    private int _remain;
    private readonly HashAlgorithmName _hashName;

    /// <summary>Number of bytes that have not yet been consumed by <see cref="ReadInto"/>.</summary>
    public int Remain => _remain;

    private ExpandMsgXmd(ExpandMessageDST dst, byte[] b0, byte[] b1, int lenInBytes, HashAlgorithmName hashName)
    {
        _dst = dst;
        _b0 = b0;
        _bi = b1;
        _i = 2;
        _bOffs = 0;
        _remain = lenInBytes;
        _hashName = hashName;
    }

    /// <summary>
    /// Initialises an expand_message_xmd generator (RFC 9380 §5.3.1).
    /// Eagerly computes <c>b_0</c> and <c>b_1</c>; subsequent blocks are generated lazily
    /// on demand inside <see cref="ReadInto"/>.
    /// </summary>
    /// <param name="message">Input message.</param>
    /// <param name="dst">Domain separation tag. Oversize DSTs are hashed down via
    /// <paramref name="hashName"/> with the oversize salt prefix (RFC 9380 §5.3.3).</param>
    /// <param name="lenInBytes">Total pseudorandom bytes to produce. Must be ≤ 65535 and
    /// <c>ceil(lenInBytes / hashOutputSize) ≤ 255</c>.</param>
    /// <param name="hashName">Hash function; must be <see cref="HashAlgorithmName.SHA256"/> or
    /// <see cref="HashAlgorithmName.SHA512"/>.</param>
    public static ExpandMsgXmd Create(
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> dst,
        int lenInBytes,
        HashAlgorithmName hashName)
    {
        if ((uint)lenInBytes > 65535)
            throw new ArgumentOutOfRangeException(nameof(lenInBytes));

        var hashSize = GetOutputSize(hashName);
        var blockSize = GetBlockSize(hashName);
        var ell = (lenInBytes + hashSize - 1) / hashSize;
        if (ell > 255)
            throw new ArgumentOutOfRangeException(nameof(lenInBytes), "ceil(lenInBytes / hashOutputSize) > 255");

        var dstObj = ExpandMessageDST.ForXmd(dst, hashName);
        byte[] lenBe = [(byte)(lenInBytes >> 8), (byte)lenInBytes];
        byte[] dstLenByte = [(byte)dstObj.Length];
        var zPad = new byte[blockSize];

        // b_0 = H(Z_pad || msg || I2OSP(len_in_bytes, 2) || I2OSP(0, 1) || DST_prime)
        byte[] b0;
        using (var h = IncrementalHash.CreateHash(hashName))
        {
            h.AppendData(zPad);
            h.AppendData(message);
            h.AppendData(lenBe);
            h.AppendData(new byte[] { 0 });
            h.AppendData(dstObj.Data.ToArray());
            h.AppendData(dstLenByte);
            b0 = h.GetHashAndReset();
        }

        // b_1 = H(b_0 || I2OSP(1, 1) || DST_prime)
        byte[] b1;
        using (var h = IncrementalHash.CreateHash(hashName))
        {
            h.AppendData(b0);
            h.AppendData(new byte[] { 1 });
            h.AppendData(dstObj.Data.ToArray());
            h.AppendData(dstLenByte);
            b1 = h.GetHashAndReset();
        }

        return new ExpandMsgXmd(dstObj, b0, b1, lenInBytes, hashName);
    }

    /// <summary>
    /// Copies up to <c>min(Remain, output.Length)</c> bytes into <paramref name="output"/>,
    /// generating the next hash block when the current block is exhausted, and returns the
    /// number of bytes written.
    /// Each new block i ≥ 2 is: <c>b_i = H((b_0 XOR b_{i-1}) || I2OSP(i, 1) || DST_prime)</c>.
    /// </summary>
    public int ReadInto(Span<byte> output)
    {
        var readLen = Math.Min(_remain, output.Length);
        var offs = 0;
        var hashSize = GetOutputSize(_hashName);
        byte[] dstLenByte = [(byte)_dst.Length];
        var dstBytes = _dst.Data.ToArray();

        while (offs < readLen)
        {
            var avail = hashSize - _bOffs;
            if (avail > 0)
            {
                var copy = Math.Min(avail, readLen - offs);
                _bi.AsSpan(_bOffs, copy).CopyTo(output.Slice(offs, copy));
                offs += copy;
                _bOffs += copy;
            }
            else
            {
                var xored = new byte[hashSize];
                for (var j = 0; j < hashSize; j++)
                    xored[j] = (byte)(_b0[j] ^ _bi[j]);

                using var h = IncrementalHash.CreateHash(_hashName);
                h.AppendData(xored);
                h.AppendData(new byte[] { (byte)_i });
                h.AppendData(dstBytes);
                h.AppendData(dstLenByte);
                _bi = h.GetHashAndReset();
                _bOffs = 0;
                _i++;
            }
        }

        _remain -= readLen;
        return readLen;
    }

    /// <summary>
    /// Returns the hash output size in bytes for the given algorithm.
    /// SHA-256 → 32; SHA-512 → 64.
    /// </summary>
    private static int GetOutputSize(HashAlgorithmName name)
    {
        if (name == HashAlgorithmName.SHA256) return 32;
        if (name == HashAlgorithmName.SHA512) return 64;
        throw new ArgumentException($"Unsupported hash: {name.Name}", nameof(name));
    }

    /// <summary>
    /// Returns the hash input block size in bytes for the given algorithm.
    /// SHA-256 → 64; SHA-512 → 128.
    /// </summary>
    private static int GetBlockSize(HashAlgorithmName name)
    {
        if (name == HashAlgorithmName.SHA256) return 64;
        if (name == HashAlgorithmName.SHA512) return 128;
        throw new ArgumentException($"Unsupported hash: {name.Name}", nameof(name));
    }
}
