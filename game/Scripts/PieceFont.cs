using Godot;

namespace AnarchyChess.Godot;

/// <summary>The font holding the chess symbols, loaded once (see THIRD_PARTY_NOTICES.md).</summary>
public static class PieceFont
{
    private const string Path = "res://fonts/NotoSansSymbols2-Regular.ttf";

    private static Font? _font;

    // Loaded straight from the .ttf so it works without the editor's import step.
    public static Font Get()
    {
        if (_font != null)
            return _font;
        var font = new FontFile();
        var error = font.LoadDynamicFont(Path);
        if (error != Error.Ok)
            Log.Info($"Couldn't load {Path} ({error}); drawing pieces with the default font");
        return _font = error == Error.Ok ? font : ThemeDB.FallbackFont;
    }
}
