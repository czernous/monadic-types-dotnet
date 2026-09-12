using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MonadicTypes.Analyzers;

/// <summary>Reports a direct Map projection that creates another Result or Option.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedRailwayAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for nested railway projections.</summary>
    public const string DiagnosticId = "MT0002";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Use Bind for a railway-returning projection",
        "{0} creates nested {1}; use {2} when the projection already returns a railway value",
        "MonadicTypes.Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A Map or MapError projection that returns Result or Option usually indicates a missing Bind or BindError. Suppress MT0002 when nested values are intentional.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        string methodName = invocation.TargetMethod.Name;
        bool isValueMap = string.Equals(methodName, "Map", StringComparison.Ordinal)
            && IsMapOwner(invocation.TargetMethod.ContainingType);
        bool isErrorMap = string.Equals(methodName, "MapError", StringComparison.Ordinal)
            && IsResultType(invocation.TargetMethod.ContainingType);
        if (!isValueMap && !isErrorMap)
        {
            return;
        }

        if (invocation.Type is not INamedTypeSymbol outer
            || outer.TypeArguments.Length <= (isErrorMap ? 1 : 0)
            || outer.TypeArguments[isErrorMap ? 1 : 0] is not INamedTypeSymbol inner
            || !IsMonadicType(inner))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            methodName,
            inner.Name,
            isErrorMap ? "BindError" : "Bind"));
    }

    private static bool IsMonadicType(INamedTypeSymbol type) =>
        type.Name is "Result" or "Option"
        && type.Arity is 1 or 2
        && string.Equals(type.ContainingNamespace.ToDisplayString(), "MonadicTypes", StringComparison.Ordinal);

    private static bool IsMapOwner(INamedTypeSymbol type) =>
        IsMonadicType(type)
        || (string.Equals(type.Name, "ResultCombination", StringComparison.Ordinal)
            && type.Arity == 0
            && string.Equals(type.ContainingNamespace.ToDisplayString(), "MonadicTypes", StringComparison.Ordinal));

    private static bool IsResultType(INamedTypeSymbol type) =>
        string.Equals(type.Name, "Result", StringComparison.Ordinal)
        && type.Arity == 2
        && string.Equals(type.ContainingNamespace.ToDisplayString(), "MonadicTypes", StringComparison.Ordinal);
}
