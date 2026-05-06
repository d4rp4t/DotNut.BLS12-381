namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static readonly Fp Zero = new(0UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    public static readonly Fp One = new(1UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    public static readonly Fp Modulus = new(
        0xb9feffffffffaaabUL,
        0x1eabfffeb153ffffUL,
        0x6730d2a0f6b0f624UL,
        0x64774b84f38512bfUL,
        0x4b1ba7b6434bacd7UL,
        0x1a0111ea397fe69aUL
    );
}