using System.Security.Cryptography;
using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Curve.G2;

namespace DotNut.BLS12_381.HashToCurve;

public static class HashToCurve
{
    // RFC 9380 §8.8.1 — BLS12381G1_XMD:SHA-256_SSWU_RO_ (random oracle)
    public static G1Affine HashToG1(ReadOnlySpan<byte> message, ReadOnlySpan<byte> dst)
    {
        Span<byte> okm = stackalloc byte[128]; // 2 × 64 bytes
        ExpandMsgXmd.Create(message, dst, 128, HashAlgorithmName.SHA256).ReadInto(okm);
        var p0 = HashToCurveMapper.FpFromOkm(okm[..64]).MapToCurve();
        var p1 = HashToCurveMapper.FpFromOkm(okm[64..]).MapToCurve();
        return G1Projective.Add(p0, p1).ClearCofactor().ToAffine();
    }

    // RFC 9380 §8.8.2 — BLS12381G1_XMD:SHA-256_SSWU_NU_ (non-uniform encoding)
    public static G1Affine EncodeToG1(ReadOnlySpan<byte> message, ReadOnlySpan<byte> dst)
    {
        Span<byte> okm = stackalloc byte[64];
        ExpandMsgXmd.Create(message, dst, 64, HashAlgorithmName.SHA256).ReadInto(okm);
        return HashToCurveMapper.FpFromOkm(okm).MapToCurve().ClearCofactor().ToAffine();
    }

    // RFC 9380 §8.8.3 — BLS12381G2_XMD:SHA-256_SSWU_RO_ (random oracle)
    public static G2Affine HashToG2(ReadOnlySpan<byte> message, ReadOnlySpan<byte> dst)
    {
        Span<byte> okm = stackalloc byte[256]; // 2 × 128 bytes
        ExpandMsgXmd.Create(message, dst, 256, HashAlgorithmName.SHA256).ReadInto(okm);
        var p0 = HashToCurveMapper.Fp2FromOkm(okm[..128]).MapToCurve();
        var p1 = HashToCurveMapper.Fp2FromOkm(okm[128..]).MapToCurve();
        return G2Projective.Add(p0, p1).ClearCofactor().ToAffine();
    }

    // RFC 9380 §8.8.4 — BLS12381G2_XMD:SHA-256_SSWU_NU_ (non-uniform encoding)
    public static G2Affine EncodeToG2(ReadOnlySpan<byte> message, ReadOnlySpan<byte> dst)
    {
        Span<byte> okm = stackalloc byte[128];
        ExpandMsgXmd.Create(message, dst, 128, HashAlgorithmName.SHA256).ReadInto(okm);
        return HashToCurveMapper.Fp2FromOkm(okm).MapToCurve().ClearCofactor().ToAffine();
    }
}
