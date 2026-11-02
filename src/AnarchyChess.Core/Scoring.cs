namespace AnarchyChess.Core;

/// <summary>
/// What a player move scores: (tactic's base Value + captured value [+ enemy king if checkmate]) × tactic's Mult.
/// Tactic is null for a quiet move (scores 0). ForkTargets are the enemy pieces this move forks.
/// </summary>
public sealed record ScoredMove(TacticDef? Tactic, int CapturedValue, int MateBonus, IReadOnlySet<Square> ForkTargets)
{
    public bool Checkmate => MateBonus > 0;
    public int Value => Tactic == null ? 0 : Tactic.Value + CapturedValue + MateBonus;
    public int Mult => Tactic?.Mult ?? 0;
    public int Score => Value * Mult;

    /// <summary>For example "21 = (2 + 5) × 3".</summary>
    public string Breakdown =>
        $"{Score} = ({Tactic?.Value ?? 0} + {CapturedValue}{(Checkmate ? $" + {MateBonus}" : "")}) × {Mult}";
}

/// <summary>Scoring rules from docs/specs/phase-1b-scoring.md. Only the player scores.</summary>
public static class Scoring
{
    public static ScoredMove Score(Position pos, Move move, IReadOnlySet<Square> forkTags, TacticCatalog tactics)
    {
        var after = pos.Apply(move);
        var captured = move.Kind == MoveKind.EnPassant ? pos.Catalog["pawn"] : pos[move.To]?.Def;
        var check = Rules.IsInCheck(after, Side.Enemy);
        var mate = check && Rules.LegalMoves(after).Count == 0;

        var qualifying = new List<TacticDef>();
        if (captured != null)
            qualifying.Add(tactics["capture"]);
        if (check)
            qualifying.Add(tactics["check"]);
        if (captured != null && forkTags.Contains(move.To))
            qualifying.Add(tactics["fork"]);
        if (captured != null && PinnedPieces(pos).Contains(move.To))
            qualifying.Add(tactics["pin"]);
        if (move.Kind == MoveKind.Promotion)
            qualifying.Add(tactics["promotion"]);
        if (move.Kind == MoveKind.EnPassant)
            qualifying.Add(tactics["enPassant"]);

        var mateBonus = mate ? pos.PiecesOf(Side.Enemy).First(p => p.Piece.Def.Royal).Piece.Def.Value : 0;
        return new ScoredMove(qualifying.MaxBy(t => t.Mult), captured?.Value ?? 0, mateBonus, ForkTargets(after, move.To));
    }

    /// <summary>
    /// Enemy pieces the piece on <paramref name="attacker"/> forks: it must hit 2+ real targets (the king, an undefended
    /// piece, or one worth more than the attacker). Returns the non-king targets, which get Forked tags.
    /// </summary>
    public static IReadOnlySet<Square> ForkTargets(Position pos, Square attacker)
    {
        var piece = pos[attacker]!;
        var real = Rules.Attacks(pos, attacker, piece)
            .Where(s => pos[s] is { } target && target.Side != piece.Side &&
                        (target.Def.Royal || target.Def.Value > piece.Def.Value || !Rules.IsAttacked(pos, s, target.Side)))
            .ToList();
        return real.Count >= 2 ? real.Where(s => !pos[s]!.Def.Royal).ToHashSet() : new HashSet<Square>();
    }

    /// <summary>
    /// Enemy pieces pinned by the player's sliding pieces: on a clear line, with the enemy king or a more valuable
    /// enemy piece directly behind.
    /// </summary>
    public static IReadOnlySet<Square> PinnedPieces(Position pos)
    {
        var pinned = new HashSet<Square>();
        foreach (var (from, piece) in pos.PiecesOf(Side.Player))
        foreach (var dir in piece.Def.Moves.Where(m => m.Slides))
        {
            var front = FirstPieceFrom(pos, from, dir);
            if (front is not { } f || pos[f]!.Side != Side.Enemy || pos[f]!.Def.Royal)
                continue;
            var behind = FirstPieceFrom(pos, f, dir);
            if (behind is { } b && pos[b]!.Side == Side.Enemy && (pos[b]!.Def.Royal || pos[b]!.Def.Value > pos[f]!.Def.Value))
                pinned.Add(f);
        }
        return pinned;
    }

    private static Square? FirstPieceFrom(Position pos, Square from, MovePattern dir)
    {
        for (var s = from.Offset(dir.Files, dir.Ranks); pos.Contains(s); s = s.Offset(dir.Files, dir.Ranks))
            if (pos[s] != null)
                return s;
        return null;
    }
}
