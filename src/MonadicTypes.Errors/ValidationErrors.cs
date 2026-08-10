using System.Collections;

namespace MonadicTypes;

/// <summary>
/// Owns one or more validation issues. Allocation is confined to the failure
/// path; successful <see cref="Result{T, E}"/> values do not construct it.
/// </summary>
public sealed class ValidationErrors : IReadOnlyList<ValidationIssue>
{
    private readonly ValidationIssue[] _issues;

    public ValidationErrors(IEnumerable<ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        _issues = [.. issues];
        if (_issues.Length == 0)
        {
            throw new ArgumentException("At least one validation issue is required.", nameof(issues));
        }
    }

    public ValidationErrors(params ValidationIssue[] issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        if (issues.Length == 0)
        {
            throw new ArgumentException("At least one validation issue is required.", nameof(issues));
        }

        _issues = [.. issues];
    }

    /// <summary>Maps a third-party validation list without coupling to its assembly.</summary>
    public static ValidationErrors Create<TFailure>(
        IReadOnlyList<TFailure> failures,
        Func<TFailure, ValidationIssue> map)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentNullException.ThrowIfNull(map);
        return CreateCore(failures, map);
    }

    /// <summary>Maps a third-party validation list with caller-owned state.</summary>
    public static ValidationErrors Create<TFailure, TState>(
        IReadOnlyList<TFailure> failures,
        TState state,
        Func<TFailure, TState, ValidationIssue> map)
    {
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentNullException.ThrowIfNull(map);
        if (failures.Count == 0)
        {
            throw new ArgumentException("At least one validation failure is required.", nameof(failures));
        }

        ValidationIssue[] issues = new ValidationIssue[failures.Count];
        for (int index = 0; index < issues.Length; index++)
        {
            issues[index] = map(failures[index], state);
        }

        return new ValidationErrors(issues, takeOwnership: true);
    }

    /// <summary>Maps a third-party validation list through an inlineable value function.</summary>
    public static ValidationErrors Create<TFailure, TMapper>(
        IReadOnlyList<TFailure> failures,
        TMapper map)
        where TMapper : struct, IValueFunction<TFailure, ValidationIssue>
    {
        ArgumentNullException.ThrowIfNull(failures);
        if (failures.Count == 0)
        {
            throw new ArgumentException("At least one validation failure is required.", nameof(failures));
        }

        ValidationIssue[] issues = new ValidationIssue[failures.Count];
        for (int index = 0; index < issues.Length; index++)
        {
            issues[index] = map.Invoke(failures[index]);
        }

        return new ValidationErrors(issues, takeOwnership: true);
    }

    public int Count => _issues.Length;
    public ValidationIssue this[int index] => _issues[index];
    public ReadOnlySpan<ValidationIssue> AsSpan() => _issues;
    public IEnumerator<ValidationIssue> GetEnumerator() =>
        ((IEnumerable<ValidationIssue>)_issues).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _issues.GetEnumerator();

    private ValidationErrors(ValidationIssue[] issues, bool takeOwnership) =>
        _issues = takeOwnership ? issues : [.. issues];

    private static ValidationErrors CreateCore<TFailure>(
        IReadOnlyList<TFailure> failures,
        Func<TFailure, ValidationIssue> map)
    {
        if (failures.Count == 0)
        {
            throw new ArgumentException("At least one validation failure is required.", nameof(failures));
        }

        ValidationIssue[] issues = new ValidationIssue[failures.Count];
        for (int index = 0; index < issues.Length; index++)
        {
            issues[index] = map(failures[index]);
        }

        return new ValidationErrors(issues, takeOwnership: true);
    }
}
