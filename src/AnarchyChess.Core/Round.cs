namespace AnarchyChess.Core;

public enum Outcome { Playing, WonTarget, WonCheckmate, LostCheckmated, LostOutOfMoves }

/// <summary>
/// A round in progress (docs/specs/phase-1b-scoring.md and phase-1c-round.md): the position, your score against the
/// target, your moves left, and which enemy pieces are Forked. Fork tags last until your next move, and leave with a
/// piece that moves away. Seed is this round's seed; the enemy's choices come from it.
/// </summary>
public sealed record Round(
    Position Position, TacticCatalog Tactics, int Score, IReadOnlySet<Square> ForkTags,
    int Target, int MovesLeft, long Seed, int Ply, Outcome Outcome)
{
    public static Round Start(Position position, TacticCatalog tactics, int target, int moves, long seed) =>
        new(position, tactics, 0, new HashSet<Square>(), target, moves, seed, 0, Outcome.Playing);

    /// <summary>Enemy pieces currently pinned by the player.</summary>
    public IReadOnlySet<Square> Pinned => Scoring.PinnedPieces(Position);

    /// <summary>The random numbers for the enemy's next move: fixed by the round seed and how far the round has got.</summary>
    public Rng EnemyRng() => new(unchecked(Seed + Ply));

    /// <summary>What a player move would score. The same calculation Play uses, so they always agree.</summary>
    public ScoredMove Preview(Move move) => Scoring.Score(Position, move, ForkTags, Tactics);

    /// <summary>Plays a legal move. Returns how it scored, or null for an enemy move (the enemy never scores).</summary>
    public (Round Next, ScoredMove? Scored) Play(Move move)
    {
        if (Outcome != Outcome.Playing)
            throw new InvalidOperationException("The round is over.");

        var next = Rules.Play(Position, move);
        Round after;
        ScoredMove? scored = null;
        if (Position.ToMove == Side.Player)
        {
            scored = Preview(move);
            after = this with { Score = Score + scored.Score, ForkTags = scored.ForkTargets, MovesLeft = MovesLeft - 1 };
        }
        else
        {
            var youPassed = next.ToMove == Side.Enemy; // you were stalemated: the pass costs one of your moves
            after = this with { ForkTags = ForkTags.Where(s => s != move.From).ToHashSet(), MovesLeft = youPassed ? MovesLeft - 1 : MovesLeft };
        }
        after = after with { Position = next, Ply = Ply + 1 };
        return (after with { Outcome = after.Decide() }, scored);
    }

    private Outcome Decide()
    {
        var status = Rules.Status(Position);
        if (status == GameStatus.Checkmate)
            return Position.ToMove == Side.Enemy ? Outcome.WonCheckmate : Outcome.LostCheckmated;
        if (Score >= Target)
            return Outcome.WonTarget;
        // Out of moves, or nobody can move at all (a stalemate that couldn't be passed).
        return MovesLeft <= 0 || status == GameStatus.Stalemate ? Outcome.LostOutOfMoves : Outcome.Playing;
    }
}
