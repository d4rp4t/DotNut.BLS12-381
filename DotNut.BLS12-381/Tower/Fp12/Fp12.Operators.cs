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
    
    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp12 a, Fp12 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp12 a, Fp12 b) => !Equal(a, b);
}