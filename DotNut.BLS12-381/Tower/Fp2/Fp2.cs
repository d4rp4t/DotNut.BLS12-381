namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public static readonly Fp2 Zero = new(Fp.Zero, Fp.Zero);
    public static readonly Fp2 One = new(Fp.One, Fp.Zero);
    // xi = u + 1; used as Fp6 cubic non-residue in BLS12-381 tower
    public static readonly Fp2 NonResidue = new(Fp.One, Fp.One);

    public readonly Fp C0;
    public readonly Fp C1;
    
    public Fp2(Fp c0, Fp c1)
    {
        C0 = c0;
        C1 = c1;
    }
}
