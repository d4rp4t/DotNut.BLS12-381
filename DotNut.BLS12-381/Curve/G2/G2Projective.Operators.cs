using DotNut.BLS12_381.Curve.G1;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

public readonly partial struct G2Projective
{
    
    /// <summary>
    /// Projective equivalence: (X1:Y1:Z1) == (X2:Y2:Z2) iff X1·Z2 = X2·Z1 and Y1·Z2 = Y2·Z1.
    /// Both points at infinity are equal.
    /// </summary>
    public static bool operator ==(G2Projective a, G2Projective b)
    {
        bool aInf = a.IsInfinity, bInf = b.IsInfinity;
        if (aInf && bInf) return true;
        if (aInf || bInf) return false;
        return Fp2.Equal(Fp2.Multiply(a.X, b.Z), Fp2.Multiply(b.X, a.Z))
               && Fp2.Equal(Fp2.Multiply(a.Y, b.Z), Fp2.Multiply(b.Y, a.Z));
    }

    public static bool operator !=(G2Projective a, G2Projective b) => !(a == b);

    public static G2Projective operator +(G2Projective a, G2Projective b) => Add(a, b);
    public static G2Projective operator +(G2Projective a, G2Affine b) => Add(a, b);
    public static G2Projective operator +(G2Affine a, G2Projective b) => Add(a, b);
    
    public static G2Projective operator -(G2Projective a, G2Projective b) => Subtract(a, b);
    public static G2Projective operator -(G2Affine a, G2Projective b) => Subtract(a, b);
    public static G2Projective operator -(G2Projective a, G2Affine b) => Subtract(a, b);

    public static G2Projective operator -(G2Projective a) => Negate(a);

    public static G2Projective operator *(G2Projective a, Scalar b) => ScalarMultiply(a, b);
}