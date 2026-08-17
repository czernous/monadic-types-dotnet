using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;

namespace MonadicTypes.Tooling;

internal static class Pack
{
    private const int PackageCount = 10;
    private const ulong AnalyzerLower8 = 0x72657A796C616E61;
    private const ulong AsciiLowerMask8 = 0x2020202020202020;
    private static readonly SearchValues<byte> AnalyzerStarts = SearchValues.Create("Aa"u8);
    private static readonly SearchValues<byte> XmlWhitespace = SearchValues.Create(" \t\r\n"u8);
    private static readonly string DotNetHost =
        Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";

    public static int Run(ReadOnlySpan<string> args)
    {
        if (args is not ([] or [_] or [_, _]))
        {
            Console.Error.WriteLine("Usage: mt-pack [version] [output]");
            return 2;
        }

        string version = args is [var suppliedVersion, ..] ? suppliedVersion : "0.1.0-dev";
        if (!SemanticVersion.IsValid(version))
        {
            Console.Error.WriteLine($"Invalid semantic version: {version}");
            return 2;
        }

        string repositoryRoot = FindRepositoryRoot(Environment.CurrentDirectory);
        string output = ResolveOutput(repositoryRoot, args is [_, var suppliedOutput] ? suppliedOutput : null);
        ResetDirectory(output);
        int exitCode = RunPack(repositoryRoot, output, version);
        if (exitCode != 0)
        {
            return exitCode;
        }

        return VerifyPackages(output, version);
    }

    private static int VerifyPackages(string output, string version)
    {
        int packageCount = 0;
        int symbolCount = 0;
        foreach (string file in Directory.EnumerateFiles(output, "*.*nupkg", SearchOption.TopDirectoryOnly))
        {
            ReadOnlySpan<char> extension = Path.GetExtension(file.AsSpan());
            (packageCount, symbolCount) = extension switch
            {
                ".nupkg" => (packageCount + 1, symbolCount),
                ".snupkg" => (packageCount, symbolCount + 1),
                _ => (packageCount, symbolCount)
            };
        }

        if (packageCount != PackageCount || symbolCount != PackageCount - 1)
        {
            return Fail(
                $"Expected {PackageCount} packages and {PackageCount - 1} symbol packages; " +
                $"found {packageCount} and {symbolCount}.");
        }

        for (int index = 0; index < PackageCount; index++)
        {
            string packageId = GetPackageId(index);
            string package = BuildPackagePath(output, packageId, version);
            if (!File.Exists(package))
            {
                return Fail($"Missing package: {package}");
            }

            int exitCode = VerifyPackage(package, packageId);
            if (exitCode != 0)
            {
                return exitCode;
            }
        }

        Console.WriteLine($"Verified {PackageCount} packages and symbol packages for {version}.");
        return 0;
    }

    private static string GetPackageId(int index) => index switch
    {
        0 => "MonadicTypes.NET",
        1 => "MonadicTypes.NET.Errors",
        2 => "MonadicTypes.NET.Async",
        3 => "MonadicTypes.NET.Collections",
        4 => "MonadicTypes.NET.Linq",
        5 => "MonadicTypes.NET.Effects",
        6 => "MonadicTypes.NET.Diagnostics",
        7 => "MonadicTypes.NET.AspNetCore",
        8 => "MonadicTypes.NET.AspNetCore.OpenApi",
        9 => "MonadicTypes.NET.Generators",
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    private static string BuildPackagePath(string output, string packageId, string version)
    {
        const string Extension = ".nupkg";
        bool needsSeparator = !Path.EndsInDirectorySeparator(output);
        int length = output.Length + packageId.Length + version.Length + Extension.Length
            + (needsSeparator ? 2 : 1);
        return string.Create(
            length,
            (Output: output, PackageId: packageId, Version: version, NeedsSeparator: needsSeparator),
            static (destination, state) =>
            {
                int offset = 0;
                state.Output.CopyTo(destination);
                offset += state.Output.Length;
                if (state.NeedsSeparator)
                {
                    destination[offset++] = Path.DirectorySeparatorChar;
                }

                state.PackageId.CopyTo(destination[offset..]);
                offset += state.PackageId.Length;
                destination[offset++] = '.';
                state.Version.CopyTo(destination[offset..]);
                offset += state.Version.Length;
                Extension.CopyTo(destination[offset..]);
            });
    }

    private static int VerifyPackage(string package, string packageId)
    {
        using ZipArchive archive = ZipFile.OpenRead(package);
        int requiredResult = RequireEntry(archive, packageId, "README.md");
        if (requiredResult is 0)
        {
            requiredResult = RequireEntry(archive, packageId, "LICENSE");
        }

        if (requiredResult is 0)
        {
            requiredResult = RequireEntry(archive, packageId, "NOTICE");
        }

        if (requiredResult is not 0)
        {
            return requiredResult;
        }

        return packageId switch
        {
            "MonadicTypes.NET.Generators" => VerifyGenerator(archive, packageId),
            "MonadicTypes.NET.AspNetCore.OpenApi" => VerifyOpenApi(archive, packageId),
            _ => VerifyRuntimeAssembly(archive, packageId)
        };
    }

    private static int VerifyGenerator(ZipArchive archive, string packageId)
    {
        if (archive.GetEntry("analyzers/dotnet/cs/MonadicTypes.Generators.dll") is null
            || archive.GetEntry("analyzers/dotnet/cs/MonadicTypes.Generators.pdb") is null)
        {
            return Fail($"{packageId} does not contain its analyzer assembly and portable PDB.");
        }

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.StartsWith("lib/", StringComparison.Ordinal))
            {
                return Fail($"{packageId} exposes an unintended runtime library.");
            }
        }

        return 0;
    }

    private static int VerifyOpenApi(ZipArchive archive, string packageId)
    {
        int assemblyResult = VerifyRuntimeAssembly(archive, packageId);
        if (assemblyResult != 0)
        {
            return assemblyResult;
        }

        if (archive.GetEntry("analyzers/dotnet/cs/MonadicTypes.AspNetCore.OpenApi.Analyzers.dll") is null
            || archive.GetEntry("buildTransitive/MonadicTypes.NET.AspNetCore.OpenApi.targets") is null)
        {
            return Fail($"{packageId} does not contain its analyzer and reflection-boundary target.");
        }

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(
                "Microsoft.AspNetCore.OpenApi.SourceGenerators.dll",
                StringComparison.Ordinal))
            {
                return Fail($"{packageId} contains the opt-in Microsoft XML-comment generator.");
            }
        }

        if (archive.GetEntry($"{packageId}.nuspec") is not { } nuspec)
        {
            return Fail($"{packageId} does not contain its Nuspec.");
        }

        using Stream stream = nuspec.Open();
        return HasOpenApiAnalyzerExclusion(stream)
            ? 0
            : Fail($"{packageId} does not exclude the Microsoft XML-comment analyzer transitively.");
    }

    private static bool HasOpenApiAnalyzerExclusion(Stream stream)
    {
        const int InitialCapacity = 4096;
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

                int read = stream.Read(buffer, length, buffer.Length - length);
                if (read == 0)
                {
                    break;
                }

                length += read;
            }

            ReadOnlySpan<byte> xml = buffer.AsSpan(0, length);
            ReadOnlySpan<byte> marker = "<dependency"u8;
            while (xml.IndexOf(marker) is >= 0 and var markerOffset)
            {
                xml = xml[(markerOffset + marker.Length)..];
                int tagEnd = xml.IndexOf((byte)'>');
                if (tagEnd < 0)
                {
                    return false;
                }

                ReadOnlySpan<byte> tag = xml[..tagEnd];
                ReadOnlySpan<byte> id = ReadXmlAttribute(tag, "id"u8);
                ReadOnlySpan<byte> exclude = ReadXmlAttribute(tag, "exclude"u8);
                if (id.SequenceEqual("Microsoft.AspNetCore.OpenApi"u8)
                    && ContainsAnalyzersIgnoreCase(exclude))
                {
                    return true;
                }

                xml = xml[(tagEnd + 1)..];
            }

            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal static ReadOnlySpan<byte> ReadXmlAttribute(
        ReadOnlySpan<byte> tag,
        ReadOnlySpan<byte> name)
    {
        while (tag.IndexOf(name) is >= 0 and var nameOffset)
        {
            bool validStart = nameOffset is 0 || XmlWhitespace.Contains(tag[nameOffset - 1]);
            tag = tag[(nameOffset + name.Length)..];
            int next = tag.IndexOfAnyExcept(XmlWhitespace);
            if (validStart && next >= 0 && tag[next] is (byte)'=')
            {
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
        }

        return [];
    }

    internal static bool ContainsAnalyzersIgnoreCase(ReadOnlySpan<byte> value)
    {
        const int Length = 9;
        while (value.Length >= Length)
        {
            int offset = value.IndexOfAny(AnalyzerStarts);
            if (offset < 0 || value.Length - offset < Length)
            {
                return false;
            }

            ReadOnlySpan<byte> candidate = value[offset..];
            ulong lower8 = BinaryPrimitives.ReadUInt64LittleEndian(candidate) | AsciiLowerMask8;
            bool completeToken = (offset is 0 || IsAssetSeparator(value[offset - 1]))
                && (candidate.Length == Length || IsAssetSeparator(candidate[Length]));
            if (completeToken
                && lower8 == AnalyzerLower8
                && (candidate[8] | 0x20) == 's')
            {
                return true;
            }

            value = candidate[1..];
        }

        return false;
    }

    private static bool IsAssetSeparator(byte value) => value is
        (byte)';' or (byte)',' or (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n';

    private static int VerifyRuntimeAssembly(ZipArchive archive, string packageId)
    {
        string assembly = packageId switch
        {
            "MonadicTypes.NET" => "MonadicTypes",
            "MonadicTypes.NET.Errors" => "MonadicTypes.Errors",
            "MonadicTypes.NET.Async" => "MonadicTypes.Async",
            "MonadicTypes.NET.Collections" => "MonadicTypes.Collections",
            "MonadicTypes.NET.Linq" => "MonadicTypes.Linq",
            "MonadicTypes.NET.Effects" => "MonadicTypes.Effects",
            "MonadicTypes.NET.Diagnostics" => "MonadicTypes.Diagnostics",
            "MonadicTypes.NET.AspNetCore" => "MonadicTypes.AspNetCore",
            "MonadicTypes.NET.AspNetCore.OpenApi" => "MonadicTypes.AspNetCore.OpenApi",
            _ => throw new ArgumentOutOfRangeException(nameof(packageId))
        };
        return archive.GetEntry($"lib/net10.0/{assembly}.dll") is null
            ? Fail($"{packageId} does not contain lib/net10.0/{assembly}.dll.")
            : 0;
    }

    private static int RequireEntry(ZipArchive archive, string packageId, string required) =>
        archive.GetEntry(required) is null
            ? Fail($"{packageId} does not contain {required}.")
            : 0;

    private static string ResolveOutput(string repositoryRoot, string? suppliedOutput)
    {
        string artifacts = Path.GetFullPath(Path.Combine(repositoryRoot, "artifacts"));
        string output = Path.GetFullPath(Path.Combine(repositoryRoot, suppliedOutput ?? "artifacts/packages"));
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!output.StartsWith(artifacts, comparison)
            || output.Length <= artifacts.Length
            || !IsDirectorySeparator(output[artifacts.Length]))
        {
            throw new ArgumentException(
                "Package output must be under the repository artifacts directory.",
                nameof(suppliedOutput));
        }

        return output;
    }

    private static bool IsDirectorySeparator(char value) =>
        value is '/' || (OperatingSystem.IsWindows() && value is '\\');

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static int RunPack(
        string repositoryRoot,
        string output,
        string version)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = DotNetHost,
                WorkingDirectory = repositoryRoot,
                UseShellExecute = false
            }
        };
        process.StartInfo.ArgumentList.Add("msbuild");
        process.StartInfo.ArgumentList.Add("eng/Pack.proj");
        process.StartInfo.ArgumentList.Add("-t:Pack");
        process.StartInfo.ArgumentList.Add("-m");
        process.StartInfo.ArgumentList.Add("-nr:false");
        process.StartInfo.ArgumentList.Add("-p:Configuration=Release");
        process.StartInfo.ArgumentList.Add($"-p:PackageOutputPath={output}");
        process.StartInfo.ArgumentList.Add($"-p:Version={version}");

        process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
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
}
