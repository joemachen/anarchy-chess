namespace AnarchyChess.Core;

/// <summary>An immutable board state: pieces, whose turn it is, and any en passant chance. Apply returns a new one.</summary>
public sealed class Position
{
    private readonly Piece?[] _squares;

    public Position(PieceCatalog catalog, int width, int height, IReadOnlyDictionary<Square, Piece> pieces, Side toMove)
    {
        Catalog = catalog;
        Width = width;
        Height = height;
        ToMove = toMove;
        _squares = new Piece?[width * height];
        foreach (var (square, piece) in pieces)
            _squares[Index(square)] = piece;
    }

    private Position(Position from, Piece?[] squares, Side toMove, Square? enPassant)
    {
        Catalog = from.Catalog;
        Width = from.Width;
        Height = from.Height;
        _squares = squares;
        ToMove = toMove;
        EnPassant = enPassant;
    }

    public PieceCatalog Catalog { get; }
    public int Width { get; }
    public int Height { get; }
    public Side ToMove { get; }

    /// <summary>The square a pawn skipped over with a double step on the last move, if any.</summary>
    public Square? EnPassant { get; }

    public Piece? this[Square square] => _squares[Index(square)];

    public bool Contains(Square s) => s.File >= 0 && s.File < Width && s.Rank >= 0 && s.Rank < Height;

    /// <summary>The side's pieces, in a fixed order (a1, b1, … then up the ranks).</summary>
    public IEnumerable<(Square Square, Piece Piece)> PiecesOf(Side side)
    {
        for (var i = 0; i < _squares.Length; i++)
            if (_squares[i] is { } piece && piece.Side == side)
                yield return (new Square(i % Width, i / Width), piece);
    }

    public Position Apply(Move move)
    {
        var squares = (Piece?[])_squares.Clone();
        var piece = squares[Index(move.From)]!;
        squares[Index(move.From)] = null;
        if (move.Kind == MoveKind.EnPassant)
            squares[Index(new Square(move.To.File, move.From.Rank))] = null;
        squares[Index(move.To)] = move.Kind == MoveKind.Promotion ? piece with { Def = Catalog.Promotion } : piece;

        Square? enPassant = move.Kind == MoveKind.DoubleStep
            ? new Square(move.From.File, (move.From.Rank + move.To.Rank) / 2)
            : null;
        return new Position(this, squares, ToMove.Opponent(), enPassant);
    }

    /// <summary>The side to move skips its turn (used when it has no legal move but isn't in check).</summary>
    public Position Pass() => new(this, _squares, ToMove.Opponent(), null);

    private int Index(Square s) => s.Rank * Width + s.File;
}
