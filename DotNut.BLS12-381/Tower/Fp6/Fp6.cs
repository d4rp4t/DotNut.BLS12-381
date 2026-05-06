namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6(Fp2 c0, Fp2 c1, Fp2 c2)
{
    public static readonly Fp6 Zero = new(Fp2.Zero, Fp2.Zero, Fp2.Zero);
    public static readonly Fp6 One = new(Fp2.One, Fp2.Zero, Fp2.Zero);

    public readonly Fp2 C0 = c0;
    public readonly Fp2 C1 = c1;
    public readonly Fp2 C2 = c2;
}
