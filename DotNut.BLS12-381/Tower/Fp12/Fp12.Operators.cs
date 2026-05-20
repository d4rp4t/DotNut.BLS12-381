namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp12
{
    public static Fp12 operator +(Fp12 p, Fp12 q)
    {
        return Fp12.Add(p, q);
    }

    public static Fp12 operator -(Fp12 p, Fp12 q)
    {
        return Fp12.Subtract(p, q);
    }

    public static Fp12 operator -(Fp12 p)
    {
        return Fp12.Negate(p);
    }

    public static Fp12 operator *(Fp12 p, Fp12 q)
    {
        return Fp12.Multiply(p, q);
    }
    
    public static bool operator ==(Fp12 a, Fp12 b) => Equal(a, b);
    public static bool operator !=(Fp12 a, Fp12 b) => !Equal(a, b);

    public static bool operator <(Fp12 a, Fp12 b) => Compare(a, b) < 0;
    public static bool operator >(Fp12 a, Fp12 b) => Compare(a, b) > 0;
    public static bool operator <=(Fp12 a, Fp12 b) => Compare(a, b) <= 0;
    public static bool operator >=(Fp12 a, Fp12 b) => Compare(a, b) >= 0;
}