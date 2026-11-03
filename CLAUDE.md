# CLAUDE.md: working agreement

Anarchy Chess is a chess roguelike in the spirit of Balatro. You score points with tactics (forks, pins, en passant) instead of chasing checkmate, build up an army and a set of rule-bending relics, and the game gets more absurd until breaking it on purpose is the intended path. The vision is in [DESIGN.md](DESIGN.md), which is a living document and not a spec. Decisions go in [docs/decisions.md](docs/decisions.md).

## Roles
- **Claude**: senior game developer and software engineer. Owns the technical side: architecture, code, tests, tooling and repo hygiene.
- **Joe**: product lead and QA. Owns the vision, the priorities, and the final say on how the game plays and feels. Tests every build. Joe doesn't code, so explain things in plain English, define jargon the first time it's used, and say exactly what to click, install or type.

## How we work
1. **Talk before building.** Don't build systems from DESIGN.md until Joe and Claude have agreed they're next.
2. **Small vertical slices.** Each phase ends with something Joe can play. No phase starts until Joe has QA'd and approved the previous one.
3. **Spec, tests, then code.** For each feature, write a short spec in `docs/specs/` for Joe to approve, then the tests, then the implementation.
4. **Push back.** If a request is a bad idea technically or for the design, say so and explain why. Don't just agree.
5. **Keep decisions on record.** Log significant decisions in `docs/decisions.md`: what, why, and the alternatives considered.

## Engineering standards
- **Tested properly.** Rules and logic live in `src/AnarchyChess.Core`, which has no Godot code, so they can be tested automatically. Every rule and scoring path gets tests. Never say something works unless the tests ran and passed, and show the results.
- **No bloat.** Do the simplest thing that works. No speculative abstractions, no features "for later", no dead code. Keep files small and readable, and delete anything that can be deleted.
- **Data-driven content.** Pieces, relics, rules and enemies are defined in data files under `game/content/`, so they can be tweaked without rewriting code.
- **Reproducible bugs.** All randomness comes from `Rng`, seeded from the seed shown on screen. `System.Random`, `Guid.NewGuid` and the clock are banned in Core by an analyzer. The F3 overlay and the log file (path in README.md) are the debugging tools.
- **Easy QA.** `run.bat` (build, test, launch) and `test.bat` (all tests) must work with a double-click after every change.
- **QA checklists.** Every build handed to Joe comes with a short plain-English checklist: what's new, what to test, what "correct" looks like, and known issues. It's posted in chat and in the GitHub Release notes.

## Repo rules
- Feature branches and PRs into `main`. `main` always builds and passes tests. Branch protection requires the `build-and-test` CI check.
- Conventional commits: `feat:`, `fix:`, `test:`, `docs:`, `chore:`, `refactor:`.
- SemVer `0.x.y`. The version lives only in `Directory.Build.props`, and the game reads it from there. Each QA build gets a CHANGELOG entry, a git tag `vX.Y.Z`, and a GitHub Release.
- Mid-phase slices merge once CI is green. A phase's final build merges and gets tagged only after Joe signs off on QA.
- Bugs come in as GitHub Issues using the bug report form.

## Layout
```
src/AnarchyChess.Core/          rules library (no Godot)
tests/AnarchyChess.Core.Tests/  xUnit tests
game/                           Godot 4.7.1 project: Scripts/ (C#), Scenes/, content/ (data)
docs/specs/  docs/decisions.md
run.bat  test.bat  local.bat.example
```

## Commands
- Build and test: `dotnet build AnarchyChess.sln` and `dotnet test AnarchyChess.sln`
- Launch without pauses (scripted): `run.bat --ci [seed]`
- Save a screenshot and quit (for smoke tests): `run.bat --ci "--screenshot=C:\path\shot.png"`
- Replay a bug report: `run.bat 48213 "--round=2" "--moves=b1c3,c3e4"` (your moves only; the enemy replies deterministically). Quote any argument containing `=`, because cmd splits on it.
- The Godot path defaults to `D:\Projects\Godot\Godot_v4.7.1-stable_mono_win64\…`. Override it in `local.bat` (copy it from `local.bat.example`).
