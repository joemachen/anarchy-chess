# Spec: Phase 0, Foundations (v0.0.1)

Status: covered by the approved project plan (2026-11-01).

## Goal
An empty but working game shell, so every later feature lands on tested plumbing: build, tests, CI, seed, version, debug overlay, logging.

## Behaviour
1. **Launch.** Double-clicking `run.bat` builds the code, runs every test, and opens the game window. If the build or any test fails, the game doesn't open, and the window says what failed and waits for a key press.
2. **Board.** The window shows an empty 6×6 chessboard with files a–f and ranks 1–6 labelled. Clicking a square highlights it. Clicking it again, or clicking off the board, clears the highlight.
3. **Seed.** Each launch picks a random 6-digit seed. `run.bat 48213` uses seed 48213 instead. The seed and the version (`v0.0.1`) are always visible in a corner.
4. **Seeded randomness.** The rules library has one random number generator. The same seed always produces the same numbers, on any machine and in any version.
5. **Debug overlay.** F3 shows or hides it. It contains:
   - the version, seed and Godot version
   - the latest 20 log lines
   - a **Verbose log** button that switches detailed logging on or off (for example, every square click)
   - a **Copy bug info** button that copies the version, seed and recent log to the clipboard for a bug report
6. **Logging.** Every log line also goes to a log file on disk. README.md says where to find it.
7. **Tests.** Double-clicking `test.bat` runs every test and ends with a clear PASSED or FAILED line, then waits for a key press.

## Out of scope
Pieces, moves, scoring, rounds and seed entry inside the game. All of these arrive in Phase 1.
