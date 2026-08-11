using BenchmarkDotNet.Running;
using Benchmarks;

string[] benchmarkArguments = args.Length is 0 ? ["--filter", "*"] : args;
var summaries = BenchmarkSwitcher.FromTypes([typeof(CompositionBenchmarks)]).Run(benchmarkArguments).ToArray();
Environment.ExitCode = summaries.Length is 0 || summaries.Any(static summary =>
    summary.HasCriticalValidationErrors ||
    summary.Reports.Length is 0 ||
    summary.Reports.Any(static report => !report.Success))
    ? 1
    : 0;
