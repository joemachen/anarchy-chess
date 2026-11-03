# Changelog

All notable changes to this project. Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versions follow [SemVer](https://semver.org/).

## [0.1.0] - 2026-11-02
### Added
- Move guides (on by default; G or the Guides button toggles them):
  - each of your pieces shows its best move this turn ("+21" or "FORK")
  - move dots show "SETS UP FORK" for fork setups, and a red "UNSAFE" if the enemy could take your piece there
  - your pieces the enemy can capture right now show "THREATENED"
- Decision log entries D10-D13.

## [0.0.4] - 2026-11-01
### Added
- Real rounds: each board is dealt from (seed, round). Your fixed army (K, R, B, N, 2 P) vs an enemy king plus a weighted-random army. 10 moves to reach the target; checkmate wins outright.
- A 5-round ladder (targets 15/25/35/50/70) in `game/content/rounds.json`, with result panel, Next round, and Retry (new seed).
- Greedy, predictable enemy AI that moves by itself: checkmate > take your best piece (cheapest attacker) > save its threatened piece > check > safe move.
- Enemy's last move highlighted; HUD shows round, target and moves left.
- Bug info includes a one-line replay command; `--round=` and `--moves=` (your moves only) replay a report.
- A greedy test bot plays every round across 200 seeds as a smoke test and tuning report.

### Removed
- The hand-played sandbox and its Reset button.

## [0.0.3] - 2026-11-01
### Added
- Scoring: Score = (tactic base Value + captured value) x tactic Mult; the highest-Mult tactic wins. Capture, Check, Fork, Pin, Promotion, En passant (HOLY HELL), checkmate bonus. Numbers in `game/content/tactics.json`.
- Forks tag their targets for one turn; pins count while they hold.
- Sandbox shows your score, "+21 FORK"-style previews on move dots, FORKED/PINNED markers, and a callout with the sum.
- `--select=` launch option (select a square, for screenshots and bug repro).

## [0.0.2] - 2026-11-01
### Added
- Chess rules on a 6x6 board: every piece's moves, check, checkmate, stalemate (the stuck side passes), en passant, promotion to queen. No castling.
- Piece movement and values defined in `game/content/pieces.json`.
- Rules sandbox: move both sides by hand from a fixed start position, with legal-move dots, check highlight, status line and Reset.
- `--moves=` launch option to replay a move list (for bug reports).

## [0.0.1] - 2026-11-01
### Added
- Project foundations: rules library, tests, Godot game shell, `run.bat`, `test.bat`, CI.
- Empty 6×6 board with clickable squares.
- Seed (random, or passed to `run.bat`) and version shown on screen.
- Deterministic seeded random number generator.
- F3 debug overlay with recent log, verbose-log toggle and "Copy bug info".
- Log file on disk.
