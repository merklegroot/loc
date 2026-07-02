namespace Loc;

public static class DevelopmentCosts
{
    public static IReadOnlyDictionary<ResourceType, int>? WeaponCost(GameLevel level) => level switch
    {
        GameLevel.Beginner => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 2 },
        _ => new Dictionary<ResourceType, int>
        {
            [ResourceType.Iron] = 1,
            [ResourceType.Coal] = 1
        }
    };

    public static IReadOnlyDictionary<ResourceType, int>? CityCost(GameLevel level) => level switch
    {
        GameLevel.Beginner => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 4 },
        _ => new Dictionary<ResourceType, int>
        {
            [ResourceType.Gold] = 1,
            [ResourceType.Iron] = 1,
            [ResourceType.Coal] = 1,
            [ResourceType.Timber] = 1
        }
    };

    public static IReadOnlyDictionary<ResourceType, int>? BoatCostTimber() =>
        new Dictionary<ResourceType, int> { [ResourceType.Timber] = 3 };

    public static IReadOnlyDictionary<ResourceType, int>? BoatCostGold() =>
        new Dictionary<ResourceType, int> { [ResourceType.Gold] = 3 };
}
