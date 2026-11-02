using AnarchyChess.Core;
using Godot;

namespace AnarchyChess.Godot;

/// <summary>F3 panel: build info, recent log, verbose toggle and "Copy bug info" for bug reports.</summary>
public partial class DebugOverlay : PanelContainer
{
    private Label _log = null!;
    private Button _verbose = null!;

    public long Seed { get; init; }

    public override void _Ready()
    {
        Visible = false;
        SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        GrowHorizontal = GrowDirection.Begin;
        OffsetTop = 12;
        OffsetRight = -12;
        CustomMinimumSize = new Vector2(460, 0);
        AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.07f, 0.94f),
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 10, ContentMarginBottom = 10,
        });

        var box = new VBoxContainer();
        AddChild(box);
        box.AddChild(new Label { Text = $"DEBUG (F3)\n{BuildInfo()}" });

        _log = new Label { AutowrapMode = TextServer.AutowrapMode.Arbitrary };
        _log.AddThemeFontSizeOverride("font_size", 12);
        box.AddChild(_log);

        _verbose = new Button();
        _verbose.Pressed += () =>
        {
            Log.Verbose = !Log.Verbose;
            Log.Info($"Verbose logging {(Log.Verbose ? "on" : "off")}");
        };
        box.AddChild(_verbose);

        var copy = new Button { Text = "Copy bug info" };
        copy.Pressed += () =>
        {
            DisplayServer.ClipboardSet($"{BuildInfo()}\nRecent log:\n{string.Join("\n", Log.RecentLines)}");
            Log.Info("Bug info copied to clipboard");
        };
        box.AddChild(copy);

        Log.Changed += Refresh;
        Refresh();
    }

    public override void _ExitTree() => Log.Changed -= Refresh;

    private void Refresh()
    {
        _log.Text = string.Join("\n", Log.RecentLines);
        _verbose.Text = $"Verbose log: {(Log.Verbose ? "ON" : "off")}";
    }

    private string BuildInfo() =>
        $"Anarchy Chess v{GameInfo.Version}\nSeed: {Seed}\nGodot: {Engine.GetVersionInfo()["string"]}\nOS: {OS.GetName()}";
}
