# Changelog

All notable changes to this project. Format: [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versions follow [SemVer](https://semver.org/).

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
