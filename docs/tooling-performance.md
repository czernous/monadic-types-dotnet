# Tooling Performance

The repository automation is implemented as four small NativeAOT executables.
These measurements are engineering baselines for the repository tooling, not
BenchmarkDotNet microbenchmarks. They include process startup and the work the
command owns. Inputs were prepared outside the timed region where applicable.
No machine name, user path, processor model, or other host identifier is stored.

## Accepted Measurements

| Operation | Previous implementation | Current implementation | Change |
| --- | ---: | ---: | ---: |
| Git diff plus affected-project resolution | 345.94 ms Bash pipeline | 80.88 ms NativeAOT pipeline | -265.06 ms (-76.6%) |
| Raw Git transport within that pipeline | n/a | 78.98 ms | Resolver adds about 1.90 ms |
| Lockfile verification | 31.02 ms | 28.83 ms | -2.19 ms (-7.1%) |
| Pack and inspect ten packages | 40.10 s, ten pack hosts | 12.95 s, one parallel traversal | -27.15 s (-67.7%) |
| Consume a changed package identity | 25.16 s, paired four-host runner | 25.09 s, one traversal host | Effectively unchanged |
| Pack plus changed-identity consumption | 65.26 s | 38.04 s | -27.22 s (-41.7%) |

Final `win-x64` executable sizes were 1,064,448 bytes for `mt-affected`,
1,828,864 bytes for `mt-pack`, 1,537,024 bytes for `mt-test-packages`, and
1,042,944 bytes for `mt-verify-locks`. These values are tracked as footprint
observations rather than cross-SDK regression gates.

Three unique-identity consumption samples measured 23.28 s, 25.09 s, and
26.82 s; the table uses the 25.09 s median. The final runner intentionally
refuses an identity already present in the NuGet cache, so repeated-identity
incremental timings are not treated as release evidence.

At 100 package workflow executions, the measured reduction is approximately
45.37 runner-minutes before provider rounding or included-minute policy. A cost
estimate should multiply that figure by the runner's current per-minute rate;
the repository does not hardcode a provider price.

## Allocation Boundaries

- Git paths remain NUL-delimited UTF-8 slices in pooled buffers.
- Project closure uses pooled bit matrices and vectorized intersections when the
  graph is large enough; the scalar path remains optimal for a one-word graph.
- Lock and Nuspec parsing use pooled byte buffers, `SearchValues`, spans, and
  fixed-width comparisons rather than JSON/XML object models.
- Process and filesystem APIs still require final managed strings. Path and
  argument builders create those final strings directly without intermediate
  concatenation, normalization copies, or split arrays.
- Package diagnostics allocate only on failure. Successful lock validation does
  not retain a diagnostics collection.
- Package builds use MSBuild's unconstrained parallel scheduler rather than a
  machine-specific worker cap.

## Native Alternatives

Rewriting only the orchestration in Rust or C would not remove Git, MSBuild,
NuGet, or NativeAOT compilation time. The affected-project resolver currently
adds about 1.90 ms above raw Git transport, so a separate Rust or C process can
recover at most a few milliseconds unless it also replaces the Git operation.
Pack and package-consumption time is dominated by child build work; changing the
orchestrator language would have substantially less effect than the parallel
pack traversal.

Rust or C could produce smaller standalone resolver/verifier binaries, but that
has not been measured in this repository. Such an experiment belongs in a
separate tooling package and must preserve cross-platform behavior, parser
validation, and the same end-to-end workload before its result is comparable.
