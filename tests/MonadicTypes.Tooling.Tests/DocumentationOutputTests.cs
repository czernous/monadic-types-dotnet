namespace MonadicTypes.Tooling.Tests;

public sealed class DocumentationOutputTests
{
    [Fact]
    public void PublicApiReaderExcludesNestedTypesHiddenByContainingType()
    {
        Dictionary<string, PublicApiMember> members = PublicApiReader.Read(
            typeof(DocumentationOutputTests).Assembly.Location);

        Assert.DoesNotContain(
            members.Keys,
            static id => id.Contains("HiddenApiContainer", StringComparison.Ordinal)
                && id.EndsWith("ExposedNestedType", StringComparison.Ordinal));
    }

    [Fact]
    public void DocumentationModelRejectsCopiedArtifactWhenProjectReleaseOutputIsMissing()
    {
        string root = Path.Combine(Path.GetTempPath(), $"mt-docs-{Guid.NewGuid():N}");
        string projectDirectory = Path.Combine(root, "src", "Example");
        string artifactDirectory = Path.Combine(root, "artifacts", "stale");
        Directory.CreateDirectory(projectDirectory);
        Directory.CreateDirectory(artifactDirectory);
        try
        {
            File.WriteAllText(
                Path.Combine(root, "Directory.Build.props"),
                "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(
                Path.Combine(root, "src", "Directory.Build.props"),
                "<Project><PropertyGroup><RepositoryUrl>https://example.invalid/repository</RepositoryUrl></PropertyGroup></Project>");
            File.WriteAllText(
                Path.Combine(projectDirectory, "Example.csproj"),
                "<Project><PropertyGroup><AssemblyName>MonadicTypes.Tooling.Tests</AssemblyName></PropertyGroup></Project>");

            string assembly = typeof(DocumentationOutputTests).Assembly.Location;
            File.Copy(assembly, Path.Combine(artifactDirectory, "MonadicTypes.Tooling.Tests.dll"));
            File.WriteAllText(
                Path.Combine(artifactDirectory, "MonadicTypes.Tooling.Tests.xml"),
                "<doc><members /></doc>");

            InvalidDataException error = Assert.Throws<InvalidDataException>(
                () => DocumentationModel.Load(root));

            Assert.Contains("No Release assembly", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void MemberFragmentsDoNotDependOnMemberOrder()
    {
        DocumentedMember map = Member("M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})");
        DocumentedMember bind = Member("M:MonadicTypes.Result`2.Bind``1(System.Func{`0,MonadicTypes.Result{``0,`1}})");

        string first = FragmentFor(DocumentationOutput.RenderPackage(Package(map, bind)), "Map");
        string reordered = FragmentFor(DocumentationOutput.RenderPackage(Package(bind, map)), "Map");

        Assert.Equal(first, reordered);
    }

    [Fact]
    public void SemanticFragmentSplitsPascalCaseMemberNames()
    {
        string reference = DocumentationOutput.RenderPackage(
            Package(Member("M:MonadicTypes.Result`2.MapError``1(System.Func{`1,``0})")));

        Assert.Contains("#### Member: `Result<T1, T2>.MapError`", reference, StringComparison.Ordinal);
        Assert.Contains("[`MapError`](#member-resultt1-t2maperror)", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void OverloadFragmentsAreUniqueAndResolveToNativeHeadings()
    {
        DocumentedMember delegateMap = Member("M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})");
        DocumentedMember stateMap = Member("M:MonadicTypes.Result`2.Map``2(``1,System.Func{`0,``1,``0})");

        string reference = DocumentationOutput.RenderPackage(Package(delegateMap, stateMap));
        string[] fragments = ExtractOverloadHeadingFragments(reference);

        Assert.Equal(2, fragments.Length);
        Assert.Equal(2, fragments.Distinct(StringComparer.Ordinal).Count());
        Assert.All(fragments, fragment =>
        {
            Assert.Contains($"](#{fragment})", reference, StringComparison.Ordinal);
            Assert.Contains(
                reference.Split('\n'),
                line => line.StartsWith("##### ", StringComparison.Ordinal)
                    && string.Equals(
                        DocumentationOutput.CreateMarkdownAnchor(line.AsSpan(6)),
                        fragment,
                        StringComparison.Ordinal));
        });
        Assert.Equal(1, Count(reference, "#### Member: `Result<T1, T2>.Map`"));
        Assert.DoesNotContain("<h", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageIsGroupedByTypeAndMemberFamily()
    {
        DocumentedMember option = Member("T:MonadicTypes.Option`1", "Represents optional data.");
        DocumentedMember map = Member("M:MonadicTypes.Option`1.Map``1(System.Func{`0,``0})", "Maps `Some`.");
        DocumentedMember bind = Member("M:MonadicTypes.Option`1.Bind``1(System.Func{`0,MonadicTypes.Option{``0}})", "Binds `Some`.");

        string reference = DocumentationOutput.RenderPackage(Package(bind, option, map));

        Assert.Contains("### Type: `Option<T1>`", reference, StringComparison.Ordinal);
        Assert.Contains("#### Member: `Option<T1>.Map`", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("Exact XML ID", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("MonadicTypes.Option", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void OneXmlExampleDocumentsTheOverloadFamilyOnce()
    {
        DocumentedMember delegateMap = new(
            "M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})",
            "Maps a value.",
            "Result<int, Error> mapped = result.Map(static value => value.Id);");
        DocumentedMember stateMap = Member(
            "M:MonadicTypes.Result`2.Map``2(``1,System.Func{`0,``1,``0})",
            "Maps with state.");

        string reference = DocumentationOutput.RenderPackage(Package(delegateMap, stateMap));

        Assert.Equal(1, Count(reference, "**Example**"));
        Assert.Contains("```csharp", reference, StringComparison.Ordinal);
        Assert.Contains("result.Map(static value => value.Id)", reference, StringComparison.Ordinal);
        Assert.True(
            reference.IndexOf("#### Member: `Result<T1, T2>.Map`", StringComparison.Ordinal)
            < reference.IndexOf("**Example**", StringComparison.Ordinal));
        Assert.True(
            reference.IndexOf("**Example**", StringComparison.Ordinal)
            < reference.IndexOf("| Overload |", StringComparison.Ordinal));
    }

    [Fact]
    public void ReferenceProvidesPackageAndTypeNavigation()
    {
        DocumentationModel model = new() { Packages = [Package(Member("T:MonadicTypes.Option`1"))] };

        string reference = DocumentationOutput.RenderReference(model);

        Assert.Contains("## Packages", reference, StringComparison.Ordinal);
        Assert.Contains("[MonadicTypes](#package-monadictypes)", reference, StringComparison.Ordinal);
        Assert.Contains("**Types:** [`Option<T1>`](#type-optiont1)", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferencePreservesInstallablePackageIdInHeading()
    {
        PackageDocumentation package = new()
        {
            PackageId = "MonadicTypes.NET",
            ProjectName = "MonadicTypes",
            RepositoryUrl = "https://example.invalid/repository",
            Members = [Member("T:MonadicTypes.Option`1")]
        };

        string reference = DocumentationOutput.RenderReference(
            new DocumentationModel { Packages = [package] });

        Assert.Contains("[MonadicTypes.NET](#package-monadictypesnet)", reference, StringComparison.Ordinal);
        Assert.Contains("## Package MonadicTypes.NET", reference, StringComparison.Ordinal);
        Assert.Contains(
            "api-reference.md#package-monadictypesnet",
            DocumentationOutput.RenderManifest(new DocumentationModel { Packages = [package] }),
            StringComparison.Ordinal);
        Assert.Contains(
            "api-reference.md#package-monadictypesnet",
            DocumentationOutput.RenderPackageIndex(package),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestContainsSearchableMemberMetadataAndExactTargets()
    {
        DocumentedMember member = new(
            "M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})",
            "Maps success.",
            string.Empty,
            DisplaySignature: "Result<TResult, E> Map<TResult>(Func<T, TResult> map)",
            DisplayDeclaringType: "Result<T, E>");
        DocumentationModel model = new() { Packages = [Package(member)] };

        string manifest = DocumentationOutput.RenderManifest(model);

        Assert.Contains("\"memberCount\": 1", manifest, StringComparison.Ordinal);
        Assert.Contains(
            "\"id\": \"M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})\"",
            manifest,
            StringComparison.Ordinal);
        Assert.Contains("\"declaringType\": \"Result<T, E>\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"name\": \"Map\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"signature\": \"Result<TResult, E> Map<TResult>(Func<T, TResult> map)\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"summary\": \"Maps success.\"", manifest, StringComparison.Ordinal);
        Assert.Contains(
            "api-reference.md#overload-resulttresult-e-maptresultfunct-tresult-map-on-resultt-e",
            manifest,
            StringComparison.Ordinal);
        Assert.Contains(
            "\"familyReference\": \"api-reference.md#member-resultt-emap\"",
            manifest,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ReadmeApiLinksResolveFromVisibleCSharpLabels()
    {
        DocumentationModel model = new()
        {
            Packages =
            [
                Package(
                    Member("M:MonadicTypes.Result`2.Map``1(System.Func{`0,``0})"),
                    Member("M:MonadicTypes.Result.Ok``1"),
                    Member("M:MonadicTypes.ResultCompositionExtensions.RequireSome``2(MonadicTypes.Result{MonadicTypes.Option{``0},``1}@,System.Func{``1})"),
                    Member("P:MonadicTypes.ValidationErrors.Item(System.Int32)"))
            ]
        };
        const string readme = """
            [`Result<T,E>.Map`](docs/api-reference.md#old-map)
            [`Result.Ok`](docs/api-reference.md#old-ok)
            [`Result<Option<T>,E>.RequireSome`](docs/api-reference.md#old-require)
            [`ValidationErrors.this[int]`](docs/api-reference.md#old-indexer)
            """;

        string updated = DocumentationOutput.RewriteApiReferenceLinks(readme, model);

        Assert.Contains("#member-resultt1-t2map", updated, StringComparison.Ordinal);
        Assert.Contains("#member-resultok", updated, StringComparison.Ordinal);
        Assert.Contains("#member-resultcompositionextensionsrequiresome", updated, StringComparison.Ordinal);
        Assert.Contains("#member-validationerrorsthis", updated, StringComparison.Ordinal);
    }

    [Fact]
    public void TableSummariesPreserveInlineCode()
    {
        string reference = DocumentationOutput.RenderPackage(
            Package(Member("P:MonadicTypes.Option`1.IsSome", "Gets whether this is `Some`.")));

        Assert.Contains("Gets whether this is `Some`.", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("\\`Some\\`", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingExampleDoesNotProduceFabricatedExampleSection()
    {
        DocumentedMember execute = Member("M:Microsoft.AspNetCore.Http.IResult.ExecuteAsync(Microsoft.AspNetCore.Http.HttpContext)");

        string reference = DocumentationOutput.RenderPackage(Package(execute));

        Assert.DoesNotContain("### Example", reference, StringComparison.Ordinal);
        Assert.DoesNotContain("Add a focused example", reference, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedReferenceRejectsMissingFragmentTarget()
    {
        const string reference = "# API Reference\n\n[Missing](#not-present)\n";

        InvalidDataException error = Assert.Throws<InvalidDataException>(
            () => DocumentationOutput.ValidateReference(reference));

        Assert.Contains("not-present", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratedReferenceRejectsDuplicateExplicitFragment()
    {
        const string reference = "<h4 id=\"duplicate\">First</h4>\n<h4 id=\"duplicate\">Second</h4>\n";

        InvalidDataException error = Assert.Throws<InvalidDataException>(
            () => DocumentationOutput.ValidateReference(reference));

        Assert.Contains("duplicate", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DocumentationModelRejectsPublicMemberWithoutSummary()
    {
        const string id = "M:MonadicTypes.Option`1.Map``1(System.Func{`0,``0})";
        DocumentationModel model = new()
        {
            Packages = [Package(new DocumentedMember(id, string.Empty, string.Empty))]
        };

        InvalidDataException error = Assert.Throws<InvalidDataException>(model.Validate);

        Assert.Contains(id, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void XmlReaderConsumesAdjacentPublicMembersAndExamples()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <doc><members>
                  <member name="T:MonadicTypes.Tooling.Tests.AffectedProjectsTests">
                    <summary>First public type.</summary>
                  </member>
                  <member name="T:MonadicTypes.Tooling.Tests.DocumentationOutputTests">
                    <summary>Second public type.</summary>
                    <example><code>DocumentationOutputTests value = new();</code></example>
                  </member>
                </members></doc>
                """);

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);

            DocumentedMember first = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "T:MonadicTypes.Tooling.Tests.AffectedProjectsTests",
                    StringComparison.Ordinal));
            DocumentedMember second = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "T:MonadicTypes.Tooling.Tests.DocumentationOutputTests",
                    StringComparison.Ordinal));
            Assert.Equal("First public type.", first.Summary);
            Assert.Equal("DocumentationOutputTests value = new();", second.XmlExample);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlReaderFormatsLanguageKeywordsAndTypeParameters()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <doc><members>
                  <member name="M:MonadicTypes.Tooling.Tests.DocumentationApiFixture.Echo``1(``0)">
                    <summary>Returns <typeparamref name="T"/> or <see langword="null"/>.</summary>
                  </member>
                </members></doc>
                """);

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);
            DocumentedMember member = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "M:MonadicTypes.Tooling.Tests.DocumentationApiFixture.Echo``1(``0)",
                    StringComparison.Ordinal));

            Assert.Equal("Returns `T` or `null`.", member.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlReaderRetainsPublicMembersMissingFromCompilerDocumentation()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "<doc><members /></doc>");

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);

            DocumentedMember member = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "T:MonadicTypes.Tooling.Tests.DocumentationOutputTests",
                    StringComparison.Ordinal));
            Assert.Empty(member.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlReaderRendersGenericCrefAsReadableMemberName()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <doc><members>
                  <member name="T:MonadicTypes.Tooling.Tests.DocumentationOutputTests">
                    <summary>Constructed by <see cref="M:MonadicTypes.Result`2.Ok(`0)"/>.</summary>
                  </member>
                </members></doc>
                """);

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);

            DocumentedMember member = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "T:MonadicTypes.Tooling.Tests.DocumentationOutputTests",
                    StringComparison.Ordinal));
            Assert.Equal("Constructed by `Ok`.", member.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlReaderPreservesTextAfterInlineCode()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <doc><members>
                  <member name="T:MonadicTypes.Tooling.Tests.DocumentationOutputTests">
                    <summary>Propagates <c>None</c>. No branch executes.</summary>
                  </member>
                </members></doc>
                """);

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);
            DocumentedMember member = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "T:MonadicTypes.Tooling.Tests.DocumentationOutputTests",
                    StringComparison.Ordinal));

            Assert.Equal("Propagates `None`. No branch executes.", member.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReferenceRendersTypeParameterAndParameterDocumentation()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <doc><members>
                  <member name="M:MonadicTypes.Tooling.Tests.DocumentationApiFixture.Echo``1(``0)">
                    <summary>Returns the supplied value.</summary>
                    <typeparam name="T">Value type.</typeparam>
                    <param name="value">Value to return.</param>
                    <returns>The unchanged value.</returns>
                  </member>
                </members></doc>
                """);

            List<DocumentedMember> members = XmlDocumentationReader.Read(
                typeof(DocumentationOutputTests).Assembly.Location,
                path);
            DocumentedMember member = Assert.Single(
                members,
                static candidate => string.Equals(
                    candidate.Id,
                    "M:MonadicTypes.Tooling.Tests.DocumentationApiFixture.Echo``1(``0)",
                    StringComparison.Ordinal));

            string reference = DocumentationOutput.RenderPackage(Package(member));

            Assert.Contains("**Type parameters**", reference, StringComparison.Ordinal);
            Assert.Contains("- `T`: Value type.", reference, StringComparison.Ordinal);
            Assert.Contains("**Parameters**", reference, StringComparison.Ordinal);
            Assert.Contains("- `value`: Value to return.", reference, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PublicApiReaderExcludesExtensionBlockImplementationTypes()
    {
        Dictionary<string, PublicApiMember> members = PublicApiReader.Read(
            typeof(DocumentationOutputTests).Assembly.Location);

        Assert.DoesNotContain(
            members.Keys,
            static id => id.AsSpan().Contains("+<G>$", StringComparison.Ordinal));
    }

    [Fact]
    public void PublicApiReaderExcludesAccessorsAndCompilerGeneratedRecordMembers()
    {
        Dictionary<string, PublicApiMember> members = PublicApiReader.Read(
            typeof(DocumentationOutputTests).Assembly.Location);

        Assert.Contains(
            "P:MonadicTypes.Tooling.Tests.DocumentationRecordFixture.Value",
            members.Keys);
        Assert.DoesNotContain(
            members.Keys,
            static id => id.AsSpan().Contains("DocumentationRecordFixture.get_", StringComparison.Ordinal));
        Assert.DoesNotContain(
            members.Keys,
            static id => id.AsSpan().Contains("DocumentationRecordFixture.op_Equality", StringComparison.Ordinal));
        Assert.DoesNotContain(
            "F:MonadicTypes.Tooling.Tests.DocumentationEnumFixture.value__",
            members.Keys);
    }

    [Fact]
    public void PublicApiReaderPreservesTopLevelNullableAnnotations()
    {
        Dictionary<string, PublicApiMember> members = PublicApiReader.Read(
            typeof(DocumentationOutputTests).Assembly.Location);

        PublicApiMember member = members[
            "M:MonadicTypes.Tooling.Tests.DocumentationApiFixture.NullableEcho(System.String)"];

        Assert.Equal("string? NullableEcho(string? value)", member.Signature);
        Assert.Equal(
            "string? NullableValue",
            members["P:MonadicTypes.Tooling.Tests.DocumentationApiFixture.NullableValue"].Signature);
    }

    [Fact]
    public void ProjectInfoReadsPackageAndAssemblyNamesFromProjectXml()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, """
                <Project>
                  <PropertyGroup>
                    <PackageId>MonadicTypes.NET.Example</PackageId>
                    <AssemblyName>MonadicTypes.Example</AssemblyName>
                  </PropertyGroup>
                </Project>
                """);

            ProjectInfo project = ProjectInfo.Load(path);

            Assert.Equal("MonadicTypes.NET.Example", project.PackageId);
            Assert.Equal("MonadicTypes.Example", project.AssemblyName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static PackageDocumentation Package(params DocumentedMember[] members) => new()
    {
        PackageId = "MonadicTypes",
        ProjectName = "MonadicTypes",
        RepositoryUrl = "https://example.invalid/repository",
        Members = [.. members]
    };

    private static DocumentedMember Member(string id, string summary = "Summary.") => new(id, summary, string.Empty);

    private static string FragmentFor(string reference, string label)
    {
        string prefix = $"[`{label}`](#";
        int start = reference.IndexOf(prefix, StringComparison.Ordinal);
        Assert.True(start >= 0);
        start += prefix.Length;
        int end = reference.IndexOf(')', start);
        Assert.True(end > start);
        return reference[start..end];
    }

    private static string[] ExtractFragments(string reference)
    {
        const string marker = "](#";
        List<string> fragments = [];
        int offset = 0;
        while ((offset = reference.IndexOf(marker, offset, StringComparison.Ordinal)) >= 0)
        {
            int start = offset + marker.Length;
            int end = reference.IndexOf(')', start);
            Assert.True(end > start);
            fragments.Add(reference[start..end]);
            offset = end + 1;
        }

        return [.. fragments];
    }

    private static string[] ExtractOverloadHeadingFragments(string reference)
    {
        List<string> fragments = [];
        ReadOnlySpan<char> content = reference;
        foreach (Range range in content.Split('\n'))
        {
            ReadOnlySpan<char> line = content[range].TrimEnd('\r');
            if (line.StartsWith("##### Overload: `", StringComparison.Ordinal))
            {
                fragments.Add(DocumentationOutput.CreateMarkdownAnchor(line[6..]));
            }
        }

        return [.. fragments];
    }

    private static int Count(string value, string search)
    {
        int count = 0;
        int offset = 0;
        while ((offset = value.IndexOf(search, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += search.Length;
        }

        return count;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "src", "Directory.Build.props")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}

internal static class HiddenApiContainer
{
    public sealed class ExposedNestedType;
}

public static class ExtensionBlockDocumentationFixture
{
    extension<T>(T value)
    {
        public T Identity() => value;
    }
}

public readonly record struct DocumentationRecordFixture(int Value);

public enum DocumentationEnumFixture : byte
{
    Value
}

public static class DocumentationApiFixture
{
    public static T Echo<T>(T value) => value;

    public static string? NullableEcho(string? value) => value;

    public static string? NullableValue => null;
}
