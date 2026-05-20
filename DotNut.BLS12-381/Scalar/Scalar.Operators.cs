namespace DotNut.BLS12_381;

public readonly partial struct Scalar
{
    /// <summary>Equality operator; delegates to <see cref="Equal"/> (constant-time).</summary>
    public static bool operator ==(Scalar a, Scalar b) => Equal(a, b);

    /// <summary>Inequality operator; delegates to <see cref="Equal"/> (constant-time).</summary>
    public static bool operator !=(Scalar a, Scalar b) => !Equal(a, b);

    public static Scalar operator +(Scalar a, Scalar b) => Add(a, b);
    public static Scalar operator -(Scalar a, Scalar b) => Sub(a, b);
    public static Scalar operator -(Scalar a) => Negate(a);
    public static Scalar operator *(Scalar a, Scalar b) => Mul(a, b);

    public static bool operator <(Scalar a, Scalar b) => Compare(a, b) < 0;

    public static bool operator >(Scalar a, Scalar b) => Compare(a, b) > 0;

    public static bool operator <=(Scalar a, Scalar b) => Compare(a, b) <= 0;

    public static bool operator >=(Scalar a, Scalar b) => Compare(a, b) >= 0;
}