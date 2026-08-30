using System.Text;

namespace MonadicTypes.Tooling;

internal static class DocumentationTool
{
    private const string GeneratedStart = "<!-- BEGIN GENERATED API INDEX -->";
    private const string GeneratedEnd = "<!-- END GENERATED API INDEX -->";

    public static int Run(string[] args)
    {
        if (args.Length is 0 or > 2
            || args[0] is not ("benchmark" or "generate" or "verify" or "manifest"))
        {
            Console.Error.WriteLine("Usage: mt-docs <benchmark|generate|verify|manifest> [repository-root]");
            return 2;
        }

        string root = Path.GetFullPath(args.Length is 2 ? args[1] : Environment.CurrentDirectory);
        if (args[0] is "benchmark")
        {
            return DocumentationPerformance.Run(root);
        }

        DocumentationModel model;
        try
        {
            model = DocumentationModel.Load(root);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or System.Xml.XmlException)
        {
            Console.Error.WriteLine($"Documentation input is invalid: {exception.Message}");
            return 1;
        }

        return args[0] switch
        {
            "manifest" => EmitManifest(model),
            "generate" => WriteGenerated(model, root, write: true),
            "verify" => WriteGenerated(model, root, write: false),
            _ => 2
        };
    }

    private static int EmitManifest(DocumentationModel model)
    {
        Console.Out.Write(DocumentationOutput.RenderManifest(model));
        return 0;
    }

    private static int WriteGenerated(DocumentationModel model, string root, bool write)
    {
        bool valid = WriteOrVerify(
            Path.Combine(root, "docs", "api-reference.md"),
            DocumentationOutput.RenderReference(model),
            write);
        valid &= WriteOrVerify(
            Path.Combine(root, "docs", "api-manifest.json"),
            DocumentationOutput.RenderManifest(model),
            write);
        valid &= UpdateReadme(
            Path.Combine(root, "README.md"),
            DocumentationOutput.RenderRootIndex(model),
            model,
            write);

        foreach (PackageDocumentation package in model.Packages)
        {
            valid &= UpdateReadme(
                Path.Combine(root, "docs", "package-readmes", package.ProjectName + ".md"),
                DocumentationOutput.RenderPackageIndex(package),
                model: null,
                write);
        }

        if (!valid)
        {
            return 1;
        }

        try
        {
            MarkdownLinkValidator.Validate(root, model.Packages[0].RepositoryUrl);
            return 0;
        }
        catch (InvalidDataException exception)
        {
            Console.Error.WriteLine($"Documentation link validation failed: {exception.Message}");
            return 1;
        }
    }

    private static bool UpdateReadme(
        string path,
        string index,
        DocumentationModel? model,
        bool write)
    {
        if (!File.Exists(path))
        {
            return true;
        }

        string existing = File.ReadAllText(path);
        int start = existing.IndexOf(GeneratedStart, StringComparison.Ordinal);
        string updated = start < 0
            ? existing.TrimEnd() + "\n\n" + index + "\n"
            : ReplaceGeneratedRegion(existing, start, index, path);
        updated = model is null ? updated : DocumentationOutput.RewriteApiReferenceLinks(updated, model);
        return WriteOrVerify(path, updated, write);
    }

    private static string ReplaceGeneratedRegion(string content, int start, string replacement, string path)
    {
        int end = content.IndexOf(GeneratedEnd, start, StringComparison.Ordinal);
        return end < 0
            ? throw new InvalidDataException($"Generated API marker is incomplete: {path}.")
            : content[..start] + replacement + content[(end + GeneratedEnd.Length)..];
    }

    private static bool WriteOrVerify(string path, string expected, bool write)
    {
        string actual = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            return true;
        }

        if (!write)
        {
            Console.Error.WriteLine($"Generated documentation is stale: {path}");
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, expected, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return true;
    }
}
