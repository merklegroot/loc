using Raylib_cs;

namespace Loc;

public sealed class PlayerState
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required Color Color { get; init; }
    public required bool IsHuman { get; init; }
    public AiPersonality Personality { get; init; } = AiPersonality.Defensive;

    public ResourceStockpile Stockpile { get; } = new();
    public int? StockpileTerritoryId { get; set; }
}
