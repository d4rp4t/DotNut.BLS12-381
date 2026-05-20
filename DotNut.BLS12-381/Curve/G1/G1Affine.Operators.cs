using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Curve.G1;

public readonly partial struct G1Affine
{
    public static bool operator ==(G1Affine a, G1Affine b)
    {
        if (a.IsInfinity && b.IsInfinity) return true;
        if (a.IsInfinity || b.IsInfinity) return false;
        return Fp.Equal(a.X, b.X) && Fp.Equal(a.Y, b.Y);
    }

    public static bool operator !=(G1Affine a, G1Affine b) => !(a == b);
    
    public static G1Affine operator -(G1Affine a) => Negate(a);
}