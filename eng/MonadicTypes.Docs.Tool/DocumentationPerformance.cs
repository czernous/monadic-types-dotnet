using System.Diagnostics;
using System.Globalization;

namespace MonadicTypes.Tooling;

internal static class DocumentationPerformance
{
    private const int WarmupCount = 3;
    private const int SampleCount = 15;

    public static int Run(string root)
    {
        List<(string Assembly, string Xml)> inputs = DiscoverInputs(root);
        for (int index = 0; index < WarmupCount; index++)
        {
            GC.KeepAlive(DocumentationModel.Load(root));
        }

        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int index = 0; index < SampleCount; index++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            DocumentationModel model = DocumentationModel.Load(root);
            elapsed[index] = Stopwatch.GetTimestamp() - started;
            allocated[index] = GC.GetTotalAllocatedBytes(precise: true) - before;
            GC.KeepAlive(model);
        }

        elapsed.Sort();
        allocated.Sort();
        Console.Out.Write("model-load median: ");
        Console.Out.Write(
            Stopwatch.GetElapsedTime(0, elapsed[SampleCount / 2]).TotalMilliseconds
                .ToString("F3", CultureInfo.InvariantCulture));
        Console.Out.Write(" ms; allocated median: ");
        Console.Out.Write(allocated[SampleCount / 2].ToString(CultureInfo.InvariantCulture));
        Console.Out.WriteLine(" B");

        MeasureApi(inputs);
        MeasureXml(inputs);
        MeasureOutput(DocumentationModel.Load(root));
        MeasureEndToEnd(root);
        return 0;
    }

    private static List<(string Assembly, string Xml)> DiscoverInputs(string root)
    {
        List<(string Assembly, string Xml)> inputs = [];
        string defaultFramework = ProjectInfo.ReadTargetFramework(Path.Combine(root, "Directory.Build.props"));
        foreach (string projectPath in Directory.EnumerateFiles(
                     Path.Combine(root, "src"),
                     "*.csproj",
                     SearchOption.AllDirectories))
        {
            ProjectInfo project = ProjectInfo.Load(projectPath, defaultFramework);
            if (project.ProjectName.EndsWith(".Analyzers", StringComparison.Ordinal))
            {
                continue;
            }

            string output = Path.Combine(
                Path.GetDirectoryName(projectPath)!,
                "bin",
                "Release",
                project.TargetFramework,
                project.AssemblyName);
            if (File.Exists(output + ".dll") && File.Exists(output + ".xml"))
            {
                inputs.Add((output + ".dll", output + ".xml"));
            }
        }

        return inputs;
    }

    private static void MeasureApi(List<(string Assembly, string Xml)> inputs)
    {
        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            foreach ((string assembly, _) in inputs)
            {
                GC.KeepAlive(PublicApiReader.Read(assembly));
            }

            elapsed[sample] = Stopwatch.GetTimestamp() - started;
            allocated[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
        }

        WriteMeasurement("api-read", elapsed, allocated);
    }

    private static void MeasureXml(List<(string Assembly, string Xml)> inputs)
    {
        List<(Dictionary<string, PublicApiMember> Api, string Xml)> prepared = new(inputs.Count);
        foreach ((string assembly, string xml) in inputs)
        {
            prepared.Add((PublicApiReader.Read(assembly), xml));
        }

        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            foreach ((Dictionary<string, PublicApiMember> api, string xml) in prepared)
            {
                GC.KeepAlive(XmlDocumentationReader.Read(api, xml));
            }

            elapsed[sample] = Stopwatch.GetTimestamp() - started;
            allocated[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
        }

        WriteMeasurement("xml-merge", elapsed, allocated);
    }

    private static void MeasureOutput(DocumentationModel model)
    {
        MeasureOutputStage("reference-render", model, static value => DocumentationOutput.RenderReference(value));
        MeasureOutputStage(
            "reference-render-unchecked",
            model,
            static value => DocumentationOutput.RenderReferenceUnchecked(value));
        MeasureOutputStage("manifest-render", model, static value => DocumentationOutput.RenderManifest(value));

        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            string reference = DocumentationOutput.RenderReference(model);
            string manifest = DocumentationOutput.RenderManifest(model);
            string root = DocumentationOutput.RenderRootIndex(model);
            elapsed[sample] = Stopwatch.GetTimestamp() - started;
            allocated[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
            GC.KeepAlive(reference);
            GC.KeepAlive(manifest);
            GC.KeepAlive(root);
        }

        WriteMeasurement("output-render", elapsed, allocated);
    }

    private static void MeasureOutputStage(
        string name,
        DocumentationModel model,
        Func<DocumentationModel, string> render)
    {
        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            string output = render(model);
            elapsed[sample] = Stopwatch.GetTimestamp() - started;
            allocated[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
            GC.KeepAlive(output);
        }

        WriteMeasurement(name, elapsed, allocated);
    }

    private static void MeasureEndToEnd(string root)
    {
        Span<long> elapsed = stackalloc long[SampleCount];
        Span<long> allocated = stackalloc long[SampleCount];
        for (int sample = 0; sample < SampleCount; sample++)
        {
            long before = GC.GetTotalAllocatedBytes(precise: true);
            long started = Stopwatch.GetTimestamp();
            DocumentationModel model = DocumentationModel.Load(root);
            string reference = DocumentationOutput.RenderReference(model);
            string manifest = DocumentationOutput.RenderManifest(model);
            string index = DocumentationOutput.RenderRootIndex(model);
            elapsed[sample] = Stopwatch.GetTimestamp() - started;
            allocated[sample] = GC.GetTotalAllocatedBytes(precise: true) - before;
            GC.KeepAlive(reference);
            GC.KeepAlive(manifest);
            GC.KeepAlive(index);
        }

        WriteMeasurement("end-to-end", elapsed, allocated);
    }

    private static void WriteMeasurement(
        string name,
        Span<long> elapsed,
        Span<long> allocated)
    {
        elapsed.Sort();
        allocated.Sort();
        Console.Out.Write(name);
        Console.Out.Write(" median: ");
        Console.Out.Write(
            Stopwatch.GetElapsedTime(0, elapsed[SampleCount / 2]).TotalMilliseconds
                .ToString("F3", CultureInfo.InvariantCulture));
        Console.Out.Write(" ms; allocated median: ");
        Console.Out.Write(allocated[SampleCount / 2].ToString(CultureInfo.InvariantCulture));
        Console.Out.WriteLine(" B");
    }
}
