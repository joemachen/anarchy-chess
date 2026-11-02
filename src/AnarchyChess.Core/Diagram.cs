namespace AnarchyChess.Core;

/// <summary>
/// Reads a board drawn as text: one line per rank, top line = highest rank, squares separated by spaces.
/// "." is empty, an uppercase letter is a player piece, lowercase is an enemy piece (letters from pieces.json).
/// </summary>
public static class Diagram
{
    public static Position Parse(string text, PieceCatalog catalog, Side toMove)
    {
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        var width = rows[0].Length;
        var pieces = new Dictionary<Square, Piece>();
        for (var row = 0; row < rows.Count; row++)
        {
            if (rows[row].Length != width)
                throw new FormatException($"Diagram line {row + 1} has {rows[row].Length} squares, expected {width}.");
            for (var file = 0; file < width; file++)
            {
                var token = rows[row][file];
                if (token == ".")
                    continue;
                var def = token.Length == 1 ? catalog.ByLetter(token[0]) : null;
                if (def == null)
                    throw new FormatException($"Diagram line {row + 1}: unknown piece '{token}'.");
                var side = char.IsUpper(token[0]) ? Side.Player : Side.Enemy;
                pieces[new Square(file, rows.Count - 1 - row)] = new Piece(def, side);
            }
        }
        return new Position(catalog, width, rows.Count, pieces, toMove);
    }
}
