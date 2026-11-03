using AnarchyChess.Core;
using Godot;
using Side = AnarchyChess.Core.Side;

namespace AnarchyChess.Godot;

/// <summary>
/// Draws a position and lets the side to move pick a piece, then one of its legal moves (shown as dots).
/// Rank 1 is at the bottom. Pieces are placeholder letters until real art lands.
/// </summary>
public partial class BoardView : Control
{
    private const float SquarePx = 96f;
    private const float MarginPx = 28f; // room for file/rank labels

    private static readonly Color Light = new(0.93f, 0.85f, 0.72f);
    private static readonly Color Dark = new(0.62f, 0.45f, 0.33f);
    private static readonly Color Highlight = new(0.98f, 0.82f, 0.2f, 0.75f);
    private static readonly Color CheckColor = new(0.9f, 0.15f, 0.15f, 0.6f);
    private static readonly Color DotColor = new(0.1f, 0.1f, 0.12f, 0.45f);
    private static readonly Color LabelColor = new(0.8f, 0.78f, 0.85f);
    private static readonly Color PlayerFill = new(0.97f, 0.96f, 0.92f);
    private static readonly Color EnemyFill = new(0.13f, 0.12f, 0.14f);

    private static readonly Color ForkedColor = new(0.85f, 0.2f, 0.55f);
    private static readonly Color PinnedColor = new(0.2f, 0.45f, 0.85f);
    private static readonly Color PreviewColor = new(0.05f, 0.45f, 0.15f);
    private static readonly Color LastMoveColor = new(0.35f, 0.75f, 0.95f, 0.45f);
    private static readonly Color HintColor = new(0.8f, 0.55f, 0.05f);
    private static readonly Color DangerColor = new(0.8f, 0.1f, 0.1f);

    private Round _round = null!;
    private Position _position = null!;
    private IReadOnlyList<Move> _legal = [];
    private IReadOnlySet<Square> _pinned = new HashSet<Square>();
    private IReadOnlySet<Square> _threatened = new HashSet<Square>();
    private Dictionary<Square, PieceHint> _hints = [];
    private Square? _selected;
    private bool _showGuides = true;

    /// <summary>Move guides (spec 1d): piece badges, fork-setup and unsafe labels on dots, threat markers.</summary>
    public bool ShowGuides
    {
        get => _showGuides;
        set
        {
            _showGuides = value;
            QueueRedraw();
        }
    }

    public event Action<Move>? MovePlayed;

    /// <summary>Highlighted so you can see what just happened (mainly the enemy's reply).</summary>
    public Move? LastMove { get; set; }

    public Round Round
    {
        get => _round;
        set
        {
            _round = value;
            _position = value.Position;
            // Only you pick moves, and only while the round is on; the enemy moves by itself.
            _legal = value.Outcome == Outcome.Playing && _position.ToMove == Side.Player ? Rules.LegalMoves(_position) : [];
            _pinned = value.Pinned;
            _threatened = _legal.Count > 0 ? Guides.Threatened(_position) : new HashSet<Square>();
            _hints = _legal.Select(m => m.From).Distinct()
                .Select(s => (Square: s, Hint: Guides.ForPiece(value, s)))
                .Where(x => x.Hint != null)
                .ToDictionary(x => x.Square, x => x.Hint!);
            _selected = null;
            CustomMinimumSize = new Vector2(_position.Width * SquarePx + MarginPx, _position.Height * SquarePx + MarginPx);
            QueueRedraw();
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Pass; // margin clicks reach Main, which clears the selection
    }

    public void Select(Square square)
    {
        _selected = square;
        QueueRedraw();
    }

    public void ClearSelection()
    {
        if (_selected == null)
            return;
        _selected = null;
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent e)
    {
        if (e is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } click)
            return;
        var square = SquareAt(click.Position);
        if (square is not { } clicked)
            return; // click on the label margin: let it fall through and clear the selection
        AcceptEvent();

        var move = _legal.FirstOrDefault(m => m.From == _selected && m.To == clicked);
        if (_selected != null && move != default)
        {
            MovePlayed?.Invoke(move);
            return;
        }

        _selected = _legal.Any(m => m.From == clicked) && clicked != _selected ? clicked : null;
        Log.Debug(_selected is { } s ? $"Selected {s}" : "Cleared selection");
        QueueRedraw();
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        for (var file = 0; file < _position.Width; file++)
        for (var rank = 0; rank < _position.Height; rank++)
        {
            var square = new Square(file, rank);
            var rect = new Rect2(ScreenPos(square), new Vector2(SquarePx, SquarePx));
            DrawRect(rect, (file + rank) % 2 == 0 ? Dark : Light);
            if (square == LastMove?.From || square == LastMove?.To)
                DrawRect(rect, LastMoveColor);
            if (square == _selected)
                DrawRect(rect, Highlight);
            if (_position[square] is { Def.Royal: true } king && Rules.IsInCheck(_position, king.Side))
                DrawRect(rect, CheckColor);
            if (_position[square] is { } piece)
                DrawPiece(font, rect, piece);
            if (_round.ForkTags.Contains(square))
                DrawBadge(font, rect, "FORKED", ForkedColor);
            else if (_pinned.Contains(square))
                DrawBadge(font, rect, "PINNED", PinnedColor);
            if (_showGuides && _threatened.Contains(square))
                DrawBadge(font, rect, "THREATENED", DangerColor);
            if (_showGuides && _hints.TryGetValue(square, out var hint) && square != _selected)
                DrawBadge(font, BottomStrip(square), hint.Label, HintColor);
        }

        foreach (var move in _legal.Where(m => m.From == _selected))
        {
            var center = ScreenPos(move.To) + new Vector2(SquarePx, SquarePx) / 2;
            var captures = _position[move.To] != null || move.Kind == MoveKind.EnPassant;
            if (captures)
                DrawArc(center, SquarePx * 0.44f, 0, Mathf.Tau, 32, DotColor, 6f);
            else
                DrawCircle(center, SquarePx * 0.14f, DotColor);

            // Score preview (only you ever have moves here: decision D8), plus fork setups when guides are on.
            var preview = _round.Preview(move);
            var fork = _showGuides && Guides.SetsUpFork(preview);
            if (preview.Tactic is { } tactic)
                DrawBadge(font, BottomStrip(move.To), $"+{preview.Score} {tactic.Name.ToUpperInvariant()}{(fork ? " +FORK" : "")}", PreviewColor);
            else if (fork)
                DrawBadge(font, BottomStrip(move.To), "SETS UP FORK", ForkedColor);
            if (_showGuides && Guides.IsUnsafe(_position, move))
                DrawBadge(font, new Rect2(ScreenPos(move.To), SquarePx, SquarePx), "UNSAFE", DangerColor);
        }

        for (var i = 0; i < _position.Width; i++)
            DrawString(font, ScreenPos(new Square(i, 0)) + new Vector2(SquarePx / 2 - 5, SquarePx + 20),
                ((char)('a' + i)).ToString(), fontSize: 18, modulate: LabelColor);
        for (var i = 0; i < _position.Height; i++)
            DrawString(font, ScreenPos(new Square(0, i)) + new Vector2(-MarginPx + 6, SquarePx / 2 + 6),
                (i + 1).ToString(), fontSize: 18, modulate: LabelColor);
    }

    private void DrawPiece(Font font, Rect2 rect, Piece piece)
    {
        var center = rect.GetCenter();
        var player = piece.Side == Side.Player;
        DrawCircle(center, SquarePx * 0.36f, player ? PlayerFill : EnemyFill);
        DrawArc(center, SquarePx * 0.36f, 0, Mathf.Tau, 32, player ? EnemyFill : PlayerFill, 2f);
        const int size = 40;
        var letter = piece.Def.Letter.ToString();
        var width = font.GetStringSize(letter, fontSize: size).X;
        DrawString(font, center + new Vector2(-width / 2, size * 0.36f), letter, fontSize: size,
            modulate: player ? EnemyFill : PlayerFill);
    }

    /// <summary>A small label on a coloured strip across the top of <paramref name="rect"/>.</summary>
    private void DrawBadge(Font font, Rect2 rect, string text, Color color)
    {
        const int size = 13;
        var strip = new Rect2(rect.Position, new Vector2(rect.Size.X, size + 8));
        DrawRect(strip, color);
        var width = font.GetStringSize(text, fontSize: size).X;
        DrawString(font, strip.Position + new Vector2((strip.Size.X - width) / 2, size + 2), text, fontSize: size, modulate: Colors.White);
    }

    private Rect2 BottomStrip(Square s) =>
        new(ScreenPos(s) + new Vector2(0, SquarePx * 0.72f), SquarePx, SquarePx * 0.28f);

    private Vector2 ScreenPos(Square s) => new(MarginPx + s.File * SquarePx, (_position.Height - 1 - s.Rank) * SquarePx);

    private Square? SquareAt(Vector2 pos)
    {
        var square = new Square((int)Mathf.Floor((pos.X - MarginPx) / SquarePx), _position.Height - 1 - (int)Mathf.Floor(pos.Y / SquarePx));
        return _position.Contains(square) ? square : null;
    }
}
