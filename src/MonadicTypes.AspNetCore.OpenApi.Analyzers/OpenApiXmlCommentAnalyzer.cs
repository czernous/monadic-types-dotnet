using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MonadicTypes.AspNetCore.OpenApi.Analyzers;

/// <summary>
/// Reports documented Minimal API handlers whose XML comments are not projected
/// by either explicit metadata or Microsoft's opt-in XML comment generator.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OpenApiXmlCommentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for an unprojected endpoint XML comment.</summary>
    public const string DiagnosticId = "MTAPI001";

    private static readonly DiagnosticDescriptor MissingProjection = new(
        DiagnosticId,
        "Endpoint XML comments are not projected to OpenAPI",
        "XML comments on endpoint handler '{0}' are not projected by the reflection-free profile; "
        + "add explicit OpenAPI metadata or directly reference Microsoft.AspNetCore.OpenApi 10.0.10 "
        + "to opt into its reflection-based XML transformer",
        "MonadicTypes.OpenApi",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "MonadicTypes does not silently activate a reflection-based XML comment transformer.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    private static readonly ImmutableHashSet<string> RouteMethods =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "MapDelete",
            "MapGet",
            "MapMethods",
            "MapPatch",
            "MapPost",
            "MapPut");

    private static readonly ImmutableHashSet<string> ExplicitDocumentationMethods =
        ImmutableHashSet.Create(
            StringComparer.Ordinal,
            "WithDescription",
            "WithSummary");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MissingProjection];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            ConcurrentBag<Candidate> candidates = [];
            compilationContext.RegisterSyntaxNodeAction(
                syntaxContext => AnalyzeInvocation(syntaxContext, candidates),
                SyntaxKind.InvocationExpression);
            compilationContext.RegisterCompilationEndAction(endContext =>
                ReportCandidates(endContext, candidates));
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        ConcurrentBag<Candidate> candidates)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: var methodName }
            || !RouteMethods.Contains(methodName)
            || GetMethodSymbol(context.SemanticModel.GetSymbolInfo(
                invocation,
                context.CancellationToken)) is not { } routeMethod
            || !routeMethod.ContainingNamespace.ToDisplayString()
                .StartsWith("Microsoft.AspNetCore.Builder", StringComparison.Ordinal)
            || HasExplicitDocumentation(invocation))
        {
            return;
        }

        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count == 0
            || GetMethodSymbol(context.SemanticModel.GetSymbolInfo(
                arguments[arguments.Count - 1].Expression,
                context.CancellationToken)) is not { } handler
            || string.IsNullOrWhiteSpace(handler.GetDocumentationCommentXml(
                cancellationToken: context.CancellationToken)))
        {
            return;
        }

        candidates.Add(new Candidate(handler.Name, arguments[arguments.Count - 1].GetLocation()));
    }

    private static IMethodSymbol? GetMethodSymbol(SymbolInfo symbolInfo) =>
        symbolInfo.Symbol as IMethodSymbol
        ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

    private static bool HasExplicitDocumentation(InvocationExpressionSyntax routeInvocation)
    {
        for (SyntaxNode? current = routeInvocation.Parent;
             current is not null and not ExpressionStatementSyntax;
             current = current.Parent)
        {
            if (current is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: var methodName }
                }
                && ExplicitDocumentationMethods.Contains(methodName))
            {
                return true;
            }
        }

        return false;
    }

    private static void ReportCandidates(
        CompilationAnalysisContext context,
        ConcurrentBag<Candidate> candidates)
    {
        if (candidates.IsEmpty || HasMicrosoftXmlGenerator(context.Compilation))
        {
            return;
        }

        foreach (Candidate candidate in candidates)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MissingProjection,
                candidate.Location,
                candidate.HandlerName));
        }
    }

    private static bool HasMicrosoftXmlGenerator(Compilation compilation) =>
        compilation.SyntaxTrees.Any(static tree => tree.FilePath.EndsWith(
            "OpenApiXmlCommentSupport.generated.cs",
            StringComparison.OrdinalIgnoreCase));

    private readonly struct Candidate(string handlerName, Location location)
    {
        internal string HandlerName { get; } = handlerName;
        internal Location Location { get; } = location;
    }
}
