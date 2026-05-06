namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    internal readonly ulong L0, L1, L2, L3, L4, L5;
    
    public Fp(ulong l0, ulong l1, ulong l2, ulong l3, ulong l4, ulong l5)
    {
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
        L4 = l4;
        L5 = l5;
    }
}