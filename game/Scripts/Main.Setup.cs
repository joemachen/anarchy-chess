using AnarchyChess.Core;
using Godot;
using Side = AnarchyChess.Core.Side;

namespace AnarchyChess.Godot;

/// <summary>Free setup before each round (docs/specs/phase-2-army-bag.md). All the rules live in Deployment.</summary>
public partial class Main
{
    private Deployment? _deployment;
    private int? _trayPick;
    private Square? _placedPick;

    private void ShowSetup()
    {
        var deployment = _deployment!;

        // A placed piece being moved counts as lifted, so its own square and the placement limit work out.
        var lifted = _placedPick is { } from ? deployment.PickUp(from) : deployment;
        var held = _placedPick is { } s ? deployment.Position[s]!.Def : _trayPick is { } i ? deployment.Tray[i] : null;
        var allowed = held != null ? lifted.SquaresFor(held) : new HashSet<Square>();
        var risky = allowed.Where(sq => lifted.IsUnsafe(held!, sq)).ToHashSet();

        _board.LastMove = null;
        _board.Marks = new SetupMarks(allowed, risky);
        _board.Round = deployment.Preview;
        if (_placedPick is { } selected)
            _board.Select(selected);
        _tray.ShowSetup(deployment.Tray, _trayPick, deployment.Placed, deployment.FreePlacements, deployment.StartBlockedReason);
        UpdateHud();
    }

    private void OnTrayPiece(int index)
    {
        _trayPick = _trayPick == index ? null : index;
        _placedPick = null;
        ShowSetup();
    }

    private void OnSetupSquare(Square square)
    {
        var deployment = _deployment!;
        if (_trayPick is { } index && deployment.SquaresFor(deployment.Tray[index]).Contains(square))
        {
            _deployment = deployment.Place(deployment.Tray[index], square);
            _trayPick = null;
        }
        else if (_placedPick == square)
        {
            _deployment = deployment.PickUp(square);
            _placedPick = null;
        }
        else if (_placedPick is { } from && deployment.PickUp(from).SquaresFor(deployment.Position[from]!.Def).Contains(square))
        {
            _deployment = deployment.PickUp(from).Place(deployment.Position[from]!.Def, square);
            _placedPick = null;
        }
        else
        {
            _trayPick = null;
            _placedPick = deployment.Position[square] is { Side: Side.Player, Def.Royal: false } ? square : null;
        }
        ShowSetup();
    }

    private void ClearSetupPick()
    {
        _trayPick = null;
        _placedPick = null;
        ShowSetup();
    }

    private void OnStartRound()
    {
        var deployment = _deployment!;
        _deployment = null;
        _trayPick = null;
        _placedPick = null;
        _placements = Placements(deployment);
        _board.Marks = null;
        _board.Round = deployment.Start();
        _tray.ShowReserves(deployment.Tray);
        Log.Info($"Round started, setup {_placements}:\n{Diagram.Write(deployment.Position)}");
        UpdateHud();
    }

    /// <summary>Your placed pieces as text like "Nb2,Pa2", for bug reports and --setup.</summary>
    private static string Placements(Deployment deployment) =>
        string.Join(",", deployment.Position.PiecesOf(Side.Player)
            .Where(p => !p.Piece.Def.Royal)
            .Select(p => $"{p.Piece.Def.Letter}{p.Square}"));

    /// <summary>Places pieces from text like "Nb2,Pa2": replays a bug report's setup.</summary>
    private void ReplaySetup(string placements)
    {
        foreach (var text in placements.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var piece = _catalog.ByLetter(text[0]);
            var square = Square.Parse(text[1..]);
            if (piece == null || !_deployment!.Tray.Contains(piece) || !_deployment.SquaresFor(piece).Contains(square))
            {
                Log.Info($"Setup replay stopped: can't place '{text}'");
                break;
            }
            _deployment = _deployment.Place(piece, square);
        }
        ShowSetup();
    }
}
