using DotNut.BLS12_381.HashToCurve;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests
{
    public class AdditionChainTests
    {
        private static readonly byte[] SEED =
        [
            0x59, 0x62, 0xbe, 0x5d, 0x76, 0x3d, 0x31, 0x8d,
            0x17, 0xdb, 0x37, 0x32, 0x54, 0x06, 0xbc, 0xe5
        ];

        [Fact]
        public void TestFpChain()
        {
            var rng = new XorShiftRng(SEED);

            ulong[] p_m3_over4 =
            [
                0xee7f_bfff_ffff_eaaaUL,
                0x07aa_ffff_ac54_ffffUL,
                0xd9cc_34a8_3dac_3d89UL,
                0xd91d_d2e1_3ce1_44afUL,
                0x92c6_e9ed_90d2_eb35UL,
                0x0680_447a_8e5f_f9a6UL,
            ];

            for (int i = 0; i < 32; i++)
            {
                Fp input = rng.NextFp();

                Fp expected = Fp.PowVartime(input, p_m3_over4);
                Fp actual = AdditionChains.Pm3Div4(input);

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestFp2Chain()
        {
            var rng = new XorShiftRng(SEED);

            ulong[] p_sq_m9_over16 =
            [
                0xb26a_a000_01c7_18e3UL,
                0xd7ce_d6b1_d763_82eaUL,
                0x3162_c338_3621_13cfUL,
                0x966b_f91e_d3e7_1b74UL,
                0xb292_e85a_8709_1a04UL,
                0x11d6_8619_c861_85c7UL,
                0xef53_1493_3097_8ef0UL,
                0x050a_62cf_d16d_dca6UL,
                0x466e_59e4_9349_e8bdUL,
                0x9e2d_c90e_50e7_046bUL,
                0x74bd_278e_aa22_f25eUL,
                0x002a_437a_4b8c_35fcUL,
            ];

            for (int i = 0; i < 32; i++)
            {
                Fp2 input = rng.NextFp2();

                Fp2 expected = Fp2.PowVartime(input, p_sq_m9_over16);
                Fp2 actual = AdditionChains.P2M9Div16(input);
                Assert.True(Equals(expected, actual));
            }
        }
    }
}
