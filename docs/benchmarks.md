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
- NativeAOT size is verified separately by the smoke executable.

Run the complete suite with:

```powershell
dotnet run -c Release --project benchmarks\MonadicTypes.Benchmarks -- --filter *
```
