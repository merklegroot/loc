namespace Loc;

public static class ForceCalculator
{
    public static int DefensiveForce(WorldMap map, Territory home, int defenderId, bool countBoats = true)
    {
        int total = 1;
        foreach (int neighborId in home.NeighborIds)
        {
            var neighbor = map.GetTerritory(neighborId)!;
            if (neighbor.OwnerId == defenderId) total += 1;
        }

        if (home.HasHorse) total += 1;
        if (home.HasCity) total += 2;
        if (home.HasWeapon) total += 3;
        if (countBoats) total += home.BoatCount * 2;

        foreach (int neighborId in home.NeighborIds)
        {
            var neighbor = map.GetTerritory(neighborId)!;
            if (neighbor.OwnerId != defenderId) continue;
            if (neighbor.HasHorse) total += 1;
            if (neighbor.HasCity) total += 2;
            if (neighbor.HasWeapon) total += 3;
        }

        return total;
    }

    public static int OffensiveForce(WorldMap map, Territory target, int attackerId, bool includeBoats = false)
    {
        int total = 0;
        foreach (int neighborId in target.NeighborIds)
        {
            var neighbor = map.GetTerritory(neighborId)!;
            if (neighbor.OwnerId != attackerId) continue;
            total += 1;
            if (neighbor.HasHorse) total += 1;
            if (neighbor.HasCity) total += 2;
            if (neighbor.HasWeapon) total += 3;
            if (includeBoats) total += neighbor.BoatCount * 2;
        }
        return total;
    }

    public static bool AttackerWins(int attackForce, int defendForce, ChanceLevel chance, Random rng)
    {
        if (attackForce > defendForce) return true;
        if (attackForce < defendForce) return false;

        return chance switch
        {
            ChanceLevel.Low => true,
            ChanceLevel.Medium => rng.Next(2) == 0,
            ChanceLevel.High => rng.Next(2) == 0,
            _ => true
        };
    }

    public static bool AttackerWinsHighChance(int attackForce, int defendForce, Random rng)
    {
        if (attackForce >= defendForce) return true;
        if (attackForce <= 0) return false;
        double odds = (double)attackForce / (attackForce + defendForce);
        return rng.NextDouble() < odds;
    }
}
