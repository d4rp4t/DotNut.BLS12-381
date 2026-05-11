namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public static int Compare(Fp2 a, Fp2 b)
    {
        int c1 = Fp.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;
        return Fp.Compare(a.C0, b.C0);
    }

    public static bool Equal(Fp2 a, Fp2 b)
    {
        return Fp.Equal(a.C0, b.C0)
               & Fp.Equal(a.C1, b.C1);
    }

    public static bool operator ==(Fp2 a, Fp2 b) => Equal(a, b);
    public static bool operator !=(Fp2 a, Fp2 b) => !Equal(a, b);

    public override bool Equals(object? obj)
    {
        return obj is Fp2 other && Equal(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1);
    }
}
