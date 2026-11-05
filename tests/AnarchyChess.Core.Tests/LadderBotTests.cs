using Xunit.Abstractions;
using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

/// <summary>
/// A greedy stand-in player (always takes the best-scoring move) plays every round against the real AI.
/// Checks nothing crashes, hangs or plays an illegal move, and prints how often each round is won: first tuning data.
/// </summary>
public class LadderBotTests(ITestOutputHelper output)
{
    private const int Seeds = 200;

    private static Round PlayOut(Round round)
    {
        for (var ply = 0; round.Outcome == Outcome.Playing; ply++)
        {
            Assert.True(ply < 200, "round did not end");
            var legal = Rules.LegalMoves(round.Position);
            var move = round.Position.ToMove == Side.Player
                ? legal.MaxBy(m => round.Preview(m).Score)
                : EnemyAi.Choose(round.Position, round.EnemyRng()).Move;
            Assert.Contains(move, legal);
            round = round.Play(move).Next;
        }
        return round;
    }

    [Fact]
    public void GreedyBot_PlaysEveryRound_ToAnEnd()
    {
        var wins = new int[Ladder.Rounds.Count];
        var scores = new List<int>[Ladder.Rounds.Count];
        for (var r = 0; r < Ladder.Rounds.Count; r++)
        {
            scores[r] = [];
            for (var seed = 0; seed < Seeds; seed++)
            {
                var end = PlayOut(AutoStart(seed, r));
                if (end.Outcome is Outcome.WonTarget or Outcome.WonCheckmate)
                    wins[r]++;
                scores[r].Add(end.Score);
            }
        }

        output.WriteLine($"Greedy bot over {Seeds} seeds per round:");
        for (var r = 0; r < Ladder.Rounds.Count; r++)
        {
            var sorted = scores[r].Order().ToList();
            output.WriteLine($"  Round {r + 1} (target {Ladder.Rounds[r].Target}): won {100 * wins[r] / Seeds}%, " +
                             $"median score {sorted[Seeds / 2]}");
        }
    }

    [Fact]
    public void SameSeed_PlaysOutTheSameWay()
    {
        var a = PlayOut(AutoStart(48213, 2));
        var b = PlayOut(AutoStart(48213, 2));
        Assert.Equal((a.Outcome, a.Score, a.MovesLeft), (b.Outcome, b.Score, b.MovesLeft));
        Assert.Equal(Diagram.Write(a.Position), Diagram.Write(b.Position));
    }
}
