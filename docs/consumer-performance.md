# Consumer API performance verification

Measured on 2026-09-11 using BenchmarkDotNet 0.15.8 on Windows 11 x64,
AMD Ryzen 7 4800H. The measured NativeAOT runtime reported .NET 10.0.11;
the benchmark host reported .NET 10.0.12. These benchmark runs used SDK
10.0.303; the repository now pins serviced SDK 10.0.401.
Each job uses one launch, three warmups, ten measurements, and 250 ms target
iterations. These are local observations, not replacements for the accepted
[primitive baselines](../benchmarks/baseline.md).

## Composition and traversal

The first completed consumer run measured the following. Error is half the
99.9% confidence interval; a dash in the raw allocation column means zero bytes.

| Operation | Mean | Error | Allocated |
| --- | ---: | ---: | ---: |
| Existing Bind/ToResult, absent | 6.932 ns | 0.4022 ns | 0 B |
| Eager RequireSome, absent | 2.353 ns | 0.0845 ns | 0 B |
| Caller-state RequireSome, absent | 3.567 ns | 0.1201 ns | 0 B |
| Existing Bind/ToResult, present | 6.540 ns | 0.0410 ns | 0 B |
| Eager RequireSome, present | 2.400 ns | 0.0935 ns | 0 B |
| Caller-state RequireSome, present | 3.077 ns | 0.2124 ns | 0 B |
| Bare list traversal | 39.9435 ns | 0.9406 ns | 88 B |
| Generated list traversal | 36.7366 ns | 1.7649 ns | 88 B |
| Bare Option traversal | 0.6946 ns | 0.1549 ns | 0 B |
| Generated Option traversal | 0.6732 ns | 0.0079 ns | 0 B |
| Bare span traversal | 18.1859 ns | 0.3636 ns | 88 B |
| Generated span traversal | 18.8034 ns | 1.0159 ns | 88 B |

The eight-element list/span allocation is exactly one owned `long[]`, matching
the existing control. Option and RequireSome paths add no managed allocation.
Span and Option timing intervals overlap their controls. Sub-nanosecond values
are close to measurement overhead and are not universal latency promises.

## Additive Option operations

The newly added Option operations were measured separately under the serviced
NativeAOT runtime (.NET 10.0.12) so they could not perturb the established
traversal controls. The callback is initialized outside the measured operation;
the struct path writes to the same observable benchmark state.

| Operation | Mean | Error | Allocated |
| --- | ---: | ---: | ---: |
| Direct Option branch control | 1.094 ns | 0.0300 ns | 0 B |
| `Option.Tap(Action<T>)` | 1.608 ns | 0.0484 ns | 0 B |
| `Option.Tap` struct callable | 0.434 ns | 0.0258 ns | 0 B |
| `Option.Zip` | 1.919 ns | 0.1351 ns | 0 B |

The delegate Tap path carries the expected delegate dispatch cost relative to a
hand-written branch; it does not allocate. The struct-callable path is the hot
path option when that cost matters. These are additive observations, not a
replacement for the accepted primitive baseline.

## Caller-state Result recovery and fallback

The #21 overload sweep found the same captured-state gap in `Result.Recover`
and `Result.ValueOrElse`. A dedicated NativeAOT run on .NET 10.0.12 compared
their caller-state forms with direct branches and callbacks that capture the
benchmark instance:

| Operation | Branch | Mean | Error | Allocated |
| --- | --- | ---: | ---: | ---: |
| Recover direct branch | Success | 1.005 ns | 0.0638 ns | 0 B |
| Recover caller state | Success | 1.677 ns | 0.0414 ns | 0 B |
| Recover captured | Success | 8.774 ns | 0.6053 ns | 48 B |
| Recover direct branch | Failure | 2.351 ns | 0.0578 ns | 0 B |
| Recover caller state | Failure | 6.627 ns | 0.0818 ns | 0 B |
| Recover captured | Failure | 13.563 ns | 2.4500 ns | 48 B |
| ValueOrElse direct branch | Success | 2.326 ns | 0.0678 ns | 0 B |
| ValueOrElse caller state | Success | 1.337 ns | 0.1345 ns | 0 B |
| ValueOrElse captured | Success | 10.266 ns | 2.4434 ns | 48 B |
| ValueOrElse direct branch | Failure | 1.628 ns | 0.0361 ns | 0 B |
| ValueOrElse caller state | Failure | 1.945 ns | 0.0458 ns | 0 B |
| ValueOrElse captured | Failure | 8.876 ns | 0.7481 ns | 48 B |

Both caller-state forms meet the zero-allocation target and remain materially
faster than the captured alternatives. The failure-side Recover callback still
has ordinary delegate-dispatch cost relative to a hand-written branch. Unsafe
or function-pointer APIs are not justified by this application-level path; a
struct-callable form remains a future option only if profiling demonstrates a
recurring need.

## Rejected HTTP override candidate

The initial per-response URI formatting implementation failed the intended
response-cost bar:

| Operation | Mean | Error | Allocated |
| --- | ---: | ---: | ---: |
| Default response | 88.85 ns | 1.576 ns | 280 B |
| Initial explicit response | 108.28 ns | 2.751 ns | 384 B |

The extra 104 B and non-overlapping timing intervals are not accepted. They
motivated bounded reusable status metadata. Keep this result as evidence of the
rejected candidate; do not reinterpret it as an allowed new baseline.

The corrected three-method HTTP run measured default construction at
93.861 ± 4.633 ns and explicit construction at 79.403 ± 0.842 ns, both at
280 B per response. These are steady-state results: the first explicit call
initialized a bounded 200-entry title/URI table. That eager table was subsequently
replaced because its cold ownership cost remained unnecessarily high.

The exact final serviced-runtime NativeAOT run measured default construction at
94.587 ± 6.327 ns and explicit construction at 96.773 ± 5.055 ns, both at
280 B per response. Their 99.9% confidence intervals overlap, so the explicit
path does not establish a timing regression against its same-run control. The
same run measured catalog construction at 35.395 ± 9.880 ns and 72 B; the wide
interval is evidence, not a replacement timing baseline. These results retain
the existing architectural target and do not replace the accepted primitive
baselines.

## Catalog storage regression

The initial nullable status field grew `ErrorCatalogEntry` from 24 to 32 bytes
on x64. A regression test reproduced that increase. A compact validated status
field restores the 24-byte stride while retaining the nullable public property;
all 45 ASP.NET Core tests pass. The full 18-method catalog run completed with
the following ownership results:

| Operation | Mean | Error | Allocated |
| --- | ---: | ---: | ---: |
| Entry construction | 5.4331 ns | 0.3825 ns | 0 B |
| Owned array copy | 17.9788 ns | 0.6534 ns | 72 B |
| Metadata construction | 46.6450 ns | 2.9620 ns | 96 B |
| Metadata read | 0.7604 ns | 0.0529 ns | 0 B |

Allocation matches the accepted ownership contract. Timing does not meet the
recorded historical means, including the unchanged array-copy control, and
remains **unaccepted** pending a same-host committed-source comparison. All
current validation paths reported 0 B; the legacy colliding-metadata control
reported 1 B/op and also requires investigation. No baseline was replaced.

The same full suite from committed revision
`f5d4bd8747cb812aa979bf2000a3e12efae6d3f3`, copied outside the working tree,
measured entry construction at 5.1524 ± 0.1316 ns, owned copy at
17.7334 ± 0.4220 ns, metadata construction at 42.0730 ± 2.2770 ns, and
reads at 0.7631 ± 0.0481 ns. Allocations were identical. All four confidence
intervals overlap the candidate's intervals. This comparison does not isolate
a new ownership-path timing regression, but both revisions miss the historical
means; it does not clear the historical timing gate or establish a new baseline.

The broader validator comparison is not uniformly overlapping: for example,
current small-metadata validation measured 420.1 ± 24.21 ns in the candidate
versus 322.6 ± 8.56 ns in the committed copy. This remains a timing finding,
not an accepted increase. The fixtures use process-randomized string hashes,
so probe distributions also differ between processes; that variable needs
control before attributing validator movement to production code. The legacy
colliding-metadata control reported 1 B/op in both revisions. Its candidate raw
GC total was 672 B across 992 operations, while other nonallocating validation
rows also recorded small fixed totals. This is evidence to investigate harness
overhead, not permission to loosen any allocation gate.

## Primitive regression verification

The unchanged 28-method, 100,000,000-invocation NativeAOT job completed. All
accepted zero-allocation paths remained at 0 B; structured-error construction
remained at its existing 40 B. Timing verification failed: twelve of the nineteen
historically timed primitive controls had lower confidence bounds above their
accepted means. Examples include `MapResult` at 3.2241 ± 0.0362 ns against
2.7587 ns, `MapResultToOtherType` at 3.2805 ± 0.1826 ns against 2.7047 ns,
and `BindResult` at 9.0709 ± 0.1783 ns against 8.8458 ns. The unchanged
`LegacyMap` also measured 3.1863 ± 0.0969 ns against 2.7738 ns.

These results do not establish release readiness. The original benchmark job
and accepted baseline file are unchanged.

## Cold HTTP initialization

The eager status-table candidate previously measured 27,352 B on its first
explicit response. A fresh minimal NativeAOT probe on the serviced runtime later
measured 19,384 B total, or 19,104 B beyond its 280 B steady default response.
Both observations failed the cold-ownership standard even though repeated
responses allocated no extra bytes.

The final adapter uses a bounded 200-reference flyweight index and materializes
only the status URIs a process actually uses. The permanent NativeAOT smoke test
measured a 280 B default response, a 2,064 B first explicit response, and a 280 B
second explicit response. The one-time identity cost is therefore 1,784 B and
was identical across five fresh processes. The smoke executable rejects more
than 4 KiB of cold identity allocation or any steady allocation above its
same-process default control.

These allocation checks are architectural gates, not latency baselines. The
status-title switch and direct three-digit URI formatter avoid runtime phrase
tables and culture initialization. No unsafe or SIMD path is used because the
remaining operation is cold, bounded, and already below its allocation target.

## Toolchain servicing finding

The benchmark runs above used SDK 10.0.303 and NativeAOT runtime 10.0.11.
The repository pin has since moved to SDK 10.0.401. Microsoft's
[release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json)
confirms that association. Its
[September servicing notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.12/10.0.12.md)
identify runtime 10.0.12 as a security update, included in SDK 10.0.401.

Recommendation: use the serviced SDK and update its implicit linker locks
together, then rerun verification. Do not keep an older toolchain merely because
it matches existing locks or benchmark observations. Measurements across runtime
versions remain separately labeled and cannot silently replace accepted targets.
