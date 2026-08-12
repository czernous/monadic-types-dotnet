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
        CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            PlatformReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValueFunctionGenerator())
            .RunGenerators(compilation);

        return driver.GetRunResult();
    }

    private static readonly ImmutableArray<PortableExecutableReference> PlatformReferences =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(IValueFunction<,>).Assembly.Location))
    ];
}
