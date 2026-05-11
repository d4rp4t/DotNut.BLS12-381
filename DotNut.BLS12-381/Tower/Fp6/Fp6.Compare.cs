namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6
{
    public static int Compare(Fp6 a, Fp6 b)
    {
        var c2 = Fp2.Compare(a.C2, b.C2);
        if (c2 != 0) return c2;

        var c1 = Fp2.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;

        return Fp2.Compare(a.C0, b.C0);
    }

    public static bool Equal(Fp6 a, Fp6 b)
    {
        return Fp2.Equal(a.C0, b.C0)
               & Fp2.Equal(a.C1, b.C1)
               & Fp2.Equal(a.C2, b.C2);
    }

    public static bool operator ==(Fp6 a, Fp6 b) => Equal(a, b);
    public static bool operator !=(Fp6 a, Fp6 b) => !Equal(a, b);

    public override bool Equals(object? obj)
    {
        return obj is Fp6 other && Equal(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1, C2);
    }
}
