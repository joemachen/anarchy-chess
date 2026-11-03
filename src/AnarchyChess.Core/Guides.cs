namespace AnarchyChess.Core;

/// <summary>What one of your pieces can do this turn: its best score, and whether it can set up a fork.</summary>
public sealed record PieceHint(int BestScore, bool CanFork)
{
    /// <summary>"+21", or "FORK" when nothing scores but a fork can be set up.</summary>
    public string Label => BestScore > 0 ? $"+{BestScore}" : "FORK";
}

/// <summary>Move guides for players who don't read chess yet (docs/specs/phase-1d-guides.md).</summary>
public static class Guides
{
    /// <summary>The piece's best move this turn, or null if none of its moves score or set up a fork.</summary>
    public static PieceHint? ForPiece(Round round, Square square)
    {
        var previews = Rules.LegalMoves(round.Position).Where(m => m.From == square).Select(round.Preview).ToList();
        var best = previews.Select(p => p.Score).DefaultIfEmpty(0).Max();
        var canFork = previews.Any(SetsUpFork);
        return best > 0 || canFork ? new PieceHint(best, canFork) : null;
    }

    public static bool SetsUpFork(ScoredMove preview) => preview.ForkTargets.Count > 0;

    /// <summary>The enemy could capture your piece where it lands. Ignores recaptures, as the greedy enemy does.</summary>
    public static bool IsUnsafe(Position pos, Move move) => Rules.IsAttacked(pos.Apply(move), move.To, Side.Enemy);

    /// <summary>Your pieces (not the king) the enemy can capture right now.</summary>
    public static IReadOnlySet<Square> Threatened(Position pos) =>
        pos.PiecesOf(Side.Player)
            .Where(p => !p.Piece.Def.Royal && Rules.IsAttacked(pos, p.Square, Side.Enemy))
            .Select(p => p.Square)
            .ToHashSet();
}
