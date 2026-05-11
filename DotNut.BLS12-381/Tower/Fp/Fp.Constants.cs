namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static readonly Fp Zero = new(0UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    internal static readonly Fp RawOne = new(1UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    
    // p = 4002409555221667393417789825735904156556882819939007885332058136124031650490837864442687629129015664037894272559787
    public static readonly Fp Modulus = new(
        0xb9fe_ffff_ffff_aaabUL,
        0x1eab_fffe_b153_ffffUL,
        0x6730_d2a0_f6b0_f624UL,
        0x6477_4b84_f385_12bfUL,
        0x4b1b_a7b6_434b_acd7UL,
        0x1a01_11ea_397f_e69aUL
    );
    
    // 2^384 mod p
    public static readonly Fp One = new(
        0x7609_0000_0002_fffdUL,
        0xebf4_000b_c40c_0002UL,
        0x5f48_9857_53c7_58baUL,
        0x77ce_5853_7052_5745UL,
        0x5c07_1a97_a256_ec6dUL,
        0x15f6_5ec3_fa80_e493UL
    );
    
    // 2^(384*2) mod p
    internal static readonly Fp MontgomeryR2 = new(
        0xf4df_1f34_1c34_1746UL,
        0x0a76_e6a6_09d1_04f1UL,
        0x8de5_476c_4c95_b6d5UL,
        0x67eb_88a9_939d_83c0UL,
        0x9a79_3e85_b519_952dUL,
        0x1198_8fe5_92ca_e3aaUL
    );
    
    // 2^(384*3) mod p
    internal static readonly Fp MontgomeryR3 = new(
        0xed48_ac6b_d94c_a1e0UL,
        0x315f_831e_03a7_adf8UL,
        0x9a53_352a_615e_29ddUL,
        0x34c0_4e5e_921e_1761UL,
        0x2512_d435_6572_4728UL,
        0x0aa6_3460_9175_5d4dUL
    );
    
    // -(p^{-1} mod 2^64) mod 2^64
    internal const ulong MontgomeryInv = 0x89f3_fffc_fffc_fffdUL;
}
