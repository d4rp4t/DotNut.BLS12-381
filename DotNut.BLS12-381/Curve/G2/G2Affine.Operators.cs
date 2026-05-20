using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G2;

public readonly partial struct G2Affine 
{
    public static bool operator ==(G2Affine a, G2Affine b)
    {
        if (a.IsInfinity && b.IsInfinity) return true;
        if (a.IsInfinity || b.IsInfinity) return false;
        return Fp2.Equal(a.X, b.X) && Fp2.Equal(a.Y, b.Y);
    }

    public static bool operator !=(G2Affine a, G2Affine b) => !(a == b);
    
    public static G2Affine operator -(G2Affine a) => Negate(a);
}