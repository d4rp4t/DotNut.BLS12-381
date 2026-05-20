using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.HashToCurve;

internal static partial class HashToCurveMapper
{
    // 2^256 mod p, stored in Montgomery form — matches Rust from_raw_unchecked constant.
    private static readonly Fp F2_256 = new Fp(
        0x075b_3cd7_c5ce_820f,
        0x3ec6_ba62_1c3e_db0b,
        0x168a_13d8_2bff_6bce,
        0x8766_3c4b_f8c4_49d2,
        0x15f3_4c83_ddc8_d830,
        0x0f96_28b4_9caa_2e85
    );

    // RFC 9380 §5.2 hash_to_field for Fp.
    // okm must be 64 bytes: L = ceil((log2(p) + k) / 8) = ceil((381 + 128) / 8) = 64.
    // Splits into two 32-byte halves, interprets each as a big-endian integer padded to 48 bytes,
    // then returns high * 2^256 + low (mod p).
    internal static Fp FpFromOkm(ReadOnlySpan<byte> okm)
    {
        Span<byte> bs = stackalloc byte[48];
        bs.Clear();
        okm[..32].CopyTo(bs[16..]);
        var high = Fp.FromBytesBigEndian(bs);
        okm[32..].CopyTo(bs[16..]);
        var low = Fp.FromBytesBigEndian(bs);
        return Fp.Add(Fp.Multiply(high, F2_256), low);
    }

    // RFC 9380 §5.2 hash_to_field for Fp2.
    // okm must be 128 bytes: 2 × 64 bytes, one per Fp2 component (c0 from first half, c1 from second).
    internal static Fp2 Fp2FromOkm(ReadOnlySpan<byte> okm)
        => new Fp2(FpFromOkm(okm[..64]), FpFromOkm(okm[64..]));
}
