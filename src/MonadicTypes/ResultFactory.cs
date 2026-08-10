using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Factories for results whose success case carries no data.</summary>
public static class Result
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, E> Ok<E>() where E : notnull =>
        Result<Unit, E>.Ok(Unit.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, E> Fail<E>(E error) where E : notnull =>
        Result<Unit, E>.Fail(error);
}
