# Spec: Phase 1d, Move guides

Status: **draft, waiting for Joe's approval**

## Why
Playtest of v0.0.4 (Joe): *"I suck at chess and don't know which moves to make."* Two things in the current build cause this:
1. You only see what a move is worth after clicking that piece, so finding a good move means clicking all six pieces.
2. **Setting up a fork scores 0, so the move that creates one shows nothing at all.** Forks are what Phase 1 is testing, and right now the game hides them from anyone who can't already spot them.

DESIGN.md calls the score preview "the main teaching tool" and says players should learn chess "by accident". So these guides are a real, player-facing game feature, not a developer cheat. Most players won't be chess players either.

## The guides (all on by default; the **Guides** button or the G key toggles them)
1. **Opportunity badges on your pieces.** On your turn, each of your pieces shows the best thing it can do right now, without you clicking it:
   - **"+21"**: its best-scoring move is worth 21.
   - **"FORK"**: it can't score right now, but it can set up a fork.
   - Nothing: none of its moves score or set up a fork.

   One glance tells you which pieces are worth looking at.
2. **Fork setups on move dots.** When you select a piece, a dot that would set up a fork says **"SETS UP FORK"**, even though it scores nothing yet. If the move also scores (for example a check that forks), the dot shows both.
3. **Unsafe warnings on move dots.** A dot shows a red **"UNSAFE"** strip if the enemy could capture your piece after it lands there. The enemy is greedy, so it usually will. This is the most common way non-chess players lose pieces without understanding why.
4. **Threat markers on your pieces.** Any of your pieces the enemy can capture right now shows a red **"THREATENED"** badge. Your king already turns red when it's in check.

### Exact definitions (what the tests check)
- **Best score of a piece:** the highest preview score among its legal moves. It's the same calculation as the move dots, so the badge can never disagree with them.
- **Sets up a fork:** the move's preview tags at least one piece as Forked, using the fork rule from spec 1b.
- **Unsafe:** after the move, the square you landed on is attacked by an enemy piece. This deliberately ignores whether you could recapture, because the greedy enemy ignores that too.
- **Threatened:** one of your pieces other than the king, on a square an enemy piece attacks right now.

## Not included, on purpose
- **A "show me the best move" button.** I'm pushing back on this one. The badges already point at the good options. A button that just plays the best move would play the game for you, and then we'd never learn whether *you* enjoy finding forks, which is the whole Phase 1 question. If the guides aren't enough, we can revisit.
- **Animated callouts and typing a seed in the game.** These were planned for 1d, but neither helps answer the Phase 1 question. I'd move them to after the Phase 1 gate, so we get your verdict on forks sooner.

## What you'll be able to try (v0.1.0, the Phase 1 gate build)
The v0.0.4 game with the four guides added, plus a **Guides: ON/OFF** button. Once you've played it, we decide together whether the core loop is fun, and what changes before Phase 2.

## How it's tested
Each guide gets board-diagram tests:
- the badge shows the piece's best score, or "FORK" when nothing scores but a fork is available, or nothing at all
- a quiet knight move that forks is flagged "sets up fork"
- a move onto an attacked square is flagged unsafe, and the same move onto a safe square isn't
- a threatened piece is flagged, and a piece the enemy can't reach isn't
- the king is never marked "threatened"

A consistency test also checks, on every legal move across many dealt boards, that the badge equals the best dot on that piece.

## Decisions for you
1. **Guides are on by default**, with a toggle. Okay?
2. **No "show best move" button** for now. Agree?
3. **Animated callouts and seed entry move to after the Phase 1 gate.** Okay?
