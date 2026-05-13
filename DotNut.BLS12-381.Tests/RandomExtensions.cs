using DotNut.BLS12_381.Tower;

namespace DotNut.BLS12_381.Tests;

public static class RandomExtensions
{
    // p.L5 = 0x1a0111ea397fe69a — any Fp with L5 strictly less than this is guaranteed < p
    private const ulong ModulusL5 = 0x1a0111ea397fe69aUL;

    public static Fp NextFp(this ref XorShiftRng rng)
    {
        while (true)
        {
            ulong l0 = rng.NextU64(), l1 = rng.NextU64(), l2 = rng.NextU64();
            ulong l3 = rng.NextU64(), l4 = rng.NextU64();
            ulong l5 = rng.NextU64() & 0x1fffffffffffffffUL; // clear top 3 bits → < 2^61
            if (l5 < ModulusL5)
                return new Fp(l0, l1, l2, l3, l4, l5);
        }
    }

    public static Fp2 NextFp2(this ref XorShiftRng rng)
    {
        return new Fp2(rng.NextFp(), rng.NextFp());
    }
}