namespace DotNut.BLS12_381;

public ref struct XorShiftRng
{
    private ulong state;

    public XorShiftRng(byte[] seed)
    {
        state = BitConverter.ToUInt64(seed, 0);
    }

    public ulong NextU64()
    {
        ulong x = state;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        state = x;
        return x * 2685821657736338717UL;
    }
}