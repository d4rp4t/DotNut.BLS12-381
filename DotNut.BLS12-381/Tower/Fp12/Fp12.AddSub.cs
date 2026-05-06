namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    public static Fp12 Add(Fp12 a, Fp12 b)
    {
        return new Fp12(
            Fp6.Add(a.C0, b.C0),
            Fp6.Add(a.C1, b.C1)
        );
    }

    public static Fp12 Subtract(Fp12 a, Fp12 b)
    {
        return new Fp12(
            Fp6.Subtract(a.C0, b.C0),
            Fp6.Subtract(a.C1, b.C1)
        );
    }
}