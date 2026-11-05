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

## D10: Scoring works like a Balatro poker hand (design, 2026-11-01)
- **What:** each tactic has a base Value and a Mult. Score = (tactic Value + captured piece value) × Mult, and only the highest-Mult tactic counts. En passant gets a base Value of 4, so its "royal flush" (50) beats a forked rook (21).
- **Why:** it matches DESIGN.md's tactic levels ("raise a tactic's Value and Mult") and keeps one readable number per move.
- **Alternatives:** stacking every tactic a move triggers; a separate Value for each tactic. Spec: docs/specs/phase-1b-scoring.md.

## D11: Forks need real targets (design, 2026-11-01)
- **What:** a fork counts only if 2 or more of its targets are the king, undefended, or worth more than the attacker.
- **Why:** on crowded 6×6 boards, "attacks two pieces" happens nearly every move, and the callout would stop meaning anything.
- **Alternatives:** any two attacked pieces.

## D12: The enemy is greedy and predictable (design, 2026-11-01)
- **What:** it plays the first of: checkmate, take your most valuable piece (with its cheapest attacker), save its own threatened piece, check, a safe move. Ties are broken by the seed.
- **Why:** DESIGN.md's "dumb but numerous AI"; the rescue step creates the "it can only save one" moment that makes forks work.
- **Alternatives:** a search-based engine (unpredictable, and fights the design); pure greed with no rescue (makes forks trivial). Spec: docs/specs/phase-1c-round.md.

## D13: Move guides are a game feature, not a dev tool (design, 2026-11-02)
- **What:** opportunity badges on your pieces, fork-setup and UNSAFE labels on move dots, and THREATENED markers. On by default; G toggles them. There is no "play the best move for me" button.
- **Why:** Joe's v0.0.4 playtest showed that non-chess players can't find moves, and that fork setups (which score 0) were invisible. An auto-play button would hide the Phase 1 question of whether finding forks is fun.
- **Alternatives:** a dev-only overlay; a best-move hint button. Spec: docs/specs/phase-1d-guides.md.

## D14: Phase 1 gate passed (design, 2026-11-04)
- **What:** after playing v0.1.0, Joe's verdict on "does banking points with a fork feel good?" is yes, forks are fun. The core scoring loop stays, and Phase 2 can start.
- **Why it took two tries:** on v0.0.4 Joe couldn't find good moves, so the question couldn't be answered. The move guides (D13) and chess symbols made it answerable.
- **Alternatives:** rework the scoring before adding more systems; not needed.
