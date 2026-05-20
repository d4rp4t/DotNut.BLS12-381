namespace DotNut.BLS12_381.Pairing;

public readonly partial struct Gt
{
    public static bool operator ==(Gt a, Gt b) => Equal(a, b);
    public static bool operator !=(Gt a, Gt b) => !Equal(a, b);
    
    public static Gt operator +(Gt a, Gt b) => Add(a, b);
    public static Gt operator -(Gt a, Gt b) => Subtract(a, b);
    public static Gt operator -(Gt a) => Negate(a);

    public static Gt operator *(Gt a, Scalar b) => Multiply(a, b);
}