using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

/// <summary>
/// Precomputed line-evaluation coefficients for a fixed G2 point.
/// Use with <see cref="Bls12Pairing.MultiMillerLoop"/> for efficient multi-pairings.
/// </summary>
public sealed class G2Prepared
{
    public bool IsInfinity { get; }
    internal (Fp2 c0, Fp2 c1, Fp2 c2)[] Coeffs { get; }

    internal G2Prepared(bool isInfinity, (Fp2, Fp2, Fp2)[] coeffs)
    {
        IsInfinity = isInfinity;
        Coeffs = coeffs;
    }

    public static G2Prepared From(G2Affine q) => Bls12Pairing.BuildG2Prepared(q);
}
