# Spec: Phase 1c, The round and the enemy AI

Status: **draft, waiting for Joe's approval**

## Goal
This is the first build that plays like the game rather than a sandbox. Each round deals a fresh board from the seed, and the enemy moves on its own. You get **10 moves to reach a score target**, across a ladder of 5 rounds. This build answers the Phase 1 question: does banking points with a fork feel good?

## The round
1. **Setup from the seed.** Each round's board is generated from (seed, round number). That means "seed 48213, round 3" always deals the same board, even if you skipped straight to round 3 when replaying a bug.
2. **Your army** is the same every round for now: king, rook, bishop, knight and 2 pawns. Your pieces are placed randomly: the pawns on rank 2, so they can still double-step, and everything else on rank 1 or 2. The army bag and drawing a hand arrive in Phase 2.
3. **The enemy army** is a king plus a few random pieces, scattered across ranks 4–6 rather than lined up. Pieces are drawn from a weighted pool: mostly pawns, some minor pieces, rarely a queen. Enemy pawns sit on ranks 4–5 only.
4. **Every board is legal and fair:** neither king starts in check, the kings aren't next to each other, and no pawn starts on a promotion rank. If a generated board breaks a rule, the generator tries again, which stays deterministic because the retries come from the same seed.
5. **You move first.** You have **10 moves**, and only your moves count down. If you're stalemated, you pass, and the pass uses one of your moves.
6. **You win the round** the moment your score reaches the target, like Balatro ending the blind as soon as you pass the chips. **Checkmating the enemy also wins instantly**, whatever your score (decision D5). Any moves you didn't use are shown; in Phase 2 they'll pay out money.
7. **You lose the round** if you're checkmated, or if your 10 moves run out short of the target.

## The ladder (numbers in `game/content/rounds.json`)
| Round | Target | Enemy pieces (besides the king) |
| --- | --- | --- |
| 1 | 15 | 4 |
| 2 | 25 | 5 |
| 3 | 35 | 5 |
| 4 | 50 | 6 |
| 5 | 70 | 7 |

Win a round and you go on to the next, with a new board and your army back to full. Lose and the ladder ends, showing how far you got; **Retry** starts round 1 again with a new seed. Clear round 5 and you've won the ladder. These targets are guesses, and tuning them is what your playtesting is for.

## The enemy AI: greedy and predictable
DESIGN.md asks for an enemy that is "dumb but numerous", so you can plan against it. On each turn it goes down this list and plays the first thing that applies:
1. **Checkmate you** if it can.
2. **Take your most valuable piece** it can reach. If more than one enemy piece can take it, it uses its *cheapest* one.
3. **Save its most valuable threatened piece:** if one of your pieces attacks an enemy piece that is undefended or worth more than your attacker, it moves that piece to a safe square. This is what makes forks work: it can only save one piece.
4. **Give check** if it can.
5. Otherwise, a **random safe move** (to a square you don't attack), chosen from the seed.

If it's in check, it can only choose from moves that escape the check, and the same list applies. Whenever two options are equally good, the seed breaks the tie, so a replay always plays out the same way.

## What you'll see (v0.0.4)
- A heads-up display (the always-on info around the board) showing Round 2/5, Target 25, Score and Moves left.
- After your move, the enemy replies on its own after a short pause, and its last move is highlighted so you can see what it did.
- A result panel: "Round won! (3 moves to spare)", "Out of moves: 18 / 25" or "Checkmated", with a **Next round** or **Retry** button.
- Everything from v0.0.3 stays: previews, FORKED and PINNED badges, and callouts.
- The sandbox and its Reset button go away.

The animated callouts, markers showing which of your pieces are under threat, and typing a seed in-game all arrive in 1d.

## How it's tested
- **Generator:** the same seed and round give the same board, and different rounds give different boards. Across 1,000 seeds, every board follows rule 4, armies have the sizes rounds.json asks for, and pawns sit only on their allowed ranks.
- **Round rules:**
  - Only your moves count down.
  - Reaching the target wins at once.
  - Checkmating the enemy wins.
  - Being checkmated loses.
  - Running out of moves loses.
  - A stalemate pass costs you a move.
- **AI:** each priority on the list gets its own board diagram showing the AI picks it over everything below it. The AI always plays a legal move, and the same seed always gives the same moves.
- **A full ladder played by a bot:** a simple stand-in player takes the best-scoring move each turn and plays all 5 rounds across many seeds. This checks that no board crashes or hangs, and tells us roughly how often the targets are reachable, which is our first tuning data.

## Decisions for you
1. **The round ends as soon as you hit the target,** rather than playing out all 10 moves. Leftover moves become money in Phase 2.
2. **The AI saves its threatened pieces** (step 3). Without it, a fork has no real "the enemy can only save one" moment. With it, the enemy is a bit less dumb.
3. **A fixed army for you this phase:** king, rook, bishop, knight and 2 pawns, with no queen. A queen on a 6×6 board does nearly everything on its own, which would hide whether forks are fun.
4. **The ladder numbers above** as a starting point.
