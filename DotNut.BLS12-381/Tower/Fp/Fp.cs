namespace DotNut.BLS12_381.Tower;

/// <summary>
/// Represents an element of the base field Fp of the BLS12-381 scalar tower.
///
/// Values are stored internally in Montgomery form as six 64-bit limbs
/// in little-endian limb order (<c>L0</c> is the least significant limb).
/// </summary>
public readonly partial struct Fp
{
    // 6 * 64 = 384 bit
    internal readonly ulong L0, L1, L2, L3, L4, L5;
    
    // these constructors are in raw form. all of these limbs MUST be in little endian at this point, and 
    // SHOULD be in montgomery domain, OR you'll have to use Fp.FromCanonical. Math won't work on canonical form. 
    // i'd rather not use these ctors until explictly needed
    
    public Fp(ulong l0, ulong l1, ulong l2, ulong l3, ulong l4, ulong l5)
    {
        L0 = l0;
        L1 = l1;
        L2 = l2;
        L3 = l3;
        L4 = l4;
        L5 = l5;
    }
    
    public Fp(ReadOnlySpan<ulong> limbs){
        if (limbs.Length < 6)
        {
            throw new ArgumentOutOfRangeException(nameof(limbs), "Fp contains only 6 limbs!");
        }
        L0 = limbs[0];
        L1 = limbs[1];
        L2 = limbs[2];
        L3 = limbs[3];
        L4 = limbs[4];
        L5 = limbs[5];
    }
}