using BenchmarkDotNet.Running;

Environment.SetEnvironmentVariable("MonadicTypesBenchmarkRestore", "true");
var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args).ToArray();
Environment.ExitCode = summaries.Length is 0 || summaries.Any(static summary =>
    summary.HasCriticalValidationErrors ||
    summary.Reports.Length is 0 ||
    summary.Reports.Any(static report => !report.Success))
    ? 1
    : 0;
