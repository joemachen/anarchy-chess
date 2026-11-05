namespace AnarchyChess.Core.Tests;

/// <summary>Loads the real game content and builds positions from diagrams for tests.</summary>
internal static class TestContent
{
    public static readonly PieceCatalog Catalog = PieceCatalog.FromJson(Read("pieces.json"));
    public static readonly TacticCatalog Tactics = TacticCatalog.FromJson(Read("tactics.json"));

    public static readonly LadderConfig Ladder = LadderConfig.FromJson(Read("rounds.json"));

    public static Deployment Deploy(long seed, int round) => Setup.Deploy(Catalog, Tactics, Ladder, seed, round);

    /// <summary>A stand-in for a player's setup: its most valuable pieces, on random safe squares.</summary>
    public static Round AutoStart(long seed, int round)
    {
        var deployment = Deploy(seed, round);
        var rng = new Rng(seed + round);
        foreach (var piece in deployment.Tray.OrderByDescending(p => p.Value).Take(Ladder.FreePlacements).ToList())
        {
            var squares = deployment.SquaresFor(piece).OrderBy(s => (s.Rank, s.File)).ToList();
            var safe = squares.Where(s => !deployment.IsUnsafe(piece, s)).ToList();
            var choices = safe.Count > 0 ? safe : squares;
            if (choices.Count > 0)
                deployment = deployment.Place(piece, choices[rng.NextInt(choices.Count)]);
        }
        return deployment.Start();
    }

    public static string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content", file));

    public static Position Pos(string diagram, Side toMove = Side.Player) => Diagram.Parse(diagram, Catalog, toMove);

    /// <summary>Where the piece on <paramref name="from"/> can legally go, as sorted square names.</summary>
    public static string[] Targets(Position pos, string from) =>
        Rules.LegalMoves(pos).Where(m => m.From == Square.Parse(from)).Select(m => m.To.Name).Order().ToArray();

    public static string[] AllMoves(Position pos) =>
        Rules.LegalMoves(pos).Select(m => m.From.Name + m.To.Name).Order().ToArray();

    public static Position Play(Position pos, string fromTo) => pos.Apply(Find(pos, fromTo));

    /// <summary>The standard 6x6 opening (was the v0.0.2 sandbox start).</summary>
    public const string StartPosition = """
        r n b q k r
        p p p p p p
        . . . . . .
        . . . . . .
        P P P P P P
        R N B Q K R
        """;

    /// <summary>A round from a diagram, with a target and move limit high enough not to end it unless a test says so.</summary>
    public static Round StartRound(string diagram, Side toMove = Side.Player, int target = int.MaxValue, int moves = int.MaxValue) =>
        Round.Start(Pos(diagram, toMove), Tactics, target, moves, seed: 0);

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
