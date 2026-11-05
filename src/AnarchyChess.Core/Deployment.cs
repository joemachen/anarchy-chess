namespace AnarchyChess.Core;

/// <summary>
/// The setup before a round (docs/specs/phase-2-army-bag.md): the board so far, the pieces still in your tray, and how
/// many you may place for free. Immutable: Place and PickUp return a new one. Start begins the round.
/// </summary>
public sealed record Deployment(
    Position Position, IReadOnlyList<PieceDef> Tray, int FreePlacements, TacticCatalog Tactics, int Target, int Moves, long Seed)
{
    private const int SetupRanks = 2;
    private const int PawnRank = 1;

    /// <summary>How many pieces you've placed (your king doesn't count).</summary>
    public int Placed => Position.PiecesOf(Side.Player).Count(p => !p.Piece.Def.Royal);

    /// <summary>A round on the current board, for the guides to read. Nothing is played on it.</summary>
    public Round Preview => Round.Start(Position, Tactics, Target, Moves, Seed);

    /// <summary>Why you can't start yet, or null if you can.</summary>
    public string? StartBlockedReason =>
        Placed == 0 ? "Place at least one piece."
        : Rules.IsInCheck(Position, Side.Enemy) ? "The enemy king would start in check: move a piece."
        : null;

    /// <summary>Where this piece may go: empty squares on your back two ranks, pawns on rank 2, never giving check.</summary>
    public IReadOnlySet<Square> SquaresFor(PieceDef piece)
    {
        var squares = new HashSet<Square>();
        if (Placed >= FreePlacements)
            return squares;
        for (var rank = 0; rank < SetupRanks; rank++)
        for (var file = 0; file < Position.Width; file++)
        {
            var square = new Square(file, rank);
            if (Position[square] == null && (!piece.Pawn || rank == PawnRank) && !Rules.IsInCheck(With(piece, square), Side.Enemy))
                squares.Add(square);
        }
        return squares;
    }

    /// <summary>The enemy could capture this piece on this square.</summary>
    public bool IsUnsafe(PieceDef piece, Square square) => Rules.IsAttacked(With(piece, square), square, Side.Enemy);

    public Deployment Place(PieceDef piece, Square square)
    {
        var tray = Tray.ToList();
        if (!tray.Remove(piece) || !SquaresFor(piece).Contains(square))
            throw new InvalidOperationException($"Can't place {piece.Id} on {square}.");
        return this with { Position = With(piece, square), Tray = tray };
    }

    public Deployment PickUp(Square square)
    {
        if (Position[square] is not { Side: Side.Player, Def.Royal: false } piece)
            throw new InvalidOperationException($"Nothing of yours to pick up on {square}.");
        return this with { Position = Position.With(square, null), Tray = [.. Tray, piece.Def] };
    }

    public Round Start() =>
        StartBlockedReason is { } reason ? throw new InvalidOperationException(reason) : Preview;

    private Position With(PieceDef piece, Square square) => Position.With(square, new Piece(piece, Side.Player));
}
