namespace Loc;

public sealed class WorldMap
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required bool[,] IsWater { get; init; }
    public required int[,] TerritoryGrid { get; init; }
    public required List<Territory> Territories { get; init; }

    public Territory? GetTerritory(int id) => Territories.FirstOrDefault(t => t.Id == id);

    public Territory? TerritoryAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        if (IsWater[x, y]) return null;
        int id = TerritoryGrid[x, y];
        return GetTerritory(id);
    }

    public IEnumerable<Territory> OwnedBy(int playerId) =>
        Territories.Where(t => t.OwnerId == playerId);

    public IEnumerable<Territory> Unowned() =>
        Territories.Where(t => t.OwnerId < 0);

    public IEnumerable<Territory> NeighborsOf(Territory territory) =>
        territory.NeighborIds.Select(id => GetTerritory(id)!).Where(t => t != null);

    public bool AreAdjacent(int territoryA, int territoryB)
    {
        var a = GetTerritory(territoryA);
        return a != null && a.NeighborIds.Contains(territoryB);
    }
}
