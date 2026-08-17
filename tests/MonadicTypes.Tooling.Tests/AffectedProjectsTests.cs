namespace MonadicTypes.Tooling.Tests;

public sealed class AffectedProjectsTests
{
    [Theory]
    [InlineData("tests/MonadicTypes.PackageSmoke/Program.cs")]
    [InlineData("eng/Pack.cs")]
    [InlineData("eng/Pack.proj")]
    [InlineData("eng/TestPackages.cs")]
    [InlineData("eng/TestPackages.proj")]
    public void PackageInfrastructureChangesRunPackageValidation(string path) =>
        Assert.True(AffectedProjects.IsPackageChange(System.Text.Encoding.UTF8.GetBytes(path)));

    [Theory]
    [InlineData("eng/Pack.cs")]
    [InlineData("eng/VerifyLockfiles.cs")]
    [InlineData("eng/MonadicTypes.Tooling/AffectedProjects.cs")]
    public void ToolChangesDoNotRebuildEveryProject(string path) =>
        Assert.False(AffectedProjects.IsGlobalChange(System.Text.Encoding.UTF8.GetBytes(path)));

    [Theory]
    [InlineData("eng/Pack.cs", true)]
    [InlineData("eng/TestPackages.cs", true)]
    [InlineData("eng/SemanticVersion.cs", true)]
    [InlineData("eng/NativeTool.props", true)]
    [InlineData("eng/tools/linux-x64/mt-pack", true)]
    [InlineData("eng/tools/win-x64/mt-pack.exe", true)]
    [InlineData("eng/Pack.proj", false)]
    [InlineData("eng/TestPackages.proj", false)]
    public void OnlyCompiledToolInputsRebuildNativeBinaries(string path, bool expected) =>
        Assert.Equal(
            expected,
            AffectedProjects.RequiresToolCompilation(System.Text.Encoding.UTF8.GetBytes(path)));

    [Theory]
    [InlineData("Directory.Build.props")]
    [InlineData("MonadicTypes.slnx")]
    [InlineData(".github/workflows/ci.yml")]
    public void RepositoryWideInputsRebuildEveryProject(string path) =>
        Assert.True(AffectedProjects.IsGlobalChange(System.Text.Encoding.UTF8.GetBytes(path)));
}
