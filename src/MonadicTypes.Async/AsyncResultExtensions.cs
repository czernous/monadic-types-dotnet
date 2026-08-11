using System.Runtime.CompilerServices;

namespace MonadicTypes.Async;

/// <summary>
/// Lifts synchronous and asynchronous result combinators over <see cref="Task{TResult}"/>
/// and <see cref="ValueTask{TResult}"/> receivers. Every operator consumes its source once
/// and converges on <see cref="ValueTask{TResult}"/> for continued composition.
/// </summary>
public static class AsyncResultExtensions
{
    extension<T, TError>(in Result<T, TError> result) where TError : notnull
    {
        /// <summary>Maps a successful value through an asynchronous callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>An awaitable result containing the mapped value or original failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> MapAsync<TResult>(Func<T, ValueTask<TResult>> map) =>
            MapResultAsync(result, map);

        /// <summary>Maps a successful value through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>An awaitable result containing the mapped value or original failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> MapAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, ValueTask<TResult>> =>
            MapResultAsync(result, map);

        /// <summary>Maps a successful value through a Task-returning callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>An awaitable result containing the mapped value or original failure.</returns>
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult>(Func<T, Task<TResult>> map) =>
            MapResultTaskAsync(result, map);

        /// <summary>Maps a successful value through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>An awaitable result containing the mapped value or original failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, Task<TResult>> =>
            MapResultTaskAsync(result, map);

        /// <summary>Binds a successful value through an asynchronous continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>The asynchronous continuation result or original failure.</returns>
        public ValueTask<Result<TResult, TError>> BindAsync<TResult>(
            Func<T, ValueTask<Result<TResult, TError>>> bind) => BindResultAsync(result, bind);

        /// <summary>Binds a successful value through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>The asynchronous continuation result or original failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> BindAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>> =>
            BindResultAsync(result, bind);

        /// <summary>Binds a successful value through a Task-returning continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>The asynchronous continuation result or original failure.</returns>
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult>(
            Func<T, Task<Result<TResult, TError>>> bind) => BindResultTaskAsync(result, bind);

        /// <summary>Binds a successful value through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>The asynchronous continuation result or original failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>> =>
            BindResultTaskAsync(result, bind);

        /// <summary>Binds a failure through an asynchronous continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>The unchanged success or asynchronous failure continuation result.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError>(
            Func<TError, ValueTask<Result<T, TNextError>>> bind)
            where TNextError : notnull => BindErrorResultAsync(result, bind);

        /// <summary>Binds a failure through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>The unchanged success or asynchronous failure continuation result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError, TFunction>(
            ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>> =>
            BindErrorResultAsync(result, bind);

        /// <summary>Binds a failure through a Task-returning continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>The unchanged success or asynchronous failure continuation result.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError>(
            Func<TError, Task<Result<T, TNextError>>> bind)
            where TNextError : notnull => BindErrorResultTaskAsync(result, bind);

        /// <summary>Binds a failure through a generated Task-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>The unchanged success or asynchronous failure continuation result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError, TFunction>(
            ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>> =>
            BindErrorResultTaskAsync(result, bind);
    }

    extension<T, TError>(in ValueTask<Result<T, TError>> source) where TError : notnull
    {
        /// <summary>Maps a completed or pending result through a synchronous callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the mapped result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Result<TResult, TError>> Map<TResult>(Func<T, TResult> map) =>
            MapSource(source, map);

        /// <summary>Binds a completed or pending result through a synchronous continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> Bind<TResult>(Func<T, Result<TResult, TError>> bind) =>
            BindSource(source, bind);

        /// <summary>Binds a completed or pending failure through a synchronous continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A single-consumption awaitable containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindError<TNextError>(
            Func<TError, Result<T, TNextError>> bind)
            where TNextError : notnull => BindErrorSource(source, bind);

        /// <summary>Maps a completed or pending result through an asynchronous callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapAsync<TResult>(Func<T, ValueTask<TResult>> map) =>
            MapSourceAsync(source, map);

        /// <summary>Maps a completed or pending result through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, ValueTask<TResult>> =>
            MapSourceAsync(source, map);

        /// <summary>Maps a completed or pending result through a Task-returning callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult>(Func<T, Task<TResult>> map) =>
            MapSourceTaskAsync(source, map);

        /// <summary>Maps a completed or pending result through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, Task<TResult>> =>
            MapSourceTaskAsync(source, map);

        /// <summary>Binds a completed or pending result through an asynchronous continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindAsync<TResult>(
            Func<T, ValueTask<Result<TResult, TError>>> bind) => BindSourceAsync(source, bind);

        /// <summary>Binds a completed or pending result through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>> =>
            BindSourceAsync(source, bind);

        /// <summary>Binds a completed or pending result through a Task-returning continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult>(
            Func<T, Task<Result<TResult, TError>>> bind) => BindSourceTaskAsync(source, bind);

        /// <summary>Binds a completed or pending result through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>A single-consumption awaitable containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>> =>
            BindSourceTaskAsync(source, bind);

        /// <summary>Binds a completed or pending failure through an asynchronous continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A single-consumption awaitable containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError>(
            Func<TError, ValueTask<Result<T, TNextError>>> bind)
            where TNextError : notnull => BindErrorSourceAsync(source, bind);

        /// <summary>Binds a completed or pending failure through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>A single-consumption awaitable containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError, TFunction>(
            ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>> =>
            BindErrorSourceAsync(source, bind);

        /// <summary>Binds a completed or pending failure through a Task-returning continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A single-consumption awaitable containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError>(
            Func<TError, Task<Result<T, TNextError>>> bind)
            where TNextError : notnull => BindErrorSourceTaskAsync(source, bind);

        /// <summary>Binds a completed or pending failure through a generated Task-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>A single-consumption awaitable containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError, TFunction>(
            ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>> =>
            BindErrorSourceTaskAsync(source, bind);
    }

    extension<T, TError>(Task<Result<T, TError>> source) where TError : notnull
    {
        /// <summary>Maps a Task-backed result through a synchronous callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> Map<TResult>(Func<T, TResult> map) =>
            MapSource(new ValueTask<Result<T, TError>>(source), map);

        /// <summary>Binds a Task-backed result through a synchronous continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> Bind<TResult>(Func<T, Result<TResult, TError>> bind) =>
            BindSource(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed failure through a synchronous continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A ValueTask-backed pipeline containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindError<TNextError>(
            Func<TError, Result<T, TNextError>> bind)
            where TNextError : notnull =>
            BindErrorSource(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Maps a Task-backed result through an asynchronous callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapAsync<TResult>(Func<T, ValueTask<TResult>> map) =>
            MapSourceAsync(new ValueTask<Result<T, TError>>(source), map);

        /// <summary>Maps a Task-backed result through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, ValueTask<TResult>> =>
            MapSourceAsync(new ValueTask<Result<T, TError>>(source), map);

        /// <summary>Maps a Task-backed result through a Task-returning callback.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <param name="map">Callback invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult>(Func<T, Task<TResult>> map) =>
            MapSourceTaskAsync(new ValueTask<Result<T, TError>>(source), map);

        /// <summary>Maps a Task-backed result through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Mapped success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="map">Allocation-free callable token invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the mapped result.</returns>
        public ValueTask<Result<TResult, TError>> MapTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<TResult>, TFunction> map)
            where TFunction : struct, IValueFunction<T, Task<TResult>> =>
            MapSourceTaskAsync(new ValueTask<Result<T, TError>>(source), map);

        /// <summary>Binds a Task-backed result through an asynchronous continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindAsync<TResult>(
            Func<T, ValueTask<Result<TResult, TError>>> bind) =>
            BindSourceAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed result through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindAsync<TResult, TFunction>(
            ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>> =>
            BindSourceAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed result through a Task-returning continuation.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <param name="bind">Continuation invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult>(
            Func<T, Task<Result<TResult, TError>>> bind) =>
            BindSourceTaskAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed result through a generated Task-returning callable.</summary>
        /// <typeparam name="TResult">Continuation success type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for success.</param>
        /// <returns>A ValueTask-backed pipeline containing the continuation result.</returns>
        public ValueTask<Result<TResult, TError>> BindTaskAsync<TResult, TFunction>(
            ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
            where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>> =>
            BindSourceTaskAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed failure through an asynchronous continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A ValueTask-backed pipeline containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError>(
            Func<TError, ValueTask<Result<T, TNextError>>> bind)
            where TNextError : notnull =>
            BindErrorSourceAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed failure through a generated ValueTask-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>A ValueTask-backed pipeline containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorAsync<TNextError, TFunction>(
            ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>> =>
            BindErrorSourceAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed failure through a Task-returning continuation.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <param name="bind">Continuation invoked only for failure.</param>
        /// <returns>A ValueTask-backed pipeline containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError>(
            Func<TError, Task<Result<T, TNextError>>> bind)
            where TNextError : notnull =>
            BindErrorSourceTaskAsync(new ValueTask<Result<T, TError>>(source), bind);

        /// <summary>Binds a Task-backed failure through a generated Task-returning callable.</summary>
        /// <typeparam name="TNextError">Continuation error type.</typeparam>
        /// <typeparam name="TFunction">Generated callable adapter type.</typeparam>
        /// <param name="bind">Allocation-free callable token invoked only for failure.</param>
        /// <returns>A ValueTask-backed pipeline containing the resulting value.</returns>
        public ValueTask<Result<T, TNextError>> BindErrorTaskAsync<TNextError, TFunction>(
            ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
            where TNextError : notnull
            where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>> =>
            BindErrorSourceTaskAsync(new ValueTask<Result<T, TError>>(source), bind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapResultAsync<T, TResult, TError>(
        in Result<T, TError> result,
        Func<T, ValueTask<TResult>> map)
        where TError : notnull
    {
        if (result.IsFailure)
        {
            return ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));
        }

        return CompleteMap<TResult, TError>(map(result.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapResultTaskAsync<T, TResult, TError>(
        in Result<T, TError> result,
        Func<T, Task<TResult>> map)
        where TError : notnull
    {
        if (result.IsFailure)
        {
            return ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));
        }

        return CompleteMap<TResult, TError>(new ValueTask<TResult>(map(result.Value)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> CompleteMap<TResult, TError>(
        in ValueTask<TResult> pending)
        where TError : notnull => pending.IsCompletedSuccessfully
            ? ValueTask.FromResult(Result<TResult, TError>.Ok(pending.Result))
            : AwaitMap<TResult, TError>(pending);

    private static async ValueTask<Result<TResult, TError>> AwaitMap<TResult, TError>(ValueTask<TResult> pending)
        where TError : notnull => Result<TResult, TError>.Ok(await pending.ConfigureAwait(false));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindResultAsync<T, TResult, TError>(
        in Result<T, TError> result,
        Func<T, ValueTask<Result<TResult, TError>>> bind)
        where TError : notnull => result.IsSuccess
            ? bind(result.Value)
            : ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindResultTaskAsync<T, TResult, TError>(
        in Result<T, TError> result,
        Func<T, Task<Result<TResult, TError>>> bind)
        where TError : notnull => result.IsSuccess
            ? new ValueTask<Result<TResult, TError>>(bind(result.Value))
            : ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorResultAsync<T, TError, TNextError>(
        in Result<T, TError> result,
        Func<TError, ValueTask<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull => result.IsSuccess
            ? ValueTask.FromResult(Result<T, TNextError>.Ok(result.Value))
            : bind(result.Error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorResultTaskAsync<T, TError, TNextError>(
        in Result<T, TError> result,
        Func<TError, Task<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull => result.IsSuccess
            ? ValueTask.FromResult(Result<T, TNextError>.Ok(result.Value))
            : new ValueTask<Result<T, TNextError>>(bind(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapResultAsync<T, TResult, TError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<T, ValueTask<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<TResult>> => result.IsFailure
            ? ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error))
            : CompleteMap<TResult, TError>(map.Invoke(result.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapResultTaskAsync<T, TResult, TError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<T, Task<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<TResult>> => result.IsFailure
            ? ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error))
            : CompleteMap<TResult, TError>(new ValueTask<TResult>(map.Invoke(result.Value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindResultAsync<T, TResult, TError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>> => result.IsSuccess
            ? bind.Invoke(result.Value)
            : ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindResultTaskAsync<T, TResult, TError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>> => result.IsSuccess
            ? new ValueTask<Result<TResult, TError>>(bind.Invoke(result.Value))
            : ValueTask.FromResult(Result<TResult, TError>.Fail(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorResultAsync<T, TError, TNextError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>> => result.IsSuccess
            ? ValueTask.FromResult(Result<T, TNextError>.Ok(result.Value))
            : bind.Invoke(result.Error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorResultTaskAsync<T, TError, TNextError, TFunction>(
        in Result<T, TError> result,
        ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>> => result.IsSuccess
            ? ValueTask.FromResult(Result<T, TNextError>.Ok(result.Value))
            : new ValueTask<Result<T, TNextError>>(bind.Invoke(result.Error));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapSource<T, TResult, TError>(
        in ValueTask<Result<T, TError>> source,
        Func<T, TResult> map)
        where TError : notnull => source.IsCompletedSuccessfully
            ? ValueTask.FromResult(source.Result.Map(map))
            : AwaitMapSource(source, map);

    private static async ValueTask<Result<TResult, TError>> AwaitMapSource<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, TResult> map)
        where TError : notnull => (await source.ConfigureAwait(false)).Map(map);

    private static ValueTask<Result<TResult, TError>> BindSource<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Result<TResult, TError>> bind)
        where TError : notnull => source.IsCompletedSuccessfully
            ? ValueTask.FromResult(source.Result.Bind(bind))
            : AwaitBindSource(source, bind);

    private static async ValueTask<Result<TResult, TError>> AwaitBindSource<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Result<TResult, TError>> bind)
        where TError : notnull => (await source.ConfigureAwait(false)).Bind(bind);

    private static ValueTask<Result<T, TNextError>> BindErrorSource<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, Result<T, TNextError>> bind)
        where TError : notnull
        where TNextError : notnull => source.IsCompletedSuccessfully
            ? ValueTask.FromResult(source.Result.BindError(bind))
            : AwaitBindErrorSource(source, bind);

    private static async ValueTask<Result<T, TNextError>> AwaitBindErrorSource<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, Result<T, TNextError>> bind)
        where TError : notnull
        where TNextError : notnull => (await source.ConfigureAwait(false)).BindError(bind);

    private static ValueTask<Result<TResult, TError>> MapSourceAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, ValueTask<TResult>> map)
        where TError : notnull => source.IsCompletedSuccessfully
            ? MapResultAsync(source.Result, map)
            : AwaitMapSourceAsync(source, map);

    private static async ValueTask<Result<TResult, TError>> AwaitMapSourceAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, ValueTask<TResult>> map)
        where TError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TResult, TError>.Ok(await map(result.Value).ConfigureAwait(false))
            : Result<TResult, TError>.Fail(result.Error);
    }

    private static ValueTask<Result<TResult, TError>> MapSourceTaskAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Task<TResult>> map)
        where TError : notnull => source.IsCompletedSuccessfully
            ? MapResultTaskAsync(source.Result, map)
            : AwaitMapSourceTaskAsync(source, map);

    private static async ValueTask<Result<TResult, TError>> AwaitMapSourceTaskAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Task<TResult>> map)
        where TError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TResult, TError>.Ok(await map(result.Value).ConfigureAwait(false))
            : Result<TResult, TError>.Fail(result.Error);
    }

    private static ValueTask<Result<TResult, TError>> BindSourceAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, ValueTask<Result<TResult, TError>>> bind)
        where TError : notnull => source.IsCompletedSuccessfully
            ? BindResultAsync(source.Result, bind)
            : AwaitBindSourceAsync(source, bind);

    private static async ValueTask<Result<TResult, TError>> AwaitBindSourceAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, ValueTask<Result<TResult, TError>>> bind)
        where TError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<TResult, TError>.Fail(result.Error);
    }

    private static ValueTask<Result<TResult, TError>> BindSourceTaskAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Task<Result<TResult, TError>>> bind)
        where TError : notnull => source.IsCompletedSuccessfully
            ? BindResultTaskAsync(source.Result, bind)
            : AwaitBindSourceTaskAsync(source, bind);

    private static async ValueTask<Result<TResult, TError>> AwaitBindSourceTaskAsync<T, TResult, TError>(
        ValueTask<Result<T, TError>> source,
        Func<T, Task<Result<TResult, TError>>> bind)
        where TError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<TResult, TError>.Fail(result.Error);
    }

    private static ValueTask<Result<T, TNextError>> BindErrorSourceAsync<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, ValueTask<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull => source.IsCompletedSuccessfully
            ? BindErrorResultAsync(source.Result, bind)
            : AwaitBindErrorSourceAsync(source, bind);

    private static async ValueTask<Result<T, TNextError>> AwaitBindErrorSourceAsync<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, ValueTask<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<T, TNextError>.Ok(result.Value)
            : await bind(result.Error).ConfigureAwait(false);
    }

    private static ValueTask<Result<T, TNextError>> BindErrorSourceTaskAsync<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, Task<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull => source.IsCompletedSuccessfully
            ? BindErrorResultTaskAsync(source.Result, bind)
            : AwaitBindErrorSourceTaskAsync(source, bind);

    private static async ValueTask<Result<T, TNextError>> AwaitBindErrorSourceTaskAsync<T, TError, TNextError>(
        ValueTask<Result<T, TError>> source,
        Func<TError, Task<Result<T, TNextError>>> bind)
        where TError : notnull
        where TNextError : notnull
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<T, TNextError>.Ok(result.Value)
            : await bind(result.Error).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapSourceAsync<T, TResult, TError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<T, ValueTask<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<TResult>> => source.IsCompletedSuccessfully
            ? MapResultAsync(source.Result, map)
            : AwaitMapSourceAsync(source, map);

    private static async ValueTask<Result<TResult, TError>> AwaitMapSourceAsync<T, TResult, TError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<T, ValueTask<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<TResult>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TResult, TError>.Ok(await map.Invoke(result.Value).ConfigureAwait(false))
            : Result<TResult, TError>.Fail(result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> MapSourceTaskAsync<T, TResult, TError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<T, Task<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<TResult>> => source.IsCompletedSuccessfully
            ? MapResultTaskAsync(source.Result, map)
            : AwaitMapSourceTaskAsync(source, map);

    private static async ValueTask<Result<TResult, TError>> AwaitMapSourceTaskAsync<T, TResult, TError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<T, Task<TResult>, TFunction> map)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<TResult>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TResult, TError>.Ok(await map.Invoke(result.Value).ConfigureAwait(false))
            : Result<TResult, TError>.Fail(result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindSourceAsync<T, TResult, TError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>> =>
        source.IsCompletedSuccessfully
            ? BindResultAsync(source.Result, bind)
            : AwaitBindSourceAsync(source, bind);

    private static async ValueTask<Result<TResult, TError>> AwaitBindSourceAsync<T, TResult, TError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<T, ValueTask<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, ValueTask<Result<TResult, TError>>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? await bind.Invoke(result.Value).ConfigureAwait(false)
            : Result<TResult, TError>.Fail(result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<TResult, TError>> BindSourceTaskAsync<T, TResult, TError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>> =>
        source.IsCompletedSuccessfully
            ? BindResultTaskAsync(source.Result, bind)
            : AwaitBindSourceTaskAsync(source, bind);

    private static async ValueTask<Result<TResult, TError>> AwaitBindSourceTaskAsync<T, TResult, TError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<T, Task<Result<TResult, TError>>, TFunction> bind)
        where TError : notnull
        where TFunction : struct, IValueFunction<T, Task<Result<TResult, TError>>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? await bind.Invoke(result.Value).ConfigureAwait(false)
            : Result<TResult, TError>.Fail(result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorSourceAsync<T, TError, TNextError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>> =>
        source.IsCompletedSuccessfully
            ? BindErrorResultAsync(source.Result, bind)
            : AwaitBindErrorSourceAsync(source, bind);

    private static async ValueTask<Result<T, TNextError>> AwaitBindErrorSourceAsync<T, TError, TNextError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<TError, ValueTask<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, ValueTask<Result<T, TNextError>>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<T, TNextError>.Ok(result.Value)
            : await bind.Invoke(result.Error).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValueTask<Result<T, TNextError>> BindErrorSourceTaskAsync<T, TError, TNextError, TFunction>(
        in ValueTask<Result<T, TError>> source,
        ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>> =>
        source.IsCompletedSuccessfully
            ? BindErrorResultTaskAsync(source.Result, bind)
            : AwaitBindErrorSourceTaskAsync(source, bind);

    private static async ValueTask<Result<T, TNextError>> AwaitBindErrorSourceTaskAsync<T, TError, TNextError, TFunction>(
        ValueTask<Result<T, TError>> source,
        ValueFunction<TError, Task<Result<T, TNextError>>, TFunction> bind)
        where TError : notnull
        where TNextError : notnull
        where TFunction : struct, IValueFunction<TError, Task<Result<T, TNextError>>>
    {
        Result<T, TError> result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<T, TNextError>.Ok(result.Value)
            : await bind.Invoke(result.Error).ConfigureAwait(false);
    }
}
