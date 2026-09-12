using System.Collections.Immutable;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MonadicTypes.Analyzers;

/// <summary>Reports null comparisons whose operands are <c>Option&lt;T&gt;</c> values.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionNullComparisonAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for an ambiguous Option null comparison.</summary>
    public const string DiagnosticId = "MT0001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Do not compare Option<T> with null",
        "Use IsNone or IsSome when checking Option<T>; null comparison changes meaning with T",
        "MonadicTypes.Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Option<T> is a value type and its implicit value conversion makes null comparisons ambiguous.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeComparison, SyntaxKind.EqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeComparison, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var comparison = (BinaryExpressionSyntax)context.Node;
        ExpressionSyntax? optionOperand = IsNull(comparison.Left)
            ? comparison.Right
            : IsNull(comparison.Right) ? comparison.Left : null;

        if (optionOperand is null
            || !IsOption(context.SemanticModel.GetTypeInfo(optionOperand, context.CancellationToken).Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, comparison.GetLocation()));
    }

    private static bool IsNull(ExpressionSyntax expression) => expression.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool IsOption(ITypeSymbol? type) => type is INamedTypeSymbol named
        && string.Equals(named.Name, "Option", StringComparison.Ordinal)
        && named.Arity == 1
        && string.Equals(named.ContainingNamespace.ToDisplayString(), "MonadicTypes", StringComparison.Ordinal);
}
