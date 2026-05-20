namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp2
{
    public static Fp2 operator +(Fp2 p, Fp2 q)
    {
        return Fp2.Add(p, q);
    }

    public static Fp2 operator -(Fp2 p, Fp2 q)
    {
        return Fp2.Subtract(p, q);
    }

    public static Fp2 operator -(Fp2 p)
    {
        return Fp2.Negate(p);
    }

    public static Fp2 operator *(Fp2 p, Fp2 q)
    {
        return Fp2.Multiply(p, q);
    }
    
    public static bool operator ==(Fp2 a, Fp2 b) => Equal(a, b);
    public static bool operator !=(Fp2 a, Fp2 b) => !Equal(a, b);

    public static bool operator <(Fp2 a, Fp2 b) => Compare(a, b) < 0;
    public static bool operator >(Fp2 a, Fp2 b) => Compare(a, b) > 0;
    public static bool operator <=(Fp2 a, Fp2 b) => Compare(a, b) <= 0;
    public static bool operator >=(Fp2 a, Fp2 b) => Compare(a, b) >= 0;
}