using Godot;

namespace AnarchyChess.Godot;

/// <summary>
/// Game log. Every line goes to Godot's output, which Godot also writes to
/// %APPDATA%\Godot\app_userdata\Anarchy Chess\logs\godot.log. The last lines are kept for the debug overlay.
/// Debug lines are only written while Verbose is on.
/// </summary>
public static class Log
{
    private const int KeepLines = 20;
    private static readonly Queue<string> Recent = new();

    public static bool Verbose { get; set; }

    public static IEnumerable<string> RecentLines => Recent;

    /// <summary>Raised after every logged line.</summary>
    public static event Action? Changed;

    public static void Info(string message) => Write("INFO", message);

    public static void Debug(string message)
    {
        if (Verbose)
            Write("DEBUG", message);
    }

    private static void Write(string level, string message)
    {
        var ms = Time.GetTicksMsec();
        var line = $"[{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}] {level} {message}";
        GD.Print(line);
        Recent.Enqueue(line);
        if (Recent.Count > KeepLines)
            Recent.Dequeue();
        Changed?.Invoke();
    }
}
