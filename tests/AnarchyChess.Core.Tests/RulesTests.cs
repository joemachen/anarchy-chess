using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// Diagrams: top line is rank 6, left column is file a. Uppercase = player, lowercase = enemy.
public class RulesTests
{
    // ---- Piece movement ------------------------------------------------------

    [Fact]
    public void Knight_InTheMiddle_HasEightMoves_InTheCorner_Two()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . N . . .
            . . . . . .
            N . . . . .
            """);
        Assert.Equal(["a2", "a4", "b1", "b5", "d1", "d5", "e2", "e4"], Targets(pos, "c3"));
        Assert.Equal(["b3", "c2"], Targets(pos, "a1"));
    }

    [Fact]
    public void Rook_SlidesUntilBlocked_CapturesEnemy_NotOwn()
    {
        var pos = Pos("""
            . . . . . .
            . . p . . .
            . . . . . .
            . . . . . .
            . . R . P .
            . . . . . .
            """);
        Assert.Equal(["a2", "b2", "c1", "c3", "c4", "c5", "d2"], Targets(pos, "c2"));
    }

    [Fact]
    public void Bishop_And_Queen_OnEmptyBoard()
    {
        var bishop = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . B . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal(["a1", "a5", "b2", "b4", "d2", "d4", "e1", "e5", "f6"], Targets(bishop, "c3"));

        var queen = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . Q . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal(19, Targets(queen, "c3").Length);
    }

    [Fact]
    public void King_MovesOneSquareAnyDirection()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . K . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal(["b2", "b3", "b4", "c2", "c4", "d2", "d3", "d4"], Targets(pos, "c3"));
    }

    // ---- Pawns ---------------------------------------------------------------

    [Fact]
    public void Pawn_OnStartRank_MayMoveOneOrTwo()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . P . . . .
            . . . . . .
            """);
        Assert.Equal(["b3", "b4"], Targets(pos, "b2"));
    }

    [Fact]
    public void Pawn_DoubleStep_NeedsBothSquaresEmpty()
    {
        var blockedFar = Pos("""
            . . . . . .
            . . . . . .
            . n . . . .
            . . . . . .
            . P . . . .
            . . . . . .
            """);
        Assert.Equal(["b3"], Targets(blockedFar, "b2"));

        var blockedNear = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . n . . . .
            . P . . . .
            . . . . . .
            """);
        Assert.Empty(Targets(blockedNear, "b2"));
    }

    [Fact]
    public void Pawn_OffStartRank_MovesOnlyOne()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . P . . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal(["b4"], Targets(pos, "b3"));
    }

    [Fact]
    public void Pawn_CapturesDiagonallyForward_OnlyEnemies()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . n . N . .
            . . P . . .
            . . . . . .
            """);
        Assert.Equal(["b3", "c3", "c4"], Targets(pos, "c2"));
    }

    [Fact]
    public void EnemyPawn_MovesDownTheBoard()
    {
        var pos = Pos("""
            . . . . . .
            . . p . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            """, Side.Enemy);
        Assert.Equal(["c3", "c4"], Targets(pos, "c5"));
    }

    // ---- Check and pins ------------------------------------------------------

    [Fact]
    public void King_CannotMoveIntoAttack()
    {
        var pos = Pos("""
            . r . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            K . . . . .
            """);
        Assert.Equal(["a2"], Targets(pos, "a1"));
    }

    [Fact]
    public void King_CannotStepNextToEnemyKing()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . k . . .
            . . . . . .
            . . K . . .
            """);
        Assert.Equal(["b1", "d1"], Targets(pos, "c1"));
    }

    [Fact]
    public void PinnedPiece_CannotLeaveThePinLine()
    {
        var pos = Pos("""
            . . . . r .
            . . . . . .
            . . . . . .
            . . . . N .
            . . . . . .
            . . . . K .
            """);
        Assert.Empty(Targets(pos, "e3"));
    }

    [Fact]
    public void PinnedPiece_MayMoveAlongThePinLine()
    {
        var pos = Pos("""
            . . . . . .
            . . . . b .
            . . . . . .
            . . B . . .
            . . . . . .
            K . . . . .
            """);
        Assert.Equal(["b2", "d4", "e5"], Targets(pos, "c3"));
    }

    [Fact]
    public void InCheck_OnlyMovesThatEscapeAreLegal()
    {
        var pos = Pos("""
            . . . . r .
            . . . . . .
            . . . . . .
            R . . . . .
            . . . . . .
            . . . . K .
            """);
        Assert.Equal(["a3e3", "e1d1", "e1d2", "e1f1", "e1f2"], AllMoves(pos));
    }

    [Fact]
    public void King_CannotRetreatAlongTheCheckingLine()
    {
        var pos = Pos("""
            . . r . . .
            . . . . . .
            . . . . . .
            . . K . . .
            . . . . . .
            . . . . . .
            """);
        Assert.DoesNotContain("c2", Targets(pos, "c3"));
        Assert.Contains("b2", Targets(pos, "c3"));
    }

    // ---- En passant ----------------------------------------------------------

    private const string EnPassantSetup = """
        . . . . . k
        . p . . . .
        . . . . . .
        . . P . . .
        . . . . . .
        K . . . . .
        """;

    [Fact]
    public void EnPassant_AvailableRightAfterTheDoubleStep()
    {
        var pos = Play(Pos(EnPassantSetup, Side.Enemy), "b5b3");
        Assert.Equal(["b4", "c4"], Targets(pos, "c3"));

        var after = Play(pos, "c3b4");
        Assert.Null(after[Square.Parse("b3")]); // the enemy pawn is captured
        Assert.Equal(Catalog["pawn"], after[Square.Parse("b4")]!.Def);
    }

    [Fact]
    public void EnPassant_ExpiresAfterOneMove()
    {
        var pos = Play(Pos(EnPassantSetup, Side.Enemy), "b5b3");
        Assert.Contains("b4", Targets(pos, "c3")); // available now...
        pos = Play(pos, "a1b1");
        pos = Play(pos, "f6f5");
        Assert.Equal(["c4"], Targets(pos, "c3"));
    }

    [Fact]
    public void EnPassant_IllegalIfItExposesYourKing()
    {
        const string pinned = """
            . . . . . k
            . p . . . .
            . . . . . .
            K . P . . r
            . . . . . .
            . . . . . .
            """;
        Assert.Equal(["c4"], Targets(Play(Pos(pinned, Side.Enemy), "b5b3"), "c3"));

        // Control: the same position without the rook allows it.
        var unpinned = Play(Pos(pinned.Replace(". . r", ". . ."), Side.Enemy), "b5b3");
        Assert.Contains("b4", Targets(unpinned, "c3"));
    }

    // ---- Promotion -----------------------------------------------------------

    [Fact]
    public void Pawn_PromotesToQueen_ByMovingOrCapturing()
    {
        var pos = Pos("""
            r . . . . .
            . P . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal(Catalog["queen"], Play(pos, "b5b6")[Square.Parse("b6")]!.Def);
        Assert.Equal(Catalog["queen"], Play(pos, "b5a6")[Square.Parse("a6")]!.Def);
        Assert.All(Rules.LegalMoves(pos), m => Assert.Equal(MoveKind.Promotion, m.Kind));
    }

    [Fact]
    public void EnemyPawn_PromotesOnRankOne()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . p . . . .
            . . . . . .
            """, Side.Enemy);
        var after = Play(pos, "b2b1");
        Assert.Equal(new Piece(Catalog["queen"], Side.Enemy), after[Square.Parse("b1")]);
    }

    // ---- Checkmate, stalemate, passing ---------------------------------------

    [Fact]
    public void BackRankMate_IsCheckmate()
    {
        var pos = Play(Pos("""
            k . . . . .
            p p . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . R . K
            """), "d1d6");
        Assert.Equal(GameStatus.Checkmate, Rules.Status(pos));
    }

    [Fact]
    public void CheckWithAnEscape_IsCheck()
    {
        var pos = Play(Pos("""
            k . . . . .
            p . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . R . K
            """), "d1d6");
        Assert.Equal(GameStatus.Check, Rules.Status(pos));
    }

    [Fact]
    public void NoMovesButNotInCheck_IsStalemate()
    {
        var pos = Pos("""
            k . . . . .
            . . . . . .
            . Q . . . .
            . . . . . .
            . . . . . .
            . . . . . K
            """, Side.Enemy);
        Assert.Equal(GameStatus.Stalemate, Rules.Status(pos));
    }

    [Fact]
    public void Play_StalematedSidePasses()
    {
        var pos = Pos("""
            k . . . . .
            . . . . . .
            . . . Q . .
            . . . . . .
            . . . . . .
            . . . . . K
            """);
        var next = Rules.Play(pos, Rules.LegalMoves(pos).Single(m => m.From.Name + m.To.Name == "d4b4"));
        Assert.Equal(Side.Player, next.ToMove); // the enemy was stalemated and passed
        Assert.Equal(GameStatus.Ongoing, Rules.Status(next));
    }

    [Fact]
    public void Play_WhenNobodyCanMove_NobodyPasses()
    {
        var pos = Pos("""
            . . . . . .
            . . . . . .
            . p . . . .
            . . . . . .
            . P . . . .
            . . . . . .
            """);
        var next = Rules.Play(pos, Rules.LegalMoves(pos).Single(m => m.To.Name == "b3"));
        Assert.Equal(Side.Enemy, next.ToMove);
        Assert.Equal(GameStatus.Stalemate, Rules.Status(next));
    }

    [Fact]
    public void Play_NormalMove_HandsTheTurnOver()
    {
        var next = Rules.Play(Pos(StartPosition), Rules.LegalMoves(Pos(StartPosition))[0]);
        Assert.Equal(Side.Enemy, next.ToMove);
    }

    [Fact]
    public void Pass_HandsTheTurnOver_AndCancelsEnPassant()
    {
        var pos = Play(Pos(EnPassantSetup, Side.Enemy), "b5b3");
        var passed = pos.Pass();
        Assert.Equal(Side.Enemy, passed.ToMove);
        Assert.Null(passed.EnPassant);
    }

    [Fact]
    public void Ongoing_WhenNothingSpecial()
    {
        Assert.Equal(GameStatus.Ongoing, Rules.Status(Pos(StartPosition)));
    }

    // ---- Whole-position checks -----------------------------------------------

    // Counted by hand: 6 pawns × 2 moves + knight to a3 or c3 = 14 for each side.
    [Theory]
    [InlineData(Side.Player)]
    [InlineData(Side.Enemy)]
    public void StartPosition_HasFourteenLegalMoves(Side side)
    {
        Assert.Equal(14, Rules.LegalMoves(Pos(StartPosition, side)).Count);
    }

    [Fact]
    public void Apply_ReturnsANewPosition_LeavingTheOldOneUnchanged()
    {
        var before = Pos(StartPosition);
        var after = Play(before, "a2a3");
        Assert.NotNull(before[Square.Parse("a2")]);
        Assert.Null(after[Square.Parse("a2")]);
        Assert.Equal(Side.Player, before.ToMove);
        Assert.Equal(Side.Enemy, after.ToMove);
    }
}
