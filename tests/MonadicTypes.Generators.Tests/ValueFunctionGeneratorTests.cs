using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MonadicTypes.Generators;

namespace MonadicTypes.Generators.Tests;

public class ValueFunctionGeneratorTests
{
    [Fact]
    public void ValidMethod_GeneratesCallableToken()
    {
        const string source = """
namespace Consumer;

public static partial class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public static long Widen(int value) => value;
}
""";

        GeneratorDriverRunResult result = Run(source);

        Assert.Empty(result.Diagnostics);
        Assert.Contains(
            result.GeneratedTrees,
            static tree => tree.ToString().Contains("ValueFunction<int, long", StringComparison.Ordinal));
    }

    [Fact]
    public void VoidMethod_GeneratesCallableActionToken()
    {
        const string source = """
namespace Consumer;

public static partial class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public static void Observe(int value) { }
}
""";

        GeneratorDriverRunResult result = Run(source);

        Assert.Empty(result.Diagnostics);
        Assert.Contains(
            result.GeneratedTrees,
            static tree => tree.ToString().Contains("ValueAction<int", StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedPublicContractIncludesXmlDocumentation()
    {
        const string source = """
namespace Consumer;

public static partial class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public static long Widen(int value) => value;
}
""";

        GeneratorDriverRunResult result = Run(source);
        ImmutableArray<GeneratedSourceResult> sources =
        [
            .. result.Results.SelectMany(static generator => generator.GeneratedSources)
        ];
        string attribute = Assert.Single(
            sources,
            static generated => string.Equals(
                generated.HintName,
                "GenerateValueFunctionAttribute.g.cs",
                StringComparison.Ordinal)).SourceText.ToString();
        string callable = Assert.Single(
            sources,
            static generated => generated.HintName.EndsWith(
                ".ValueFunctions.g.cs",
                StringComparison.Ordinal)).SourceText.ToString();

        Assert.Contains("/// <summary>Requests an allocation-free callable wrapper", attribute, StringComparison.Ordinal);
        Assert.Contains("/// <example><code>[GenerateValueFunction]", attribute, StringComparison.Ordinal);
        Assert.Contains("/// <summary>Gets the generated callable token", callable, StringComparison.Ordinal);
        Assert.Contains(
            "/// <example><code>var result = Operations.Functions.Widen.Invoke(value);</code></example>",
            callable,
            StringComparison.Ordinal);
        Assert.DoesNotContain("language=", attribute, StringComparison.Ordinal);
        Assert.DoesNotContain("language=", callable, StringComparison.Ordinal);
        Assert.Contains("/// <summary>Forwards the input to", callable, StringComparison.Ordinal);
        Assert.Contains("/// <param name=\"value\">Input value.</param>", callable, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedExamplesAreAvailableThroughRoslynSymbolDocumentation()
    {
        const string source = """
namespace Consumer;

public static partial class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public static long Widen(int value) => value;
}
""";

        CSharpCompilation compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValueFunctionGenerator());
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        INamedTypeSymbol operations = output.GetTypeByMetadataName("Consumer.Operations")!;
        INamedTypeSymbol functions = Assert.Single(operations.GetTypeMembers("Functions"));
        ISymbol callable = Assert.Single(functions.GetMembers("Widen"));
        INamedTypeSymbol attribute = output.GetTypeByMetadataName(
            "MonadicTypes.GenerateValueFunctionAttribute")!;

        string callableDocumentation = callable.GetDocumentationCommentXml() ?? string.Empty;
        string attributeDocumentation = attribute.GetDocumentationCommentXml() ?? string.Empty;
        Assert.Contains("<example>", callableDocumentation, StringComparison.Ordinal);
        Assert.Contains("<code>", callableDocumentation, StringComparison.Ordinal);
        Assert.Contains("Operations.Functions.Widen.Invoke(value)", callableDocumentation, StringComparison.Ordinal);
        Assert.DoesNotContain("language=", callableDocumentation, StringComparison.Ordinal);
        Assert.Contains("<example>", attributeDocumentation, StringComparison.Ordinal);
        Assert.Contains("<code>", attributeDocumentation, StringComparison.Ordinal);
        Assert.DoesNotContain("language=", attributeDocumentation, StringComparison.Ordinal);
    }

    [Fact]
    public void InstanceMethod_ReportsMethodDiagnostic()
    {
        const string source = """
namespace Consumer;

public partial class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public long Widen(int value) => value;
}
""";

        GeneratorDriverRunResult result = Run(source);

        Assert.Contains(result.Diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, "MTGEN001", StringComparison.Ordinal));
    }

    [Fact]
    public void NonPartialType_ReportsContainingTypeDiagnostic()
    {
        const string source = """
namespace Consumer;

public static class Operations
{
    [MonadicTypes.GenerateValueFunction]
    public static long Widen(int value) => value;
}
""";

        GeneratorDriverRunResult result = Run(source);

        Assert.Contains(result.Diagnostics, static diagnostic =>
            string.Equals(diagnostic.Id, "MTGEN002", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateGeneratedName_ReportsCollisionDiagnostic()
    {
        const string source = """
namespace Consumer;

public static partial class Operations
{
    [MonadicTypes.GenerateValueFunction("Convert")]
    public static long Widen(int value) => value;

    [MonadicTypes.GenerateValueFunction("Convert")]
    public static int Increment(int value) => value;
}
""";

        GeneratorDriverRunResult result = Run(source);

        Assert.Equal(2, result.Diagnostics.Count(static diagnostic =>
            string.Equals(diagnostic.Id, "MTGEN004", StringComparison.Ordinal)));
    }

    private static GeneratorDriverRunResult Run(string source)
    {
        CSharpCompilation compilation = CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValueFunctionGenerator())
            .RunGenerators(compilation);

        return driver.GetRunResult();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source);
        return CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            PlatformReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static readonly ImmutableArray<PortableExecutableReference> PlatformReferences =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(IValueFunction<,>).Assembly.Location))
    ];
}
