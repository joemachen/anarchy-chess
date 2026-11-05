using AnarchyChess.Core;
using Godot;

namespace AnarchyChess.Godot;

/// <summary>The panel beside the board: your hand during setup, your reserves during the round.</summary>
public partial class TrayView : VBoxContainer
{
    private const int PieceFontSize = 44;
    private static readonly Color Picked = new(0.98f, 0.82f, 0.2f);

    private Label _title = null!;
    private GridContainer _grid = null!;
    private Label _message = null!;
    private Button _start = null!;

    public event Action<int>? PieceClicked;
    public event Action? StartClicked;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 12);
        _title = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _title.AddThemeFontSizeOverride("font_size", 20);
        AddChild(_title);

        _grid = new GridContainer { Columns = 3 };
        AddChild(_grid);

        _message = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        AddChild(_message);

        _start = new Button { Text = "Start round" };
        _start.AddThemeFontSizeOverride("font_size", 22);
        _start.Pressed += () => StartClicked?.Invoke();
        AddChild(_start);
    }

    public void ShowSetup(IReadOnlyList<PieceDef> tray, int? picked, int placed, int limit, string? blocked)
    {
        _title.Text = $"YOUR HAND\nPlaced {placed} of {limit}. The rest stay as reserves.";
        ShowPieces(tray, picked, clickable: true);
        _message.Text = blocked ?? "";
        _start.Visible = true;
        _start.Disabled = blocked != null;
    }

    public void ShowReserves(IReadOnlyList<PieceDef> reserves)
    {
        _title.Text = "RESERVES\nDropping these in the round arrives in the next build.";
        ShowPieces(reserves, null, clickable: false);
        _message.Text = "";
        _start.Visible = false;
    }

    private void ShowPieces(IReadOnlyList<PieceDef> pieces, int? picked, bool clickable)
    {
        foreach (var child in _grid.GetChildren())
        {
            _grid.RemoveChild(child);
            child.QueueFree();
        }

        for (var i = 0; i < pieces.Count; i++)
        {
            var index = i;
            var button = new Button { Text = pieces[i].Glyph, CustomMinimumSize = new Vector2(76, 72), Disabled = !clickable };
            button.AddThemeFontOverride("font", PieceFont.Get());
            button.AddThemeFontSizeOverride("font_size", PieceFontSize);
            if (i == picked)
                button.Modulate = Picked;
            // Deferred: the click rebuilds this tray, which must not happen while the button is still handling it.
            button.Pressed += () => Callable.From(() => PieceClicked?.Invoke(index)).CallDeferred();

            var cell = new VBoxContainer();
            cell.AddChild(button);
            cell.AddChild(new Label { Text = pieces[i].Id, HorizontalAlignment = HorizontalAlignment.Center });
            _grid.AddChild(cell);
        }
    }
}
