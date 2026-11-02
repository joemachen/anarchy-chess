namespace AnarchyChess.Core.Tests;

public class SquareTests
{
    [Theory]
    [InlineData(0, 0, "a1")]
    [InlineData(4, 2, "e3")]
    [InlineData(5, 5, "f6")]
    public void Name_RoundTripsWithParse(int file, int rank, string name)
    {
        Assert.Equal(name, new Square(file, rank).Name);
        Assert.Equal(new Square(file, rank), Square.Parse(name));
    }
}
