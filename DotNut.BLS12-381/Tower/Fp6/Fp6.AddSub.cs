namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6
{
    public static Fp6 Add(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Add(a.C0, b.C0),
            Fp2.Add(a.C1, b.C1),
            Fp2.Add(a.C2, b.C2)
            );
    }

    public static Fp6 Subtract(Fp6 a, Fp6 b)
    {
        return new Fp6(
            Fp2.Sub(a.C0, b.C0),
            Fp2.Sub(a.C1, b.C1),
            Fp2.Sub(a.C2, b.C2)
        );
    }
}