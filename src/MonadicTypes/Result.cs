using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MonadicTypes;

public readonly record struct Result<T, E> where E : notnull
{
    private const int Uninitialized = 0;
    private const int Success = 1;
    private const int Failure = 2;

    private readonly T? _value;
    private readonly E? _error;
    private readonly int _state;

    public bool IsInitialized => _state != Uninitialized;
    public bool IsSuccess => _state == Success;
    public bool IsFailure => _state == Failure;

    public T Value => _state switch
    {
        Success => _value!,
        Failure => throw new InvalidOperationException("Cannot access Value of a failed Result."),
        _ => throw UninitializedResult()
    };

    public E Error => _state switch
    {
        Failure => _error!,
        Success => throw new InvalidOperationException("Cannot access Error of a successful Result."),
        _ => throw UninitializedResult()
    };

    private Result(T? value, E? error, int state) =>
        (_value, _error, _state) = (value, error, state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Ok(T value) => new(value, default, Success);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Fail(E error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        return new(default, error, Failure);
    }

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        ThrowIfUninitialized();
        value = _value;
        return _state == Success;
    }

    public bool TryGetError([MaybeNullWhen(false)] out E error)
    {
        ThrowIfUninitialized();
        error = _error;
        return _state == Failure;
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TR Match<TR, TOk, TError>(TOk ok, TError error)
        where TOk : struct, IValueFunction<T, TR>
        where TError : struct, IValueFunction<E, TR> => _state switch
    {
        Success => ok.Invoke(_value!),
        Failure => error.Invoke(_error!),
        _ => throw UninitializedResult()
    };

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Map(Func<T, T> map) => _state switch
    {
        Success => Ok(map(_value!)),
        Failure => this,
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Map<TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, T> => _state switch
    {
        Success => Ok(map.Invoke(_value!)),
        Failure => this,
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR, TFunction>(ValueFunction<T, TR, TFunction> map)
        where TFunction : struct, IValueFunction<T, TR> => _state switch
    {
        Success => Result<TR, E>.Ok(map.Invoke(_value!)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TState, TR>(TState state, Func<T, TState, TR> map) => _state switch
    {
        Success => Result<TR, E>.Ok(map(_value!, state)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR>(Func<T, TR> map) => _state switch
    {
        Success => Result<TR, E>.Ok(map(_value!)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Map<TR, TFunction>(TFunction map)
        where TFunction : struct, IValueFunction<T, TR> => _state switch
    {
        Success => Result<TR, E>.Ok(map.Invoke(_value!)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TE> MapError<TE>(Func<E, TE> map) where TE : notnull => _state switch
    {
        Success => Result<T, TE>.Ok(_value!),
        Failure => Result<T, TE>.Fail(map(_error!)),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TE> MapError<TState, TE>(TState state, Func<E, TState, TE> map)
        where TE : notnull => _state switch
    {
        Success => Result<T, TE>.Ok(_value!),
        Failure => Result<T, TE>.Fail(map(_error!, state)),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Bind(Func<T, Result<T, E>> next) => _state switch
    {
        Success => next(_value!),
        Failure => this,
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Bind<TFunction>(TFunction next)
        where TFunction : struct, IValueFunction<T, Result<T, E>> => _state switch
    {
        Success => next.Invoke(_value!),
        Failure => this,
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR, TFunction>(ValueFunction<T, Result<TR, E>, TFunction> next)
        where TFunction : struct, IValueFunction<T, Result<TR, E>> => _state switch
    {
        Success => next.Invoke(_value!),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TState, TR>(TState state, Func<T, TState, Result<TR, E>> next) => _state switch
    {
        Success => next(_value!, state),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR>(Func<T, Result<TR, E>> next) => _state switch
    {
        Success => next(_value!),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> BindError<TR, TNextError>(Func<T, Result<TR, TNextError>> next)
        where TNextError : notnull, IErrorConvertible<E> => _state switch
    {
        Success => ConvertNextError(next(_value!)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> BindError<TState, TR, TNextError>(
        TState state,
        Func<T, TState, Result<TR, TNextError>> next)
        where TNextError : notnull, IErrorConvertible<E> => _state switch
    {
        Success => ConvertNextError(next(_value!, state)),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TR, E> Bind<TR, TFunction>(TFunction next)
        where TFunction : struct, IValueFunction<T, Result<TR, E>> => _state switch
    {
        Success => next.Invoke(_value!),
        Failure => Result<TR, E>.Fail(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result<TR, E>> Bind<TR>(Func<T, ValueTask<Result<TR, E>>> next) => _state switch
    {
        Success => next(_value!),
        Failure => ValueTask.FromResult(Result<TR, E>.Fail(_error!)),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Recover(Func<E, Result<T, E>> recover) => _state switch
    {
        Success => this,
        Failure => recover(_error!),
        _ => throw UninitializedResult()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> Ensure(Func<T, bool> predicate, Func<T, E> onFailure) => _state switch
    {
        Success when predicate(_value!) => this,
        Success => Fail(onFailure(_value!)),
        Failure => this,
        _ => throw UninitializedResult()
    };

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

    public Result<T, E> Tap(Action<T> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action(_value!);
        }

        return this;
    }

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

    public Result<T, E> Tap<TState>(TState state, Action<T, TState> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            action(_value!, state);
        }

        return this;
    }

    public async ValueTask<Result<T, E>> Tap(Func<T, ValueTask> action)
    {
        ThrowIfUninitialized();
        if (_state == Success)
        {
            await action(_value!).ConfigureAwait(false);
        }

        return this;
    }

    public Result<T, E> TapError(Action<E> action)
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action(_error!);
        }

        return this;
    }

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

    public Result<T, E> TapError<TState>(TState state, Action<E, TState> action)
    {
        ThrowIfUninitialized();
        if (_state == Failure)
        {
            action(_error!, state);
        }

        return this;
    }

    public Result<T, E> Finally<TState>(TState state, Action<TState> action)
    {
        ThrowIfUninitialized();
        action(state);
        return this;
    }

    public async ValueTask<Result<T, E>> Finally<TState>(TState state, Func<TState, ValueTask> action)
    {
        ThrowIfUninitialized();
        await action(state).ConfigureAwait(false);
        return this;
    }

    public override string ToString() => _state switch
    {
        Success => $"Ok({_value})",
        Failure => $"Fail({_error})",
        _ => "Uninitialized"
    };

    public static implicit operator Result<T, E>(T value) => Ok(value);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Result<TR, E> ConvertNextError<TR, TNextError>(Result<TR, TNextError> result)
        where TNextError : notnull, IErrorConvertible<E> => result.IsSuccess
            ? Result<TR, E>.Ok(result.Value)
            : Result<TR, E>.Fail(result.Error.ToError());
}
