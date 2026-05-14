using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors.HashToCurve;

internal static class FpTestConstants
{
    // (p-1)/2 — used to test sgn0 in both MapG1Tests and MapG2Tests
    internal static readonly Fp P_M1_OVER2 = new([
        0xa1fa_ffff_fffe_5557,
        0x995b_fff9_76a3_fffe,
        0x03f4_1d24_d174_ceb4,
        0xf654_7998_c199_5dbd,
        0x778a_468f_507a_6034,
        0x0205_5993_1f7f_8103,
    ]);
}
