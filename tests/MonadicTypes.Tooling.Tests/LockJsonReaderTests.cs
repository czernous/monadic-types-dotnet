using System.Text;
using MonadicTypes.Tooling;

namespace MonadicTypes.Tooling.Tests;

public sealed class LockJsonReaderTests
{
    [Fact]
    public void PortableTargetIsAccepted()
    {
        bool valid = Parse(
            """{"version":2,"dependencies":{"net10.0":{"Package":{"type":"Direct"}}}}""",
            out bool found,
            out string target);

        Assert.True(valid);
        Assert.True(found);
        Assert.Empty(target);
    }

    [Theory]
    [InlineData("net10.0/win-x64")]
    [InlineData("net10.0\\/linux-x64")]
    [InlineData("net10.0\\u002Fosx-arm64")]
    public void RuntimeSpecificTargetIsReported(string runtimeTarget)
    {
        string json = "{\"dependencies\":{\"" + runtimeTarget + "\":{}}}";
        bool valid = Parse(json, out bool found, out string target);

        Assert.True(valid);
        Assert.True(found);
        Assert.NotEmpty(target);
    }

    [Fact]
    public void NestedPackageDependencyIsNotATarget()
    {
        bool valid = Parse(
            """{"dependencies":{"net10.0":{"Package":{"dependencies":{"runtime/win-x64":"1.0.0"}}}}}""",
            out bool found,
            out string target);

        Assert.True(valid);
        Assert.True(found);
        Assert.Empty(target);
    }

    [Fact]
    public void EscapedDependenciesPropertyIsRecognized()
    {
        bool valid = Parse(
            """{"depend\u0065ncies":{"net10.0":{}}}""",
            out bool found,
            out string target);

        Assert.True(valid);
        Assert.True(found);
        Assert.Empty(target);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("{\"dependencies\":[]}")]
    [InlineData("{\"dependencies\":{},}")]
    [InlineData("{\"dependencies\":{},\"number\":01}")]
    [InlineData("{\"dependencies\":{},\"text\":\"unterminated}")]
    public void MalformedJsonIsRejected(string json)
    {
        Assert.False(Parse(json, out _, out _));
    }

    [Fact]
    public void MissingDependenciesIsReportedSeparately()
    {
        bool valid = Parse("""{"version":2}""", out bool found, out string target);

        Assert.True(valid);
        Assert.False(found);
        Assert.Empty(target);
    }

    [Fact]
    public void ExcessiveNestingIsRejected()
    {
        const string Prefix = "{\"dependencies\":{\"net10.0\":";
        const string Nested = "{\"nested\":";
        const int Depth = 65;
        const int ClosingBraces = 67;
        string json = string.Create(
            Prefix.Length + (Nested.Length * Depth) + "null".Length + ClosingBraces,
            0,
            static (destination, _) =>
            {
                Prefix.CopyTo(destination);
                int offset = Prefix.Length;
                for (int index = 0; index < Depth; index++)
                {
                    Nested.CopyTo(destination[offset..]);
                    offset += Nested.Length;
                }

                "null".CopyTo(destination[offset..]);
                destination[^ClosingBraces..].Fill('}');
            });

        Assert.False(Parse(json, out _, out _));
    }

    private static bool Parse(string json, out bool found, out string target)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(json);
        var reader = new VerifyLockfiles.LockJsonReader(utf8);
        bool valid = reader.TryRead(out found, out ReadOnlySpan<byte> runtimeTarget);
        target = Encoding.UTF8.GetString(runtimeTarget);
        return valid;
    }
}
