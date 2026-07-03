namespace Loc;

public sealed record GameConfig
{
    public int HumanPlayerCount { get; init; } = 1;
    public int TotalPlayers { get; init; } = 2;
    public GameLevel Level { get; init; } = GameLevel.Beginner;
    public ChanceLevel Chance { get; init; } = ChanceLevel.Low;
    public ResourceAbundance Abundance { get; init; } = ResourceAbundance.Medium;
    public int CitiesToWin { get; init; } = 3;
    public int AiDifficulty { get; init; } = 2;
    public AiPersonality AiPersonality { get; init; } = AiPersonality.Defensive;
    public int TerritoryCount { get; init; } = 16;
    public int Seed { get; init; } = 42;
    public double WaterRatio { get; init; } = 0.22;

    public bool HasBoats => Level is GameLevel.Advanced or GameLevel.Expert;
    public bool ExpertShipment => Level == GameLevel.Expert;
    public bool TradingEnabled => TotalPlayers >= 3;

    public IReadOnlyList<ResourceType> AvailableResources => Level switch
    {
        GameLevel.Beginner => [ResourceType.Gold, ResourceType.Horses],
        _ => [ResourceType.Gold, ResourceType.Horses, ResourceType.Iron, ResourceType.Coal, ResourceType.Timber]
    };
}
