using static AnarchyChess.Core.Tests.TestContent;

namespace AnarchyChess.Core.Tests;

// docs/specs/phase-1c-round.md, "The round" rules 5-7.
public class RoundRulesTests
{
    private const string PawnTakesKnight = """
        . . . . . k
        . . . . . .
        . . . . . .
        . . . n . .
        . . P . . .
        K . . . . .
        """;

    [Fact]
    public void OnlyYourMovesCountDown()
    {
        var (afterYou, _) = Play(StartRound(PawnTakesKnight, moves: 10), "a1a2");
        Assert.Equal(9, afterYou.MovesLeft);
        var (afterEnemy, _) = Play(afterYou, "f6f5");
        Assert.Equal(9, afterEnemy.MovesLeft);
    }

    [Fact]
    public void ReachingTheTarget_WinsAtOnce()
    {
        var (round, _) = Play(StartRound(PawnTakesKnight, target: 3, moves: 10), "c2d3");
        Assert.Equal(Outcome.WonTarget, round.Outcome);
        Assert.Equal(9, round.MovesLeft); // moves to spare
    }

    [Fact]
    public void ReachingTheTarget_OnTheLastMove_StillWins()
    {
        var (round, _) = Play(StartRound(PawnTakesKnight, target: 3, moves: 1), "c2d3");
        Assert.Equal(Outcome.WonTarget, round.Outcome);
    }

    [Fact]
    public void RunningOutOfMoves_ShortOfTheTarget_Loses()
    {
        var (round, _) = Play(StartRound(PawnTakesKnight, target: 99, moves: 1), "c2d3");
        Assert.Equal(Outcome.LostOutOfMoves, round.Outcome);
    }

    [Fact]
    public void CheckmatingTheEnemy_Wins_WhateverTheScore()
    {
        var (round, _) = Play(StartRound("""
            k . . p . .
            p p . . . .
            . . . . . .
            . . . . . .
            . . . . . .
            . . . Q . K
            """, target: 999), "d1d6");
        Assert.Equal(Outcome.WonCheckmate, round.Outcome);
    }

    private const string EnemyCanMate = """
        . . . r . k
        . . . . . .
        . . . . . .
        . . . . . .
        P P . . . .
        K . . . . .
        """;

    [Fact]
    public void BeingCheckmated_Loses()
    {
        var (round, _) = Play(StartRound(EnemyCanMate, Side.Enemy), "d6d1");
        Assert.Equal(Outcome.LostCheckmated, round.Outcome);
    }

    [Fact]
    public void BeingStalemated_PassesAndCostsAMove()
    {
        // The queen move leaves your king with no squares but not in check, and your pawn is blocked.
        var (round, _) = Play(StartRound("""
            . . . . . k
            . . . . . .
            . . . . . .
            p . . q . .
            P . . . . .
            K . . . . .
            """, Side.Enemy, moves: 5), "d3c2");
        Assert.Equal(Side.Enemy, round.Position.ToMove);
        Assert.Equal(4, round.MovesLeft);
        Assert.Equal(Outcome.Playing, round.Outcome);
    }

    [Fact]
    public void WhenNobodyCanMove_TheRoundIsLost()
    {
        var (round, _) = Play(StartRound("""
            . . . . . .
            . . . . . .
            . p . . . .
            . . . . . .
            . P . . . .
            . . . . . .
            """), "b2b3");
        Assert.Equal(Outcome.LostOutOfMoves, round.Outcome);
    }

    [Fact]
    public void FinishedRound_RefusesMoreMoves()
    {
        var (round, _) = Play(StartRound(PawnTakesKnight, target: 3), "c2d3");
        var anyMove = Rules.LegalMoves(round.Position)[0];
        Assert.Throws<InvalidOperationException>(() => round.Play(anyMove));
    }
}
