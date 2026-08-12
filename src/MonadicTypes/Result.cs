using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

/// <summary>Represents either a successful value or a non-null error as a readonly value type.</summary>
/// <typeparam name="T">Success value type.</typeparam>
/// <typeparam name="E">Error value type.</typeparam>
public readonly record struct Result<T, E> where E : notnull
{
    private const int Uninitialized = 0;
    private const int Success = 1;
    private const int Failure = 2;

    private readonly T? _value;
    private readonly E? _error;
    private readonly int _state;

    /// <summary>Gets whether this value was constructed through <see cref="Ok"/> or <see cref="Fail"/>.</summary>
    public bool IsInitialized => _state != Uninitialized;
    /// <summary>Gets whether this result contains a successful value.</summary>
    public bool IsSuccess => _state == Success;
    /// <summary>Gets whether this result contains an error.</summary>
    public bool IsFailure => _state == Failure;

    /// <summary>Gets the successful value.</summary>
    /// <exception cref="InvalidOperationException">The result is failed or uninitialized.</exception>
    public T Value => _state switch
    {
        Success => _value!,
        Failure => throw new InvalidOperationException("Cannot access Value of a failed Result."),
        _ => throw UninitializedResult()
    };

    /// <summary>Gets the failure error.</summary>
    /// <exception cref="InvalidOperationException">The result is successful or uninitialized.</exception>
    public E Error => _state switch
    {
        Failure => _error!,
        Success => throw new InvalidOperationException("Cannot access Error of a successful Result."),
        _ => throw UninitializedResult()
    };

    private Result(T? value, E? error, int state) =>
        (_value, _error, _state) = (value, error, state);

    /// <summary>Creates a successful result containing <paramref name="value"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Ok(T value) => new(value, default, Success);

    /// <summary>Creates a failed result containing a non-null <paramref name="error"/>.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Fail(E error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        return new(default, error, Failure);
    }

    /// <summary>Attempts to retrieve the successful value and rejects an uninitialized result.</summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        ThrowIfUninitialized();
        value = _value;
        return _state == Success;
    }

    /// <summary>Attempts to retrieve the failure error and rejects an uninitialized result.</summary>
    public bool TryGetError([MaybeNullWhen(false)] out E error)
    {
        ThrowIfUninitialized();
        error = _error;
        return _state == Failure;
    }

    /// <summary>Folds the active case through exactly one branch function.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TR>(Func<T, TR> ok, Func<E, TR> error)
    {
        if (_state == Success)
        {
            return ok(_value!);
        }

        return _state == Failure
            ? error(_error!)
            : ThrowUninitialized<TR>();
    }

    /// <summary>
    /// Folds the active case while passing caller-owned state to non-capturing
    /// branch functions.
    /// </summary>
    /// <typeparam name="TState">Caller state type.</typeparam>
    /// <typeparam name="TR">Folded result type.</typeparam>
    /// <param name="state">State passed unchanged to the selected branch.</param>
    /// <param name="ok">Success branch.</param>
    /// <param name="error">Failure branch.</param>
    /// <returns>The value returned by the selected branch.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TState, TR>(
        TState state,
        Func<T, TState, TR> ok,
        Func<E, TState, TR> error) => _state switch
        {
            Success => ok(_value!, state),
            Failure => error(_error!, state),
            _ => throw UninitializedResult()
        };

    /// <summary>Folds the active case through allocation-free callable values.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TR, TOk, TError>(TOk ok, TError error)
        where TOk : struct, IValueFunction<T, TR>
        where TError : struct, IValueFunction<E, TR> => _state switch
        {
            Success => ok.Invoke(_value!),
            Failure => error.Invoke(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Executes exactly one action for the active case.</summary>
    public void Switch(Action<T> ok, Action<E> error)
    {
        switch (_state)
        {
            case Success:
                ok(_value!);
                return;
            case Failure:
                error(_error!);
                return;
            default:
                throw UninitializedResult();
        }
    }

    /// <summary>Maps a successful value without changing its type and propagates failures unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Map(Func<T, T> map) => _state switch
    {
        Success => Ok(map(_value!)),
        Failure => this,
        _ => throw UninitializedResult()
    };

    /// <summary>Maps a successful value through an allocation-free callable and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Map<TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, T> => _state switch
        {
            Success => Ok(map.Invoke(_value!)),
            Failure => this,
            _ => throw UninitializedResult()
        };

    /// <summary>Maps a successful value through a generated callable wrapper and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)
        where TFunction : struct, IValueFunction<T, TR> => _state switch
        {
            Success => Result<TR, E>.Ok(map.Invoke(_value!)),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Maps a successful value with caller-owned state and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TState, TR>(TState state, Func<T, TState, TR> map) => _state switch
    {
        Success => Result<TR, E>.Ok(map(_value!, state)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    /// <summary>Maps a successful value to another type and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR>(Func<T, TR> map)
    {
        int state = _state;
        if (state == Uninitialized)
        {
            throw UninitializedResult();
        }

        TR? value = state == Success ? map(_value!) : default;
        return new Result<TR, E>(value, _error, state);
    }

    /// <summary>Maps a successful value to another type through an allocation-free callable.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR, TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, TR> => _state switch
        {
            Success => Result<TR, E>.Ok(map.Invoke(_value!)),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Maps the active error to another non-null type and preserves successes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TE> MapError<TE>(Func<E, TE> map) where TE : notnull => _state switch
    {
        Success => Result<T, TE>.Ok(_value!),
        Failure => Result<T, TE>.Fail(map(_error!)),
        _ => throw UninitializedResult()
    };

    /// <summary>Maps the active error with caller-owned state and preserves successes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TE> MapError<TState, TE>(TState state, Func<E, TState, TE> map)
        where TE : notnull => _state switch
        {
            Success => Result<T, TE>.Ok(_value!),
            Failure => Result<T, TE>.Fail(map(_error!, state)),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes a success with another same-shaped result and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Bind(Func<T, Result<T, E>> next) => _state switch
    {
        Success => next(_value!),
        Failure => this,
        _ => throw UninitializedResult()
    };

    /// <summary>Composes a success through an allocation-free same-shaped continuation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Bind<TFunction>(TFunction next)
        where TFunction : struct, IValueFunction<T, Result<T, E>> => _state switch
        {
            Success => next.Invoke(_value!),
            Failure => this,
            _ => throw UninitializedResult()
        };

    /// <summary>Composes a success through a generated callable wrapper and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR, TFunction>(ValueFunction<T, Result<TR, E>, TFunction> next)
        where TFunction : struct, IValueFunction<T, Result<TR, E>> => _state switch
        {
            Success => next.Invoke(_value!),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes a success with caller-owned state and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TState, TR>(TState state, Func<T, TState, Result<TR, E>> next) => _state switch
    {
        Success => next(_value!, state),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    /// <summary>Composes a success with another result and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR>(Func<T, Result<TR, E>> next) => _state switch
    {
        Success => next(_value!),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    /// <summary>Composes a success and maps the continuation's error into this result's error type.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR, TNextError>(
        Func<T, Result<TR, TNextError>> next,
        Func<TNextError, E> mapNextError)
        where TNextError : notnull => _state switch
        {
            Success => next(_value!).MapError(mapNextError),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes with caller-owned state and maps the continuation's error type.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TState, TR, TNextError>(
        TState state,
        Func<T, TState, Result<TR, TNextError>> next,
        Func<TNextError, E> mapNextError)
        where TNextError : notnull => _state switch
        {
            Success => next(_value!, state).MapError(mapNextError),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes the failure case while preserving a successful value.</summary>
    /// <typeparam name="TNextError">Error type returned by the failure continuation.</typeparam>
    /// <param name="next">Continuation invoked only when this result is a failure.</param>
    /// <returns>The unchanged success or the result returned by <paramref name="next"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TNextError> BindError<TNextError>(Func<E, Result<T, TNextError>> next)
        where TNextError : notnull => _state switch
        {
            Success => Result<T, TNextError>.Ok(_value!),
            Failure => next(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes the failure case with caller-owned state.</summary>
    /// <typeparam name="TState">Caller state passed to the continuation.</typeparam>
    /// <typeparam name="TNextError">Error type returned by the failure continuation.</typeparam>
    /// <param name="state">State passed unchanged to <paramref name="next"/>.</param>
    /// <param name="next">Continuation invoked only when this result is a failure.</param>
    /// <returns>The unchanged success or the result returned by <paramref name="next"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TNextError> BindError<TState, TNextError>(
        TState state,
        Func<E, TState, Result<T, TNextError>> next)
        where TNextError : notnull => _state switch
        {
            Success => Result<T, TNextError>.Ok(_value!),
            Failure => next(_error!, state),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes the failure case through an allocation-free callable value.</summary>
    /// <typeparam name="TNextError">Error type returned by the failure continuation.</typeparam>
    /// <typeparam name="TFunction">Callable value type.</typeparam>
    /// <param name="next">Continuation invoked only when this result is a failure.</param>
    /// <returns>The unchanged success or the result returned by <paramref name="next"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TNextError> BindError<TNextError, TFunction>(TFunction next)
        where TNextError : notnull
        where TFunction : struct, IValueFunction<E, Result<T, TNextError>> => _state switch
        {
            Success => Result<T, TNextError>.Ok(_value!),
            Failure => next.Invoke(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Transforms both cases without invoking the inactive branch.</summary>
    /// <typeparam name="TResult">Mapped success type.</typeparam>
    /// <typeparam name="TNextError">Mapped error type.</typeparam>
    /// <param name="mapValue">Success mapping function.</param>
    /// <param name="mapError">Failure mapping function.</param>
    /// <returns>A result containing the mapped active case.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult, TNextError> BiMap<TResult, TNextError>(
        Func<T, TResult> mapValue,
        Func<E, TNextError> mapError)
        where TNextError : notnull => _state switch
        {
            Success => Result<TResult, TNextError>.Ok(mapValue(_value!)),
            Failure => Result<TResult, TNextError>.Fail(mapError(_error!)),
            _ => throw UninitializedResult()
        };

    /// <summary>Composes a success through an allocation-free callable and propagates failures.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR, TFunction>(TFunction next)
        where TFunction : struct, IValueFunction<T, Result<TR, E>> => _state switch
        {
            Success => next.Invoke(_value!),
            Failure => Result<TR, E>.Fail(_error!),
            _ => throw UninitializedResult()
        };

    /// <summary>Recovers a failure through <paramref name="recover"/> and preserves successes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Recover(Func<E, Result<T, E>> recover) => _state switch
    {
        Success => this,
        Failure => recover(_error!),
        _ => throw UninitializedResult()
    };

    /// <summary>Returns the success value or an eagerly supplied fallback.</summary>
    /// <param name="fallback">Value returned for a failure.</param>
    /// <returns>The success value or <paramref name="fallback"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ValueOr(T fallback) => _state switch
    {
        Success => _value!,
        Failure => fallback,
        _ => throw UninitializedResult()
    };

    /// <summary>Returns the success value or lazily maps the active error to a fallback.</summary>
    /// <param name="fallback">Function invoked only for a failure.</param>
    /// <returns>The success value or the fallback value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ValueOrElse(Func<E, T> fallback) => _state switch
    {
        Success => _value!,
        Failure => fallback(_error!),
        _ => throw UninitializedResult()
    };

    /// <summary>Converts a success to failure when <paramref name="predicate"/> is false.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Ensure(Func<T, bool> predicate, Func<T, E> onFailure) => _state switch
    {
        Success when predicate(_value!) => this,
        Success => Fail(onFailure(_value!)),
        Failure => this,
        _ => throw UninitializedResult()
    };

    /// <summary>Validates a success while passing caller-owned state to both callbacks.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Ensure<TState>(
        TState state,
        Func<T, TState, bool> predicate,
        Func<T, TState, E> onFailure) => _state switch
        {
            Success when predicate(_value!, state) => this,
            Success => Fail(onFailure(_value!, state)),
            Failure => this,
            _ => throw UninitializedResult()
        };

    /// <summary>Invokes <paramref name="action"/> only for success and returns this result.</summary>
    public Result<T, E> Tap(Action<T> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action(_value!);
        }

        return this;
    }

    /// <summary>Invokes an allocation-free action only for success and returns this result.</summary>
    public Result<T, E> Tap<TAction>(TAction action)
        where TAction : struct, IValueAction<T>
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action.Invoke(_value!);
        }

        return this;
    }

    /// <summary>Invokes a generated callable action only for success and returns this result.</summary>
    public Result<T, E> Tap<TAction>(ValueAction<T, TAction> action)
        where TAction : struct, IValueAction<T>
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action.Invoke(_value!);
        }

        return this;
    }

    /// <summary>Invokes a success action with caller-owned state and returns this result.</summary>
    public Result<T, E> Tap<TState>(TState state, Action<T, TState> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action(_value!, state);
        }

        return this;
    }

    /// <summary>Asynchronously invokes <paramref name="action"/> only for success.</summary>
    public async ValueTask<Result<T, E>> TapAsync(Func<T, ValueTask> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            await action(_value!).ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>Invokes <paramref name="action"/> only for failure and returns this result.</summary>
    public Result<T, E> TapError(Action<E> action)
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action(_error!);
        }

        return this;
    }

    /// <summary>Invokes an allocation-free action only for failure and returns this result.</summary>
    public Result<T, E> TapError<TAction>(TAction action)
        where TAction : struct, IValueAction<E>
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action.Invoke(_error!);
        }

        return this;
    }

    /// <summary>Invokes a generated callable action only for failure and returns this result.</summary>
    public Result<T, E> TapError<TAction>(ValueAction<E, TAction> action)
        where TAction : struct, IValueAction<E>
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action.Invoke(_error!);
        }

        return this;
    }

    /// <summary>Invokes a failure action with caller-owned state and returns this result.</summary>
    public Result<T, E> TapError<TState>(TState state, Action<E, TState> action)
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action(_error!, state);
        }

        return this;
    }

    /// <summary>Invokes a synchronous finalizer for either initialized case and returns this result.</summary>
    public Result<T, E> Finally<TState>(TState state, Action<TState> action)
    {
        ThrowIfUninitialized();
        action(state);
        return this;
    }

    /// <summary>Invokes an asynchronous finalizer for either initialized case and returns this result.</summary>
    public async ValueTask<Result<T, E>> FinallyAsync<TState>(TState state, Func<TState, ValueTask> action)
    {
        ThrowIfUninitialized();
        await action(state).ConfigureAwait(false);
        return this;
    }

    /// <summary>Returns <c>Ok(value)</c>, <c>Fail(error)</c>, or <c>Uninitialized</c>.</summary>
    public override string ToString() => _state switch
    {
        Success => $"Ok({_value})",
        Failure => $"Fail({_error})",
        _ => "Uninitialized"
    };

    /// <summary>Converts a success value into a successful result.</summary>
    public static implicit operator Result<T, E>(T value) => Ok(value);
    /// <summary>Converts an error into a failed result.</summary>
    public static implicit operator Result<T, E>(E error) => Fail(error);

    private void ThrowIfUninitialized()
    {
        if (_state == Uninitialized)
        {
            throw UninitializedResult();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static InvalidOperationException UninitializedResult() =>
        new("A default Result is uninitialized. Construct it with Ok or Fail before use.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TR ThrowUninitialized<TR>() => throw UninitializedResult();

}
