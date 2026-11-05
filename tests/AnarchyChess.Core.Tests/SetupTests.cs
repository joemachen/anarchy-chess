using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// Dealing a round: docs/specs/phase-1c-round.md rules 1-4, and the bag in phase-2-army-bag.md.
public class SetupTests
{
    private static string Hand(Deployment d) => string.Join(",", d.Tray.Select(p => p.Id).Order());

    [Fact]
    public void SameSeedAndRound_DealsTheSameBoardAndHand()
    {
        var (a, b) = (Deploy(48213, 2), Deploy(48213, 2));
        Assert.Equal(Diagram.Write(a.Position), Diagram.Write(b.Position));
        Assert.Equal(a.Tray, b.Tray);
    }

    [Fact]
    public void DifferentRoundsAndSeeds_DealDifferentBoards()
    {
        var boards = Enumerable.Range(0, 5).Select(r => Diagram.Write(Deploy(48213, r).Position))
            .Append(Diagram.Write(Deploy(48214, 0).Position))
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
    public void EveryDeal_FollowsTheRules_Across1000Seeds()
    {
        for (var seed = 0; seed < 1000; seed++)
        for (var round = 0; round < Ladder.Rounds.Count; round++)
        {
            var deal = Deploy(seed, round);
            var pos = deal.Position;
            var where = $"seed {seed}, round {round + 1}:\n{Diagram.Write(pos)}";
            var player = pos.PiecesOf(Side.Player).ToList();
            var enemy = pos.PiecesOf(Side.Enemy).ToList();

            Assert.True(pos.Width == 6 && pos.Height == 6, where);
            Assert.True(pos.ToMove == Side.Player, where);

            // Only your king is on the board, on rank 1.
            Assert.True(player.Count == 1 && player[0].Piece.Def.Royal && player[0].Square.Rank == 0, where);
            Assert.True(enemy.Count == Ladder.Rounds[round].Enemies + 1, where);
            Assert.True(enemy.Count(p => p.Piece.Def.Royal) == 1, where);
            Assert.False(Rules.IsInCheck(pos, Side.Player), where);
            Assert.False(Rules.IsInCheck(pos, Side.Enemy), where);
            Assert.All(enemy, p => Assert.True(p.Square.Rank >= 3, where));
            Assert.All(enemy.Where(p => p.Piece.Def.Pawn), p => Assert.True(p.Square.Rank <= 4, where));

            // The hand comes from the bag, and there's always a way to start the round.
            Assert.True(deal.Tray.Count == Ladder.HandSize, where);
            var bag = Ladder.Bag.ToList();
            Assert.All(deal.Tray, p => Assert.True(bag.Remove(p.Id), where));
            Assert.True(deal.Tray.Any(p => deal.SquaresFor(p).Count > 0), where);
        }
    }

    [Fact]
    public void TheBagRefills_EveryRoundDrawsFromAllTenPieces()
    {
        // Five hands of 6 is 30 pieces from a bag of 10: only possible if the bag is full each round.
        Assert.All(Enumerable.Range(0, 5), r => Assert.Equal(Ladder.HandSize, Deploy(48213, r).Tray.Count));
    }

    [Fact]
    public void Hands_VaryWithTheSeed_AndCanContainEveryKindOfPiece()
    {
        var hands = Enumerable.Range(0, 200).Select(seed => Deploy(seed, 0)).ToList();
        Assert.True(hands.Select(Hand).Distinct().Count() > 5);
        Assert.All(Ladder.Bag.Distinct(), id => Assert.Contains(hands, h => h.Tray.Any(p => p.Id == id)));
    }

    [Fact]
    public void Content_LadderMatchesTheSpec()
    {
        Assert.Equal(10, Ladder.Moves);
        Assert.Equal([15, 25, 35, 50, 70], Ladder.Rounds.Select(r => r.Target));
        Assert.Equal([4, 5, 5, 6, 7], Ladder.Rounds.Select(r => r.Enemies));
        Assert.Equal(6, Ladder.HandSize);
        Assert.Equal(4, Ladder.FreePlacements);
        Assert.Equal(["bishop", "bishop", "knight", "knight", "pawn", "pawn", "pawn", "pawn", "pawn", "rook"], Ladder.Bag.Order());
    }

    [Fact]
    public void Deploy_CarriesTheRoundsTargetAndMoves()
    {
        var round = AutoStart(48213, 3);
        Assert.Equal(50, round.Target);
        Assert.Equal(10, round.MovesLeft);
        Assert.Equal(Outcome.Playing, round.Outcome);
    }
}
