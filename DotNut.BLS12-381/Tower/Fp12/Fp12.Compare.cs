namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    public static int Compare(Fp12 a, Fp12 b)
    {
        var c1 = Fp6.Compare(a.C1, b.C1);
        if (c1 != 0) return c1;
        return Fp6.Compare(a.C0, b.C0);
    }

    public static bool Equal(Fp12 a, Fp12 b)
    {
        return Fp6.Equal(a.C0, b.C0)
               && Fp6.Equal(a.C1, b.C1);
    }

    public static bool operator ==(Fp12 a, Fp12 b) => Equal(a, b);
    public static bool operator !=(Fp12 a, Fp12 b) => !Equal(a, b);

    public override bool Equals(object? obj)
    {
        return obj is Fp12 other && Equal(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(C0, C1);
    }
}
