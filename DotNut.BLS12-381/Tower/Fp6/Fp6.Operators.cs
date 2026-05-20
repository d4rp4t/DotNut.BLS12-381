namespace DotNut.BLS12_381.Tower;

public readonly partial struct Fp6 
{
    public static Fp6 operator +(Fp6 p, Fp6 q)
    {
        return Fp6.Add(p, q);
    }

    public static Fp6 operator -(Fp6 p, Fp6 q)
    {
        return Fp6.Subtract(p, q);
    }

    public static Fp6 operator -(Fp6 p)
    {
        return Fp6.Negate(p);
    }

    public static Fp6 operator *(Fp6 p, Fp6 q)
    {
        return Fp6.Multiply(p, q);
    }
    
    /// <summary>Equality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator ==(Fp6 a, Fp6 b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/>.</summary>
    public static bool operator !=(Fp6 a, Fp6 b) => !Equal(a, b);
}