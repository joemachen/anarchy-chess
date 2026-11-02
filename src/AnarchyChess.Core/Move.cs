namespace AnarchyChess.Core;

public enum MoveKind { Normal, DoubleStep, EnPassant, Promotion }

public readonly record struct Move(Square From, Square To, MoveKind Kind = MoveKind.Normal)
{
    public override string ToString() => $"{From}-{To}";
}
