using DotNut.BLS12_381.Curve.G2;
using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Pairing;

/// <summary>
/// Precomputed line-evaluation coefficients for a fixed G2 point.
/// Stores the 68 coefficient triples (c0, c1, c2) produced by running the Miller loop doubling
/// and addition steps on the G2 point in advance.
/// Use with <see cref="Bls12Pairing.MultiMillerLoop"/> for efficient multi-pairings where
/// the same G2 point is paired with multiple G1 points.
/// </summary>
public sealed class G2Prepared
{
    /// <summary>Returns <see langword="true"/> if the G2 point used to build this object was the point at infinity.</summary>
    public bool IsInfinity { get; }

    /// <summary>
    /// The 68 precomputed line coefficient triples (c0, c1, c2) corresponding to each doubling/addition
    /// step of the BLS_X/2 Miller loop scalar.
    /// </summary>
    internal (Fp2 c0, Fp2 c1, Fp2 c2)[] Coeffs { get; }

    internal G2Prepared(bool isInfinity, (Fp2, Fp2, Fp2)[] coeffs)
    {
        IsInfinity = isInfinity;
        Coeffs = coeffs;
    }

    /// <summary>
    /// Builds a <see cref="G2Prepared"/> object by running the Miller loop G2 steps on <paramref name="q"/>
    /// and storing the resulting line coefficients.
    /// </summary>
    /// <param name="q">The G2 point to precompute. May be the infinity point.</param>
    public static G2Prepared From(G2Affine q) => Bls12Pairing.BuildG2Prepared(q);
}
