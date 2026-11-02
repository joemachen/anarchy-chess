namespace AnarchyChess.Core;

/// <summary>
/// The greedy, predictable enemy (docs/specs/phase-1c-round.md). It plays the first of: checkmate, take your most
/// valuable piece (with its cheapest attacker), save its most valuable threatened piece, give check, a safe move.
/// Ties are broken by the seed. Returns the move and which rule chose it, for the debug log.
/// </summary>
public static class EnemyAi
{
    public static (Move Move, string Reason) Choose(Position pos, Rng rng)
    {
        var legal = Rules.LegalMoves(pos);
        Move Pick(IReadOnlyList<Move> moves) => moves[rng.NextInt(moves.Count)];

        var mates = legal.Where(m => Rules.Status(pos.Apply(m)) == GameStatus.Checkmate).ToList();
        if (mates.Count > 0)
            return (Pick(mates), "checkmate");

        var captures = legal.Where(m => CapturedValue(pos, m) > 0).ToList();
        if (captures.Count > 0)
        {
            var best = captures.Max(m => CapturedValue(pos, m));
            var cheapest = captures.Where(m => CapturedValue(pos, m) == best).Min(m => pos[m.From]!.Def.Value);
            return (Pick(captures.Where(m => CapturedValue(pos, m) == best && pos[m.From]!.Def.Value == cheapest).ToList()), "capture");
        }

        foreach (var square in Threatened(pos).OrderByDescending(s => pos[s]!.Def.Value))
        {
            var escapes = legal.Where(m => m.From == square && IsSafe(pos, m)).ToList();
            if (escapes.Count > 0)
                return (Pick(escapes), "rescue");
        }

        var checks = legal.Where(m => Rules.IsInCheck(pos.Apply(m), Side.Player)).ToList();
        if (checks.Count > 0)
            return (Pick(checks), "check");

        var safe = legal.Where(m => IsSafe(pos, m)).ToList();
        return safe.Count > 0 ? (Pick(safe), "safe") : (Pick(legal), "any");
    }

    private static int CapturedValue(Position pos, Move move) =>
        move.Kind == MoveKind.EnPassant ? pos.Catalog["pawn"].Value : pos[move.To]?.Def.Value ?? 0;

    private static bool IsSafe(Position pos, Move move) => !Rules.IsAttacked(pos.Apply(move), move.To, Side.Player);

    /// <summary>Enemy pieces (not the king) you attack that are undefended or worth more than your cheapest attacker.</summary>
    private static IEnumerable<Square> Threatened(Position pos)
    {
        foreach (var (square, piece) in pos.PiecesOf(Side.Enemy).Where(p => !p.Piece.Def.Royal))
        {
            var attackers = pos.PiecesOf(Side.Player).Where(p => Rules.Attacks(pos, p.Square, p.Piece).Contains(square)).ToList();
            if (attackers.Count > 0 &&
                (!Rules.IsAttacked(pos, square, Side.Enemy) || piece.Def.Value > attackers.Min(a => a.Piece.Def.Value)))
                yield return square;
        }
    }
}
