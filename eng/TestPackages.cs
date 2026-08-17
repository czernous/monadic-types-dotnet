using System.Buffers;
using System.Diagnostics;

namespace MonadicTypes.Tooling;

internal static class TestPackages
{
    private static readonly SearchValues<char> RuntimeIdentifierCharacters = SearchValues.Create(
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-.");
    private static readonly string DotNetHost =
        Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";

    public static int Run(ReadOnlySpan<string> args)
    {
        if (args is not ([_] or [_, _]))
        {
            Console.Error.WriteLine(
                "Usage: mt-test-packages <version> [runtime-identifier]");
            return 2;
        }

        string version = args[0];
        if (!SemanticVersion.IsValid(version))
        {
            Console.Error.WriteLine($"Invalid semantic version: {version}");
            return 2;
        }

        string? rid = args is [_, var runtimeIdentifier] ? runtimeIdentifier : null;
        if (rid is not null && (rid.Length is 0 || rid.AsSpan().IndexOfAnyExcept(RuntimeIdentifierCharacters) >= 0))
        {
            Console.Error.WriteLine($"Invalid runtime identifier: {rid}");
            return 2;
        }

        string repositoryRoot = FindRepositoryRoot(Environment.CurrentDirectory);
        string output = GetOutput(repositoryRoot, "package-smoke", rid);
        string packageCache = GetGlobalPackageCache();
        if (IsPackageVersionCached(packageCache, version))
        {
            Console.Error.WriteLine(
                $"Package version {version} is already present in the NuGet cache; " +
                "pack and test with a unique version.");
            return 1;
        }

        using Process build = CreateBuild(repositoryRoot, packageCache, version, rid);
        int exitCode = Run(build);
        if (exitCode is not 0)
        {
            return exitCode;
        }

        string executable = Path.Combine(
            output,
            IsWindowsTarget(rid) ? "MonadicTypes.PackageSmoke.exe" : "MonadicTypes.PackageSmoke");
        using Process process = CreateProcess(executable, repositoryRoot);
        return Run(process);
    }

    private static Process CreateBuild(
        string repositoryRoot,
        string packageCache,
        string version,
        string? rid)
    {
        Process process = CreateDotNetProcess(repositoryRoot);
        process.StartInfo.ArgumentList.Add("msbuild");
        process.StartInfo.ArgumentList.Add("eng/TestPackages.proj");
        process.StartInfo.ArgumentList.Add("-t:Test");
        process.StartInfo.ArgumentList.Add("-m");
        process.StartInfo.ArgumentList.Add("-nr:false");
        AddProperty(process, "-p:PackageVersionToTest=", version);
        process.StartInfo.Environment["NUGET_PACKAGES"] = packageCache;
        if (rid is not null)
        {
            AddProperty(process, "-p:RuntimeIdentifier=", rid);
        }

        return process;
    }

    private static string GetOutput(string repositoryRoot, string name, string? rid) =>
        rid is null
            ? Path.Combine(repositoryRoot, "artifacts", name)
            : Path.Combine(repositoryRoot, "artifacts", name, rid);

    private static string GetGlobalPackageCache() =>
        Environment.GetEnvironmentVariable("NUGET_PACKAGES") is { Length: > 0 } configured
            ? Path.GetFullPath(configured)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

    private static bool IsWindowsTarget(string? rid) =>
        rid is null ? OperatingSystem.IsWindows() : rid.StartsWith("win-", StringComparison.Ordinal);

    private static void AddProperty(
        Process process,
        string property,
        string value)
    {
        int length = property.Length + value.Length;
        string argument = string.Create(
            length,
            (Property: property, Value: value),
            static (destination, state) =>
            {
                state.Property.CopyTo(destination);
                state.Value.CopyTo(destination[state.Property.Length..]);
            });
        process.StartInfo.ArgumentList.Add(argument);
    }

    private static bool IsPackageVersionCached(string packageCache, string version)
    {
        if (!Directory.Exists(packageCache))
        {
            return false;
        }

        foreach (string packageDirectory in Directory.EnumerateDirectories(
                     packageCache,
                     "monadictypes.net*",
                     SearchOption.TopDirectoryOnly))
        {
            string versionDirectory = BuildVersionPath(packageDirectory, version);
            if (Directory.Exists(versionDirectory))
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildVersionPath(string packageDirectory, string version)
    {
        bool needsSeparator = !Path.EndsInDirectorySeparator(packageDirectory);
        int length = packageDirectory.Length + version.Length + (needsSeparator ? 1 : 0);
        return string.Create(
            length,
            (PackageDirectory: packageDirectory, Version: version, NeedsSeparator: needsSeparator),
            static (destination, state) =>
            {
                state.PackageDirectory.CopyTo(destination);
                int offset = state.PackageDirectory.Length;
                if (state.NeedsSeparator)
                {
                    destination[offset++] = Path.DirectorySeparatorChar;
                }

                for (int index = 0; index < state.Version.Length; index++)
                {
                    char value = state.Version[index];
                    destination[offset + index] = value is >= 'A' and <= 'Z'
                        ? (char)(value | 0x20)
                        : value;
                }
            });
    }

    private static Process CreateDotNetProcess(string repositoryRoot) => CreateProcess(
        DotNetHost,
        repositoryRoot);

    private static Process CreateProcess(string executable, string repositoryRoot) => new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false
        }
    };

    private static int Run(Process process)
    {
        process.Start();
        process.WaitForExit();
        return process.ExitCode;
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
