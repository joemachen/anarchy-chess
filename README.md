# Anarchy Chess

A chess roguelike where you score points with tactics (forks, pins, en passant) instead of chasing checkmate. You build an army of rule-bending pieces and relics until the board stops making sense. Think Balatro's run structure, Disgaea's "break the game on purpose" endgame, and r/AnarchyChess's sense of humour.

Status: early development. See [CHANGELOG.md](CHANGELOG.md) for what's in each build and [DESIGN.md](DESIGN.md) for the vision.

## Play (Windows)
1. Double-click **`run.bat`**. It builds the game, runs every test, then opens the game window. If anything fails, the window says what went wrong and the game doesn't open.
2. To replay a specific seed, open a terminal in this folder and type `run.bat 48213`.

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [Godot 4.7.1 .NET](https://godotengine.org/download). If Godot isn't at the default path, copy `local.bat.example` to `local.bat` and edit the path inside.

## Run the tests
Double-click **`test.bat`**. It ends with **ALL TESTS PASSED** or **TESTS FAILED**.

## Reporting bugs
1. In the game, press **F3** (or click **Debug** bottom-left) and click **Copy bug info**.
2. Go to the repo's [Issues](https://github.com/joemachen/anarchy-chess/issues/new/choose) page, choose **Bug report**, and paste the bug info into the last box.

The seed and version are always in the bottom-right corner. The full log is at
`%APPDATA%\Godot\app_userdata\Anarchy Chess\logs\godot.log` (paste that path into File Explorer's address bar).

## Project layout
| Folder | What's in it |
| --- | --- |
| `src/AnarchyChess.Core` | All game rules, in plain C# with no Godot code, so they can be tested automatically |
| `tests/AnarchyChess.Core.Tests` | Automated tests |
| `game/` | The Godot project: drawing the game and handling input |
| `docs/specs/` | Short specs, one per feature, approved before building |
| `docs/decisions.md` | Why things are the way they are |

## License
Copyright (c) 2026 joemachen. All rights reserved. See [LICENSE](LICENSE).
