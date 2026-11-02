# Spec: Phase 1b, Tactics and scoring

Status: **draft, waiting for Joe's approval**

## Goal
Your moves score points: **Score = Value × Mult**, where the tactic you pulled off sets the Mult. This is the heart of the game, so every tactic is defined precisely enough to test. The enemy never scores (decision D8).

## The Balatro mapping
In Balatro, a poker hand (pair, flush…) has its own **base chips and mult**, and the cards you play add their chips on top. Here:
- a **tactic** is the poker hand. It has a base Value and a Mult.
- the **piece you capture** is the cards. Its value (pawn 1, knight 3, bishop 3, rook 5, queen 9) is added to the Value.

DESIGN.md already says tactic levels will raise "a tactic's Value and Mult", so this fits what's planned.

**Score = (tactic's base Value + value of the captured piece) × tactic's Mult**

If a move qualifies for several tactics, **only the one with the highest Mult counts**, just as Balatro scores only your best hand. Stacking tactics can come later through Anarchists.

## Tactics (proposed numbers, all tunable in `game/content/tactics.json`)
| Tactic | Base Value | Mult | When it counts |
| --- | --- | --- | --- |
| Capture | 0 | ×1 | You capture anything |
| Check | 2 | ×2 | Your move puts the enemy king in check, including discovered checks and checks without a capture |
| Fork | 2 | ×3 | You capture a piece that is **Forked** (see below) |
| Pin | 2 | ×4 | You capture a piece that is **Pinned** at that moment |
| Promotion | 1 | ×6 | Your pawn promotes ("counts as a capture of a pawn") |
| En passant | 4 | ×10 | You capture en passant. Callout: **HOLY HELL** |

**Checkmate** adds the enemy king's value (20, from pieces.json) to the Value, on top of the Check tactic. Ending the round on a checkmate is part of 1c.

A move that triggers none of these (a quiet move) scores 0.

### Examples
| Move | Calculation | Score |
| --- | --- | --- |
| Pawn takes knight | (0 + 3) × 1 | 3 |
| Quiet check | (2 + 0) × 2 | 4 |
| Rook takes bishop, giving check | (2 + 3) × 2 | 10 |
| Knight takes a forked rook | (2 + 5) × 3 | 21 |
| Bishop takes a pinned knight | (2 + 3) × 4 | 20 |
| Pawn promotes | (1 + 0) × 6 | 6 |
| Pawn promotes by taking a rook | (1 + 5) × 6 | 36 |
| En passant | (4 + 1) × 10 | **50** |
| Queen takes pawn, checkmate | (2 + 1 + 20) × 2 | 46 |

I gave en passant a base Value of 4 on purpose. With only the pawn's 1 point, the "royal flush" would score 10, which is less than taking a forked rook.

## Fork (decision D7: tag on setup, pay on capture)
1. **Setting it up:** after your move, the piece **you just moved** attacks 2 or more enemy pieces that are each a *real target*. A real target is the enemy king, an undefended piece (no enemy piece guards its square), or a piece worth more than your attacker.
2. Each real target, apart from the king, gets a **FORKED** tag on screen. The fork callout plays. The setup move itself scores only what it captured or checked, if anything.
3. **Paying off:** if you capture a Forked piece **on your very next move**, with any of your pieces, it scores as a Fork (×3).
4. **Tags expire:** a Forked piece that moves away escapes, and its tag goes. Any tag left after your next move disappears. The enemy gets one move to save one piece, just like a real fork.

The real-target rule means a queen attacking two defended pawns isn't a "fork". Without it, crowded boards would throw up forks constantly, and the word would stop meaning anything.

A **royal fork** (check plus another piece) works naturally: the enemy must deal with the check, so the other piece is usually yours.

## Pin
1. An enemy piece is **Pinned** when one of your sliding pieces (rook, bishop or queen, or anything with a "slide" move in pieces.json) points at it along a clear line, and directly behind it on that line sits the enemy king or an enemy piece worth more.
2. Pinned pieces show a **PINNED** marker **for as long as the pin holds**. There's no countdown: a pin lasts until something breaks the line.
3. Capturing a piece that is Pinned at that moment, with any of your pieces, scores as a Pin (×4).

Forks expire after one turn but pins don't, because that's how they work in real chess: a fork is a one-turn threat, while a pin is a standing situation.

## Score preview
One function works out exactly what any move would score: tactic, Value, Mult, total, and any Forks it would set up. The preview and the actual scoring use that same function, so **the preview can never disagree with what you get**, and a test checks this on every legal move in many positions.

## What you'll be able to try (v0.0.3, the sandbox with scoring)
- A **score counter** for you. Enemy moves never score.
- On your turn, each move dot shows what it would earn, for example "+21 FORK". Quiet moves show nothing.
- FORKED and PINNED markers on enemy pieces.
- A plain-text callout after each scoring move, for example "FORK! 21 = (2 + 5) × 3". Animated callouts come in 1d.

You still move the enemy by hand, which makes it easy to set up forks and pins on purpose and check that they score correctly.

## How it's tested
Every row of both tables above gets its own test, built from a board diagram. On top of that:
- **Fork boundaries:** a fork on two defended pawns doesn't count, and neither does a fork on a single piece. A royal fork does count. A tag expires after one turn, and so does a tag on a piece that moved away.
- **Pin boundaries:** a pin onto a less valuable piece doesn't count, a knight can't pin, and a pin broken by a blocking piece no longer counts.
- **Highest Mult wins:** for example, a promotion that captures a pinned piece scores as a Promotion (×6), not a Pin (×4).
- **Preview matches the result:** checked on every legal move across several positions.

## Decisions for you
1. **Balatro-style base Values per tactic**, with the numbers in the table. OK, or different numbers? They're easy to tune later either way.
2. **Real-target forks** (a threat has to matter) rather than "attacks any two pieces"?
3. **Fork tags last one turn, pins last while they hold?**
