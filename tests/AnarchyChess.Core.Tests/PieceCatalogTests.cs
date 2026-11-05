namespace AnarchyChess.Core.Tests;

public class PieceCatalogTests
{
    [Theory]
    [InlineData("pawn", 1)]
    [InlineData("knight", 3)]
    [InlineData("bishop", 3)]
    [InlineData("rook", 5)]
    [InlineData("queen", 9)]
    public void Content_HasDesignDocValues(string id, int value)
    {
        Assert.Equal(value, TestContent.Catalog[id].Value);
    }

    [Fact]
    public void Content_KingIsRoyal_PawnIsPawn_PromotionIsQueen()
    {
        Assert.True(TestContent.Catalog["king"].Royal);
        Assert.True(TestContent.Catalog["pawn"].Pawn);
        Assert.Equal("queen", TestContent.Catalog.Promotion.Id);
    }

    [Theory]
    [InlineData("king", "♚")]
    [InlineData("queen", "♛")]
    [InlineData("rook", "♜")]
    [InlineData("bishop", "♝")]
    [InlineData("knight", "♞")]
    [InlineData("pawn", "♟")]
    public void Content_EveryPieceHasItsChessSymbol(string id, string glyph)
    {
        Assert.Equal(glyph, TestContent.Catalog[id].Glyph);
    }

    [Fact]
    public void PieceWithoutAGlyph_FallsBackToItsLetter()
    {
        var catalog = PieceCatalog.FromJson("""
            { "promotion": "x", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { "leap": [1, 0] } ] } ] }
            """);
        Assert.Equal("X", catalog["x"].Glyph);
    }

    [Fact]
    public void Patterns_ExpandToEveryDirection()
    {
        Assert.Equal(8, TestContent.Catalog["knight"].Moves.Count);
        Assert.Equal(4, TestContent.Catalog["rook"].Moves.Count);
        Assert.Equal(8, TestContent.Catalog["queen"].Moves.Count);
    }

    [Theory]
    [InlineData("""{ "promotion": "x", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { "leap": [1, 0] } ] }, { "id": "y", "letter": "X", "value": 1, "moves": [ { "leap": [1, 0] } ] } ] }""", "letter")]
    [InlineData("""{ "promotion": "x", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { "leap": [1, 0], "slide": [1, 0] } ] } ] }""", "exactly one")]
    [InlineData("""{ "promotion": "x", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { } ] } ] }""", "exactly one")]
    [InlineData("""{ "promotion": "x", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { "leap": [1] } ] } ] }""", "two numbers")]
    [InlineData("""{ "promotion": "nope", "pieces": [ { "id": "x", "letter": "X", "value": 1, "moves": [ { "leap": [1, 0] } ] } ] }""", "promotion")]
    public void BadContent_IsRejectedWithAClearMessage(string json, string messagePart)
    {
        var error = Assert.Throws<InvalidDataException>(() => PieceCatalog.FromJson(json));
        Assert.Contains(messagePart, error.Message);
    }
}
