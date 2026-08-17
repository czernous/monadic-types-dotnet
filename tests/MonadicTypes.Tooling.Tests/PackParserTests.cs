namespace MonadicTypes.Tooling.Tests;

public sealed class PackParserTests
{
    [Theory]
    [InlineData("0.1.0")]
    [InlineData("1.2.3-preview.1")]
    [InlineData("1.2.3-ci.42")]
    public void SemanticVersionAcceptsValidVersions(string version) =>
        Assert.True(SemanticVersion.IsValid(version));

    [Theory]
    [InlineData("")]
    [InlineData("1.2")]
    [InlineData("01.2.3")]
    [InlineData("1.02.3")]
    [InlineData("1.2.03")]
    [InlineData("1.2.3-01")]
    [InlineData("1.2.3-alpha..1")]
    [InlineData("1.2.3+")]
    [InlineData("1.2.3+build")]
    [InlineData("1.2.3+build..1")]
    [InlineData("1.2.3+build+other")]
    public void SemanticVersionRejectsInvalidVersions(string version) =>
        Assert.False(SemanticVersion.IsValid(version));

    [Fact]
    public void XmlAttributeMatchesACompleteNameOnly()
    {
        ReadOnlySpan<byte> tag = " packageid=\"wrong\" id = 'correct' excluded=\"wrong\""u8;

        Assert.True(Pack.ReadXmlAttribute(tag, "id"u8).SequenceEqual("correct"u8));
    }

    [Fact]
    public void XmlAttributeRejectsMissingEquals()
    {
        ReadOnlySpan<byte> tag = " id value=\"wrong\""u8;

        Assert.True(Pack.ReadXmlAttribute(tag, "id"u8).IsEmpty);
    }

    [Theory]
    [InlineData("Analyzers")]
    [InlineData("build; analyzers;compile")]
    [InlineData("BUILD,ANALYZERS")]
    public void AnalyzerExclusionAcceptsCompleteTokens(string value) =>
        Assert.True(Pack.ContainsAnalyzersIgnoreCase(System.Text.Encoding.ASCII.GetBytes(value)));

    [Theory]
    [InlineData("NotAnalyzers")]
    [InlineData("AnalyzersSuffix")]
    [InlineData("build;notanalyzers;compile")]
    public void AnalyzerExclusionRejectsSubstrings(string value) =>
        Assert.False(Pack.ContainsAnalyzersIgnoreCase(System.Text.Encoding.ASCII.GetBytes(value)));
}
