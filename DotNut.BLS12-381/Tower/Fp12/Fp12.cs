namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    public static readonly Fp12 Zero = new(Fp6.Zero, Fp6.Zero);
    public static readonly Fp12 One = new(Fp6.One, Fp6.Zero);

    public readonly Fp6 C0;
    public readonly Fp6 C1;

    public Fp12(Fp6 c0, Fp6 c1)
    {
        C0 = c0;
        C1 = c1;
    }
}
