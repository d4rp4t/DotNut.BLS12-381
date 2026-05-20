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
    
    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp2 a, Fp2 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp2 a, Fp2 b) => !Equal(a, b);
}