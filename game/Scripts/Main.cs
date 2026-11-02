using AnarchyChess.Core;
using Godot;
using FileAccess = Godot.FileAccess;
using Side = AnarchyChess.Core.Side;

namespace AnarchyChess.Godot;

/// <summary>
/// Root of the game UI, built in code.
/// Command-line args (after "--"): --seed=N (play that seed), --overlay (start with the debug overlay open),
/// --moves=b2b4,c5c3 (replay moves from the start), --screenshot=path.png (save a screenshot after the first frames and quit).
/// </summary>
public partial class Main : Control
{
    private PieceCatalog _catalog = null!;
    private BoardView _board = null!;
    private Label _status = null!;
    private DebugOverlay _overlay = null!;
    private int _moveNumber;

    public override void _Ready()
    {
        var args = OS.GetCmdlineUserArgs();
        var seed = ParseSeed(args) ?? GD.RandRange(100_000, 999_999);
        Log.Info($"Anarchy Chess v{GameInfo.Version}, seed {seed}");

        _catalog = PieceCatalog.FromJson(FileAccess.GetFileAsString("res://content/pieces.json"));

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        center.AddChild(column);

        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _status.AddThemeFontSizeOverride("font_size", 22);
        column.AddChild(_status);

        _board = new BoardView();
        _board.MovePlayed += Play;
        column.AddChild(_board);

        var reset = new Button { Text = "Reset", SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        reset.Pressed += Reset;
        column.AddChild(reset);

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

        Reset();
        var moves = args.FirstOrDefault(a => a.StartsWith("--moves="));
        if (moves != null)
            Replay(moves["--moves=".Length..]);

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

    private void Reset()
    {
        _moveNumber = 0;
        _board.Board = Diagram.Parse(FileAccess.GetFileAsString("res://content/sandbox.txt"), _catalog, Side.Player);
        Log.Info("Sandbox reset");
        UpdateStatus();
    }

    private void Play(Move move)
    {
        var mover = _board.Board.ToMove;
        var next = Rules.Play(_board.Board, move);
        Log.Info($"Move {++_moveNumber}: {mover} {move.From.Name}{move.To.Name}{(move.Kind == MoveKind.Normal ? "" : $" ({move.Kind})")}");
        if (next.ToMove == mover)
            Log.Info($"{mover.Opponent()} has no legal move and passes");
        _board.Board = next;
        UpdateStatus();
    }

    /// <summary>Plays moves like "b2b4,c5c3" from the start: replays a bug report's move list.</summary>
    private void Replay(string moves)
    {
        foreach (var text in moves.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var move = Rules.LegalMoves(_board.Board).FirstOrDefault(m => m.From.Name + m.To.Name == text);
            if (move == default)
            {
                Log.Info($"Replay stopped: '{text}' isn't a legal move here");
                return;
            }
            Play(move);
        }
    }

    private void UpdateStatus()
    {
        var pos = _board.Board;
        var turn = pos.ToMove == Side.Player ? "Your move" : "Enemy's move (you play them for now)";
        _status.Text = Rules.Status(pos) switch
        {
            GameStatus.Check => $"{turn}: CHECK!",
            GameStatus.Checkmate => pos.ToMove == Side.Player ? "Checkmate. The enemy wins." : "Checkmate! You win.",
            GameStatus.Stalemate => "Stalemate: nobody can move.",
            _ => turn,
        };
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
