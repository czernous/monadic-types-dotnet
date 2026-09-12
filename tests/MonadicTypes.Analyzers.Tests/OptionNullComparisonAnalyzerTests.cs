using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using MonadicTypes;
using MonadicTypes.Analyzers;

namespace MonadicTypes.Analyzers.Tests;

public sealed class OptionNullComparisonAnalyzerTests
{
    [Fact]
    public async Task ReportsBothNullComparisonDirections()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static bool CheckValue(Option<int> option) => option == null || null != option;
    static bool CheckReference(Option<string> option) => option != null || null == option;
}
""", new OptionNullComparisonAnalyzer());

        Assert.Equal(4, diagnostics.Count(static diagnostic =>
            string.Equals(diagnostic.Id, OptionNullComparisonAnalyzer.DiagnosticId, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task LeavesExplicitOptionChecksAndNullableValuesAlone()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static bool Check(Option<int> option, int? value) => option.IsNone || option.IsSome || value == null;
}
""", new OptionNullComparisonAnalyzer());

        Assert.DoesNotContain(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, OptionNullComparisonAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportsMapThatCreatesNestedResult()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Result<Result<int, string>, string> Check(Result<int, string> result) =>
        result.Map(static value => Result<int, string>.Ok(value + 1));
}
""", new NestedRailwayAnalyzer());

        Assert.Contains(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task LeavesBindAndFlatMapValuesAlone()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Result<int, string> Check(Result<int, string> result) =>
        result.Bind(static value => Result<int, string>.Ok(value + 1));
}
""", new NestedRailwayAnalyzer());

        Assert.DoesNotContain(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportsNestedOptionProjection()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Option<Option<int>> Check(Option<int> option) =>
        option.Map(static value => Option<int>.Some(value + 1));
}
""", new NestedRailwayAnalyzer());

        Assert.Contains(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportsNestedResultCombinationProjection()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Result<Result<int, string>, string> Check(
        Result<int, string> first,
        Result<int, string> second) =>
        ResultCombination.Map(first, second, static (left, right) =>
            Result<int, string>.Ok(left + right));
}
""", new NestedRailwayAnalyzer());

        Assert.Contains(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportsNestedResultErrorProjection()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Result<int, Result<string, string>> Check(Result<int, string> result) =>
        result.MapError(static error => Result<string, string>.Ok(error));
}
""", new NestedRailwayAnalyzer());

        Assert.Contains(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task LeavesBindErrorAlone()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
using MonadicTypes;
static class Sample
{
    static Result<int, string> Check(Result<int, string> result) =>
        result.BindError(static _ => Result<int, string>.Ok(42));
}
""", new NestedRailwayAnalyzer());

        Assert.DoesNotContain(diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, NestedRailwayAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        DiagnosticAnalyzer analyzer)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            [CSharpSyntaxTree.ParseText(source)],
            PlatformReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(compilation.GetDiagnostics(), static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        CompilationWithAnalyzers analyzed = compilation.WithAnalyzers([analyzer]);
        return await analyzed.GetAnalyzerDiagnosticsAsync();
    }

    private static readonly ImmutableArray<PortableExecutableReference> PlatformReferences =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path)),
        MetadataReference.CreateFromFile(typeof(Option<>).Assembly.Location)
    ];
}
