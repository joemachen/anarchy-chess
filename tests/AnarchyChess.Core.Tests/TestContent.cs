namespace AnarchyChess.Core.Tests;

/// <summary>Loads the real game content and builds positions from diagrams for tests.</summary>
internal static class TestContent
{
    public static readonly PieceCatalog Catalog = PieceCatalog.FromJson(Read("pieces.json"));

    public static string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content", file));

    public static Position Pos(string diagram, Side toMove = Side.Player) => Diagram.Parse(diagram, Catalog, toMove);

    /// <summary>Where the piece on <paramref name="from"/> can legally go, as sorted square names.</summary>
    public static string[] Targets(Position pos, string from) =>
        Rules.LegalMoves(pos).Where(m => m.From == Square.Parse(from)).Select(m => m.To.Name).Order().ToArray();

    public static string[] AllMoves(Position pos) =>
        Rules.LegalMoves(pos).Select(m => m.From.Name + m.To.Name).Order().ToArray();

    public static Position Play(Position pos, string fromTo)
    {
        var move = Rules.LegalMoves(pos).Single(m => m.From.Name + m.To.Name == fromTo);
        return pos.Apply(move);
    }
}
