using DotNut.BLS12_381.HashToCurve;

namespace DotNut.BLS12_381.Tests.ZkCryptoVectors.HashToCurve;

public class MapScalarTests
{
    public static IEnumerable<object[]> GetTestCase()
    {
        yield return new object[]
        {
            new byte[48], 
            "0x0000000000000000000000000000000000000000000000000000000000000000"
        };
        yield return new object[]
        {
            "aaaaaabbbbbbccccccddddddeeeeeeffffffgggggghhhhhh"u8.ToArray(), 
            "0x2228450bf55d8fe62395161bd3677ff6fc28e45b89bc87e02a818eda11a8c5da"
        };
        yield return new object[]
        {
            "111111222222333333444444555555666666777777888888"u8.ToArray(), 
            "0x4aa543cbd2f0c8f37f8a375ce2e383eb343e7e3405f61e438b0a15fb8899d1ae"
        };
    }
    
    [Theory]
    [MemberData(nameof(GetTestCase))]
    public void test_hash_to_scalar(byte[] a, string expected)
    {
        var mapped = HashToCurveMapper.ScalarFromOkm(a);
        var actual = Scalar.ToHexString(mapped);
        Assert.Equal(expected, actual);
    }
}