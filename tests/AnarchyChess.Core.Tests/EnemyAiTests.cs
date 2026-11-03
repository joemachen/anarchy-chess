using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// docs/specs/phase-1c-round.md, "The enemy AI". Each test shows a priority beating everything below it.
public class EnemyAiTests
{
    private static (string Move, string Reason) Choose(string diagram, long seed = 1)
    {
        var (move, reason) = EnemyAi.Choose(Pos(diagram, Side.Enemy), new Rng(seed));
        return (move.From.Name + move.To.Name, reason);
    }

    [Fact]
    public void Checkmate_BeatsTakingARook()
    {
        // The rook can mate on d1; the knight could take your rook on f3 instead.
        Assert.Equal(("d6d1", "checkmate"), Choose("""
            . . . r k .
            . . . . n .
            . . . . . .
            . . . . . R
            P P . . . .
            K . . . . .
            """));
    }

    [Fact]
    public void TakesYourMostValuablePiece()
    {
        // The rook on a3 can take your pawn on c3 or your rook on a1.
        Assert.Equal(("a3a1", "capture"), Choose("""
            . . . . . k
            . . . . . .
            . . . . . .
            r . P . . .
            . . . . . .
            R . . . . K
            """));
    }

    [Fact]
    public void TakesWithItsCheapestPiece()
    {
        // Both the queen and the knight can take your rook on d3: the knight does.
        Assert.Equal(("b4d3", "capture"), Choose("""
            . . . q . k
            . . . . . .
            . n . . . .
            . . . R . .
            . . . . . .
            K . . . . .
            """));
    }

    [Fact]
    public void SavesAThreatenedPiece_BeforeGivingCheck()
    {
        // Your knight attacks the undefended rook on c4. The rook could also check from c1 or a4, but saving it comes first.
        var (move, reason) = Choose("""
            . . . . . k
            . . . . . .
            . . r . . .
            . . . . . .
            . N . . . .
            K . . . . .
            """);
        Assert.Equal("rescue", reason);
        Assert.StartsWith("c4", move);
        Assert.DoesNotContain(move[2..], new[] { "a4", "c4", "d3", "d1", "a2", "b1", "b2" }); // squares you attack
    }

    [Fact]
    public void ADefendedCheapPiece_IsNotThreatened_SoItGivesCheck()
    {
        // Your knight attacks the pawn on c4, but it is defended and worth less than the knight.
        var (_, reason) = Choose("""
            . . . . . k
            . . . p . r
            . . p . . .
            . . . . . .
            . N . . . .
            K . . . . .
            """);
        Assert.Equal("check", reason);
    }

    private const string QuietPosition = """
        . . . . . k
        . p . . . .
        . . . . . .
        . . . . . .
        . . . . . .
        K . . . R .
        """;

    [Fact]
    public void OtherwiseMakesASafeMove()
    {
        // The king must not step onto e5 or e6, which your rook covers.
        for (var seed = 0; seed < 50; seed++)
        {
            var (move, reason) = Choose(QuietPosition, seed);
            Assert.Equal("safe", reason);
            Assert.DoesNotContain(move[2..], new[] { "e5", "e6" });
        }
    }

    [Fact]
    public void SafeMoves_ComeFromTheSeed()
    {
        Assert.Equal(Choose(QuietPosition, 5), Choose(QuietPosition, 5));
        var choices = Enumerable.Range(0, 50).Select(seed => Choose(QuietPosition, seed).Move).Distinct().Count();
        Assert.True(choices > 1, "the seed should vary the choice between equally good moves");
    }
}
