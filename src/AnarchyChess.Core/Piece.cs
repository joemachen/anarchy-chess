namespace AnarchyChess.Core;

public enum Side { Player, Enemy }

public static class SideExtensions
{
    public static Side Opponent(this Side side) => side == Side.Player ? Side.Enemy : Side.Player;
}

/// <summary>One direction a piece moves in: a single jump, or a slide that repeats until blocked.</summary>
public readonly record struct MovePattern(int Files, int Ranks, bool Slides);

/// <summary>A kind of piece, loaded from pieces.json. Royal pieces can be checked; pawns use the pawn rules.</summary>
public sealed record PieceDef(string Id, char Letter, int Value, bool Royal, bool Pawn, IReadOnlyList<MovePattern> Moves);

public sealed record Piece(PieceDef Def, Side Side);
