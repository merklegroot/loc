namespace Loc;

public static class TerritoryPicker
{
    public static void Pick(GameSession session)
    {
        var candidates = session.Map.Unowned().ToList();
        if (candidates.Count == 0) return;

        var owned = session.Map.OwnedBy(session.CurrentPlayer.Id).ToList();
        Territory? pick = candidates
            .Where(t => t.Resource != null)
            .OrderByDescending(t => t.Resource == ResourceType.Gold ? 3 : 2)
            .FirstOrDefault();

        if (pick == null && owned.Count > 0)
        {
            var ownedIds = owned.Select(t => t.Id).ToHashSet();
            pick = candidates
                .Where(t => t.NeighborIds.Any(n => ownedIds.Contains(n)))
                .OrderByDescending(t => t.NeighborIds.Count(n => ownedIds.Contains(n)))
                .FirstOrDefault();
        }

        pick ??= candidates[0];
        session.SelectTerritory(pick.Id);
    }
}
