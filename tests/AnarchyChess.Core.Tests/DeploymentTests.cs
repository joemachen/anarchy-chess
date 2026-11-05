using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// docs/specs/phase-2-army-bag.md, "Free setup". Diagrams: top line is rank 6, uppercase = yours.
public class DeploymentTests
{
    private static Deployment From(string diagram, params string[] tray) =>
        new(Pos(diagram), tray.Select(id => Catalog[id]).ToList(), 4, Tactics, 15, 10, 7);

    private const string Empty = """
        . . . . k .
        . . . . . .
        . . . . . .
        . . . . . .
        . . . . . .
        K . . . . .
        """;

    private static string[] Names(IEnumerable<Square> squares) => squares.Select(s => s.Name).Order().ToArray();

    [Fact]
    public void Pieces_GoOnYourTwoBackRanks_OnEmptySquares()
    {
        var squares = From(Empty, "knight").SquaresFor(Catalog["knight"]);
        Assert.Equal(11, squares.Count); // 12 squares, minus your king's
        Assert.All(squares, s => Assert.True(s.Rank <= 1));
        Assert.DoesNotContain(Square.Parse("a1"), squares);
    }

    [Fact]
    public void Pawns_GoOnRank2Only()
    {
        Assert.Equal(["a2", "b2", "c2", "d2", "e2", "f2"], Names(From(Empty, "pawn").SquaresFor(Catalog["pawn"])));
    }

    [Fact]
    public void SquaresThatWouldCheckTheEnemyKing_AreNotOffered()
    {
        var deployment = From(Empty, "rook", "bishop");
        var rook = deployment.SquaresFor(Catalog["rook"]);
        Assert.DoesNotContain(Square.Parse("e1"), rook); // straight up the open e-file
        Assert.DoesNotContain(Square.Parse("e2"), rook);
        Assert.Contains(Square.Parse("d1"), rook);
        Assert.DoesNotContain(Square.Parse("a2"), deployment.SquaresFor(Catalog["bishop"])); // along the a2-e6 diagonal
    }

    private const string FourPlaced = """
        . . . . k .
        . . . . . .
        . . . . . .
        . . . . . .
        P P P P . .
        K . . . . .
        """;

    [Fact]
    public void TheFourPieceLimit_Holds()
    {
        var full = From(FourPlaced, "knight", "bishop");
        Assert.Equal(4, full.Placed);
        Assert.Empty(full.SquaresFor(Catalog["knight"]));
        Assert.Throws<InvalidOperationException>(() => full.Place(Catalog["knight"], Square.Parse("f1")));
    }

    [Fact]
    public void Placing_MovesThePieceFromTheTrayToTheBoard()
    {
        var placed = From(Empty, "pawn", "pawn", "knight").Place(Catalog["pawn"], Square.Parse("c2"));
        Assert.Equal(new Piece(Catalog["pawn"], Side.Player), placed.Position[Square.Parse("c2")]);
        Assert.Equal(["knight", "pawn"], placed.Tray.Select(p => p.Id).Order());
        Assert.Equal(1, placed.Placed);
    }

    [Fact]
    public void PlacingOnASquareThatIsNotOffered_IsRefused()
    {
        var deployment = From(Empty, "pawn");
        Assert.Throws<InvalidOperationException>(() => deployment.Place(Catalog["pawn"], Square.Parse("c1")));
    }

    [Fact]
    public void PlacingAPieceThatIsNotInYourTray_IsRefused()
    {
        var deployment = From(Empty, "pawn");
        Assert.Throws<InvalidOperationException>(() => deployment.Place(Catalog["knight"], Square.Parse("c1")));
    }

    [Fact]
    public void PickingUp_ReturnsThePieceToTheTray()
    {
        var back = From(Empty, "knight").Place(Catalog["knight"], Square.Parse("c1")).PickUp(Square.Parse("c1"));
        Assert.Null(back.Position[Square.Parse("c1")]);
        Assert.Equal([Catalog["knight"]], back.Tray);
        Assert.Equal(0, back.Placed);
    }

    [Theory]
    [InlineData("a1")] // your king stays put
    [InlineData("e6")] // an enemy piece
    [InlineData("c2")] // nothing there
    public void OnlyYourPlacedPieces_CanBePickedUp(string square)
    {
        Assert.Throws<InvalidOperationException>(() => From(Empty).PickUp(Square.Parse(square)));
    }

    [Fact]
    public void UnsafeSquares_AreThoseTheEnemyAttacks()
    {
        var deployment = From("""
            . r . . k .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            K . . . . .
            """, "knight");
        Assert.True(deployment.IsUnsafe(Catalog["knight"], Square.Parse("b2"))); // the rook covers the b-file
        Assert.False(deployment.IsUnsafe(Catalog["knight"], Square.Parse("c2")));
    }

    [Fact]
    public void YouNeedAtLeastOnePiece_ToStart()
    {
        var empty = From(Empty, "knight");
        Assert.NotNull(empty.StartBlockedReason);
        Assert.Throws<InvalidOperationException>(() => empty.Start());
        Assert.Null(empty.Place(Catalog["knight"], Square.Parse("c1")).StartBlockedReason);
    }

    [Fact]
    public void PickingUpAPiece_ThatOpensACheck_BlocksTheStart()
    {
        // The bishop shields the enemy king from your rook. Lifting it opens the a-file.
        var deployment = From("""
            k . . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            B . . . . .
            R . . . K .
            """);
        Assert.Null(deployment.StartBlockedReason);
        var opened = deployment.PickUp(Square.Parse("a2"));
        Assert.Contains("check", opened.StartBlockedReason);
        Assert.Throws<InvalidOperationException>(() => opened.Start());
    }

    [Fact]
    public void Start_BeginsTheRound_WithYourPlacedPieces()
    {
        var round = From(Empty, "knight").Place(Catalog["knight"], Square.Parse("c1")).Start();
        Assert.Equal((15, 10, 7L), (round.Target, round.MovesLeft, round.Seed));
        Assert.Equal(Side.Player, round.Position.ToMove);
        Assert.Equal(Catalog["knight"], round.Position[Square.Parse("c1")]!.Def);
    }

    [Fact]
    public void Preview_LetsTheGuidesReadASetup()
    {
        // A knight placed on c2 could take the enemy rook on d4 on move 1.
        var deployment = From("""
            . . . . . k
            . . . . . .
            . . . r . .
            . . . . . .
            . . N . . .
            K . . . . .
            """);
        Assert.Equal("+5", Guides.ForPiece(deployment.Preview, Square.Parse("c2"))?.Label);
    }
}
