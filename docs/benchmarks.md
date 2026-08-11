# Benchmark Policy

- Benchmarks run under NativeAOT with memory diagnostics.
- Inputs, delegates, meters, errors, and results are created in `GlobalSetup`.
- Construction is measured only by explicitly named construction benchmarks.
- The accepted primitive regression job retains its original 100,000,000
  invocations, three warmups, and ten measurements so results remain directly
  comparable. Additive representation jobs target 250 ms and let
  BenchmarkDotNet adapt invocation counts to the host.
- Any managed allocation in an accepted success/composition path is a failure.
- A slower run does not replace the accepted baseline without repeat evidence.
- New methods receive an architectural target derived from an analogous accepted
  primitive before their first result can become a regression baseline.
- Architectural targets and stable regression baselines are recorded separately;
  passing a regression baseline alone does not prove that an implementation is fast.
- Allocation gates are absolute. A timing regression is statistically actionable
  when its confidence interval no longer overlaps the target or unchanged control;
  rerunning solely to obtain a favorable mean is not an acceptance strategy.
- NativeAOT size is verified separately by the smoke executable.
- Benchmark runners return a nonzero process exit code for critical validation,
  failed reports, or an empty report set; a generated restore/build failure must
  not be interpreted as a passing run.
- NativeAOT benchmarks are local or manually dispatched by default. Hosted CI
  should require tests and AOT smoke publication, and only gate timings on
  stable self-hosted hardware or a deliberately budgeted scheduled job.

Run the complete suite with:

```powershell
dotnet run -c Release --project benchmarks\MonadicTypes.Benchmarks -- --filter "*PrimitiveBenchmarks*"
```

Run additive composition benchmarks separately so new references cannot change
the NativeAOT code layout of the accepted primitive baseline:

```powershell
dotnet run -c Release --project benchmarks\MonadicTypes.Composition.Benchmarks
```

Run terminal `Switch` benchmarks in their isolated executable:

```powershell
dotnet run -c Release --project benchmarks\MonadicTypes.Switch.Benchmarks
```

New benchmark families use separate NativeAOT executables because adding methods
to an accepted executable can change native code layout and perturb unchanged
controls. Setup, delegates, and input construction remain in `GlobalSetup`.
