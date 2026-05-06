namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public readonly Fp C0;
    public readonly Fp C1;
    
    public Fp2(Fp c0, Fp c1)
    {
        C0 = c0;
        C1 = c1;
    }
}