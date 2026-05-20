namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp
{
    public static Fp operator +(Fp p, Fp q)
    {
        return Fp.Add(p, q);
    }

    public static Fp operator -(Fp p, Fp q)
    {
        return Fp.Subtract(p, q);
    }

    public static Fp operator -(Fp p)
    {
        return Fp.Negate(p);
    }

    public static Fp operator *(Fp p, Fp q)
    {
        return Fp.Multiply(p, q);
    }
    
    public static bool operator ==(Fp a, Fp b) => Equal(a, b);
    public static bool operator !=(Fp a, Fp b) => !Equal(a, b);

    public static bool operator <(Fp a, Fp b) => Compare(a, b) < 0;
    public static bool operator >(Fp a, Fp b) => Compare(a, b) > 0;
    public static bool operator <=(Fp a, Fp b) => Compare(a, b) <= 0;
    public static bool operator >=(Fp a, Fp b) => Compare(a, b) >= 0;
}