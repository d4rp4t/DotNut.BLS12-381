using System.Buffers.Binary;
using System.Numerics;

namespace DotNut.BLS12_381.HashToCurve;

internal class KeccakDigest
{
    private static readonly ulong[] KeccakRoundConstants =
    [
        0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
        0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
        0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
        0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
        0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
        0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL,
    ];

    private readonly ulong[] _state = new ulong[25];
    protected readonly byte[] dataQueue = new byte[192];
    protected int rate;
    protected int bitsInQueue;
    protected int fixedOutputLength;
    protected bool squeezing;

    protected KeccakDigest(int bitLength) => Init(bitLength);

    public void BlockUpdate(ReadOnlySpan<byte> input) => Absorb(input);

    public void Reset() => Init(fixedOutputLength);

    private void Init(int bitLength)
    {
        InitSponge(bitLength switch
        {
            128 or 224 or 256 or 288 or 384 or 512 => 1600 - (bitLength << 1),
            _ => throw new ArgumentException($"Bit length {bitLength} not supported.", nameof(bitLength)),
        });
    }

    private void InitSponge(int newRate)
    {
        rate = newRate;
        _state.AsSpan().Clear();
        dataQueue.AsSpan().Clear();
        bitsInQueue = 0;
        squeezing = false;
        fixedOutputLength = (1600 - newRate) >> 1;
    }

    protected void Absorb(ReadOnlySpan<byte> data)
    {
        if ((bitsInQueue & 7) != 0)
            throw new InvalidOperationException("attempt to absorb with odd length queue");
        if (squeezing)
            throw new InvalidOperationException("attempt to absorb while squeezing");

        int bytesInQueue = bitsInQueue >> 3;
        int rateBytes = rate >> 3;
        int available = rateBytes - bytesInQueue;

        if (data.Length < available)
        {
            data.CopyTo(dataQueue.AsSpan(bytesInQueue));
            bitsInQueue += data.Length << 3;
            return;
        }

        int count = 0;
        if (bytesInQueue > 0)
        {
            data[..available].CopyTo(dataQueue.AsSpan(bytesInQueue));
            count += available;
            KeccakAbsorb(dataQueue);
        }

        int remaining;
        while ((remaining = data.Length - count) >= rateBytes)
        {
            KeccakAbsorb(data[count..]);
            count += rateBytes;
        }

        data[count..].CopyTo(dataQueue.AsSpan());
        bitsInQueue = remaining << 3;
    }

    protected void AbsorbBits(int data, int bits)
    {
        if ((bitsInQueue & 7) != 0)
            throw new InvalidOperationException("attempt to absorb with odd length queue");
        if (squeezing)
            throw new InvalidOperationException("attempt to absorb while squeezing");

        dataQueue[bitsInQueue >> 3] = (byte)(data & ((1 << bits) - 1));
        bitsInQueue += bits;
    }

    private void PadAndSwitchToSqueezingPhase()
    {
        dataQueue[bitsInQueue >> 3] |= (byte)(1 << (bitsInQueue & 7));

        if (++bitsInQueue == rate)
        {
            KeccakAbsorb(dataQueue);
        }
        else
        {
            int full = bitsInQueue >> 6, partial = bitsInQueue & 63, off = 0;
            for (int i = 0; i < full; i++, off += 8)
                _state[i] ^= BinaryPrimitives.ReadUInt64LittleEndian(dataQueue.AsSpan(off));
            if (partial > 0)
                _state[full] ^= BinaryPrimitives.ReadUInt64LittleEndian(dataQueue.AsSpan(off)) & ((1UL << partial) - 1UL);
        }

        _state[(rate - 1) >> 6] ^= 1UL << 63;
        bitsInQueue = 0;
        squeezing = true;
    }

    protected void Squeeze(Span<byte> output)
    {
        int rateBytes = rate >> 3;
        int laneCount = rate >> 6;

        if (!squeezing)
        {
            PadAndSwitchToSqueezingPhase();
        }
        else if (bitsInQueue > 0)
        {
            int available = bitsInQueue >> 3;
            int pos = rateBytes - available;

            if (output.Length <= available)
            {
                dataQueue.AsSpan(pos, output.Length).CopyTo(output);
                bitsInQueue -= output.Length << 3;
                return;
            }

            dataQueue.AsSpan(pos, available).CopyTo(output);
            output = output[available..];
            bitsInQueue = 0;
        }

        while (output.Length >= rateBytes)
        {
            KeccakPermutation(_state);
            for (int i = 0; i < laneCount; i++)
                BinaryPrimitives.WriteUInt64LittleEndian(output[(i * 8)..], _state[i]);
            output = output[rateBytes..];
        }

        if (!output.IsEmpty)
        {
            KeccakPermutation(_state);
            for (int i = 0; i < laneCount; i++)
                BinaryPrimitives.WriteUInt64LittleEndian(dataQueue.AsSpan(i * 8), _state[i]);
            dataQueue.AsSpan(0, output.Length).CopyTo(output);
            bitsInQueue = rate - (output.Length << 3);
        }
    }

    private void KeccakAbsorb(ReadOnlySpan<byte> data)
    {
        int count = rate >> 6;
        for (int i = 0; i < count; i++)
            _state[i] ^= BinaryPrimitives.ReadUInt64LittleEndian(data[(i * 8)..]);
        KeccakPermutation(_state);
    }

    private static void KeccakPermutation(Span<ulong> A)
    {
        var _ = A[24]; // bounds check

        ulong a00 = A[ 0], a01 = A[ 1], a02 = A[ 2], a03 = A[ 3], a04 = A[ 4];
        ulong a05 = A[ 5], a06 = A[ 6], a07 = A[ 7], a08 = A[ 8], a09 = A[ 9];
        ulong a10 = A[10], a11 = A[11], a12 = A[12], a13 = A[13], a14 = A[14];
        ulong a15 = A[15], a16 = A[16], a17 = A[17], a18 = A[18], a19 = A[19];
        ulong a20 = A[20], a21 = A[21], a22 = A[22], a23 = A[23], a24 = A[24];

        for (int i = 0; i < 24; i++)
        {
            // theta
            ulong c0 = a00 ^ a05 ^ a10 ^ a15 ^ a20;
            ulong c1 = a01 ^ a06 ^ a11 ^ a16 ^ a21;
            ulong c2 = a02 ^ a07 ^ a12 ^ a17 ^ a22;
            ulong c3 = a03 ^ a08 ^ a13 ^ a18 ^ a23;
            ulong c4 = a04 ^ a09 ^ a14 ^ a19 ^ a24;

            ulong d1 = BitOperations.RotateLeft(c1, 1) ^ c4;
            ulong d2 = BitOperations.RotateLeft(c2, 1) ^ c0;
            ulong d3 = BitOperations.RotateLeft(c3, 1) ^ c1;
            ulong d4 = BitOperations.RotateLeft(c4, 1) ^ c2;
            ulong d0 = BitOperations.RotateLeft(c0, 1) ^ c3;

            a00 ^= d1; a05 ^= d1; a10 ^= d1; a15 ^= d1; a20 ^= d1;
            a01 ^= d2; a06 ^= d2; a11 ^= d2; a16 ^= d2; a21 ^= d2;
            a02 ^= d3; a07 ^= d3; a12 ^= d3; a17 ^= d3; a22 ^= d3;
            a03 ^= d4; a08 ^= d4; a13 ^= d4; a18 ^= d4; a23 ^= d4;
            a04 ^= d0; a09 ^= d0; a14 ^= d0; a19 ^= d0; a24 ^= d0;

            // rho/pi
            c1  = BitOperations.RotateLeft(a01,  1);
            a01 = BitOperations.RotateLeft(a06, 44);
            a06 = BitOperations.RotateLeft(a09, 20);
            a09 = BitOperations.RotateLeft(a22, 61);
            a22 = BitOperations.RotateLeft(a14, 39);
            a14 = BitOperations.RotateLeft(a20, 18);
            a20 = BitOperations.RotateLeft(a02, 62);
            a02 = BitOperations.RotateLeft(a12, 43);
            a12 = BitOperations.RotateLeft(a13, 25);
            a13 = BitOperations.RotateLeft(a19,  8);
            a19 = BitOperations.RotateLeft(a23, 56);
            a23 = BitOperations.RotateLeft(a15, 41);
            a15 = BitOperations.RotateLeft(a04, 27);
            a04 = BitOperations.RotateLeft(a24, 14);
            a24 = BitOperations.RotateLeft(a21,  2);
            a21 = BitOperations.RotateLeft(a08, 55);
            a08 = BitOperations.RotateLeft(a16, 45);
            a16 = BitOperations.RotateLeft(a05, 36);
            a05 = BitOperations.RotateLeft(a03, 28);
            a03 = BitOperations.RotateLeft(a18, 21);
            a18 = BitOperations.RotateLeft(a17, 15);
            a17 = BitOperations.RotateLeft(a11, 10);
            a11 = BitOperations.RotateLeft(a07,  6);
            a07 = BitOperations.RotateLeft(a10,  3);
            a10 = c1;

            // chi
            c0 = a00 ^ (~a01 & a02); c1 = a01 ^ (~a02 & a03);
            a02 ^= ~a03 & a04; a03 ^= ~a04 & a00; a04 ^= ~a00 & a01;
            a00 = c0; a01 = c1;

            c0 = a05 ^ (~a06 & a07); c1 = a06 ^ (~a07 & a08);
            a07 ^= ~a08 & a09; a08 ^= ~a09 & a05; a09 ^= ~a05 & a06;
            a05 = c0; a06 = c1;

            c0 = a10 ^ (~a11 & a12); c1 = a11 ^ (~a12 & a13);
            a12 ^= ~a13 & a14; a13 ^= ~a14 & a10; a14 ^= ~a10 & a11;
            a10 = c0; a11 = c1;

            c0 = a15 ^ (~a16 & a17); c1 = a16 ^ (~a17 & a18);
            a17 ^= ~a18 & a19; a18 ^= ~a19 & a15; a19 ^= ~a15 & a16;
            a15 = c0; a16 = c1;

            c0 = a20 ^ (~a21 & a22); c1 = a21 ^ (~a22 & a23);
            a22 ^= ~a23 & a24; a23 ^= ~a24 & a20; a24 ^= ~a20 & a21;
            a20 = c0; a21 = c1;

            // iota
            a00 ^= KeccakRoundConstants[i];
        }

        A[ 0] = a00; A[ 1] = a01; A[ 2] = a02; A[ 3] = a03; A[ 4] = a04;
        A[ 5] = a05; A[ 6] = a06; A[ 7] = a07; A[ 8] = a08; A[ 9] = a09;
        A[10] = a10; A[11] = a11; A[12] = a12; A[13] = a13; A[14] = a14;
        A[15] = a15; A[16] = a16; A[17] = a17; A[18] = a18; A[19] = a19;
        A[20] = a20; A[21] = a21; A[22] = a22; A[23] = a23; A[24] = a24;
    }
}
