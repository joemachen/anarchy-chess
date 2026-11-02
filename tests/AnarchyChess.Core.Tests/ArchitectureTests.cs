using AnarchyChess.Core;

namespace AnarchyChess.Core.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Core_ReferencesNoEngineOrThirdPartyAssemblies()
    {
        var offenders = typeof(Rng).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(n => !(n == "System" || n.StartsWith("System.") || n == "netstandard" || n == "mscorlib"))
            .ToList();

        Assert.True(offenders.Count == 0, $"Core must stay engine-free, but references: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Version_IsSemVer()
    {
        Assert.Matches(@"^\d+\.\d+\.\d+$", GameInfo.Version);
    }
}
