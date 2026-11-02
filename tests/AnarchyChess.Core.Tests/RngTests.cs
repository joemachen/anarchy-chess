using AnarchyChess.Core;

namespace AnarchyChess.Core.Tests;

public class RngTests
{
    [Fact]
    public void SameSeed_GivesSameSequence()
    {
        var a = new Rng(48213);
        var b = new Rng(48213);
        for (var i = 0; i < 1000; i++)
            Assert.Equal(a.NextULong(), b.NextULong());
    }

    [Fact]
    public void DifferentSeeds_GiveDifferentSequences()
    {
        var a = new Rng(1);
        var b = new Rng(2);
        Assert.NotEqual(a.NextULong(), b.NextULong());
    }

    // Golden values: reference SplitMix64 output for seed 0. If this fails, the algorithm changed
    // and every seed from an older build (and every bug report) no longer replays. Don't "fix" the numbers.
    [Fact]
    public void Seed0_MatchesReferenceSplitMix64()
    {
        var rng = new Rng(0);
        Assert.Equal(0xE220A8397B1DCDAFUL, rng.NextULong());
        Assert.Equal(0x6E789E6AA1B965F4UL, rng.NextULong());
        Assert.Equal(0x06C45D188009454FUL, rng.NextULong());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(1000)]
    public void NextInt_StaysInRange(int max)
    {
        var rng = new Rng(7);
        for (var i = 0; i < 10_000; i++)
            Assert.InRange(rng.NextInt(max), 0, max - 1);
    }

    [Fact]
    public void NextInt_HitsEveryValueRoughlyEvenly()
    {
        var rng = new Rng(99);
        var counts = new int[6];
        for (var i = 0; i < 60_000; i++)
            counts[rng.NextInt(6)]++;
        Assert.All(counts, c => Assert.InRange(c, 9_000, 11_000));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void NextInt_RejectsEmptyRange(int max)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rng(1).NextInt(max));
    }
}
