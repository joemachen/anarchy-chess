using AnarchyChess.Core;
using Godot;

namespace AnarchyChess.Godot;

/// <summary>
/// Root of the game UI, built in code.
/// Command-line args (after "--"): --seed=N (play that seed), --overlay (start with the debug overlay open),
/// --screenshot=path.png (save a screenshot after the first frames and quit; for smoke tests).
/// </summary>
public partial class Main : Control
{
    private BoardView _board = null!;
    private DebugOverlay _overlay = null!;

    public override void _Ready()
    {
        var args = OS.GetCmdlineUserArgs();
        var seed = ParseSeed(args) ?? GD.RandRange(100_000, 999_999);
        Log.Info($"Anarchy Chess v{GameInfo.Version}, seed {seed}");

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);
        _board = new BoardView();
        center.AddChild(_board);

        var title = new Label { Text = "ANARCHY CHESS", Position = new Vector2(24, 16) };
        title.AddThemeFontSizeOverride("font_size", 28);
        AddChild(title);

        var info = new Label { Text = $"Seed {seed} · v{GameInfo.Version}", HorizontalAlignment = HorizontalAlignment.Right };
        info.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight);
        info.GrowHorizontal = GrowDirection.Begin;
        info.GrowVertical = GrowDirection.Begin;
        info.OffsetRight = -16;
        info.OffsetBottom = -12;
        AddChild(info);

        var debugButton = new Button { Text = "Debug (F3)", Flat = true };
        debugButton.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomLeft);
        debugButton.GrowVertical = GrowDirection.Begin;
        debugButton.OffsetLeft = 12;
        debugButton.OffsetBottom = -8;
        debugButton.Pressed += ToggleOverlay;
        AddChild(debugButton);

        _overlay = new DebugOverlay { Seed = seed };
        AddChild(_overlay);
        if (args.Contains("--overlay"))
            ToggleOverlay();

        var shot = args.FirstOrDefault(a => a.StartsWith("--screenshot="));
        if (shot != null)
            _ = SaveScreenshotAndQuit(shot["--screenshot=".Length..]);
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F3 })
            ToggleOverlay();
    }

    // Clicks that reach the background (not the board or a button) clear the board selection.
    public override void _GuiInput(InputEvent e)
    {
        if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            _board.ClearSelection();
    }

    private void ToggleOverlay()
    {
        _overlay.Visible = !_overlay.Visible;
        Log.Debug($"Debug overlay {(_overlay.Visible ? "shown" : "hidden")}");
    }

    private static long? ParseSeed(string[] args)
    {
        var arg = args.FirstOrDefault(a => a.StartsWith("--seed="));
        if (arg == null)
            return null;
        if (long.TryParse(arg["--seed=".Length..], out var seed) && seed >= 0)
            return seed;
        Log.Info($"Ignoring invalid seed '{arg}'; using a random one");
        return null;
    }

    private async Task SaveScreenshotAndQuit(string path)
    {
        for (var i = 0; i < 3; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var error = GetViewport().GetTexture().GetImage().SavePng(path);
        Log.Info(error == Error.Ok ? $"Screenshot saved to {path}" : $"Screenshot failed: {error}");
        GetTree().Quit();
    }
}
