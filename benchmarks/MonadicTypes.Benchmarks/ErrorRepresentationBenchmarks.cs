using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MonadicTypes;

namespace Benchmarks;

[SimpleJob(
    RuntimeMoniker.NativeAot10_0,
    launchCount: 1,
    warmupCount: 3,
    iterationCount: 10)]
[IterationTime(250)]
[MemoryDiagnoser]
public class ErrorRepresentationBenchmarks
{
    private Result<int, Error> _referenceSuccess;
    private Result<int, Error> _referenceFailure;
    private Result<int, RichStructError> _structSuccess;
    private Result<int, RichStructError> _structFailure;
    private string _message = null!;

    [GlobalSetup]
    public void Setup()
    {
        _message = "The supplied value is invalid.";
        Error referenceError = Error.Validation("INVALID_VALUE", _message);
        RichStructError structError = new(
            ErrorType.Validation,
            (int)ErrorType.Validation,
            "INVALID_VALUE",
            _message,
            true);
        _referenceSuccess = Result<int, Error>.Ok(42);
        _referenceFailure = Result<int, Error>.Fail(referenceError);
        _structSuccess = Result<int, RichStructError>.Ok(42);
        _structFailure = Result<int, RichStructError>.Fail(structError);
    }

    [Benchmark]
    public Error ConstructReferenceError() => Error.Validation("INVALID_VALUE", _message);

    [Benchmark]
    public RichStructError ConstructStructError() => new(
        ErrorType.Validation,
        (int)ErrorType.Validation,
        "INVALID_VALUE",
        _message,
        true);

    [Benchmark]
    public Result<int, Error> MapReferenceSuccess() =>
        _referenceSuccess.Map(default(IncrementCallable));

    [Benchmark]
    public Result<int, RichStructError> MapStructSuccess() =>
        _structSuccess.Map(default(IncrementCallable));

    [Benchmark]
    public Result<int, Error> PropagateReferenceFailure() =>
        _referenceFailure.Map(default(IncrementCallable));

    [Benchmark]
    public Result<int, RichStructError> PropagateStructFailure() =>
        _structFailure.Map(default(IncrementCallable));

    public readonly record struct RichStructError(
        ErrorType Type,
        int NumericType,
        string Code,
        object Detail,
        bool IsMessagePublic);

    public readonly struct IncrementCallable : IValueFunction<int, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Invoke(int value) => value + 1;
    }
}
