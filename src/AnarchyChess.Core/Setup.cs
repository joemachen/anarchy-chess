using System.Text.Json;

namespace AnarchyChess.Core;

public sealed record PoolEntry(string Piece, int Weight);

public sealed record RoundConfig(int Target, int Enemies);

/// <summary>The ladder from rounds.json: board size, moves per round, your bag and hand, the enemy pool, and each round.</summary>
public sealed record LadderConfig(
    int BoardSize, int Moves, IReadOnlyList<string> Bag, int HandSize, int FreePlacements,
    IReadOnlyList<PoolEntry> EnemyPool, IReadOnlyList<RoundConfig> Rounds)
{
    public static LadderConfig FromJson(string json) =>
        JsonSerializer.Deserialize<LadderConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidDataException("rounds.json is empty.");
}

/// <summary>
/// Deals each round from (seed, round): the enemy army, your king and your hand, ready for setup
/// (docs/specs/phase-1c-round.md rules 1-4, phase-2-army-bag.md). The bag is full again every round.
/// </summary>
public static class Setup
{
    private const int MaxAttempts = 1000;

    public static Deployment Deploy(PieceCatalog catalog, TacticCatalog tactics, LadderConfig ladder, long seed, int round)
    {
        var roundSeed = Rng.ForRound(seed, round);
        var rng = new Rng(roundSeed);
        var hand = DrawHand(catalog, ladder, rng);
        var size = ladder.BoardSize;
        var king = catalog["king"];
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var pieces = new Dictionary<Square, Piece>();
            Place(pieces, rng, new Piece(king, Side.Player), size, 0, 0);

            // Theirs: king plus weighted random picks, scattered over the top half; pawns never on the top rank.
            var enemies = Enumerable.Range(0, ladder.Rounds[round].Enemies).Select(_ => Pick(catalog, ladder.EnemyPool, rng))
                .Prepend(king).OrderByDescending(d => d.Pawn).ToList();
            foreach (var def in enemies)
                Place(pieces, rng, new Piece(def, Side.Enemy), size, size / 2, def.Pawn ? size - 2 : size - 1);

            var pos = new Position(catalog, size, size, pieces, Side.Player);
            if (!Rules.IsInCheck(pos, Side.Player) && !Rules.IsInCheck(pos, Side.Enemy) && Rules.LegalMoves(pos).Count > 0)
                return new Deployment(pos, hand, ladder.FreePlacements, tactics, ladder.Rounds[round].Target, ladder.Moves, roundSeed);
        }
        throw new InvalidOperationException($"No legal board found for seed {seed}, round {round + 1}.");
    }

    private static List<PieceDef> DrawHand(PieceCatalog catalog, LadderConfig ladder, Rng rng)
    {
        var bag = ladder.Bag.Select(id => catalog[id]).ToList();
        var hand = new List<PieceDef>();
        for (var i = 0; i < ladder.HandSize; i++)
        {
            var pick = rng.NextInt(bag.Count);
            hand.Add(bag[pick]);
            bag.RemoveAt(pick);
        }
        return hand;
    }

    private static void Place(Dictionary<Square, Piece> pieces, Rng rng, Piece piece, int size, int lowRank, int highRank)
    {
        var free = new List<Square>();
        for (var rank = lowRank; rank <= highRank; rank++)
        for (var file = 0; file < size; file++)
            if (!pieces.ContainsKey(new Square(file, rank)))
                free.Add(new Square(file, rank));
        pieces[free[rng.NextInt(free.Count)]] = piece;
    }

    private static PieceDef Pick(PieceCatalog catalog, IReadOnlyList<PoolEntry> pool, Rng rng)
    {
        var roll = rng.NextInt(pool.Sum(e => e.Weight));
        foreach (var entry in pool)
        {
            if (roll < entry.Weight)
                return catalog[entry.Piece];
            roll -= entry.Weight;
        }
        throw new InvalidOperationException("unreachable");
    }
}
