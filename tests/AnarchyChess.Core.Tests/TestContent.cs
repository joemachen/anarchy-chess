namespace AnarchyChess.Core.Tests;

/// <summary>Loads the real game content and builds positions from diagrams for tests.</summary>
internal static class TestContent
{
    public static readonly PieceCatalog Catalog = PieceCatalog.FromJson(Read("pieces.json"));
    public static readonly TacticCatalog Tactics = TacticCatalog.FromJson(Read("tactics.json"));

    public static string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content", file));

    public static Position Pos(string diagram, Side toMove = Side.Player) => Diagram.Parse(diagram, Catalog, toMove);

    /// <summary>Where the piece on <paramref name="from"/> can legally go, as sorted square names.</summary>
    public static string[] Targets(Position pos, string from) =>
        Rules.LegalMoves(pos).Where(m => m.From == Square.Parse(from)).Select(m => m.To.Name).Order().ToArray();

    public static string[] AllMoves(Position pos) =>
        Rules.LegalMoves(pos).Select(m => m.From.Name + m.To.Name).Order().ToArray();

    public static Position Play(Position pos, string fromTo) => pos.Apply(Find(pos, fromTo));

    public static Round StartRound(string diagram, Side toMove = Side.Player) => Round.Start(Pos(diagram, toMove), Tactics);

    /// <summary>Plays the moves in order (e.g. "c2d4", "f5f6") and returns the round and the last move's scoring.</summary>
    public static (Round Round, ScoredMove? Last) Play(Round round, params string[] moves)
    {
        ScoredMove? last = null;
        foreach (var text in moves)
            (round, last) = round.Play(Find(round.Position, text));
        return (round, last);
    }

    private static Move Find(Position pos, string fromTo) =>
        Rules.LegalMoves(pos).Single(m => m.From.Name + m.To.Name == fromTo);
}
