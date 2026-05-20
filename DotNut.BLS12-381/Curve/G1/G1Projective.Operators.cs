using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

public readonly partial struct G1Projective
{
    /// <summary>
    /// Projective equivalence: (X1:Y1:Z1) == (X2:Y2:Z2) iff X1·Z2 = X2·Z1 and Y1·Z2 = Y2·Z1.
    /// Both points at infinity are equal.
    /// </summary>
    public static bool operator ==(G1Projective a, G1Projective b)
    {
        bool aInf = a.IsInfinity, bInf = b.IsInfinity;
        if (aInf && bInf) return true;
        if (aInf || bInf) return false;
        return Fp.Equal(Fp.Multiply(a.X, b.Z), Fp.Multiply(b.X, a.Z))
               && Fp.Equal(Fp.Multiply(a.Y, b.Z), Fp.Multiply(b.Y, a.Z));
    }

    public static bool operator !=(G1Projective a, G1Projective b) => !(a == b);

    public static G1Projective operator +(G1Projective a, G1Projective b) => Add(a, b);
    public static G1Projective operator +(G1Projective a, G1Affine b) => Add(a, b);
    public static G1Projective operator +(G1Affine a, G1Projective b) => Add(a, b);
    
    public static G1Projective operator -(G1Projective a, G1Projective b) => Subtract(a, b);
    public static G1Projective operator -(G1Affine a, G1Projective b) => Subtract(a, b);
    public static G1Projective operator -(G1Projective a, G1Affine b) => Subtract(a, b);

    public static G1Projective operator -(G1Projective a) => Negate(a);

    public static G1Projective operator *(G1Projective a, Scalar b) => ScalarMultiply(a, b);
}