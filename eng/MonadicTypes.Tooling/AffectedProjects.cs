using System.Buffers;
using System.Numerics;
using System.Text;

namespace MonadicTypes.Tooling;

internal static class AffectedProjects
{
    private static readonly SearchValues<byte> XmlWhitespace =
        SearchValues.Create(" \t\r\n"u8);

    public static int Run(ReadOnlySpan<string> args)
    {
        if (args is not (["--stdin-z"] or ["--stdin-z", "--ci-output", _]))
        {
            Console.Error.WriteLine(
                "Usage: git diff --name-only -z <base> <head> | " +
                "mt-affected --stdin-z [--ci-output <path>]");
            return 2;
        }

        string repositoryRoot = FindRepositoryRoot(Environment.CurrentDirectory);
        using PooledStrings projects = EnumerateProjects(repositoryRoot);
        using PooledUtf8Paths changedFiles = ReadChangedFilesFromStandardInput();
        int wordCount = (projects.Count + 63) >> 6;
        ulong[]? rented = null;
        Span<ulong> affected = wordCount <= 16
            ? stackalloc ulong[wordCount]
            : (rented = ArrayPool<ulong>.Shared.Rent(wordCount)).AsSpan(0, wordCount);
        affected.Clear();
        try
        {
            ResolveAffected(projects.Span, changedFiles, repositoryRoot, affected);
            if (args is [_, _, var ciOutput])
            {
                WriteCiOutput(projects.Span, changedFiles, affected, ciOutput);
            }

            WriteAffected(projects.Span, affected);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<ulong>.Shared.Return(rented);
            }
        }

        return 0;
    }

    private static void ResolveAffected(
        ReadOnlySpan<string> projects,
        PooledUtf8Paths changedFiles,
        string repositoryRoot,
        Span<ulong> affected)
    {
        if (HasGlobalChange(changedFiles))
        {
            affected.Fill(ulong.MaxValue);
            return;
        }

        MarkDirectChanges(projects, changedFiles, affected);
        MarkToolingTests(projects, changedFiles, affected);
        MarkReverseClosure(projects, repositoryRoot, affected);
    }

    private static PooledStrings EnumerateProjects(string repositoryRoot)
    {
        string solution = Path.Combine(repositoryRoot, "MonadicTypes.slnx");
        int length = checked((int)new FileInfo(solution).Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            ReadOnlySpan<byte> xml = ReadFile(solution, buffer, length);
            var projects = new PooledStrings(32);
            ReadOnlySpan<byte> marker = "<Project Path=\""u8;
            while (xml.IndexOf(marker) is >= 0 and var markerOffset)
            {
                xml = xml[(markerOffset + marker.Length)..];
                int end = xml.IndexOf((byte)'\"');
                if (end < 0)
                {
                    throw new InvalidDataException("Malformed project path in MonadicTypes.slnx.");
                }

                ReadOnlySpan<byte> path = xml[..end];
                if (IsShippingProject(path))
                {
                    projects.Add(Encoding.UTF8.GetString(path));
                }

                xml = xml[(end + 1)..];
            }

            projects.Span.Sort(StringComparer.Ordinal);
            return projects;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static PooledUtf8Paths ReadChangedFilesFromStandardInput()
    {
        const int InitialCapacity = 4 * 1024;
        Stream input = Console.OpenStandardInput();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(InitialCapacity);
        int length = 0;
        try
        {
            while (true)
            {
                if (length == buffer.Length)
                {
                    byte[] larger = ArrayPool<byte>.Shared.Rent(checked(buffer.Length * 2));
                    buffer.AsSpan(0, length).CopyTo(larger);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = larger;
                }

                int read = input.Read(buffer, length, buffer.Length - length);
                if (read == 0)
                {
                    break;
                }

                length += read;
            }

            return new PooledUtf8Paths(buffer, length);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
    }

    private static void MarkDirectChanges(
        ReadOnlySpan<string> projects,
        PooledUtf8Paths changedFiles,
        Span<ulong> affected)
    {
        for (int changedIndex = 0; changedIndex < changedFiles.Count; changedIndex++)
        {
            ReadOnlySpan<byte> changedFile = changedFiles[changedIndex];
            int owner = -1;
            int ownerLength = -1;
            for (int index = 0; index < projects.Length; index++)
            {
                ReadOnlySpan<char> project = projects[index];
                ReadOnlySpan<char> directory = project[..project.LastIndexOf('/')];
                if (directory.Length > ownerLength
                    && StartsWithAscii(changedFile, directory)
                    && changedFile.Length > directory.Length
                    && changedFile[directory.Length] is (byte)'/')
                {
                    owner = index;
                    ownerLength = directory.Length;
                }
            }

            if (owner >= 0)
            {
                Set(affected, owner);
            }
        }
    }

    private static void MarkReverseClosure(
        ReadOnlySpan<string> projects,
        string repositoryRoot,
        Span<ulong> affected)
    {
        int wordCount = affected.Length;
        int matrixLength = checked(projects.Length * wordCount);
        ulong[] matrixArray = ArrayPool<ulong>.Shared.Rent(matrixLength);
        Span<ulong> matrix = matrixArray.AsSpan(0, matrixLength);
        matrix.Clear();
        try
        {
            ReadReferences(projects, repositoryRoot, matrix, wordCount);
            ExpandClosure(projects.Length, matrix, affected);
        }
        finally
        {
            ArrayPool<ulong>.Shared.Return(matrixArray);
        }
    }

    private static void ReadReferences(
        ReadOnlySpan<string> projects,
        string repositoryRoot,
        Span<ulong> references,
        int wordCount)
    {
        for (int index = 0; index < projects.Length; index++)
        {
            string projectPath = Path.Combine(repositoryRoot, projects[index]);
            string projectDirectory = Path.GetDirectoryName(projectPath)!;
            int length = checked((int)new FileInfo(projectPath).Length);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                ReadOnlySpan<byte> xml = ReadFile(projectPath, buffer, length);
                ReadOnlySpan<byte> marker = "<ProjectReference"u8;
                while (xml.IndexOf(marker) is >= 0 and var markerOffset)
                {
                    xml = xml[(markerOffset + marker.Length)..];
                    int tagEnd = xml.IndexOf((byte)'>');
                    if (tagEnd < 0)
                    {
                        throw new InvalidDataException($"Malformed ProjectReference in {projects[index]}.");
                    }

                    ReadOnlySpan<byte> tag = xml[..tagEnd];
                    ReadOnlySpan<byte> includeBytes = ReadAttribute(tag, "Include"u8);
                    if (includeBytes.IsEmpty)
                    {
                        xml = xml[(tagEnd + 1)..];
                        continue;
                    }

                    string include = Encoding.UTF8.GetString(includeBytes);
                    string fullPath = Path.GetFullPath(Path.Combine(projectDirectory, include));
                    string relative = Path.GetRelativePath(repositoryRoot, fullPath);
                    int referenceIndex = BinarySearchNormalized(projects, relative);
                    if (referenceIndex >= 0)
                    {
                        Set(references.Slice(index * wordCount, wordCount), referenceIndex);
                    }

                    xml = xml[(tagEnd + 1)..];
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    private static int BinarySearchNormalized(ReadOnlySpan<string> values, ReadOnlySpan<char> value)
    {
        int low = 0;
        int high = values.Length - 1;
        while (low <= high)
        {
            int middle = (int)((uint)(low + high) >> 1);
            int comparison = CompareNormalized(values[middle], value);
            if (comparison is 0)
            {
                return middle;
            }

            if (comparison < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return ~low;
    }

    private static int CompareNormalized(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        int length = Math.Min(left.Length, right.Length);
        for (int index = 0; index < length; index++)
        {
            int leftValue = left[index] is '\\' ? '/' : left[index];
            int rightValue = right[index] is '\\' ? '/' : right[index];
            int difference = leftValue - rightValue;
            if (difference is not 0)
            {
                return difference;
            }
        }

        return left.Length - right.Length;
    }

    private static ReadOnlySpan<byte> ReadAttribute(
        ReadOnlySpan<byte> tag,
        ReadOnlySpan<byte> name)
    {
        while (tag.IndexOf(name) is >= 0 and var nameOffset)
        {
            bool validStart = nameOffset is 0 || XmlWhitespace.Contains(tag[nameOffset - 1]);
            tag = tag[(nameOffset + name.Length)..];
            int next = tag.IndexOfAnyExcept(XmlWhitespace);
            if (!validStart || next < 0 || tag[next] is not (byte)'=')
            {
                continue;
            }

            ReadOnlySpan<byte> value = tag[(next + 1)..];
            int start = value.IndexOfAnyExcept(XmlWhitespace);
            value = start < 0 ? [] : value[start..];
            if (value is not [(byte)'\"' or (byte)'\'', ..])
            {
                return [];
            }

            byte quote = value[0];
            value = value[1..];
            int end = value.IndexOf(quote);
            return end < 0 ? [] : value[..end];
        }

        return [];
    }

    private static ReadOnlySpan<byte> ReadFile(string path, byte[] buffer, int length)
    {
        using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1,
            FileOptions.SequentialScan);
        stream.ReadExactly(buffer.AsSpan(0, length));
        return buffer.AsSpan(0, length);
    }

    private static bool IsShippingProject(ReadOnlySpan<byte> path) => path switch
    {
        _ when path.StartsWith("src/"u8)
            || path.StartsWith("benchmarks/"u8) => true,
        _ when path.StartsWith("tests/"u8)
            && !path.StartsWith("tests/MonadicTypes.PackageSmoke/"u8) => true,
        _ => false
    };

    private static void WriteCiOutput(
        ReadOnlySpan<string> projects,
        PooledUtf8Paths changedFiles,
        ReadOnlySpan<ulong> affected,
        string output)
    {
        ToolChanges changes = ToolChanges.None;
        for (int index = 0; index < changedFiles.Count; index++)
        {
            changes |= GetToolChange(changedFiles[index]);
        }

        int maxLength = 0;
        for (int index = 0; index < projects.Length; index++)
        {
            maxLength = Math.Max(maxLength, projects[index].Length);
        }

        byte[] utf8Buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(maxLength));
        try
        {
            using FileStream stream = new(output, FileMode.Create, FileAccess.Write, FileShare.Read);
            stream.Write("projects="u8);
            (bool hasProjects, bool hasAot, bool hasPackages) =
                WriteProjects(stream, projects, affected, utf8Buffer);
            hasPackages |= HasPackageChanges(changedFiles, changes);

            stream.Write("\nhas-projects="u8);
            WriteBoolean(stream, hasProjects);
            stream.Write("\naot="u8);
            WriteBoolean(stream, hasAot);
            stream.Write("\npackages="u8);
            WriteBoolean(stream, hasPackages);
            stream.Write("\ntooling-projects=["u8);
            bool hasPrevious = false;
            WriteTool(stream, changes, ToolChanges.Affected, "MonadicTypes.AffectedProjects.Tool"u8, ref hasPrevious);
            WriteTool(stream, changes, ToolChanges.Pack, "MonadicTypes.Pack.Tool"u8, ref hasPrevious);
            WriteTool(stream, changes, ToolChanges.TestPackages, "MonadicTypes.TestPackages.Tool"u8, ref hasPrevious);
            WriteTool(stream, changes, ToolChanges.VerifyLockfiles, "MonadicTypes.VerifyLockfiles.Tool"u8, ref hasPrevious);
            stream.Write("]\ntooling="u8);
            WriteBoolean(stream, changes is not ToolChanges.None);
            stream.WriteByte((byte)'\n');
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(utf8Buffer);
        }
    }

    private static (bool HasProjects, bool HasAot, bool HasPackages) WriteProjects(
        Stream stream,
        ReadOnlySpan<string> projects,
        ReadOnlySpan<ulong> affected,
        byte[] utf8Buffer)
    {
        bool hasProjects = false;
        bool hasAot = false;
        bool hasPackages = false;
        for (int index = 0; index < projects.Length; index++)
        {
            if (!IsSet(affected, index))
            {
                continue;
            }

            if (hasProjects)
            {
                stream.WriteByte((byte)';');
            }

            string project = projects[index];
            WriteUtf8(stream, project, utf8Buffer);
            hasProjects = true;
            hasAot |= project.StartsWith("tests/", StringComparison.Ordinal)
                && project.Contains("AotSmoke/", StringComparison.Ordinal);
            hasPackages |= project.StartsWith("src/", StringComparison.Ordinal);
        }

        return (hasProjects, hasAot, hasPackages);
    }

    private static void WriteUtf8(Stream stream, string value, byte[] buffer)
    {
        int count = Encoding.UTF8.GetBytes(value, buffer);
        stream.Write(buffer, 0, count);
    }

    private static void WriteBoolean(Stream stream, bool value) =>
        stream.Write(value ? "true"u8 : "false"u8);

    private static void WriteTool(
        Stream stream,
        ToolChanges changes,
        ToolChanges expected,
        ReadOnlySpan<byte> name,
        ref bool hasPrevious)
    {
        if ((changes & expected) is ToolChanges.None)
        {
            return;
        }

        if (hasPrevious)
        {
            stream.Write(","u8);
        }

        stream.WriteByte((byte)'\"');
        stream.Write(name);
        stream.WriteByte((byte)'\"');
        hasPrevious = true;
    }

    private static ToolChanges GetToolChange(ReadOnlySpan<byte> path) => path switch
    {
        _ when path.SequenceEqual("eng/NativeTool.props"u8) => ToolChanges.All,
        _ when path.SequenceEqual("eng/SemanticVersion.cs"u8) =>
            ToolChanges.Pack | ToolChanges.TestPackages,
        _ when path.SequenceEqual("eng/Pack.cs"u8) => ToolChanges.Pack,
        _ when path.SequenceEqual("eng/TestPackages.cs"u8) => ToolChanges.TestPackages,
        _ when path.SequenceEqual("eng/VerifyLockfiles.cs"u8) => ToolChanges.VerifyLockfiles,
        _ when path.StartsWith("eng/MonadicTypes.AffectedProjects.Tool/"u8)
            || path.StartsWith("eng/MonadicTypes.Tooling/"u8) => ToolChanges.Affected,
        _ when path.StartsWith("eng/MonadicTypes.Pack.Tool/"u8) => ToolChanges.Pack,
        _ when path.StartsWith("eng/MonadicTypes.TestPackages.Tool/"u8) =>
            ToolChanges.TestPackages,
        _ when path.StartsWith("eng/MonadicTypes.VerifyLockfiles.Tool/"u8) =>
            ToolChanges.VerifyLockfiles,
        _ when path.StartsWith("eng/tools/"u8)
            && (path.EndsWith("/mt-affected"u8) || path.EndsWith("/mt-affected.exe"u8)) =>
            ToolChanges.Affected,
        _ when path.StartsWith("eng/tools/"u8)
            && (path.EndsWith("/mt-pack"u8) || path.EndsWith("/mt-pack.exe"u8)) =>
            ToolChanges.Pack,
        _ when path.StartsWith("eng/tools/"u8)
            && (path.EndsWith("/mt-test-packages"u8) || path.EndsWith("/mt-test-packages.exe"u8)) =>
            ToolChanges.TestPackages,
        _ when path.StartsWith("eng/tools/"u8)
            && (path.EndsWith("/mt-verify-locks"u8) || path.EndsWith("/mt-verify-locks.exe"u8)) =>
            ToolChanges.VerifyLockfiles,
        _ => ToolChanges.None
    };

    internal static bool IsPackageChange(ReadOnlySpan<byte> path) => path switch
    {
        _ when path.StartsWith("tests/MonadicTypes.PackageSmoke/"u8) => true,
        _ when path.SequenceEqual("eng/Pack.proj"u8)
            || path.SequenceEqual("eng/TestPackages.proj"u8) => true,
        _ => GetToolChange(path) is ToolChanges.Pack or ToolChanges.TestPackages
    };

    internal static bool RequiresToolCompilation(ReadOnlySpan<byte> path) =>
        GetToolChange(path) is not ToolChanges.None;

    private static bool HasPackageChanges(PooledUtf8Paths changedFiles, ToolChanges changes)
    {
        if ((changes & (ToolChanges.Pack | ToolChanges.TestPackages)) is not ToolChanges.None)
        {
            return true;
        }

        for (int index = 0; index < changedFiles.Count; index++)
        {
            if (IsPackageChange(changedFiles[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void MarkToolingTests(
        ReadOnlySpan<string> projects,
        PooledUtf8Paths changedFiles,
        Span<ulong> affected)
    {
        for (int index = 0; index < changedFiles.Count; index++)
        {
            if (!RequiresToolCompilation(changedFiles[index]))
            {
                continue;
            }

            int testProject = BinarySearchNormalized(
                projects,
                "tests/MonadicTypes.Tooling.Tests/MonadicTypes.Tooling.Tests.csproj");
            if (testProject >= 0)
            {
                Set(affected, testProject);
            }

            return;
        }
    }

    private static void ExpandClosure(
        int projectCount,
        ReadOnlySpan<ulong> references,
        Span<ulong> affected)
    {
        bool changed;
        do
        {
            changed = false;
            for (int index = 0; index < projectCount; index++)
            {
                ReadOnlySpan<ulong> row = references.Slice(index * affected.Length, affected.Length);
                if (!IsSet(affected, index) && Intersects(row, affected))
                {
                    Set(affected, index);
                    changed = true;
                }
            }
        }
        while (changed);
    }

    private static bool Intersects(ReadOnlySpan<ulong> left, ReadOnlySpan<ulong> right)
    {
        int index = 0;
        if (Vector.IsHardwareAccelerated && left.Length >= Vector<ulong>.Count)
        {
            int vectorEnd = left.Length - Vector<ulong>.Count;
            for (; index <= vectorEnd; index += Vector<ulong>.Count)
            {
                var leftVector = new Vector<ulong>(left[index..]);
                var rightVector = new Vector<ulong>(right[index..]);
                if (!Vector.EqualsAll(leftVector & rightVector, Vector<ulong>.Zero))
                {
                    return true;
                }
            }
        }

        for (; index < left.Length; index++)
        {
            if ((left[index] & right[index]) is not 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasGlobalChange(PooledUtf8Paths changedFiles)
    {
        for (int index = 0; index < changedFiles.Count; index++)
        {
            if (IsGlobalChange(changedFiles[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static void WriteAffected(ReadOnlySpan<string> projects, ReadOnlySpan<ulong> affected)
    {
        for (int index = 0; index < projects.Length; index++)
        {
            if (IsSet(affected, index))
            {
                Console.WriteLine(projects[index]);
            }
        }
    }

    internal static bool IsGlobalChange(ReadOnlySpan<byte> path) => path switch
    {
        _ when path.SequenceEqual(".editorconfig"u8)
            || path.SequenceEqual(".gitattributes"u8)
            || path.SequenceEqual("BannedSymbols.txt"u8)
            || path.SequenceEqual("Directory.Build.props"u8)
            || path.SequenceEqual("Directory.Build.targets"u8)
            || path.SequenceEqual("Directory.Packages.props"u8)
            || path.SequenceEqual("global.json"u8)
            || path.SequenceEqual("MonadicTypes.slnx"u8)
            || path.SequenceEqual("src/Directory.Build.props"u8) => true,
        _ when path.StartsWith(".github/"u8)
            || path.StartsWith("docs/package-readmes/"u8) => true,
        _ => false
    };

    private static bool StartsWithAscii(ReadOnlySpan<byte> utf8, ReadOnlySpan<char> ascii)
    {
        if (utf8.Length < ascii.Length)
        {
            return false;
        }

        for (int index = 0; index < ascii.Length; index++)
        {
            if (utf8[index] != ascii[index])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSet(ReadOnlySpan<ulong> bits, int index) =>
        (bits[index >> 6] & (1UL << (index & 63))) is not 0;

    private static void Set(Span<ulong> bits, int index) =>
        bits[index >> 6] |= 1UL << (index & 63);

    private static string FindRepositoryRoot(string startPath)
    {
        if (File.Exists(Path.Combine(startPath, "MonadicTypes.slnx")))
        {
            return startPath;
        }

        for (DirectoryInfo? directory = new(startPath); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MonadicTypes.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }

    private ref struct PooledStrings(int capacity)
    {
        private string[] _buffer = ArrayPool<string>.Shared.Rent(capacity);

        public int Count { get; private set; }

        public readonly Span<string> Span => _buffer.AsSpan(0, Count);

        public void Add(string value)
        {
            if (Count == _buffer.Length)
            {
                Grow();
            }

            _buffer[Count++] = value;
        }

        public void Dispose()
        {
            Array.Clear(_buffer, 0, Count);
            ArrayPool<string>.Shared.Return(_buffer);
            _buffer = [];
            Count = 0;
        }

        private void Grow()
        {
            string[] larger = ArrayPool<string>.Shared.Rent(checked(_buffer.Length * 2));
            _buffer.AsSpan(0, Count).CopyTo(larger);
            Array.Clear(_buffer, 0, Count);
            ArrayPool<string>.Shared.Return(_buffer);
            _buffer = larger;
        }
    }

    private ref struct PooledUtf8Paths
    {
        private byte[] _bytes;
        private int[] _ranges;

        public PooledUtf8Paths(byte[] bytes, int length)
        {
            _bytes = bytes;
            _ranges = ArrayPool<int>.Shared.Rent(Math.Max(16, CountPaths(bytes.AsSpan(0, length)) * 2));
            Count = ParseRanges(bytes.AsSpan(0, length), _ranges);
        }

        public int Count { get; private set; }

        public readonly ReadOnlySpan<byte> this[int index] =>
            _bytes.AsSpan(_ranges[index * 2], _ranges[(index * 2) + 1]);

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_bytes);
            ArrayPool<int>.Shared.Return(_ranges);
            _bytes = [];
            _ranges = [];
            Count = 0;
        }

        private static int CountPaths(ReadOnlySpan<byte> bytes)
        {
            int count = 0;
            while (!bytes.IsEmpty)
            {
                int separator = bytes.IndexOf((byte)0);
                int length = separator < 0 ? bytes.Length : separator;
                count += length > 0 ? 1 : 0;
                bytes = separator < 0 ? [] : bytes[(separator + 1)..];
            }

            return count;
        }

        private static int ParseRanges(ReadOnlySpan<byte> bytes, Span<int> ranges)
        {
            int count = 0;
            int absoluteOffset = 0;
            while (!bytes.IsEmpty)
            {
                int separator = bytes.IndexOf((byte)0);
                int length = separator < 0 ? bytes.Length : separator;
                if (length > 0)
                {
                    ranges[count * 2] = absoluteOffset;
                    ranges[(count * 2) + 1] = length;
                    count++;
                }

                int consumed = separator < 0 ? bytes.Length : separator + 1;
                absoluteOffset += consumed;
                bytes = separator < 0 ? [] : bytes[consumed..];
            }

            return count;
        }
    }

    [Flags]
    private enum ToolChanges : byte
    {
        None = 0,
        Affected = 1,
        Pack = 2,
        TestPackages = 4,
        VerifyLockfiles = 8,
        All = Affected | Pack | TestPackages | VerifyLockfiles
    }
}
