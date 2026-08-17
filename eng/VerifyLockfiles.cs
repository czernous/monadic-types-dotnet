using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonadicTypes.Tooling;

internal static class VerifyLockfiles
{
    private const int InitialBufferSize = 4096;
    private const int MaximumJsonDepth = 64;
    private static readonly SearchValues<byte> JsonWhitespace =
        SearchValues.Create(" \t\r\n"u8);
    private static readonly SearchValues<byte> JsonStringSpecial = SearchValues.Create(
    [
        0, 1, 2, 3, 4, 5, 6, 7,
        8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23,
        24, 25, 26, 27, 28, 29, 30, 31,
        (byte)'\"', (byte)'\\'
    ]);

    public static int Run(ReadOnlySpan<string> args)
    {
        if (!args.IsEmpty)
        {
            Console.Error.WriteLine("Usage: mt-verify-locks");
            return 2;
        }

        string repositoryRoot = FindRepositoryRoot(Environment.CurrentDirectory);
        bool hasViolations = false;
        int lockFileCount = VerifyShippingLockFiles(repositoryRoot, ref hasViolations);

        if (!hasViolations)
        {
            Console.WriteLine($"Verified {lockFileCount} runtime-identifier-neutral shipping lock files.");
            return 0;
        }

        return 1;
    }

    private static int VerifyShippingLockFiles(
        string repositoryRoot,
        ref bool hasViolations)
    {
        string solution = Path.Combine(repositoryRoot, "MonadicTypes.slnx");
        using FileStream stream = OpenLockFile(solution);
        int length = checked((int)stream.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, InitialBufferSize));
        try
        {
            stream.ReadExactly(buffer.AsSpan(0, length));
            ReadOnlySpan<byte> xml = buffer.AsSpan(0, length);
            ReadOnlySpan<byte> marker = "<Project Path=\"src/"u8;
            int count = 0;
            while (xml.IndexOf(marker) is >= 0 and var markerOffset)
            {
                xml = xml[(markerOffset + marker.Length)..];
                int end = xml.IndexOf((byte)'\"');
                int directoryEnd = end < 0 ? -1 : xml[..end].LastIndexOf((byte)'/');
                if (directoryEnd < 0)
                {
                    throw new InvalidDataException("Malformed source project path in MonadicTypes.slnx.");
                }

                string lockFile = BuildLockFilePath(repositoryRoot, xml[..directoryEnd]);
                if (File.Exists(lockFile))
                {
                    count++;
                    VerifyLockFile(lockFile, repositoryRoot, ref hasViolations);
                }
                else
                {
                    AddViolation(
                        ref hasViolations,
                        repositoryRoot,
                        lockFile,
                        "missing packages.lock.json");
                }
                xml = xml[(end + 1)..];
            }

            return count;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static string BuildLockFilePath(
        ReadOnlySpan<char> repositoryRoot,
        ReadOnlySpan<byte> utf8Directory)
    {
        const int StackLimit = 512;
        if (utf8Directory.Contains((byte)'\\'))
        {
            throw new InvalidDataException("Solution project paths must use forward slashes.");
        }

        ReadOnlySpan<char> prefix = "src";
        ReadOnlySpan<char> fileName = "packages.lock.json";
        int capacity = repositoryRoot.Length + prefix.Length + utf8Directory.Length + fileName.Length + 3;
        char[]? rented = null;
        Span<char> path = capacity <= StackLimit
            ? stackalloc char[capacity]
            : (rented = ArrayPool<char>.Shared.Rent(capacity)).AsSpan(0, capacity);
        try
        {
            int offset = 0;
            repositoryRoot.CopyTo(path);
            offset += repositoryRoot.Length;
            path[offset++] = Path.DirectorySeparatorChar;
            prefix.CopyTo(path[offset..]);
            offset += prefix.Length;
            path[offset++] = Path.DirectorySeparatorChar;
            int written = Encoding.UTF8.GetChars(utf8Directory, path[offset..]);
            offset += written;
            path[offset++] = Path.DirectorySeparatorChar;
            fileName.CopyTo(path[offset..]);
            offset += fileName.Length;
            return new string(path[..offset]);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    private static void VerifyLockFile(
        string lockFile,
        string repositoryRoot,
        ref bool hasViolations)
    {
        using FileStream stream = OpenLockFile(lockFile);
        int length = checked((int)stream.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, InitialBufferSize));
        try
        {
            stream.ReadExactly(buffer.AsSpan(0, length));
            var reader = new LockJsonReader(buffer.AsSpan(0, length));
            if (!reader.TryRead(out bool foundDependencies, out ReadOnlySpan<byte> runtimeTarget))
            {
                AddViolation(
                    ref hasViolations,
                    repositoryRoot,
                    lockFile,
                    $"invalid JSON at byte {reader.Offset}");
            }
            else if (!foundDependencies)
            {
                AddViolation(ref hasViolations, repositoryRoot, lockFile, "missing dependencies object");
            }
            else if (!runtimeTarget.IsEmpty)
            {
                AddViolation(
                    ref hasViolations,
                    repositoryRoot,
                    lockFile,
                    $"runtime-specific target '{Encoding.UTF8.GetString(runtimeTarget)}'");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static FileStream OpenLockFile(string lockFile) => new(
        lockFile,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        InitialBufferSize,
        FileOptions.SequentialScan);

    private static void AddViolation(
        ref bool hasViolations,
        string repositoryRoot,
        string lockFile,
        string message)
    {
        if (!hasViolations)
        {
            Console.Error.WriteLine("Shipping lock files must remain runtime-identifier neutral:");
            hasViolations = true;
        }

        Console.Error.Write("  ");
        Console.Error.Write(Path.GetRelativePath(repositoryRoot, lockFile));
        Console.Error.Write(": ");
        Console.Error.WriteLine(message);
    }

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

    internal ref struct LockJsonReader(ReadOnlySpan<byte> json)
    {
        private readonly ReadOnlySpan<byte> _json = json;
        private int _offset;
        private bool _foundDependencies;
        private ReadOnlySpan<byte> _runtimeTarget;

        public readonly int Offset => _offset;

        public bool TryRead(out bool foundDependencies, out ReadOnlySpan<byte> runtimeTarget)
        {
            SkipWhitespace();
            bool valid = ParseObject(isRoot: true, isDependencies: false, depth: 0);
            SkipWhitespace();
            foundDependencies = _foundDependencies;
            runtimeTarget = _runtimeTarget;
            return valid && _offset == _json.Length;
        }

        private bool ParseObject(bool isRoot, bool isDependencies, int depth)
        {
            if (depth >= MaximumJsonDepth || !Consume((byte)'{'))
            {
                return false;
            }

            SkipWhitespace();
            if (Consume((byte)'}'))
            {
                return true;
            }

            while (_offset < _json.Length)
            {
                if (!ParseString(out ReadOnlySpan<byte> key, out bool escaped))
                {
                    return false;
                }

                SkipWhitespace();
                if (!Consume((byte)':'))
                {
                    return false;
                }

                SkipWhitespace();
                if (!ParsePropertyValue(isRoot, isDependencies, key, escaped, depth))
                {
                    return false;
                }

                SkipWhitespace();
                if (Consume((byte)'}'))
                {
                    return true;
                }

                if (!Consume((byte)','))
                {
                    return false;
                }

                SkipWhitespace();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ParsePropertyValue(
            bool isRoot,
            bool isDependencies,
            ReadOnlySpan<byte> key,
            bool escaped,
            int depth)
        {
            if (isDependencies && _runtimeTarget.IsEmpty && ContainsSlash(key, escaped))
            {
                _runtimeTarget = key;
            }

            if (!isRoot || !JsonStringEquals(key, escaped, "dependencies"u8))
            {
                return ParseValue(depth + 1);
            }

            _foundDependencies = true;
            return ParseObject(isRoot: false, isDependencies: true, depth: depth + 1);
        }

        private bool ParseArray(int depth)
        {
            if (depth >= MaximumJsonDepth || !Consume((byte)'['))
            {
                return false;
            }

            SkipWhitespace();
            if (Consume((byte)']'))
            {
                return true;
            }

            while (ParseValue(depth + 1))
            {
                SkipWhitespace();
                if (Consume((byte)']'))
                {
                    return true;
                }

                if (!Consume((byte)','))
                {
                    return false;
                }

                SkipWhitespace();
            }

            return false;
        }

        private bool ParseValue(int depth)
        {
            SkipWhitespace();
            return Peek() switch
            {
                (byte)'{' => ParseObject(isRoot: false, isDependencies: false, depth),
                (byte)'[' => ParseArray(depth),
                (byte)'\"' => ParseString(out _, out _),
                (byte)'t' => ConsumeLiteral("true"u8),
                (byte)'f' => ConsumeLiteral("false"u8),
                (byte)'n' => ConsumeLiteral("null"u8),
                (byte)'-' or >= (byte)'0' and <= (byte)'9' => ParseNumber(),
                _ => false
            };
        }

        private bool ParseString(out ReadOnlySpan<byte> value, out bool escaped)
        {
            value = [];
            escaped = false;
            if (!Consume((byte)'\"'))
            {
                return false;
            }

            int start = _offset;
            while (_offset < _json.Length)
            {
                int special = _json[_offset..].IndexOfAny(JsonStringSpecial);
                if (special < 0)
                {
                    _offset = _json.Length;
                    return false;
                }

                _offset += special;
                byte current = _json[_offset++];
                if (current is (byte)'\"')
                {
                    value = _json[start..(_offset - 1)];
                    return true;
                }

                if (current < 0x20)
                {
                    return false;
                }

                if (current is not (byte)'\\')
                {
                    continue;
                }

                escaped = true;
                if (_offset >= _json.Length)
                {
                    return false;
                }

                byte escape = _json[_offset++];
                if (escape is (byte)'\"' or (byte)'\\' or (byte)'/'
                    or (byte)'b' or (byte)'f' or (byte)'n' or (byte)'r' or (byte)'t')
                {
                    continue;
                }

                if (escape is not (byte)'u' || !ConsumeHex4())
                {
                    return false;
                }
            }

            return false;
        }

        private bool ParseNumber()
        {
            _ = Consume((byte)'-');
            if (Consume((byte)'0'))
            {
                if (Peek() is >= (byte)'0' and <= (byte)'9')
                {
                    return false;
                }
            }
            else if (!ConsumeDigits(requireOne: true))
            {
                return false;
            }

            if (Consume((byte)'.') && !ConsumeDigits(requireOne: true))
            {
                return false;
            }

            if (Peek() is (byte)'e' or (byte)'E')
            {
                _offset++;
                if (Peek() is (byte)'+' or (byte)'-')
                {
                    _offset++;
                }

                if (!ConsumeDigits(requireOne: true))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ConsumeDigits(bool requireOne)
        {
            int start = _offset;
            while (Peek() is >= (byte)'0' and <= (byte)'9')
            {
                _offset++;
            }

            return !requireOne || _offset > start;
        }

        private bool ConsumeHex4()
        {
            if (_json.Length - _offset < 4)
            {
                return false;
            }

            ReadOnlySpan<byte> hex = _json.Slice(_offset, 4);
            bool valid = IsHex(hex[0]) & IsHex(hex[1]) & IsHex(hex[2]) & IsHex(hex[3]);
            _offset += valid ? 4 : 0;
            return valid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHex(byte value) =>
            (uint)(value - '0') <= 9u | (uint)((value | 0x20) - 'a') <= 5u;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipWhitespace()
        {
            ReadOnlySpan<byte> remaining = _json[_offset..];
            int next = remaining.IndexOfAnyExcept(JsonWhitespace);
            _offset += next < 0 ? remaining.Length : next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly byte Peek() =>
            _offset < _json.Length ? _json[_offset] : byte.MaxValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Consume(byte value)
        {
            bool matches = Peek() == value;
            _offset += matches ? 1 : 0;
            return matches;
        }

        private bool ConsumeLiteral(ReadOnlySpan<byte> literal)
        {
            if (!_json[_offset..].StartsWith(literal))
            {
                return false;
            }

            _offset += literal.Length;
            return true;
        }

        private static bool ContainsSlash(ReadOnlySpan<byte> value, bool escaped) =>
            value.Contains((byte)'/')
            || (escaped
                && (value.IndexOf("\\/"u8) >= 0
                    || value.IndexOf("\\u002F"u8) >= 0
                    || value.IndexOf("\\u002f"u8) >= 0));

        private static bool JsonStringEquals(
            ReadOnlySpan<byte> encoded,
            bool escaped,
            ReadOnlySpan<byte> expected)
        {
            if (!escaped)
            {
                return encoded.SequenceEqual(expected);
            }

            int source = 0;
            int target = 0;
            while (source < encoded.Length && target < expected.Length)
            {
                byte value = encoded[source++];
                if (value is (byte)'\\')
                {
                    if (source >= encoded.Length)
                    {
                        return false;
                    }

                    byte escape = encoded[source++];
                    value = escape switch
                    {
                        (byte)'\"' => (byte)'\"',
                        (byte)'\\' => (byte)'\\',
                        (byte)'/' => (byte)'/',
                        (byte)'b' => (byte)'\b',
                        (byte)'f' => (byte)'\f',
                        (byte)'n' => (byte)'\n',
                        (byte)'r' => (byte)'\r',
                        (byte)'t' => (byte)'\t',
                        (byte)'u' when TryReadAsciiHex(encoded, ref source, out byte ascii) => ascii,
                        _ => byte.MaxValue
                    };
                }

                if (value != expected[target++])
                {
                    return false;
                }
            }

            return source == encoded.Length && target == expected.Length;
        }

        private static bool TryReadAsciiHex(
            ReadOnlySpan<byte> encoded,
            ref int offset,
            out byte value)
        {
            value = 0;
            if (encoded.Length - offset < 4
                || encoded[offset] is not (byte)'0'
                || encoded[offset + 1] is not (byte)'0')
            {
                return false;
            }

            int high = HexValue(encoded[offset + 2]);
            int low = HexValue(encoded[offset + 3]);
            if ((high | low) < 0)
            {
                return false;
            }

            offset += 4;
            value = (byte)((high << 4) | low);
            return true;
        }

        private static int HexValue(byte value) => value switch
        {
            >= (byte)'0' and <= (byte)'9' => value - '0',
            >= (byte)'A' and <= (byte)'F' => value - 'A' + 10,
            >= (byte)'a' and <= (byte)'f' => value - 'a' + 10,
            _ => -1
        };
    }
}
