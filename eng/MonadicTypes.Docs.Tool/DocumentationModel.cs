using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;

namespace MonadicTypes.Tooling;

internal sealed class DocumentationModel
{
    public required List<PackageDocumentation> Packages { get; init; }

    public static DocumentationModel Load(string root)
    {
        string sourceRoot = Path.Combine(root, "src");
        string defaultFramework = ProjectInfo.ReadTargetFramework(Path.Combine(root, "Directory.Build.props"));
        string repositoryUrl = ProjectInfo.ReadRepositoryUrl(Path.Combine(root, "src", "Directory.Build.props"));
        string[] projectPaths = Directory.GetFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories);
        Array.Sort(projectPaths, StringComparer.Ordinal);
        PackageDocumentation?[] loaded = new PackageDocumentation?[projectPaths.Length];
        try
        {
            Parallel.For(0, projectPaths.Length, index =>
            {
                string projectPath = projectPaths[index];
                ProjectInfo project = ProjectInfo.Load(projectPath, defaultFramework);
                if (!project.ProjectName.EndsWith(".Analyzers", StringComparison.Ordinal))
                {
                    loaded[index] = LoadPackage(projectPath, project, repositoryUrl);
                }
            });
        }
        catch (AggregateException exception) when (exception.InnerExceptions is [Exception inner])
        {
            ExceptionDispatchInfo.Capture(inner).Throw();
            throw;
        }

        List<PackageDocumentation> packages = new(projectPaths.Length);
        for (int index = 0; index < loaded.Length; index++)
        {
            if (loaded[index] is { } package)
            {
                packages.Add(package);
            }
        }

        packages.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.PackageId, right.PackageId));
        DocumentationModel model = new() { Packages = packages };
        model.Validate();
        return model;
    }

    private static PackageDocumentation LoadPackage(
        string projectPath,
        ProjectInfo project,
        string repositoryUrl)
    {
        string output = Path.Combine(
            Path.GetDirectoryName(projectPath)!,
            "bin",
            "Release",
            project.TargetFramework,
            project.AssemblyName);
        string assemblyPath = output + ".dll";
        string xmlPath = output + ".xml";
        if (!File.Exists(assemblyPath) || !File.Exists(xmlPath))
        {
            throw new InvalidDataException(
                $"No Release assembly and compiler XML pair found for {project.ProjectName}.");
        }

        List<DocumentedMember> members = XmlDocumentationReader.Read(assemblyPath, xmlPath);
        if (members.Count is 0)
        {
            throw new InvalidDataException($"No public XML-documented members found for {project.ProjectName}.");
        }

        return new PackageDocumentation
        {
            PackageId = project.PackageId,
            ProjectName = project.ProjectName,
            Members = members,
            RepositoryUrl = repositoryUrl
        };
    }

    public void Validate()
    {
        StringBuilder? missing = null;
        foreach (PackageDocumentation package in Packages)
        {
            foreach (DocumentedMember member in package.Members)
            {
                if (member.Summary.Length is not 0)
                {
                    continue;
                }

                missing ??= new StringBuilder("Public API members missing XML summaries:");
                missing.Append("\n- ").Append(member.Id);
            }
        }

        if (missing is not null)
        {
            throw new InvalidDataException(missing.ToString());
        }
    }

}

internal sealed class PackageDocumentation
{
    private PackageNavigation? _navigation;

    public required string PackageId { get; init; }
    public required string ProjectName { get; init; }
    public required List<DocumentedMember> Members { get; init; }
    public required string RepositoryUrl { get; init; }

    public PackageNavigation Navigation => _navigation ??= PackageNavigation.Create(this);
}

internal sealed class PackageNavigation
{
    public required string PackageAnchor { get; init; }
    public required string[] TypeAnchors { get; init; }
    public required string[] FamilyAnchors { get; init; }
    public required string[] OverloadAnchors { get; init; }

    public static PackageNavigation Create(PackageDocumentation package)
    {
        int count = package.Members.Count;
        string[] typeAnchors = new string[count];
        string[] familyAnchors = new string[count];
        string[] overloadAnchors = new string[count];
        string? previousType = null;
        string? previousName = null;
        string typeAnchor = string.Empty;
        string familyAnchor = string.Empty;
        for (int index = 0; index < count; index++)
        {
            DocumentedMember member = package.Members[index];
            if (!string.Equals(previousType, member.DeclaringType, StringComparison.Ordinal))
            {
                previousType = member.DeclaringType;
                previousName = null;
                typeAnchor = DocumentationOutput.CreateMarkdownAnchor("Type: ", member.DeclaringType);
            }

            if (member.IsType)
            {
                familyAnchor = typeAnchor;
            }
            else if (!string.Equals(previousName, member.Name, StringComparison.Ordinal))
            {
                previousName = member.Name;
                familyAnchor = DocumentationOutput.CreateMarkdownAnchor(
                    "Member: ",
                    member.DeclaringType,
                    ".",
                    member.Name);
            }

            typeAnchors[index] = typeAnchor;
            familyAnchors[index] = familyAnchor;
            overloadAnchors[index] = member.IsType
                ? typeAnchor
                : DocumentationOutput.CreateMarkdownAnchor(
                    "Overload: ",
                    member.Signature,
                    " on ",
                    member.DeclaringType);
        }

        return new PackageNavigation
        {
            PackageAnchor = DocumentationOutput.CreateMarkdownAnchor("Package ", package.PackageId),
            TypeAnchors = typeAnchors,
            FamilyAnchors = familyAnchors,
            OverloadAnchors = overloadAnchors
        };
    }
}

internal readonly record struct DocumentedMember(
    string Id,
    string Summary,
    string XmlExample,
    string DisplaySignature = "",
    string DisplayDeclaringType = "",
    string Remarks = "",
    string Returns = "",
    ApiExceptionDocumentation[]? Exceptions = null,
    ApiNamedDocumentation[]? Parameters = null,
    ApiNamedDocumentation[]? TypeParameters = null)
{
    public string Label { get; } = DocumentationIdentity.MemberLabel(Id);
    public string DeclaringType { get; } = DisplayDeclaringType.Length is 0
        ? DocumentationIdentity.DeclaringType(Id)
        : DisplayDeclaringType;
    public string Name { get; } = DocumentationIdentity.MemberName(Id);
    public string Signature { get; } = DisplaySignature.Length is 0
        ? DocumentationIdentity.Signature(Id)
        : DisplaySignature;
    public bool IsType => Id is ['T', ':', ..];
}

internal readonly record struct ApiExceptionDocumentation(string Type, string Description);

internal readonly record struct ApiNamedDocumentation(string Name, string Description);

internal sealed class ProjectInfo
{
    public required string ProjectName { get; init; }
    public required string PackageId { get; init; }
    public required string AssemblyName { get; init; }
    public required string TargetFramework { get; init; }

    public static string ReadTargetFramework(string path)
    {
        using XmlReader reader = XmlReader.Create(
            path,
            new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, Name: "TargetFramework" })
            {
                return reader.ReadElementContentAsString();
            }
        }

        throw new InvalidDataException($"TargetFramework is not defined in {path}.");
    }

    public static string ReadRepositoryUrl(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException($"Repository metadata was not found: {path}");
        }

        using XmlReader reader = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, Name: "RepositoryUrl" })
            {
                string value = reader.ReadElementContentAsString().TrimEnd('/');
                return value.EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? value[..^4] : value;
            }
        }

        throw new InvalidDataException($"RepositoryUrl is not defined in {path}.");
    }

    public static ProjectInfo Load(string path, string defaultTargetFramework = "net10.0")
    {
        string projectName = Path.GetFileNameWithoutExtension(path);
        string? packageId = null;
        string? assemblyName = null;
        string? targetFramework = null;
        using XmlReader reader = XmlReader.Create(path, new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        while (!reader.EOF)
        {
            if (reader is
                {
                    NodeType: XmlNodeType.Element,
                    Name: "PackageId" or "AssemblyName" or "TargetFramework"
                })
            {
                string property = reader.Name;
                string value = reader.ReadElementContentAsString();
                (packageId, assemblyName) = property switch
                {
                    "PackageId" => (value, assemblyName),
                    "AssemblyName" => (packageId, value),
                    "TargetFramework" => (packageId, assemblyName),
                    _ => (packageId, assemblyName)
                };
                targetFramework = property is "TargetFramework" ? value : targetFramework;
                continue;
            }

            _ = reader.Read();
        }

        return new ProjectInfo
        {
            ProjectName = projectName,
            PackageId = packageId ?? projectName,
            AssemblyName = assemblyName ?? projectName,
            TargetFramework = targetFramework ?? defaultTargetFramework
        };
    }
}
