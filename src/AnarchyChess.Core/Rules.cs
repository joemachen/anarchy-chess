namespace AnarchyChess.Core;

public enum GameStatus { Ongoing, Check, Checkmate, Stalemate }

/// <summary>Chess rules: legal moves, check, checkmate and stalemate. No castling (docs/specs/phase-1a-rules.md).</summary>
public static class Rules
{
    /// <summary>Every move the side to move may make: its pieces' moves that don't leave its own king in check.</summary>
    public static IReadOnlyList<Move> LegalMoves(Position pos) =>
        pos.PiecesOf(pos.ToMove)
            .SelectMany(p => PieceMoves(pos, p.Square, p.Piece))
            .Where(m => !IsInCheck(pos.Apply(m), pos.ToMove))
            .ToList();

    /// <summary>
    /// Plays a move. If the side due next has no legal move but isn't in check, it passes (spec 1a, rule 8),
    /// unless nobody could move at all.
    /// </summary>
    public static Position Play(Position pos, Move move)
    {
        var next = pos.Apply(move);
        return Status(next) == GameStatus.Stalemate && LegalMoves(next.Pass()).Count > 0 ? next.Pass() : next;
    }

    public static bool IsInCheck(Position pos, Side side) =>
        pos.PiecesOf(side).Any(p => p.Piece.Def.Royal && IsAttacked(pos, p.Square, side.Opponent()));

    public static bool IsAttacked(Position pos, Square target, Side by) =>
        pos.PiecesOf(by).Any(p => Attacks(pos, p.Square, p.Piece).Contains(target));

    public static GameStatus Status(Position pos)
    {
        var inCheck = IsInCheck(pos, pos.ToMove);
        if (LegalMoves(pos).Count > 0)
            return inCheck ? GameStatus.Check : GameStatus.Ongoing;
        return inCheck ? GameStatus.Checkmate : GameStatus.Stalemate;
    }

    private static IEnumerable<Move> PieceMoves(Position pos, Square from, Piece piece)
    {
        if (piece.Def.Pawn)
            return PawnMoves(pos, from, piece.Side);
        return Attacks(pos, from, piece)
            .Where(to => pos[to]?.Side != piece.Side)
            .Select(to => new Move(from, to));
    }

    /// <summary>Squares the piece attacks: where it could capture, whether or not anything is there.</summary>
    public static IEnumerable<Square> Attacks(Position pos, Square from, Piece piece)
    {
        if (piece.Def.Pawn)
        {
            foreach (var file in new[] { -1, 1 })
                if (pos.Contains(from.Offset(file, Forward(piece.Side))))
                    yield return from.Offset(file, Forward(piece.Side));
            yield break;
        }

        foreach (var pattern in piece.Def.Moves)
        {
            var to = from.Offset(pattern.Files, pattern.Ranks);
            while (pos.Contains(to))
            {
                yield return to;
                if (!pattern.Slides || pos[to] != null)
                    break;
                to = to.Offset(pattern.Files, pattern.Ranks);
            }
        }
    }

    private static IEnumerable<Move> PawnMoves(Position pos, Square from, Side side)
    {
        var forward = Forward(side);
        var startRank = side == Side.Player ? 1 : pos.Height - 2;
        var lastRank = side == Side.Player ? pos.Height - 1 : 0;
        MoveKind Advance(Square to) => to.Rank == lastRank ? MoveKind.Promotion : MoveKind.Normal;

        var one = from.Offset(0, forward);
        if (pos.Contains(one) && pos[one] == null)
        {
            yield return new Move(from, one, Advance(one));
            var two = one.Offset(0, forward);
            if (from.Rank == startRank && pos.Contains(two) && pos[two] == null)
                yield return new Move(from, two, MoveKind.DoubleStep);
        }

        foreach (var file in new[] { -1, 1 })
        {
            var to = from.Offset(file, forward);
            if (!pos.Contains(to))
                continue;
            if (pos[to]?.Side == side.Opponent())
                yield return new Move(from, to, Advance(to));
            else if (to == pos.EnPassant)
                yield return new Move(from, to, MoveKind.EnPassant);
        }
    }

    private static int Forward(Side side) => side == Side.Player ? 1 : -1;
}
