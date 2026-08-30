using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MonadicTypes.Generators;

/// <summary>Generates allocation-free value-function adapters for attributed static methods.</summary>
/// <example>
/// <code>
/// [GenerateValueFunction]
/// public static int GetId(User value) =&gt; value.Id;
/// </code>
/// </example>
[Generator(LanguageNames.CSharp)]
public sealed class ValueFunctionGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName = "MonadicTypes.GenerateValueFunctionAttribute";

    private static readonly DiagnosticDescriptor InvalidMethod = new(
        "MTGEN001",
        "Method cannot be adapted as a value function",
        "Method '{0}' must be an implemented, non-generic static method with one by-value parameter",
        "MonadicTypes.Generators",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidContainingType = new(
        "MTGEN002",
        "Containing type cannot host generated value functions",
        "Type '{0}' must be a non-generic, top-level, static partial class",
        "MonadicTypes.Generators",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidGeneratedName = new(
        "MTGEN003",
        "Generated value-function name is invalid",
        "Generated name '{0}' must be a valid C# identifier",
        "MonadicTypes.Generators",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateGeneratedName = new(
        "MTGEN004",
        "Generated value-function name is duplicated",
        "Type '{0}' contains more than one generated value function named '{1}'",
        "MonadicTypes.Generators",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Creates the generator instance used by the Roslyn compiler host.</summary>
    /// <remarks>Application code does not construct the generator; the compiler host discovers it through <see cref="GeneratorAttribute"/>.</remarks>
    public ValueFunctionGenerator()
    {
    }

    /// <summary>Registers attribute emission, method discovery, validation, and adapter generation.</summary>
    /// <remarks>The Roslyn compiler host invokes this method. Application code uses the emitted attribute and callable members instead.</remarks>
    /// <param name="context">The incremental generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static output =>
            output.AddSource("GenerateValueFunctionAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        IncrementalValuesProvider<MethodCandidate> methods = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeMetadataName,
            static (node, _) => node is MethodDeclarationSyntax,
            static (syntaxContext, _) => CreateCandidate(syntaxContext));

        context.RegisterSourceOutput(methods.Collect(), static (output, candidates) => Execute(output, candidates));
    }

    private static MethodCandidate CreateCandidate(GeneratorAttributeSyntaxContext context)
    {
        var method = (IMethodSymbol)context.TargetSymbol;
        string generatedName = method.Name;
        AttributeData attribute = context.Attributes[0];
        if (attribute.ConstructorArguments.Length == 1
            && attribute.ConstructorArguments[0].Value is string requestedName)
        {
            generatedName = requestedName;
        }

        return new MethodCandidate(method, generatedName, context.TargetNode.GetLocation());
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<MethodCandidate> candidates)
    {
        foreach (var typeGroup in candidates.GroupBy(
                     static candidate => candidate.Method.ContainingType,
                     SymbolEqualityComparer.Default))
        {
            if (typeGroup.Key is not INamedTypeSymbol containingType)
            {
                continue;
            }

            MethodCandidate[] validCandidates =
            [
                .. typeGroup.Where(candidate => Validate(context, candidate))
            ];

            foreach (IGrouping<string, MethodCandidate> duplicate in validCandidates.GroupBy(
                         static candidate => candidate.GeneratedName,
                         StringComparer.Ordinal))
            {
                if (duplicate.Skip(1).Any())
                {
                    foreach (MethodCandidate candidate in duplicate)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DuplicateGeneratedName,
                            candidate.Location,
                            containingType.Name,
                            duplicate.Key));
                    }
                }
            }

            MethodCandidate[] uniqueCandidates =
            [
                .. validCandidates
                    .GroupBy(static candidate => candidate.GeneratedName, StringComparer.Ordinal)
                    .Where(static group => !group.Skip(1).Any())
                    .Select(static group => group.First())
            ];

            if (uniqueCandidates.Length != 0)
            {
                Emit(context, containingType, uniqueCandidates);
            }
        }
    }

    private static bool Validate(SourceProductionContext context, MethodCandidate candidate)
    {
        IMethodSymbol method = candidate.Method;
        bool methodIsValid = method.IsStatic
            && !method.IsGenericMethod
            && method.Parameters.Length == 1
            && method.Parameters[0].RefKind == RefKind.None
            && !method.IsAbstract
            && !method.IsExtern;

        if (!methodIsValid)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidMethod, candidate.Location, method.Name));
            return false;
        }

        INamedTypeSymbol containingType = method.ContainingType;
        bool isPartial = containingType.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(static declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
        bool typeIsValid = containingType.IsStatic
            && containingType.TypeParameters.Length == 0
            && containingType.ContainingType is null
            && isPartial;

        if (!typeIsValid)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidContainingType,
                candidate.Location,
                containingType.Name));
            return false;
        }

        if (!SyntaxFacts.IsValidIdentifier(candidate.GeneratedName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidGeneratedName,
                candidate.Location,
                candidate.GeneratedName));
            return false;
        }

        return true;
    }

    private static void Emit(
        SourceProductionContext context,
        INamedTypeSymbol containingType,
        IReadOnlyList<MethodCandidate> candidates)
    {
        string namespaceName = containingType.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        string typeAccessibility = AccessibilityText(containingType.DeclaredAccessibility);
        var source = new StringBuilder("// <auto-generated/>\n#nullable enable\n");

        if (namespaceName.Length != 0)
        {
            source.Append("namespace ").Append(namespaceName).Append(";\n\n");
        }

        source.Append(typeAccessibility).Append(" static partial class ")
            .Append(Escape(containingType.Name)).Append("\n{\n")
            .Append("    ").Append(typeAccessibility).Append(" static partial class Functions\n    {\n");

        foreach (MethodCandidate candidate in candidates)
        {
            AppendProperty(source, candidate);
        }

        source.Append("    }\n\n");

        foreach (MethodCandidate candidate in candidates)
        {
            AppendAdapter(source, candidate);
        }

        source.Append("}\n");
        string hintName = Sanitize(namespaceName + "." + containingType.Name) + ".ValueFunctions.g.cs";
        context.AddSource(hintName, SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static void AppendProperty(StringBuilder source, MethodCandidate candidate)
    {
        string accessibility = AccessibilityText(candidate.Method.DeclaredAccessibility);
        string input = candidate.Method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string output = candidate.Method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string adapter = AdapterName(candidate);

        source.Append("        /// <summary>Gets the generated callable token for <c>")
            .Append(candidate.Method.Name)
            .Append("</c>.</summary>\n")
            .Append("        /// <example><code>");
        if (!candidate.Method.ReturnsVoid)
        {
            source.Append("var result = ");
        }

        source.Append(Escape(candidate.Method.ContainingType.Name))
            .Append(".Functions.")
            .Append(Escape(candidate.GeneratedName))
            .Append(".Invoke(")
            .Append(Escape(candidate.Method.Parameters[0].Name))
            .Append(");</code></example>\n")
            .Append("        ")
            .Append(accessibility)
            .Append(" static global::MonadicTypes.");
        if (candidate.Method.ReturnsVoid)
        {
            source.Append("ValueAction<").Append(input).Append(", ").Append(adapter).Append("> ");
        }
        else
        {
            source.Append("ValueFunction<").Append(input).Append(", ").Append(output).Append(", ")
                .Append(adapter).Append("> ");
        }

        source.Append(Escape(candidate.GeneratedName)).Append(" => default;\n");
    }

    private static void AppendAdapter(StringBuilder source, MethodCandidate candidate)
    {
        string accessibility = AccessibilityText(candidate.Method.DeclaredAccessibility);
        string input = candidate.Method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string output = candidate.Method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string adapter = AdapterName(candidate);
        string method = Escape(candidate.Method.Name);

        source.Append("    /// <summary>Forwards the input to <c>")
            .Append(candidate.Method.Name)
            .Append("</c> without delegate dispatch.</summary>\n    ")
            .Append(accessibility)
            .Append(" readonly struct ")
            .Append(adapter)
            .Append(" : global::MonadicTypes.");
        if (candidate.Method.ReturnsVoid)
        {
            source.Append("IValueAction<").Append(input).Append(">\n")
                .Append("    {\n")
                .Append("        /// <summary>Forwards <paramref name=\"value\"/> to the attributed method.</summary>\n")
                .Append("        /// <param name=\"value\">Input value.</param>\n")
                .Append("        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n")
                .Append("        public void Invoke(").Append(input).Append(" value) => ");
        }
        else
        {
            source.Append("IValueFunction<").Append(input).Append(", ").Append(output).Append(">\n")
                .Append("    {\n")
                .Append("        /// <summary>Forwards <paramref name=\"value\"/> to the attributed method.</summary>\n")
                .Append("        /// <param name=\"value\">Input value.</param>\n")
                .Append("        /// <returns>The attributed method's output.</returns>\n")
                .Append("        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n")
                .Append("        public ").Append(output).Append(" Invoke(").Append(input).Append(" value) => ");
        }

        source.Append(method).Append("(value);\n")
            .Append("    }\n\n");
    }

    private static string AdapterName(MethodCandidate candidate) =>
        "__" + candidate.GeneratedName + "ValueFunction";

    private static string AccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        Accessibility.Private => "private",
        _ => "internal"
    };

    private static string Escape(string identifier) =>
        SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None ? identifier : "@" + identifier;

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character == '_' ? character : '_');
        }

        return builder.ToString();
    }

    private const string AttributeSource = """
// <auto-generated/>
#nullable enable
namespace MonadicTypes;

/// <summary>Requests an allocation-free callable wrapper for an eligible static method.</summary>
/// <remarks>The original method remains directly callable. The generated token is exposed through the containing type's <c>Functions</c> class.</remarks>
/// <example><code>[GenerateValueFunction] public static long Widen(int value) =&gt; value;</code></example>
[global::System.Diagnostics.Conditional("MONADIC_TYPES_GENERATOR_ATTRIBUTES")]
[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class GenerateValueFunctionAttribute : global::System.Attribute
{
    /// <summary>Uses the attributed method name for the generated callable property.</summary>
    public GenerateValueFunctionAttribute()
    {
    }

    /// <summary>Uses <paramref name="name"/> for the generated callable property.</summary>
    /// <param name="name">Valid C# identifier for the generated property.</param>
    public GenerateValueFunctionAttribute(string name) => Name = name;

    /// <summary>Gets the requested generated property name, or null to use the method name.</summary>
    public string? Name { get; }
}
""";

    private sealed class MethodCandidate(
        IMethodSymbol method,
        string generatedName,
        Location location)
    {
        public IMethodSymbol Method { get; } = method;
        public string GeneratedName { get; } = generatedName;
        public Location Location { get; } = location;
    }
}
