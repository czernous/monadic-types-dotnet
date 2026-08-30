using System.Buffers;

namespace MonadicTypes.Tooling;

internal static class MarkdownLinkValidator
{
    public static void Validate(string root, string? repositoryUrl = null)
    {
        string fullRoot = Path.GetFullPath(root);
        Dictionary<string, HashSet<string>> anchors = new(PathComparer());
        ValidateIfPresent(Path.Combine(fullRoot, "README.md"), fullRoot, repositoryUrl, anchors);
        ValidateIfPresent(Path.Combine(fullRoot, "CHANGELOG.md"), fullRoot, repositoryUrl, anchors);

        string docs = Path.Combine(fullRoot, "docs");
        if (!Directory.Exists(docs))
        {
            return;
        }

        foreach (string path in Directory.EnumerateFiles(docs, "*.md", SearchOption.AllDirectories))
        {
            if (!path.Contains(Path.DirectorySeparatorChar + "superpowers" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                ValidateFile(path, fullRoot, repositoryUrl, anchors);
            }
        }
    }

    private static void ValidateIfPresent(
        string path,
        string root,
        string? repositoryUrl,
        Dictionary<string, HashSet<string>> anchors)
    {
        if (File.Exists(path))
        {
            ValidateFile(path, root, repositoryUrl, anchors);
        }
    }

    private static void ValidateFile(
        string sourcePath,
        string root,
        string? repositoryUrl,
        Dictionary<string, HashSet<string>> anchors)
    {
        string content = File.ReadAllText(sourcePath);
        ReadOnlySpan<char> document = content;
        bool fenced = false;
        foreach (Range range in document.Split('\n'))
        {
            ReadOnlySpan<char> line = document[range].TrimEnd('\r');
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                fenced = !fenced;
                continue;
            }

            if (!fenced)
            {
                ValidateLine(line, sourcePath, root, repositoryUrl, anchors);
            }
        }
    }

    private static void ValidateLine(
        ReadOnlySpan<char> line,
        string sourcePath,
        string root,
        string? repositoryUrl,
        Dictionary<string, HashSet<string>> anchors)
    {
        int offset = 0;
        while (offset < line.Length)
        {
            int relativeStart = line[offset..].IndexOf("](", StringComparison.Ordinal);
            if (relativeStart < 0)
            {
                return;
            }

            int start = offset + relativeStart + 2;
            int relativeEnd = line[start..].IndexOf(')');
            if (relativeEnd < 0)
            {
                throw new InvalidDataException($"Incomplete Markdown link in {Relative(root, sourcePath)}.");
            }

            ValidateTarget(
                line.Slice(start, relativeEnd).Trim(),
                sourcePath,
                root,
                repositoryUrl,
                anchors);
            offset = start + relativeEnd + 1;
        }
    }

    private static void ValidateTarget(
        ReadOnlySpan<char> target,
        string sourcePath,
        string root,
        string? repositoryUrl,
        Dictionary<string, HashSet<string>> anchors)
    {
        if (target.IsEmpty)
        {
            throw new InvalidDataException(
                $"Markdown link target is empty: {Relative(root, sourcePath)}.");
        }

        bool repositoryTarget = TryMapRepositoryTarget(
            target,
            repositoryUrl,
            out ReadOnlySpan<char> relative,
            out ReadOnlySpan<char> fragment);
        if (!repositoryTarget)
        {
            if (IsExternal(target))
            {
                return;
            }

            int fragmentSeparator = target.IndexOf('#');
            relative = fragmentSeparator < 0 ? target : target[..fragmentSeparator];
            fragment = fragmentSeparator < 0 ? [] : target[(fragmentSeparator + 1)..];
        }

        string targetPath = relative.IsEmpty
            ? sourcePath
            : Path.GetFullPath(Path.Combine(
                repositoryTarget ? root : Path.GetDirectoryName(sourcePath)!,
                NormalizePath(relative)));
        if (!IsWithinRoot(targetPath, root) || !File.Exists(targetPath))
        {
            throw new InvalidDataException(
                $"Local Markdown link target does not exist: {Relative(root, sourcePath)} -> {target.ToString()}.");
        }

        if (fragment.IsEmpty)
        {
            return;
        }

        HashSet<string> targetAnchors = GetAnchors(targetPath, anchors);
        if (!targetAnchors.GetAlternateLookup<ReadOnlySpan<char>>().Contains(fragment))
        {
            throw new InvalidDataException(
                $"Local Markdown fragment does not exist: {Relative(root, sourcePath)} -> {target.ToString()}.");
        }
    }

    private static bool TryMapRepositoryTarget(
        ReadOnlySpan<char> target,
        string? repositoryUrl,
        out ReadOnlySpan<char> relative,
        out ReadOnlySpan<char> fragment)
    {
        relative = [];
        fragment = [];
        if (repositoryUrl is null
            || !target.StartsWith(repositoryUrl, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        ReadOnlySpan<char> suffix = target[repositoryUrl.Length..];
        if (suffix.IsEmpty)
        {
            relative = "README.md";
            return true;
        }

        if (suffix[0] is '#')
        {
            relative = "README.md";
            fragment = suffix[1..];
            return true;
        }

        const string blob = "/blob/";
        if (!suffix.StartsWith(blob, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        ReadOnlySpan<char> path = suffix[blob.Length..];
        int referenceEnd = path.IndexOf('/');
        if (referenceEnd < 0 || referenceEnd == path.Length - 1)
        {
            return false;
        }

        path = path[(referenceEnd + 1)..];
        int fragmentStart = path.IndexOf('#');
        relative = fragmentStart < 0 ? path : path[..fragmentStart];
        fragment = fragmentStart < 0 ? [] : path[(fragmentStart + 1)..];
        return true;
    }

    private static HashSet<string> GetAnchors(
        string path,
        Dictionary<string, HashSet<string>> cache)
    {
        if (cache.TryGetValue(path, out HashSet<string>? anchors))
        {
            return anchors;
        }

        anchors = new HashSet<string>(StringComparer.Ordinal);
        string content = File.ReadAllText(path);
        ReadOnlySpan<char> document = content;
        foreach (Range range in document.Split('\n'))
        {
            ReadOnlySpan<char> line = document[range].TrimEnd('\r');
            AddMarkdownHeading(line, anchors);
            AddExplicitHeadings(line, anchors);
        }

        cache.Add(path, anchors);
        return anchors;
    }

    private static void AddMarkdownHeading(ReadOnlySpan<char> line, HashSet<string> anchors)
    {
        int level = 0;
        while (level < line.Length && level < 6 && line[level] is '#')
        {
            level++;
        }

        if (level is 0 || level >= line.Length || line[level] is not ' ')
        {
            return;
        }

        ReadOnlySpan<char> heading = line[(level + 1)..].Trim();
        while (!heading.IsEmpty && heading[^1] is '#')
        {
            heading = heading[..^1].TrimEnd();
        }

        if (!heading.IsEmpty)
        {
            anchors.Add(DocumentationOutput.CreateMarkdownAnchor(heading));
        }
    }

    private static void AddExplicitHeadings(ReadOnlySpan<char> line, HashSet<string> anchors)
    {
        const string marker = " id=\"";
        int offset = 0;
        while (offset < line.Length)
        {
            int relative = line[offset..].IndexOf(marker, StringComparison.Ordinal);
            if (relative < 0)
            {
                return;
            }

            int start = offset + relative + marker.Length;
            int end = line[start..].IndexOf('"');
            if (end < 0)
            {
                return;
            }

            anchors.Add(line.Slice(start, end).ToString());
            offset = start + end + 1;
        }
    }

    private static bool IsExternal(ReadOnlySpan<char> target) => target switch
    {
        ['h' or 'H', 't' or 'T', 't' or 'T', 'p' or 'P', ':', ..] => true,
        ['h' or 'H', 't' or 'T', 't' or 'T', 'p' or 'P', 's' or 'S', ':', ..] => true,
        ['m' or 'M', 'a' or 'A', 'i' or 'I', 'l' or 'L', 't' or 'T', 'o' or 'O', ':', ..] => true,
        _ => false
    };

    private static string NormalizePath(ReadOnlySpan<char> path)
    {
        char[]? rented = null;
        Span<char> buffer = path.Length <= 256
            ? stackalloc char[path.Length]
            : (rented = ArrayPool<char>.Shared.Rent(path.Length));
        try
        {
            for (int index = 0; index < path.Length; index++)
            {
                buffer[index] = path[index] is '/' or '\\' ? Path.DirectorySeparatorChar : path[index];
            }

            return new string(buffer[..path.Length]);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    private static bool IsWithinRoot(string path, string root) => path.StartsWith(root, PathComparison())
        && (path.Length == root.Length || path[root.Length] is '\\' or '/');

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static StringComparer PathComparer() => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static StringComparison PathComparison() => OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
}
