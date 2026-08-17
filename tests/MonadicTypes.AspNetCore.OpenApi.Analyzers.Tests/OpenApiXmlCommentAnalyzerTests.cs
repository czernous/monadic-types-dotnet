using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using MonadicTypes.AspNetCore.OpenApi.Analyzers;

namespace MonadicTypes.AspNetCore.OpenApi.Analyzers.Tests;

public sealed class OpenApiXmlCommentAnalyzerTests
{
    [Fact]
    public async Task DocumentedHandlerWithoutProjectionReportsDiagnostic()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(DocumentedEndpoint);

        Diagnostic diagnostic = Assert.Single(diagnostics, static value =>
            string.Equals(value.Id, OpenApiXmlCommentAnalyzer.DiagnosticId, StringComparison.Ordinal));
        string message = diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(message.Contains("Microsoft.AspNetCore.OpenApi 10.0.10", StringComparison.Ordinal));
        Assert.True(message.Contains("reflection-based", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExplicitMetadataSuppressesDiagnostic()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
            DocumentedEndpoint.Replace(
                "app.MapGet(\"/items\", GetItem);",
                "app.MapGet(\"/items\", GetItem).WithSummary(\"Gets an item.\");",
                StringComparison.Ordinal));

        Assert.DoesNotContain(diagnostics, static value =>
            string.Equals(value.Id, OpenApiXmlCommentAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task MicrosoftGeneratorOutputSuppressesDiagnostic()
    {
        CSharpSyntaxTree generatedTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(
            "namespace Microsoft.AspNetCore.OpenApi.Generated { }",
            path: "OpenApiXmlCommentSupport.generated.cs");
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(DocumentedEndpoint, generatedTree);

        Assert.DoesNotContain(diagnostics, static value =>
            string.Equals(value.Id, OpenApiXmlCommentAnalyzer.DiagnosticId, StringComparison.Ordinal));
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        params CSharpSyntaxTree[] additionalTrees)
    {
        CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            [syntaxTree, .. additionalTrees],
            PlatformReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.DoesNotContain(compilation.GetDiagnostics(), static diagnostic =>
            diagnostic.Severity is DiagnosticSeverity.Error);
        CompilationWithAnalyzers analyzed = compilation.WithAnalyzers(
            [new OpenApiXmlCommentAnalyzer()]);

        return await analyzed.GetAnalyzerDiagnosticsAsync();
    }

    private const string DocumentedEndpoint = """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class Endpoints
{
    /// <summary>Gets an item.</summary>
    public static string GetItem() => "item";

    public static void Map(WebApplication app)
    {
        app.MapGet("/items", GetItem);
    }
}
""";

    private static readonly ImmutableArray<PortableExecutableReference> PlatformReferences =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path))
    ];
}
