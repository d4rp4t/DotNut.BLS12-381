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
}
