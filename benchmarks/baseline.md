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

On 2026-08-11, `MapResult` and `MapResultToOtherType` were run together under
the exact NativeAOT job after unifying the type-changing construction path.
They measured 2.721 ns and 2.703 ns respectively, both at 0 B. The latter beats
its 2.7047 ns historical baseline and the same-run control, so the unified
implementation is retained without changing the accepted threshold.

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

The rich `Error` is reference-backed: successful `Result<T, Error>`
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
cold uninitialized helper is retained. A cooled-host full rerun is required
before claiming a new absolute best.

## Async, Composition, And Effects

Recorded on 2026-08-11 under the additive 250 ms NativeAOT job. Inputs,
callbacks, errors, and completed awaitables were created in `GlobalSetup`.

New operations do not define their own acceptance standard. Before promotion,
each must satisfy the provisional architectural target below and allocate 0 B.
Targets are derived from the accepted optimized primitives on this host; they
are deliberately separate from the stable regression baseline that a passing
measurement establishes.

| Method | Architectural target | Reference |
|---|---:|---|
| BindErrorFailure | <= 8.8458 ns | no slower than accepted `BindResult` |
| BindErrorSuccess | <= 5.5174 ns | at most 2x accepted `MapResult` |
| CompletedTaskBindError | <= 13.7935 ns | at most 5x accepted `MapResult` |
| CombineTwo | <= 2.2574 ns | at most 2x accepted `DirectResultBranch` |
| ZipTwo | <= 5.5174 ns | at most 2x accepted `MapResult` |
| TransposePresent | <= 2.9734 ns | accepted `MapOption` plus 10% shape allowance |
| CompletedValueTaskMap | <= 13.5175 ns | at most 5x accepted type-changing `Map` |
| CompletedAsyncMap | <= 13.7935 ns | at most 5x accepted `MapResult` |
| GeneratedCompletedAsyncMap | < same-run `CompletedAsyncMap` | generated dispatch must beat the delegate path it replaces |
| MixedCompletedPipeline | <= 20.6903 ns | at most 7.5x accepted `MapResult` |
| CompletedTaskReceiverMap | <= 13.7935 ns | at most 5x accepted `MapResult` |
| EffectSuccess | <= 2.7587 ns | no slower than accepted `MapResult` |
| TypedCompletedValueTaskEffectSuccess | <= 20.6903 ns | no slower than the mixed completed async boundary target |
| TypedCompletedTaskEffectSuccess | <= 20.6903 ns | no slower than the mixed completed async boundary target |
| TypedCallerStateCompletedTaskEffectSuccess | <= 20.6903 ns | caller-state form must meet the same completed async boundary target |

These are host-specific absolute gates, not universal API promises. Future CI
should additionally compare ratios against an unchanged same-run control to
separate code regressions from CPU power and thermal state.

The 2026-08-11 full-suite confirmation passed every target. These full-suite
measurements are the accepted stable regression baseline; focused-filter runs
are diagnostic only because NativeAOT code layout differs.

| Method | Accepted mean | Allocated |
|---|---:|---:|
| BindErrorFailure | 5.5825 ns | 0 B |
| BindErrorSuccess | 5.0950 ns | 0 B |
| CompletedTaskBindError | 4.5926 ns | 0 B |
| CombineTwo | 1.7180 ns | 0 B |
| ZipTwo | 4.0470 ns | 0 B |
| TransposePresent | 2.7478 ns | 0 B |
| CompletedValueTaskMap | 12.1341 ns | 0 B |
| CompletedAsyncMap | 13.1221 ns | 0 B |
| GeneratedCompletedAsyncMap | 6.3249 ns | 0 B |
| MixedCompletedPipeline | 18.0510 ns | 0 B |
| CompletedTaskReceiverMap | 13.1966 ns | 0 B |
| EffectSuccess | 2.4475 ns | 0 B |
| TypedCompletedValueTaskEffectSuccess | 17.3944 ns | 0 B |
| TypedCompletedTaskEffectSuccess | 15.9606 ns | 0 B |
| TypedCallerStateCompletedTaskEffectSuccess | 13.1561 ns | 0 B |

The additive suite is isolated in its own executable. Re-isolating the
primitive harness measured `Option.Map` at 2.6744 ns versus 2.7031 ns,
callable `Option.Map` at 0.4871 ns versus 0.5053 ns, and the function-pointer
control at 2.055 ns versus 2.5183 ns. Type-changing delegate `Result.Map`
reached 2.742 ns with a 2.663-2.820 ns confidence interval around its 2.7047 ns
historical value. It remains at 0 B but needs cooled-host confirmation before
claiming a better accepted baseline.

Actively instantiating async, effects, typed exception, and caller-state Task
paths grew the NativeAOT smoke binary from 8,089,088 to 8,128,512 bytes: 39,424
bytes, or 0.49%. The smoke executable runs successfully after publication.

An intermediate target-gate run measured `CompletedValueTaskMap` at 18.5213 ns
against its 13.5175 ns target. It exposed both a copied `ValueTask` receiver and
shared benchmark output directories that allowed one runner to overwrite the
other. Passing the receiver by readonly reference measured 10.673 ns in a
focused diagnostic run and 12.1341 ns in the final full-suite layout. Benchmark
output and intermediate paths now use distinct short project keys, preventing
cross-runner contamination without exceeding Windows NativeAOT linker path
limits.

The first typed async Effect targets used one-stage map thresholds and were
rejected because they omitted the explicit exception boundary. The corrected
targets compare against the existing mixed completed async boundary, which also
combines an awaitable callback with Result composition. The 14-method full run
passed both corrected targets and retained 0 B for every row. A focused check of
the caller-state typed Task overload measured 16.139 ns and 0 B.

The subsequent 15-method full run established the caller-state path at 13.1561
ns and 0 B, 19.6% faster than the same-run non-state typed Task path at 16.3664
ns. Passing a cached Task as explicit state with a static callback therefore
removes capture requirements and improves this measured NativeAOT path without
changing exception semantics.

The 2026-08-11 16-method full run added generated async callable dispatch. It
measured `GeneratedCompletedAsyncMap` at 6.3249 ns and 0 B against a same-run
`CompletedAsyncMap` control at 11.7515 ns and 0 B, a 46.2% reduction in wrapper
time. Every existing row remained at 0 B and within its architectural target.
The generated row becomes its first accepted baseline; the same run does not
replace better historical baselines for existing methods.

The generated async overload was also instantiated in the NativeAOT smoke
application. Publication and execution succeeded at 8,116,736 bytes on .NET
10.0.11. The prior 8,128,512-byte observation used .NET 10.0.10, so the smaller
number is a size-safety check rather than an attributable binary-size improvement.

The 14-method full run measured the readonly-ValueTask mixed pipeline at 19.9354
ns, down from 23.8402 ns before completed-path inlining and copy removal. Adding
the caller-state benchmark changed native layout and measured it at 20.7550 ns
with a 0.8034 ns 99.9% error interval. The target lies inside that interval, so
this is not a statistically established target regression and rerunning for a
favorable mean would be p-hacking. Neither run replaces the 18.0510 ns
historical regression baseline. That older absolute result remains the gate
until a same-run unchanged control separates native layout variance from an
implementation regression.
