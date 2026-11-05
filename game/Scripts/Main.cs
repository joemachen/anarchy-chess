using AnarchyChess.Core;
using Godot;
using FileAccess = Godot.FileAccess;
using Side = AnarchyChess.Core.Side;

namespace AnarchyChess.Godot;

/// <summary>
/// Root of the game UI, built in code: the 5-round ladder from docs/specs/phase-1c-round.md.
/// Command-line args (after "--"): --seed=N (play that seed), --round=N (start at round N), --overlay (open the debug
/// overlay), --setup=Nb2,Pa2 (place pieces), --moves=b1c3,c3e4 (start the round and replay your moves; the enemy replies
/// as it would), --select=c3 (select a square), --screenshot=path.png (save a screenshot after the first frames and quit).
/// </summary>
public partial class Main : Control
{
    private const double EnemyDelaySeconds = 0.5;

    private PieceCatalog _catalog = null!;
    private TacticCatalog _tactics = null!;
    private LadderConfig _ladder = null!;
    private long _seed;
    private int _roundIndex;
    private readonly List<string> _yourMoves = [];
    private string _placements = "";

    private BoardView _board = null!;
    private TrayView _tray = null!;
    private Label _hud = null!;
    private Label _score = null!;
    private Label _callout = null!;
    private Label _seedInfo = null!;
    private Button _guidesButton = null!;
    private PanelContainer _result = null!;
    private Label _resultText = null!;
    private Button _resultButton = null!;
    private DebugOverlay _overlay = null!;

    public override void _Ready()
    {
        var args = OS.GetCmdlineUserArgs();
        _seed = ParseNumber(args, "--seed=") ?? NewSeed();
        _catalog = PieceCatalog.FromJson(FileAccess.GetFileAsString("res://content/pieces.json"));
        _tactics = TacticCatalog.FromJson(FileAccess.GetFileAsString("res://content/tactics.json"));
        _ladder = LadderConfig.FromJson(FileAccess.GetFileAsString("res://content/rounds.json"));
        BuildUi();
        if (args.Contains("--overlay"))
            ToggleOverlay();

        var startRound = (int)Math.Clamp(ParseNumber(args, "--round=") ?? 1, 1, _ladder.Rounds.Count) - 1;
        StartRound(startRound);

        var setup = args.FirstOrDefault(a => a.StartsWith("--setup="));
        if (setup != null)
            ReplaySetup(setup["--setup=".Length..]);
        var moves = args.FirstOrDefault(a => a.StartsWith("--moves="));
        if (moves != null && _deployment!.StartBlockedReason == null)
        {
            OnStartRound();
            Replay(moves["--moves=".Length..]);
        }
        var select = args.FirstOrDefault(a => a.StartsWith("--select="));
        if (select != null && _deployment != null)
            OnSetupSquare(Square.Parse(select["--select=".Length..]));
        else if (select != null)
            _board.Select(Square.Parse(select["--select=".Length..]));
        var shot = args.FirstOrDefault(a => a.StartsWith("--screenshot="));
        if (shot != null)
            _ = SaveScreenshotAndQuit(shot["--screenshot=".Length..]);
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F3 })
            ToggleOverlay();
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.G })
            ToggleGuides();
    }

    private void ToggleGuides()
    {
        _board.ShowGuides = !_board.ShowGuides;
        _guidesButton.Text = $"Guides: {(_board.ShowGuides ? "ON" : "OFF")} (G)";
        Log.Info($"Guides {(_board.ShowGuides ? "on" : "off")}");
    }

    // Clicks that reach the background (not the board or a button) clear the board selection.
    public override void _GuiInput(InputEvent e)
    {
        if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            return;
        if (_deployment != null)
            ClearSetupPick();
        else
            _board.ClearSelection();
    }

    private void StartRound(int index)
    {
        _roundIndex = index;
        _yourMoves.Clear();
        _placements = "";
        _deployment = Setup.Deploy(_catalog, _tactics, _ladder, _seed, index);
        _callout.Text = "";
        _result.Visible = false;
        _seedInfo.Text = $"Seed {_seed} · v{GameInfo.Version}";
        Log.Info($"Round {index + 1}/{_ladder.Rounds.Count}, seed {_seed}, target {_deployment.Target}, " +
                 $"hand {string.Join(" ", _deployment.Tray.Select(p => p.Letter))}:\n{Diagram.Write(_deployment.Position)}");
        ShowSetup();
    }

    private async void OnYourMove(Move move)
    {
        _yourMoves.Add(move.From.Name + move.To.Name);
        Apply(move);
        while (_board.Round is { Outcome: Outcome.Playing, Position.ToMove: Side.Enemy })
        {
            await ToSignal(GetTree().CreateTimer(EnemyDelaySeconds), SceneTreeTimer.SignalName.Timeout);
            EnemyMove();
        }
    }

    private void EnemyMove()
    {
        var (move, reason) = EnemyAi.Choose(_board.Round.Position, _board.Round.EnemyRng());
        Log.Info($"Enemy AI chose {move.From.Name}{move.To.Name} ({reason})");
        Apply(move);
    }

    private void Apply(Move move)
    {
        var mover = _board.Round.Position.ToMove;
        var (next, scored) = _board.Round.Play(move);
        Log.Info($"{mover} {move.From.Name}{move.To.Name}{(move.Kind == MoveKind.Normal ? "" : $" ({move.Kind})")}");
        if (next.Position.ToMove == mover && next.Outcome == Outcome.Playing)
            Log.Info($"{mover.Opponent()} has no legal move and passes");

        if (scored != null)
        {
            var parts = new List<string>();
            if (scored.Tactic != null)
                parts.Add($"{(scored.Checkmate ? "CHECKMATE!" : scored.Tactic.Callout)} {scored.Breakdown}");
            if (scored.ForkTargets.Count > 0)
                parts.Add($"FORK SET UP on {scored.ForkTargets.Count} {(scored.ForkTargets.Count == 1 ? "piece" : "pieces")}");
            _callout.Text = string.Join("   ·   ", parts);
            if (scored.Score > 0)
                Log.Info($"Scored {scored.Tactic!.Name}: {scored.Breakdown}");
        }
        _board.LastMove = move;
        _board.Round = next;
        UpdateHud();
        if (next.Outcome != Outcome.Playing)
            ShowResult(next);
    }

    /// <summary>Plays your moves like "b1c3,c3e4" with the enemy replying instantly: replays a bug report.</summary>
    private void Replay(string moves)
    {
        foreach (var text in moves.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var move = _board.Round.Outcome == Outcome.Playing
                ? Rules.LegalMoves(_board.Round.Position).FirstOrDefault(m => m.From.Name + m.To.Name == text)
                : default;
            if (move == default)
            {
                Log.Info($"Replay stopped: '{text}' isn't a legal move here");
                return;
            }
            _yourMoves.Add(text);
            Apply(move);
            while (_board.Round is { Outcome: Outcome.Playing, Position.ToMove: Side.Enemy })
                EnemyMove();
        }
    }

    private void UpdateHud()
    {
        var round = _board.Round;
        var check = Rules.IsInCheck(round.Position, Side.Player) && round.Outcome == Outcome.Playing ? "   ·   CHECK!" : "";
        var progress = _deployment != null ? "SETUP" : $"MOVES LEFT {round.MovesLeft}{check}";
        _hud.Text = $"ROUND {_roundIndex + 1}/{_ladder.Rounds.Count}   ·   TARGET {round.Target}   ·   {progress}";
        _score.Text = $"SCORE\n{round.Score} / {round.Target}";
    }

    private void ShowResult(Round round)
    {
        var lastRound = _roundIndex == _ladder.Rounds.Count - 1;
        var won = round.Outcome is Outcome.WonTarget or Outcome.WonCheckmate;
        _resultText.Text = round.Outcome switch
        {
            Outcome.WonCheckmate => $"CHECKMATE! Round won with {round.MovesLeft} moves to spare.",
            Outcome.WonTarget => $"ROUND WON! {round.MovesLeft} moves to spare.",
            Outcome.LostCheckmated => $"CHECKMATED. You reached round {_roundIndex + 1}.",
            _ => $"OUT OF MOVES: {round.Score} / {round.Target}. You reached round {_roundIndex + 1}.",
        };
        if (won && lastRound)
            _resultText.Text += "\nLADDER CLEARED!";
        _resultButton.Text = !won ? "Retry (new seed)" : lastRound ? "Play again (new seed)" : "Next round";
        _result.Visible = true;
        Log.Info($"Round {_roundIndex + 1} ended: {round.Outcome}, score {round.Score}/{round.Target}");
    }

    private void OnResultButton()
    {
        var won = _board.Round.Outcome is Outcome.WonTarget or Outcome.WonCheckmate;
        if (won && _roundIndex < _ladder.Rounds.Count - 1)
        {
            StartRound(_roundIndex + 1);
            return;
        }
        _seed = NewSeed();
        StartRound(0);
    }

    private void BuildUi()
    {
        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 12);
        center.AddChild(column);

        _hud = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _hud.AddThemeFontSizeOverride("font_size", 22);
        column.AddChild(_hud);

        _board = new BoardView();
        _board.MovePlayed += OnYourMove;
        _board.SetupSquareClicked += OnSetupSquare;
        column.AddChild(_board);

        _callout = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _callout.AddThemeFontSizeOverride("font_size", 22);
        column.AddChild(_callout);

        _score = new Label { Position = new Vector2(24, 90) };
        _score.AddThemeFontSizeOverride("font_size", 40);
        AddChild(_score);

        _tray = new TrayView { Position = new Vector2(972, 110), CustomMinimumSize = new Vector2(270, 0) };
        _tray.PieceClicked += OnTrayPiece;
        _tray.StartClicked += OnStartRound;
        AddChild(_tray);

        _guidesButton = new Button { Text = "Guides: ON (G)", Position = new Vector2(24, 200) };
        _guidesButton.AddThemeFontSizeOverride("font_size", 20);
        _guidesButton.Pressed += ToggleGuides;
        AddChild(_guidesButton);

        var title = new Label { Text = "ANARCHY CHESS", Position = new Vector2(24, 16) };
        title.AddThemeFontSizeOverride("font_size", 28);
        AddChild(title);

        _seedInfo = new Label { HorizontalAlignment = HorizontalAlignment.Right };
        _seedInfo.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight);
        _seedInfo.GrowHorizontal = GrowDirection.Begin;
        _seedInfo.GrowVertical = GrowDirection.Begin;
        _seedInfo.OffsetRight = -16;
        _seedInfo.OffsetBottom = -12;
        AddChild(_seedInfo);

        var debugButton = new Button { Text = "Debug (F3)", Flat = true };
        debugButton.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomLeft);
        debugButton.GrowVertical = GrowDirection.Begin;
        debugButton.OffsetLeft = 12;
        debugButton.OffsetBottom = -8;
        debugButton.Pressed += ToggleOverlay;
        AddChild(debugButton);

        _result = new PanelContainer { Visible = false };
        _result.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.05f, 0.07f, 0.95f),
            ContentMarginLeft = 32, ContentMarginRight = 32, ContentMarginTop = 24, ContentMarginBottom = 24,
        });
        var resultCenter = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        resultCenter.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(resultCenter);
        resultCenter.AddChild(_result);
        var resultBox = new VBoxContainer();
        resultBox.AddThemeConstantOverride("separation", 16);
        _result.AddChild(resultBox);
        _resultText = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _resultText.AddThemeFontSizeOverride("font_size", 26);
        resultBox.AddChild(_resultText);
        _resultButton = new Button { SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        _resultButton.AddThemeFontSizeOverride("font_size", 22);
        _resultButton.Pressed += OnResultButton;
        resultBox.AddChild(_resultButton);

        _overlay = new DebugOverlay { GameState = () =>
            $"Seed: {_seed}, round {_roundIndex + 1}\nReplay: run.bat {_seed} \"--round={_roundIndex + 1}\" " +
            $"\"--setup={(_deployment != null ? Placements(_deployment) : _placements)}\" \"--moves={string.Join(",", _yourMoves)}\"" };
        AddChild(_overlay);
    }

    private void ToggleOverlay()
    {
        _overlay.Visible = !_overlay.Visible;
        Log.Debug($"Debug overlay {(_overlay.Visible ? "shown" : "hidden")}");
    }

    private static long NewSeed() => GD.RandRange(100_000, 999_999);

    private static long? ParseNumber(string[] args, string prefix)
    {
        var arg = args.FirstOrDefault(a => a.StartsWith(prefix));
        if (arg == null)
            return null;
        if (long.TryParse(arg[prefix.Length..], out var value) && value >= 0)
            return value;
        Log.Info($"Ignoring invalid argument '{arg}'");
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
