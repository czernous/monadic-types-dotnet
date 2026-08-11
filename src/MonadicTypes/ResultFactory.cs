using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Factories for results whose success case carries no data.</summary>
public static class Result
{
    /// <summary>Creates a successful unit result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, E> Ok<E>() where E : notnull =>
        Result<Unit, E>.Ok(Unit.Value);

    /// <summary>Creates a failed unit result containing <paramref name="error"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, E> Fail<E>(E error) where E : notnull =>
        Result<Unit, E>.Fail(error);
}
