namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>Additive identity: 0 in Montgomery form (raw limbs are all zero).</summary>
    public static readonly Scalar Zero = new(0UL, 0UL, 0UL, 0UL);

    /// <summary>
    /// Multiplicative identity: 1 in Montgomery form, stored as R mod r = 2^256 mod r.
    /// Raw limbs do NOT equal 1 — this is the Montgomery representation of 1.
    /// </summary>
    public static readonly Scalar One = new(
        0x0000_0001_ffff_fffeUL,
        0x5884_b7fa_0003_4802UL,
        0x998c_4fef_ecbc_4ff5UL,
        0x1824_b159_acc5_056fUL
    );
    
    // -r^{-1} mod 2^64
    internal const ulong MontgomeryInv = 0xfffffffeffffffffUL;

    /// <summary>R² mod r where R = 2^256. Used to convert integers into Montgomery form via MontgomeryReduce(a · R²) = a·R mod r.</summary>
    internal static readonly Scalar R2 = new(
        0xc999e990f3f29c6dUL,
        0x2b6cedcb87925c23UL,
        0x05d314967254398fUL,
        0x0748d9d99f59ff11UL
    );
    
    /// <summary>R³ mod r where R = 2^256. Used in <see cref="FromU512"/>.</summary>
    internal static readonly Scalar R3 = new(
        0xc62c_1807_439b_73afUL,
        0x1b3e_0d18_8cf0_6990UL,
        0x73d1_3c71_c7b5_f418UL,
        0x6e2a_5bb9_c8db_33e9UL
    );

    /// <summary>The prime group order r of BLS12-381.</summary>
    internal static readonly Scalar GroupOrderR = new(
        0xffffffff00000001UL,
        0x53bda402fffe5bfeUL,
        0x3339d80809a1d805UL,
        0x73eda753299d7d48UL
    );
    
    /// <summary>
    /// 2-adicity of r − 1: r − 1 = 2^<see cref="TwoAdicity"/> · t with t odd.
    /// </summary>
    public const int TwoAdicity = 32;

    /// <summary>
    /// Primitive 2^<see cref="TwoAdicity"/>-th root of unity: <c>7^t mod r</c>
    /// where t = (r − 1) / 2^32. Stored in Montgomery form.
    /// </summary>
    public static readonly Scalar RootOfUnity = new(
        0xb9b5_8d8c_5f0e_466aUL,
        0x5b1b_4c80_1819_d7ecUL,
        0x0af5_3ae3_52a3_1e64UL,
        0x5bf3_adda_19e9_b27bUL
    );

    /// <summary>
    /// Multiplicative inverse of <see cref="RootOfUnity"/>. Stored in Montgomery form.
    /// </summary>
    public static readonly Scalar RootOfUnityInv = new(
        0x4256_481a_dcf3_219aUL,
        0x45f3_7b7f_96b6_cad3UL,
        0xf9c3_f1d7_5f7a_3b27UL,
        0x2d2f_c049_658a_fd43UL
    );

    /// <summary>
    /// <c>7^(2^<see cref="TwoAdicity"/>) mod r</c> — primitive t-th root of unity
    /// where t = (r − 1) / 2^32 is the odd cofactor. Stored in Montgomery form.
    /// </summary>
    public static readonly Scalar Delta = new(
        0x70e3_10d3_d146_f96aUL,
        0x4b64_c089_19e2_99e6UL,
        0x51e1_1418_6a8b_970dUL,
        0x6185_d066_27c0_67cbUL
    );

    /// <summary>The modular inverse of 2 in Fr: 2⁻¹ mod r. Stored in Montgomery form.</summary>
    public static readonly Scalar TwoInv = new(
        0x0000_0000_ffff_ffffUL,
        0xac42_5bfd_0001_a401UL,
        0xccc6_27f7_f65e_27faUL,
        0x0c12_58ac_d662_82b7UL
    );
}
