using System.Reflection;

namespace AnarchyChess.Core;

public static class GameInfo
{
    /// <summary>The build version from Directory.Build.props, e.g. "0.0.1".</summary>
    public static string Version { get; } =
        typeof(GameInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion.Split('+')[0];
}
