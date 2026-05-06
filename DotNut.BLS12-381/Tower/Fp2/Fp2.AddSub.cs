namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public static Fp2 Add(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Add(a.C0, b.C0),
            Fp.Add(a.C1, b.C1)
        );
    }

    public static Fp2 Sub(Fp2 a, Fp2 b)
    {
        return new Fp2(
            Fp.Subtract(a.C0, b.C0),
            Fp.Subtract(a.C1, b.C1)
        );
    }
}