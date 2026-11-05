using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// docs/specs/phase-1d-guides.md, "Exact definitions".
public class GuidesTests
{
    [Fact]
    public void Badge_ShowsThePiecesBestScore()
    {
        // The knight can take the pawn on b5 (1) or the rook on e4 (5).
        var round = StartRound("""
            . . . . . .
            . p . . . .
            . . . . r .
            . . N . . .
            . . . . . .
            . . . . . .
            """);
        Assert.Equal("+5", Guides.ForPiece(round, Square.Parse("c3"))?.Label);
    }

    // The knight can't capture anything, but e2-d4 forks both rooks. The pawn has only quiet moves.
    private const string ForkAvailable = """
        k . . . . .
        . r . . . r
        . . . . . .
        . . . . . .
        . . . . N P
        . . . . . .
        """;

    [Fact]
    public void Badge_SaysFork_WhenNothingScoresButAForkIsThere()
    {
        Assert.Equal("FORK", Guides.ForPiece(StartRound(ForkAvailable), Square.Parse("e2"))?.Label);
    }

    [Fact]
    public void NoBadge_WhenNothingScoresOrForks()
    {
        Assert.Null(Guides.ForPiece(StartRound(ForkAvailable), Square.Parse("f2")));
    }

    [Fact]
    public void ForkSetupMove_IsVisibleInItsPreview()
    {
        var round = StartRound(ForkAvailable);
        var setup = Rules.LegalMoves(round.Position).Single(m => m.From.Name + m.To.Name == "e2d4");
        Assert.Equal(0, round.Preview(setup).Score);
        Assert.True(Guides.SetsUpFork(round.Preview(setup)));
    }

    [Fact]
    public void LandingWhereTheEnemyAttacks_IsUnsafe()
    {
        var round = StartRound(ForkAvailable);
        Move M(string s) => Rules.LegalMoves(round.Position).Single(m => m.From.Name + m.To.Name == s);
        Assert.True(Guides.IsUnsafe(round.Position, M("e2f4"))); // the f5 rook covers f4
        Assert.False(Guides.IsUnsafe(round.Position, M("e2d4")));
    }

    [Fact]
    public void Threatened_IsYourPiecesTheEnemyCanTake_NeverTheKing()
    {
        // The rook checks your king; the bishop attacks your pawn on b2; your pawn on e2 is out of reach.
        var pos = Pos("""
            r . . . . .
            . . . . . .
            . . . b . .
            . . . . . .
            . P . . P .
            K . . . . .
            """);
        Assert.Equal([Square.Parse("b2")], Guides.Threatened(pos));
    }

    [Fact]
    public void Badge_AlwaysMatchesTheBestDot_OnDealtBoards()
    {
        for (var seed = 0; seed < 50; seed++)
        for (var r = 0; r < Ladder.Rounds.Count; r++)
        {
            var round = AutoStart(seed, r);
            foreach (var (square, _) in round.Position.PiecesOf(Side.Player))
            {
                var previews = Rules.LegalMoves(round.Position).Where(m => m.From == square).Select(round.Preview).ToList();
                var best = previews.Select(p => p.Score).DefaultIfEmpty(0).Max();
                var forks = previews.Any(Guides.SetsUpFork);
                var hint = Guides.ForPiece(round, square);
                Assert.Equal(best > 0 || forks, hint != null);
                if (hint != null)
                    Assert.Equal((best, forks), (hint.BestScore, hint.CanFork));
            }
        }
    }
}
