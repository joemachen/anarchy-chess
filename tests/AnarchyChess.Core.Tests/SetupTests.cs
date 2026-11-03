using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// docs/specs/phase-1c-round.md, "The round" rules 1-4.
public class SetupTests
{
    private static readonly LadderConfig Ladder = LadderConfig.FromJson(Read("rounds.json"));

    private static Position Deal(long seed, int round) => Setup.Deal(Catalog, Ladder, seed, round);

    [Fact]
    public void SameSeedAndRound_DealsTheSameBoard()
    {
        Assert.Equal(Diagram.Write(Deal(48213, 2)), Diagram.Write(Deal(48213, 2)));
    }

    [Fact]
    public void DifferentRoundsAndSeeds_DealDifferentBoards()
    {
        var boards = Enumerable.Range(0, 5).Select(r => Diagram.Write(Deal(48213, r)))
            .Append(Diagram.Write(Deal(48214, 0)))
            .ToList();
        Assert.Equal(boards.Count, boards.Distinct().Count());
    }

    [Fact]
    public void RoundSeeds_AreReproducibleAndDistinct()
    {
        Assert.Equal(new Rng(Rng.ForRound(7, 3)).NextULong(), new Rng(Rng.ForRound(7, 3)).NextULong());
        Assert.NotEqual(Rng.ForRound(7, 3), Rng.ForRound(7, 4));
        Assert.NotEqual(Rng.ForRound(7, 3), Rng.ForRound(8, 3));
    }

    [Fact]
    public void EveryBoard_FollowsTheRules_Across1000Seeds()
    {
        for (var seed = 0; seed < 1000; seed++)
        for (var round = 0; round < Ladder.Rounds.Count; round++)
        {
            var pos = Deal(seed, round);
            var where = $"seed {seed}, round {round + 1}:\n{Diagram.Write(pos)}";
            var player = pos.PiecesOf(Side.Player).ToList();
            var enemy = pos.PiecesOf(Side.Enemy).ToList();

            Assert.True(pos.Width == 6 && pos.Height == 6, where);
            Assert.True(pos.ToMove == Side.Player, where);
            Assert.True(player.Count == Ladder.PlayerArmy.Count + 1, where);
            Assert.True(enemy.Count == Ladder.Rounds[round].Enemies + 1, where);
            Assert.True(player.Count(p => p.Piece.Def.Royal) == 1 && enemy.Count(p => p.Piece.Def.Royal) == 1, where);
            Assert.False(Rules.IsInCheck(pos, Side.Player), where);
            Assert.False(Rules.IsInCheck(pos, Side.Enemy), where);
            Assert.NotEmpty(Rules.LegalMoves(pos));

            Assert.All(player, p => Assert.True(p.Square.Rank <= 1, where));
            Assert.All(player.Where(p => p.Piece.Def.Pawn), p => Assert.True(p.Square.Rank == 1, where));
            Assert.All(enemy, p => Assert.True(p.Square.Rank >= 3, where));
            Assert.All(enemy.Where(p => p.Piece.Def.Pawn), p => Assert.True(p.Square.Rank <= 4, where));

            var kings = player.Concat(enemy).Where(p => p.Piece.Def.Royal).Select(p => p.Square).ToList();
            Assert.True(Math.Max(Math.Abs(kings[0].File - kings[1].File), Math.Abs(kings[0].Rank - kings[1].Rank)) > 1, where);
        }
    }

    [Fact]
    public void Content_LadderMatchesTheSpec()
    {
        Assert.Equal(10, Ladder.Moves);
        Assert.Equal([15, 25, 35, 50, 70], Ladder.Rounds.Select(r => r.Target));
        Assert.Equal([4, 5, 5, 6, 7], Ladder.Rounds.Select(r => r.Enemies));
        Assert.Equal(["rook", "bishop", "knight", "pawn", "pawn"], Ladder.PlayerArmy);
    }

    [Fact]
    public void StartRound_UsesTheLadderTargetAndMoves()
    {
        var round = Setup.StartRound(Catalog, Tactics, Ladder, 48213, 3);
        Assert.Equal(50, round.Target);
        Assert.Equal(10, round.MovesLeft);
        Assert.Equal(Outcome.Playing, round.Outcome);
        Assert.Equal(Diagram.Write(Deal(48213, 3)), Diagram.Write(round.Position));
    }
}
