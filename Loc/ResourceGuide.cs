namespace Loc;

public static class ResourceGuide
{
    public sealed record Entry(ResourceType Type, string Site, string Description);

    public static IReadOnlyList<Entry> All { get; } =
    [
        new(ResourceType.Gold, "Gold mine",
            "Produces wealth each year. Stored in your stockpile. " +
            "Beginner: 2 gold builds a weapon, 4 gold builds a city."),
        new(ResourceType.Horses, "Pasture",
            "Lives on the map, not in the stockpile. Adds force points and " +
            "spreads to adjacent territories over time."),
        new(ResourceType.Iron, "Iron mine",
            "Intermediate and higher. Combined with coal to build weapons."),
        new(ResourceType.Coal, "Coal mine",
            "Intermediate and higher. Combined with iron to build weapons."),
        new(ResourceType.Timber, "Timber",
            "Intermediate and higher. Used with gold, iron, and coal to build cities, " +
            "or alone to build boats."),
    ];
}
