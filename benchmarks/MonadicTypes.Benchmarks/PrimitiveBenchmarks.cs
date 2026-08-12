using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

[SimpleJob(
    RuntimeMoniker.NativeAot10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 10,
    invocationCount: 100_000_000)]
[MemoryDiagnoser]
public class PrimitiveBenchmarks
{
    private Result<int, BenchmarkError> _successful;
    private Option<int> _present;
    private LegacyValueResult<int, BenchmarkError> _legacySuccessful;
    private Error _error = null!;
    private Result<int, Error> _richErrorSuccessful;
    private ErrorMetrics _errorMetrics;
    private Meter _meter = null!;
    private string _message = null!;
    private int _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = 42;
        _successful = Result<int, BenchmarkError>.Ok(_value);
        _present = Option<int>.Some(_value);
        _legacySuccessful = LegacyValueResult<int, BenchmarkError>.Success(_value);
        _message = "The supplied value is invalid.";
        _error = Error.Validation("INVALID_VALUE", _message);
        _richErrorSuccessful = Result<int, Error>.Ok(_value);
        _meter = new Meter("Benchmarks.Primitives");
        _errorMetrics = new ErrorMetrics(_meter);
    }

    [GlobalCleanup]
    public void Cleanup() => _meter.Dispose();

    [Benchmark]
    public int DirectTransform() => Transform(_value);

    [Benchmark]
    public Result<int, BenchmarkError> ConstructSuccess() => Result<int, BenchmarkError>.Ok(_value);

    [Benchmark]
    public Result<int, BenchmarkError> ConstructFailure() =>
        Result<int, BenchmarkError>.Fail(new BenchmarkError(_value));

    [Benchmark]
    public Result<int, BenchmarkError> MapResult() => _successful.Map(Transform);

    [Benchmark]
    public Result<long, BenchmarkError> MapResultToOtherType() => _successful.Map(TransformToLong);

    [Benchmark]
    public Result<int, BenchmarkError> BindResult() => _successful.Bind(BindTransform);

    [Benchmark(Baseline = true)]
    public int MatchResult() => _successful.Match(Transform, ErrorCode);

    [Benchmark]
    public int MatchResultWithState() => _successful.Match(
        1,
        static (value, state) => value + state,
        static (error, state) => error.Code + state);

    [Benchmark]
    public Option<int> ConstructSome() => Option<int>.Some(_value);

    [Benchmark]
    public Option<int> MapOption() => _present.Map(Transform);

    [Benchmark]
    public Option<int> StructCallableMapOption() =>
        _present.Map<int, IncrementCallable>(default(IncrementCallable));

    [Benchmark]
    public int DirectResultBranch() => _successful.IsSuccess
        ? Transform(_successful.Value)
        : ErrorCode(_successful.Error);

    [Benchmark]
    public LegacyValueResult<int, BenchmarkError> LegacyConstructSuccess() =>
        LegacyValueResult<int, BenchmarkError>.Success(_value);

    [Benchmark]
    public LegacyValueResult<int, BenchmarkError> LegacyConstructFailure() =>
        LegacyValueResult<int, BenchmarkError>.Failure(new BenchmarkError(_value));

    [Benchmark]
    public LegacyValueResult<int, BenchmarkError> LegacyMap() =>
        _legacySuccessful.Map(Transform);

    [Benchmark]
    public LegacyValueResult<int, BenchmarkError> LegacyBind() =>
        _legacySuccessful.Bind(LegacyBindTransform);

    [Benchmark]
    public int LegacyMatch() => _legacySuccessful.Match(Transform, ErrorCode);

    [Benchmark]
    public Result<int, BenchmarkError> StructCallableMap() =>
        _successful.Map(default(IncrementCallable));

    [Benchmark]
    public Result<int, BenchmarkError> StructCallableBind() =>
        _successful.Bind(default(IncrementResultCallable));

    [Benchmark]
    public int StructCallableMatch() =>
        _successful.Match<int, IncrementCallable, ErrorCodeCallable>(default, default);

    [Benchmark]
    public unsafe Result<int, BenchmarkError> FunctionPointerMap() =>
        MapWithPointer(_successful, &Transform);

    [Benchmark]
    public Error ConstructStructuredError() => Error.Validation("INVALID_VALUE", _message);

    [Benchmark]
    public Result<int, Error> ConstructRichErrorSuccess() => Result<int, Error>.Ok(_value);

    [Benchmark]
    public Result<int, Error> StructCallableMapRichError() =>
        _richErrorSuccessful.Map(default(IncrementCallable));

    [Benchmark]
    public string ReadErrorCode() => _error.Code;

    [Benchmark]
    public void RecordErrorWithoutActivity() => ErrorTelemetry.Record(null, _error);

    [Benchmark]
    public void RecordErrorWithoutMetricsListener() => _errorMetrics.Record(_error);

    [Benchmark]
    public bool FormatErrorToSpan()
    {
        Span<char> destination = stackalloc char[64];
        return _error.TryFormat(destination, out _, default, null);
    }

    private static int Transform(int value) => value + 1;

    private static long TransformToLong(int value) => value + 1L;

    private static int ErrorCode(BenchmarkError error) => error.Code;

    private static Result<int, BenchmarkError> BindTransform(int value) =>
        Result<int, BenchmarkError>.Ok(value + 1);

    private static LegacyValueResult<int, BenchmarkError> LegacyBindTransform(int value) =>
        LegacyValueResult<int, BenchmarkError>.Success(value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Result<int, BenchmarkError> MapWithPointer(
        in Result<int, BenchmarkError> result,
        delegate* managed<int, int> map)
    {
        if (result.TryGetValue(out int value))
        {
            return Result<int, BenchmarkError>.Ok(map(value));
        }

        return Result<int, BenchmarkError>.Fail(result.Error);
    }

    public readonly record struct BenchmarkError(int Code);

    public readonly struct IncrementCallable : IValueFunction<int, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Invoke(int value) => value + 1;
    }

    public readonly struct IncrementResultCallable : IValueFunction<int, Result<int, BenchmarkError>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<int, BenchmarkError> Invoke(int value) =>
            Result<int, BenchmarkError>.Ok(value + 1);
    }

    public readonly struct ErrorCodeCallable : IValueFunction<BenchmarkError, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Invoke(BenchmarkError error) => error.Code;
    }

    // Compact unmanaged comparison shape used as a same-job control.
    public readonly record struct LegacyValueResult<T, TError>
        where T : unmanaged
        where TError : unmanaged
    {
        private readonly T _value;
        private readonly TError _error;

        private LegacyValueResult(LegacyState kind, T value, TError error)
        {
            Kind = kind;
            _value = value;
            _error = error;
        }

        public LegacyState Kind { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LegacyValueResult<T, TError> Success(T value) =>
            new(LegacyState.Success, value, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LegacyValueResult<T, TError> Failure(TError error) =>
            new(LegacyState.Failure, default, error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Match<TResult>(Func<T, TResult> success, Func<TError, TResult> failure) => Kind switch
        {
            LegacyState.Success => success(_value),
            LegacyState.Failure => failure(_error),
            _ => throw new InvalidOperationException("Result is uninitialized.")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LegacyValueResult<T, TError> Map(Func<T, T> map) =>
            Kind == LegacyState.Success ? Success(map(_value)) : this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LegacyValueResult<T, TError> Bind(Func<T, LegacyValueResult<T, TError>> bind) =>
            Kind == LegacyState.Success ? bind(_value) : this;
    }

    public enum LegacyState
    {
        Uninitialized,
        Success,
        Failure
    }
}
