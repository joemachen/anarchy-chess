# Decision log

Significant technical and design decisions: what, why, and what else was considered. Newest at the bottom. A decision can be replaced by a later one; when that happens, the old entry is marked superseded rather than deleted.

## D1: Engine is Godot 4.7.1 with C# (2026-11-01)
- **Why:** Joe chose it. It's already installed, and the same setup (Godot 4.7.1, .NET 8, a separate rules library) already works on this machine in another project. It has strong tools for game polish (particles, shaders, tweening) and exports natively to PC and mobile.
- **Alternatives:** TypeScript in the browser had a faster edit-reload loop, Claude could playtest it directly, and it would have given us a shareable web link. The trade-off was weaker built-in polish tools and a wrapper needed for Steam.

## D2: Game rules live in a plain C# library with no engine code (2026-11-01)
- **What:** `src/AnarchyChess.Core` holds every rule, score and AI decision. The Godot project in `game/` only draws state and passes clicks through. Core can't reference Godot, which a test enforces, and it can't use `System.Random` or the clock, which the banned-API analyzer enforces.
- **Why:** rules can then be tested automatically and quickly in CI, without opening Godot.
- **Alternatives:** logic written in Godot scripts can't be unit-tested without the engine.

## D3: Public repo, all rights reserved (2026-11-01)
- **Why:** a public repo gets enforced branch protection and free CI. "All rights reserved" keeps every right with Joe in case the game is sold.
- **Alternatives:** a private repo (branch rules aren't enforced on free accounts), MIT, GPL-3.0.

## D4: PC first, phone-friendly (2026-11-01)
- **What:** mouse and keyboard first, but every action works with a single click or tap, and no information is available only on hover.
- **Alternatives:** PC only; mobile first.

## D5: Real chess kings (design, 2026-11-01)
- **What:** the enemy always has a king and must escape check. Checkmating it wins the round instantly and pays a bonus. You're never offered an illegal move, and you lose if checkmated.
- **Why:** DESIGN.md had the enemy king appear only in boss rounds, which leaves Check and true pins with nothing to work against. Real kings also keep "chess you recognize".
- **Alternatives:** no enemy king outside boss rounds; kings as ordinary capturable pieces.

## D6: Free setup, then paid drops (design, 2026-11-01)
- **What:** you see the enemy, place your opening hand for free, and then the move clock starts. Dropping a piece mid-round costs a move. This starts in Phase 2.
- **Why:** with 10 moves per round, paying a move for each drop would burn half the round on setup.
- **Alternatives:** every drop costs a move; a fixed setup.

## D7: Tactics tag on setup, pay on capture (design, 2026-11-01)
- **What:** the move that creates a fork or pin tags the target pieces. Capturing a tagged piece later uses that tactic's Mult.
- **Why:** it matches how forks actually work (the opponent can only save one piece), and it rewards cashing the tactic in rather than just threatening.
- **Alternatives:** score the tactic the moment it's set up.

## D8: The enemy only threatens (design, 2026-11-01)
- **What:** enemy captures cost you pieces but never change your score.
- **Alternatives:** enemy captures subtract from your score.

## D9: Deterministic RNG is SplitMix64, pinned by golden tests (2026-11-01)
- **What:** all game randomness comes from `Rng`, a small SplitMix64 generator seeded from the number shown on screen. A test pins its first outputs.
- **Why:** a seed has to replay the same game on any machine and in any future version. `System.Random`'s algorithm isn't guaranteed to stay the same between .NET versions. The golden test fails loudly if anyone changes the algorithm, because that would break old bug-report seeds.
- **Alternatives:** `System.Random(seed)`; PCG or xoshiro (also good, slightly more code).
