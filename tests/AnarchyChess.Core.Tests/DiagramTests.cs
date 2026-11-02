using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

public class DiagramTests
{
    [Fact]
    public void TopLineIsTheHighestRank_UppercaseIsPlayer()
    {
        var pos = Pos("""
            k . .
            . . .
            . . R
            """);
        Assert.Equal(3, pos.Width);
        Assert.Equal(3, pos.Height);
        Assert.Equal(new Piece(Catalog["king"], Side.Enemy), pos[Square.Parse("a3")]);
        Assert.Equal(new Piece(Catalog["rook"], Side.Player), pos[Square.Parse("c1")]);
        Assert.Null(pos[Square.Parse("b2")]);
    }

    [Fact]
    public void Write_RoundTripsWithParse()
    {
        var text = Diagram.Write(Pos(StartPosition));
        Assert.Equal(text, Diagram.Write(Pos(text)));
        Assert.StartsWith("r n b q k r", text);
    }

    [Fact]
    public void UnknownLetter_Throws() =>
        Assert.Throws<FormatException>(() => Pos(". z\n. ."));

    [Fact]
    public void RaggedRows_Throw() =>
        Assert.Throws<FormatException>(() => Pos(". .\n. . ."));

}
