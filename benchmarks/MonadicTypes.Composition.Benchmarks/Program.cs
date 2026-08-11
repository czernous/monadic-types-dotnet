using BenchmarkDotNet.Running;
using Benchmarks;

string[] benchmarkArguments = args.Length is 0 ? ["--filter", "*"] : args;
BenchmarkSwitcher.FromTypes([typeof(CompositionBenchmarks)]).Run(benchmarkArguments);
