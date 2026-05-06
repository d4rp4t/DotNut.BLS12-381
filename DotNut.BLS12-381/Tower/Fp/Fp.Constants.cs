namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static readonly Fp Zero = new(0UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    internal static readonly Fp RawOne = new(1UL, 0UL, 0UL, 0UL, 0UL, 0UL);
    public static readonly Fp One = new(
        0x760900000002fffdUL,
        0xebf4000bc40c0002UL,
        0x5f48985753c758baUL,
        0x77ce585370525745UL,
        0x5c071a97a256ec6dUL,
        0x15f65ec3fa80e493UL
    );
    
    internal const ulong MontgomeryInv = 0x89f3fffcfffcfffdUL;
    internal static readonly Fp MontgomeryR2 = new(
        0xf4df1f341c341746UL,
        0x0a76e6a609d104f1UL,
        0x8de5476c4c95b6d5UL,
        0x67eb88a9939d83c0UL,
        0x9a793e85b519952dUL,
        0x11988fe592cae3aaUL
    );
    public static readonly Fp Modulus = new(
        0xb9feffffffffaaabUL,
        0x1eabfffeb153ffffUL,
        0x6730d2a0f6b0f624UL,
        0x64774b84f38512bfUL,
        0x4b1ba7b6434bacd7UL,
        0x1a0111ea397fe69aUL
    );
}
