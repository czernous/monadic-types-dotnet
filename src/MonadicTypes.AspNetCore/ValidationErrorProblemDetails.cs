using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Converts validation issues to strongly typed validation problem results.</summary>
public static class ValidationErrorProblemDetails
{
    /// <summary>Groups validation issues by path and preserves their machine-readable codes.</summary>
    /// <param name="validationErrors">The validation issues to convert.</param>
    /// <param name="httpContext">An optional context supplying a fallback trace identifier.</param>
    /// <returns>A strongly typed validation problem result.</returns>
    public static ValidationProblem ToHttpResult(
        ValidationErrors validationErrors,
        HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        ReadOnlySpan<ValidationIssue> issues = validationErrors.AsSpan();
        Dictionary<string, string[]> messages = new(issues.Length, StringComparer.Ordinal);
        Dictionary<string, string[]> codes = new(issues.Length, StringComparer.Ordinal);
        for (int index = 0; index < issues.Length; index++)
        {
            ValidationIssue first = issues[index];
            if (messages.ContainsKey(first.Path))
            {
                continue;
            }

            int count = 1;
            for (int candidate = index + 1; candidate < issues.Length; candidate++)
            {
                count += string.Equals(first.Path, issues[candidate].Path, StringComparison.Ordinal) ? 1 : 0;
            }

            string[] pathMessages = new string[count];
            string[] pathCodes = new string[count];
            int destination = 0;
            for (int candidate = index; candidate < issues.Length; candidate++)
            {
                ValidationIssue issue = issues[candidate];
                if (!string.Equals(first.Path, issue.Path, StringComparison.Ordinal))
                {
                    continue;
                }

                pathMessages[destination] = issue.Message;
                pathCodes[destination] = issue.Code;
                destination++;
            }

            messages.Add(first.Path, pathMessages);
            codes.Add(first.Path, pathCodes);
        }

        Dictionary<string, object?> extensions = new(2, StringComparer.Ordinal)
        {
            ["codes"] = codes
        };

        string? traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (traceId is not null)
        {
            extensions["traceId"] = traceId;
        }

        return TypedResults.ValidationProblem(
            messages,
            title: "Validation failed",
            type: "urn:problem-type:validation",
            extensions: extensions);
    }
}
