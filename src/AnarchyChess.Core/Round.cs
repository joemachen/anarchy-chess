namespace AnarchyChess.Core;

/// <summary>
/// A round in progress: the position, the player's score, and which enemy pieces are Forked.
/// Fork tags last until the player's next move, and leave with a piece that moves away (spec 1b).
/// </summary>
public sealed record Round(Position Position, TacticCatalog Tactics, int Score, IReadOnlySet<Square> ForkTags)
{
    public static Round Start(Position position, TacticCatalog tactics) => new(position, tactics, 0, new HashSet<Square>());

    /// <summary>Enemy pieces currently pinned by the player.</summary>
    public IReadOnlySet<Square> Pinned => Scoring.PinnedPieces(Position);

    /// <summary>What a player move would score. The same calculation Play uses, so they always agree.</summary>
    public ScoredMove Preview(Move move) => Scoring.Score(Position, move, ForkTags, Tactics);

    /// <summary>Plays a legal move. Returns how it scored, or null for an enemy move (the enemy never scores).</summary>
    public (Round Next, ScoredMove? Scored) Play(Move move)
    {
        var next = Rules.Play(Position, move);
        if (Position.ToMove == Side.Enemy)
            return (this with { Position = next, ForkTags = ForkTags.Where(s => s != move.From).ToHashSet() }, null);

        var scored = Preview(move);
        return (this with { Position = next, Score = Score + scored.Score, ForkTags = scored.ForkTargets }, scored);
    }
}
