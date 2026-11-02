using Godot;

namespace AnarchyChess.Godot;

/// <summary>Draws the board and lets the player select a square. Rank 1 is at the bottom.</summary>
public partial class BoardView : Control
{
    private const int Cells = 6;
    private const float SquarePx = 96f;
    private const float MarginPx = 28f; // room for file/rank labels

    private static readonly Color Light = new(0.93f, 0.85f, 0.72f);
    private static readonly Color Dark = new(0.62f, 0.45f, 0.33f);
    private static readonly Color Highlight = new(0.98f, 0.82f, 0.2f, 0.75f);
    private static readonly Color LabelColor = new(0.8f, 0.78f, 0.85f);

    private Vector2I? _selected;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(Cells * SquarePx + MarginPx, Cells * SquarePx + MarginPx);
        MouseFilter = MouseFilterEnum.Pass; // margin clicks reach Main, which clears the selection
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
        if (square == null)
            return; // click on the label margin: let it fall through and clear the selection

        AcceptEvent();
        _selected = square == _selected ? null : square;
        Log.Debug(_selected is { } s ? $"Selected {SquareName(s)}" : "Cleared selection");
        QueueRedraw();
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        for (var file = 0; file < Cells; file++)
        for (var rank = 0; rank < Cells; rank++)
        {
            var rect = new Rect2(ScreenPos(file, rank), new Vector2(SquarePx, SquarePx));
            DrawRect(rect, (file + rank) % 2 == 0 ? Dark : Light);
            if (_selected == new Vector2I(file, rank))
                DrawRect(rect, Highlight);
        }

        for (var i = 0; i < Cells; i++)
        {
            var fileLabel = ((char)('a' + i)).ToString();
            DrawString(font, ScreenPos(i, 0) + new Vector2(SquarePx / 2 - 5, SquarePx + 20), fileLabel, fontSize: 18, modulate: LabelColor);
            DrawString(font, ScreenPos(0, i) + new Vector2(-MarginPx + 6, SquarePx / 2 + 6), (i + 1).ToString(), fontSize: 18, modulate: LabelColor);
        }
    }

    private static Vector2 ScreenPos(int file, int rank) =>
        new(MarginPx + file * SquarePx, (Cells - 1 - rank) * SquarePx);

    private static Vector2I? SquareAt(Vector2 pos)
    {
        var file = (int)Mathf.Floor((pos.X - MarginPx) / SquarePx);
        var row = (int)Mathf.Floor(pos.Y / SquarePx);
        if (file < 0 || file >= Cells || row < 0 || row >= Cells)
            return null;
        return new Vector2I(file, Cells - 1 - row);
    }

    private static string SquareName(Vector2I s) => $"{(char)('a' + s.X)}{s.Y + 1}";
}
