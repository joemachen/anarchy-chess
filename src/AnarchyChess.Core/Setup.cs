using System.Text.Json;

namespace AnarchyChess.Core;

public sealed record PoolEntry(string Piece, int Weight);

public sealed record RoundConfig(int Target, int Enemies);

/// <summary>The ladder from rounds.json: board size, moves per round, your army, the enemy pool, and each round.</summary>
public sealed record LadderConfig(
    int BoardSize, int Moves, IReadOnlyList<string> PlayerArmy, IReadOnlyList<PoolEntry> EnemyPool, IReadOnlyList<RoundConfig> Rounds)
{
    public static LadderConfig FromJson(string json) =>
        JsonSerializer.Deserialize<LadderConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidDataException("rounds.json is empty.");
}

/// <summary>Deals each round's board from (seed, round): docs/specs/phase-1c-round.md, rules 1-4.</summary>
public static class Setup
{
    private const int MaxAttempts = 1000;

    public static Round StartRound(PieceCatalog catalog, TacticCatalog tactics, LadderConfig ladder, long seed, int round) =>
        Round.Start(Deal(catalog, ladder, seed, round), tactics, ladder.Rounds[round].Target, ladder.Moves, Rng.ForRound(seed, round));

    public static Position Deal(PieceCatalog catalog, LadderConfig ladder, long seed, int round)
    {
        var rng = new Rng(Rng.ForRound(seed, round));
        var size = ladder.BoardSize;
        var king = catalog["king"];
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var pieces = new Dictionary<Square, Piece>();

            // Yours: pawns on rank 2 (they keep the double step), everything else on ranks 1-2.
            var army = ladder.PlayerArmy.Select(id => catalog[id]).Prepend(king).OrderByDescending(d => d.Pawn);
            foreach (var def in army)
                Place(pieces, rng, new Piece(def, Side.Player), size, def.Pawn ? 1 : 0, 1);

            // Theirs: king plus weighted random picks, scattered over the top half; pawns never on the top rank.
            var enemies = Enumerable.Range(0, ladder.Rounds[round].Enemies).Select(_ => Pick(catalog, ladder.EnemyPool, rng))
                .Prepend(king).OrderByDescending(d => d.Pawn).ToList();
            foreach (var def in enemies)
                Place(pieces, rng, new Piece(def, Side.Enemy), size, size / 2, def.Pawn ? size - 2 : size - 1);

            var pos = new Position(catalog, size, size, pieces, Side.Player);
            if (!Rules.IsInCheck(pos, Side.Player) && !Rules.IsInCheck(pos, Side.Enemy) && Rules.LegalMoves(pos).Count > 0)
                return pos;
        }
        throw new InvalidOperationException($"No legal board found for seed {seed}, round {round + 1}.");
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
