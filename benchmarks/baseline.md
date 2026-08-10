# Primitive Benchmark

## Run Metadata

- Recorded: 2026-08-09
- OS: Windows 11 25H2, build `10.0.26200.8973`
- CPU: AMD Ryzen 7 4800H, 8 physical cores, 16 logical cores
- Runtime: .NET 10.0.10, NativeAOT x86-64-v3
- BenchmarkDotNet: `0.15.8`
- Command: `dotnet run -c Release --project Benchmarks\\Benchmarks.csproj -- Primitives`
- Job: 1 launch, 3 warmups, 10 measured iterations, 100,000,000 invocations

| Method | Mean | Allocated |
|---|---:|---:|
| DirectTransform | indistinguishable from overhead | 0 B |
| ConstructSuccess | 0.2902 ns | 0 B |
| ConstructFailure | 0.2331 ns | 0 B |
| MapResult | 2.7587 ns | 0 B |
| MapResultToOtherType | 2.7047 ns | 0 B |
| BindResult | 8.8458 ns | 0 B |
| MatchResult | 2.5200 ns | 0 B |
| ConstructSome | 0.4918 ns | 0 B |
| MapOption | 2.7031 ns | 0 B |
| StructCallableMapOption | 0.5053 ns | 0 B |
| DirectResultBranch | 1.1287 ns | 0 B |
| LegacyConstructSuccess | 0.2007 ns | 0 B |
| LegacyConstructFailure | 0.2168 ns | 0 B |
| LegacyMap | 2.7738 ns | 0 B |
| LegacyBind | 8.7745 ns | 0 B |
| LegacyMatch | 2.6868 ns | 0 B |
| StructCallableMap | 0.6987 ns | 0 B |
| StructCallableBind | 0.7143 ns | 0 B |
| StructCallableMatch | 0.5030 ns | 0 B |
| FunctionPointerMap | 2.5183 ns | 0 B |

## Additive Measurements

These methods were introduced after the accepted baseline. Their values are the
best observations across the current review runs, not accepted thresholds and
not replacements for any accepted row above.

| Method | Mean | Allocated |
|---|---:|---:|
| ConstructStructuredError | 7.1947 ns | 40 B |
| ConstructRichErrorSuccess | 0.1893 ns | 0 B |
| StructCallableMapRichError | 1.9279 ns | 0 B |
| ReadErrorCode | 0.4756 ns | 0 B |
| RecordErrorWithoutActivity | indistinguishable from overhead | 0 B |
| RecordErrorWithoutMetricsListener | 1.4704 ns | 0 B |
| FormatErrorToSpan | 14.4170 ns | 0 B |

## Current Verification

The 2026-08-09 full verification run retained 0 B allocation for every accepted
primitive method. A focused rerun cleared the apparent `FunctionPointerMap` and
`LegacyConstructSuccess` regressions at 2.5393 ns and 0.2163 ns respectively.
`MatchResult` repeated at 2.6488 ns against 2.5200 ns and remains flagged. Its
implementation did not change, so NativeAOT layout or environmental variance
must be excluded before changing code. The accepted values were not changed.

## Decision

The explicit three-state `Result`, strict-null `Option`, and their composition
methods add no measured managed allocation under NativeAOT. Benchmark inputs
are populated in `GlobalSetup`, outside measured operations, to prevent setup
cost from entering results and static readonly inputs from being constant-folded.

Delegate overloads remain the ergonomic default. Hot paths should use readonly
struct implementations of `IValueFunction<TIn, TOut>`: callable `Result.Map`
is 3.95x faster, `Result.Bind` is 12.38x faster, `Result.Match` is 5.34x faster,
and `Option.Map` is 5.35x faster than their delegate equivalents in the accepted
run.
The managed function-pointer experiment did not outperform callable structs and
does not justify an unsafe public API.

The rich production `Error` is reference-backed: successful `Result<T, Error>`
construction and mapping allocate 0 B, while constructing an actual error costs
40 B on the failure path. Modules with high-rate failures can retain compact
struct error carriers and convert at a boundary through `IErrorConvertible<T>`.
Activity recording exits at harness-overhead speed when no sampled activity
exists; the disabled `Meter` counter check costs 1.51 ns. Existing Error and
metric inputs are created in `GlobalSetup`, outside measured operations.

Constructor timings close to the harness resolution are retained primarily to
detect future allocation regressions. They must not be used for throughput
ratios. Compare means and error bounds on the same machine, and reject any
managed allocation regression in these paths.

## Rich Error Representation Check

The extraction audit compared the reference-backed `Error` with an equivalent
32-byte `readonly record struct` carrying category, numeric category, code,
message/cause payload, and visibility. Both result shapes allocated 0 B during
mapping, but the larger struct materially increased success-path copy cost.

| Method | Mean | Allocated |
|---|---:|---:|
| Construct reference Error | 8.5291 ns | 40 B |
| Construct struct Error candidate | 0.4518 ns | 0 B |
| Map successful Result with reference Error | 2.1980 ns | 0 B |
| Map successful Result with struct Error candidate | 8.6440 ns | 0 B |
| Propagate failed Result with struct Error candidate | 2.1240 ns | 0 B |

The struct removes failure allocation but made the dominant success mapping
3.93x slower. The general rich `Error` therefore remains a sealed record
reference. Allocation-sensitive domains should carry a compact domain-specific
struct error through their hot path and convert to rich `Error` only at an
observability or transport boundary.

## Extraction Host-State Control

Repeated NativeAOT compilation and benchmarking degraded this laptop below the
historical accepted run. A final exact-job control therefore ran untouched CSFM
immediately beside the extracted candidate rather than accepting absolute means
from different host states.

| Method | Extracted candidate | Same-session CSFM control | Historical accepted |
|---|---:|---:|---:|
| MapResult | 3.323 ns | 3.654 ns | 2.7587 ns |
| MatchResult | 2.656 ns | 3.118 ns | 2.5200 ns |

Both candidates remained at 0 B. The extracted build beat the same-session
control by 9.1% and 14.8%, respectively, but neither degraded host result
replaces the historical baseline. `Match`'s explicit success-first branch and
cold uninitialized helper are retained; `Map` remains implementation-identical
to the accepted source. A cooled-host full rerun is required before claiming a
new absolute best.
