namespace MonadicTypes.Tooling.Tests;

public sealed class MarkdownLinkValidatorTests
{
    [Fact]
    public void MissingLocalFragmentFailsValidation()
    {
        string root = CreateDocumentation("[Missing](api.md#absent)", "<h2 id=\"present\">Present</h2>");
        try
        {
            InvalidDataException error = Assert.Throws<InvalidDataException>(
                () => MarkdownLinkValidator.Validate(root));

            Assert.Contains("api.md#absent", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ExistingLocalHeadingAndExplicitFragmentPassValidation()
    {
        string root = CreateDocumentation(
            "[Markdown](api.md#public-api)\n[Explicit](api.md#result-map)",
            "## Public API\n\n<h3 id=\"result-map\">Result.Map</h3>");
        try
        {
            MarkdownLinkValidator.Validate(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ExternalLinksAndCodeFenceExamplesAreIgnored()
    {
        string root = CreateDocumentation(
            "[HTTPS](https://example.com/docs#api)\n\n```csharp\n[NotALink](missing.md)\n```",
            "## Public API");
        try
        {
            MarkdownLinkValidator.Validate(root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void MissingFragmentInRepositoryAbsoluteLinkFailsValidation()
    {
        const string repository = "https://github.com/example/monadic-types";
        string root = CreateDocumentation(
            $"[Missing]({repository}/blob/HEAD/api.md#absent)",
            "<h2 id=\"present\">Present</h2>");
        try
        {
            InvalidDataException error = Assert.Throws<InvalidDataException>(
                () => MarkdownLinkValidator.Validate(root, repository));

            Assert.Contains("api.md#absent", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void EmptyLinkTargetFailsValidation()
    {
        string root = CreateDocumentation("[Not navigable]()", "## Public API");
        try
        {
            InvalidDataException error = Assert.Throws<InvalidDataException>(
                () => MarkdownLinkValidator.Validate(root));

            Assert.Contains("empty", error.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateDocumentation(string readme, string api)
    {
        string root = Path.Combine(Path.GetTempPath(), "monadic-types-docs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "README.md"), readme);
        File.WriteAllText(Path.Combine(root, "api.md"), api);
        return root;
    }
}
