using System.Text.Json;

namespace AnarchyChess.Core;

/// <summary>Every kind of piece, loaded from pieces.json. Each [a, b] pattern expands to all its mirror images.</summary>
public sealed class PieceCatalog
{
    private readonly Dictionary<string, PieceDef> _byId;
    private readonly Dictionary<char, PieceDef> _byLetter;

    private PieceCatalog(List<PieceDef> pieces, string promotionId)
    {
        _byId = pieces.ToDictionary(p => p.Id);
        _byLetter = new Dictionary<char, PieceDef>();
        foreach (var piece in pieces)
            if (!_byLetter.TryAdd(piece.Letter, piece))
                throw new InvalidDataException($"pieces.json: letter '{piece.Letter}' is used by more than one piece.");
        Promotion = _byId.GetValueOrDefault(promotionId)
            ?? throw new InvalidDataException($"pieces.json: promotion piece '{promotionId}' doesn't exist.");
    }

    /// <summary>What a pawn becomes on the far rank.</summary>
    public PieceDef Promotion { get; }

    public PieceDef this[string id] => _byId[id];

    public PieceDef? ByLetter(char letter) => _byLetter.GetValueOrDefault(char.ToUpperInvariant(letter));

    public static PieceCatalog FromJson(string json)
    {
        var file = JsonSerializer.Deserialize<FileDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("pieces.json is empty.");
        var pieces = file.Pieces.Select(p => new PieceDef(
            p.Id, char.ToUpperInvariant(p.Letter[0]), p.Value, p.Royal, p.Pawn,
            (p.Moves ?? []).SelectMany(m => Expand(p.Id, m)).Distinct().ToList())).ToList();
        return new PieceCatalog(pieces, file.Promotion);
    }

    private static IEnumerable<MovePattern> Expand(string id, MoveDto move)
    {
        if ((move.Leap == null) == (move.Slide == null))
            throw new InvalidDataException($"pieces.json: each move of '{id}' needs exactly one of \"leap\" or \"slide\".");
        var v = move.Leap ?? move.Slide!;
        if (v.Length != 2)
            throw new InvalidDataException($"pieces.json: moves of '{id}' need two numbers, like [1, 2].");
        var slides = move.Slide != null;
        foreach (var (a, b) in new[] { (v[0], v[1]), (v[1], v[0]) })
        foreach (var sa in new[] { 1, -1 })
        foreach (var sb in new[] { 1, -1 })
            yield return new MovePattern(a * sa, b * sb, slides);
    }

    private sealed record FileDto(string Promotion, List<PieceDto> Pieces);

    private sealed record PieceDto(string Id, string Letter, int Value, bool Royal, bool Pawn, List<MoveDto>? Moves);

    private sealed record MoveDto(int[]? Leap, int[]? Slide);
}
