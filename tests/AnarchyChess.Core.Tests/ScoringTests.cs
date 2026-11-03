using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// Each test mirrors a row or rule in docs/specs/phase-1b-scoring.md.
// Diagrams: top line is rank 6, left column is file a. Uppercase = player, lowercase = enemy.
public class ScoringTests
{
    private static void AssertScored(ScoredMove? scored, string tactic, int value, int mult)
    {
        Assert.NotNull(scored);
        Assert.Equal(tactic, scored.Tactic?.Id);
        Assert.Equal(value, scored.Value);
        Assert.Equal(mult, scored.Mult);
        Assert.Equal(value * mult, scored.Score);
    }

    // ---- The examples table --------------------------------------------------

    [Fact]
    public void PawnTakesKnight_Is3()
    {
        var (_, scored) = Play(StartRound("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . . n . .
            . . P . . .
            . . . . . .
            """), "c2d3");
        AssertScored(scored, "capture", 3, 1);
    }

    [Fact]
    public void QuietCheck_Is4()
    {
        var (_, scored) = Play(StartRound("""
            . . . . k .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            R . . . . .
            """), "a1a6");
        AssertScored(scored, "check", 2, 2);
    }

    [Fact]
    public void RookTakesBishopWithCheck_Is10()
    {
        var (_, scored) = Play(StartRound("""
            . . . . k .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            R . . . b .
            """), "a1e1");
        AssertScored(scored, "check", 2 + 3, 2);
    }

    // Knight c2-d4 forks the king on f5 and the rook on b5 (a royal fork). The king steps away, the rook falls.
    private const string RoyalFork = """
        . . . . . .
        . r . . . k
        . . . . . .
        . . . . . .
        . . N . . .
        . . . . . .
        """;

    [Fact]
    public void KnightTakesForkedRook_Is21()
    {
        var (_, scored) = Play(StartRound(RoyalFork), "c2d4", "f5f6", "d4b5");
        AssertScored(scored, "fork", 2 + 5, 3);
    }

    [Fact]
    public void BishopTakesPinnedKnight_Is20()
    {
        // The knight on c3 is pinned to the queen on e5 by the bishop on a1.
        var (_, scored) = Play(StartRound("""
            . . . . . .
            . . . . q .
            . . . . . .
            . . n . . .
            . . . . . .
            B . . . . .
            """), "a1c3");
        AssertScored(scored, "pin", 2 + 3, 4);
    }

    [Fact]
    public void PawnPromotes_Is6()
    {
        var (_, scored) = Play(StartRound("""
            r . . . . .
            . P . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            """), "b5b6");
        AssertScored(scored, "promotion", 1, 6);
    }

    [Fact]
    public void PawnPromotesTakingRook_Is36()
    {
        var (_, scored) = Play(StartRound("""
            r . . . . .
            . P . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            """), "b5a6");
        AssertScored(scored, "promotion", 1 + 5, 6);
    }

    [Fact]
    public void EnPassant_Is50()
    {
        var (_, scored) = Play(StartRound("""
            . . . . . k
            . p . . . .
            . . . . . .
            . . P . . .
            . . . . . .
            K . . . . .
            """, Side.Enemy), "b5b3", "c3b4");
        AssertScored(scored, "enPassant", 4 + 1, 10);
        Assert.Equal("HOLY HELL", scored!.Tactic!.Callout);
    }

    [Fact]
    public void QueenTakesPawnWithCheckmate_Is46()
    {
        var (_, scored) = Play(StartRound("""
            k . . p . .
            p p . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . Q . .
            """), "d1d6");
        AssertScored(scored, "check", 2 + 1 + 20, 2);
        Assert.True(scored!.Checkmate);
    }

    [Fact]
    public void QuietMove_ScoresNothing()
    {
        var (_, scored) = Play(StartRound(StartPosition), "a2a3");
        Assert.Null(scored!.Tactic);
        Assert.Equal(0, scored.Score);
    }

    [Fact]
    public void Score_AddsUpOverTheRound()
    {
        var (round, _) = Play(StartRound(RoyalFork), "c2d4", "f5f6", "d4b5");
        Assert.Equal(4 + 21, round.Score); // the forking move gave check (4), then the fork paid off (21)
    }

    [Fact]
    public void EnemyMoves_NeverScore()
    {
        var round = StartRound("""
            . . . . . .
            . . . . . .
            . . . . . .
            . . . n . .
            . . P . . .
            . . . . . .
            """, Side.Enemy);
        var (after, scored) = Play(round, "d3c1");
        Assert.Null(scored);
        Assert.Equal(0, after.Score);
    }

    // ---- Forks ---------------------------------------------------------------

    [Fact]
    public void RoyalFork_TagsTheRookButNotTheKing()
    {
        var (round, scored) = Play(StartRound(RoyalFork), "c2d4");
        Assert.Equal([Square.Parse("b5")], scored!.ForkTargets);
        Assert.Equal([Square.Parse("b5")], round.ForkTags);
    }

    // Two rooks defend each other along rank 5, but each is worth more than the knight: still a fork.
    private const string TwoRooks = """
        k . . . . .
        . r . . . r
        . . . . . .
        . . . . . .
        . . . . N P
        . . . . . .
        """;

    [Fact]
    public void Fork_OnDefendedPiecesWorthMoreThanTheAttacker_Counts()
    {
        var (round, _) = Play(StartRound(TwoRooks), "e2d4");
        Assert.Equal(2, round.ForkTags.Count);
    }

    [Fact]
    public void Fork_OnTwoDefendedPawns_DoesNotCount()
    {
        // The queen hits b5 and d5, both guarded by the pawn on c6 and worth less than the queen,
        // and c6 behind them, which is a single real target.
        var (round, _) = Play(StartRound("""
            . . p . . .
            . p . p . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . Q . . .
            """), "c1c4");
        Assert.Empty(round.ForkTags);
    }

    [Fact]
    public void AttackingOnePiece_IsNotAFork()
    {
        var (round, _) = Play(StartRound("""
            . . . . . .
            . r . . . .
            . . . . . .
            . . . . . .
            . . N . . .
            . . . . . .
            """), "c2d4");
        Assert.Empty(round.ForkTags);
    }

    [Fact]
    public void ForkTag_LeavesWithAPieceThatEscapes()
    {
        // The b5 rook runs to b3, still in the knight's reach, but it escaped the fork: a plain capture.
        var (round, scored) = Play(StartRound(TwoRooks), "e2d4", "b5b3", "d4b3");
        AssertScored(scored, "capture", 5, 1);
        Assert.Empty(round.ForkTags);
    }

    [Fact]
    public void ForkTag_OnThePieceLeftBehind_StillPays()
    {
        var (afterEscape, _) = Play(StartRound(TwoRooks), "e2d4", "b5b3");
        Assert.Equal([Square.Parse("f5")], afterEscape.ForkTags); // b5 escaped, so its tag is gone

        var (_, scored) = Play(afterEscape, "d4f5");
        AssertScored(scored, "fork", 2 + 5, 3);
    }

    [Fact]
    public void ForkTags_ExpireAfterYourNextMove()
    {
        var (round, scored) = Play(StartRound(TwoRooks), "e2d4", "a6a5", "f2f3", "a5a6", "d4f5");
        AssertScored(scored, "capture", 5, 1);
        Assert.Empty(round.ForkTags);
    }

    // ---- Pins ----------------------------------------------------------------

    [Fact]
    public void PinToTheKing_IsAPin_AndAnyPieceCanCashIt()
    {
        // The rook on a1 pins the knight on a3 to the king; the bishop takes it.
        var round = StartRound("""
            k . . . . .
            . . . . . .
            . . . . . .
            n . . . . .
            . . . . . .
            R . B . . .
            """);
        Assert.Equal([Square.Parse("a3")], round.Pinned);
        AssertScored(Play(round, "c1a3").Last, "pin", 2 + 3, 4);
    }

    [Fact]
    public void PinOntoALessValuablePiece_DoesNotCount()
    {
        var (_, scored) = Play(StartRound("""
            . . . . . .
            . . . . n .
            . . . . . .
            . . q . . .
            . . . . . .
            B . . . . .
            """), "a1c3");
        AssertScored(scored, "capture", 9, 1);
    }

    [Theory]
    [InlineData("""
        k . . . . .
        . . . . . .
        p . . . . .
        n . . . . .
        . . . . . .
        R . . . . .
        """)] // a pawn between the knight and the king: the knight only shields a pawn
    [InlineData("""
        k . . . . .
        . . . . . .
        . . . . . .
        n . . . . .
        P . . . . .
        R . . . . .
        """)] // your own pawn blocks the rook's line
    [InlineData("""
        k . . . . .
        . . . . . .
        . . . . . .
        n . . . . .
        . . . . . .
        N . . . . .
        """)] // a knight can't pin
    public void NotAPin(string diagram)
    {
        Assert.Empty(StartRound(diagram).Pinned);
    }

    // ---- Highest Mult wins ---------------------------------------------------

    [Fact]
    public void PromotionCapturingAPinnedPiece_ScoresAsPromotion()
    {
        // The knight on c6 is pinned to the king by the rook on a6. Taking it while promoting is ×6, not ×4.
        var (_, scored) = Play(StartRound("""
            R . n . k .
            . P . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            """), "b5c6");
        AssertScored(scored, "promotion", 1 + 3, 6);
    }

    // ---- Preview -------------------------------------------------------------

    [Fact]
    public void Preview_MatchesTheActualScore_ForEveryLegalMove()
    {
        var rounds = new[]
        {
            StartRound(StartPosition),
            Play(StartRound(RoyalFork), "c2d4", "f5f6").Round,
            Play(StartRound(TwoRooks), "e2d4", "b5b3").Round,
            StartRound("""
                R . n . k .
                . P . . . .
                . . . . . .
                . . . . . .
                . . . . . .
                . . . . . .
                """),
        };
        foreach (var round in rounds)
        foreach (var move in Rules.LegalMoves(round.Position))
        {
            var preview = round.Preview(move);
            var (after, scored) = round.Play(move);
            Assert.Equal((preview.Tactic, preview.Value, preview.Mult), (scored!.Tactic, scored.Value, scored.Mult));
            Assert.Equal(preview.ForkTargets.Order(), scored.ForkTargets.Order());
            Assert.Equal(round.Score + preview.Score, after.Score);
        }
    }

    [Fact]
    public void Breakdown_ShowsTheSum()
    {
        var (_, scored) = Play(StartRound(RoyalFork), "c2d4", "f5f6", "d4b5");
        Assert.Equal("21 = (2 + 5) × 3", scored!.Breakdown);
    }

    // ---- Content -------------------------------------------------------------

    [Fact]
    public void Content_HasTheSpecNumbers()
    {
        Assert.Equal((0, 1), (Tactics["capture"].Value, Tactics["capture"].Mult));
        Assert.Equal((2, 2), (Tactics["check"].Value, Tactics["check"].Mult));
        Assert.Equal((2, 3), (Tactics["fork"].Value, Tactics["fork"].Mult));
        Assert.Equal((2, 4), (Tactics["pin"].Value, Tactics["pin"].Mult));
        Assert.Equal((1, 6), (Tactics["promotion"].Value, Tactics["promotion"].Mult));
        Assert.Equal((4, 10), (Tactics["enPassant"].Value, Tactics["enPassant"].Mult));
    }

    [Fact]
    public void Content_MissingATactic_IsRejected()
    {
        var error = Assert.Throws<InvalidDataException>(() => TacticCatalog.FromJson("""
            { "tactics": [ { "id": "capture", "name": "Capture", "callout": "CAPTURE", "value": 0, "mult": 1 } ] }
            """));
        Assert.Contains("check", error.Message);
    }
}
