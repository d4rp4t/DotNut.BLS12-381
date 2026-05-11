namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    public static readonly Scalar Zero = new(0UL, 0UL, 0UL, 0UL);

    // R mod r = 2^256 mod r  (Montgomery representation of 1)
    public static readonly Scalar One = new(
        0x0000_0001_ffff_fffeUL,
        0x5884_b7fa_0003_4802UL,
        0x998c_4fef_ecbc_4ff5UL,
        0x1824_b159_acc5_056fUL
    );
}
