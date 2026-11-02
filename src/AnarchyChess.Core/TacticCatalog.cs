using System.Text.Json;

namespace AnarchyChess.Core;

/// <summary>A scoring tactic, like a poker hand in Balatro: a base Value and a Mult (tactics.json).</summary>
public sealed record TacticDef(string Id, string Name, string Callout, int Value, int Mult);

/// <summary>The tactics from tactics.json. Scoring looks them up by id, so every id below must be present.</summary>
public sealed class TacticCatalog
{
    private static readonly string[] RequiredIds = ["capture", "check", "fork", "pin", "promotion", "enPassant"];

    private readonly Dictionary<string, TacticDef> _byId;

    private TacticCatalog(Dictionary<string, TacticDef> byId) => _byId = byId;

    public TacticDef this[string id] => _byId[id];

    public static TacticCatalog FromJson(string json)
    {
        var file = JsonSerializer.Deserialize<FileDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("tactics.json is empty.");
        var byId = file.Tactics.ToDictionary(t => t.Id);
        var missing = RequiredIds.Where(id => !byId.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new InvalidDataException($"tactics.json: missing tactic(s): {string.Join(", ", missing)}.");
        return new TacticCatalog(byId);
    }

    private sealed record FileDto(List<TacticDef> Tactics);
}
