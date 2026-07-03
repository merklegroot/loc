namespace Loc;

public static class ComputerPlayer
{
    public static void TakeTurn(GameSession session)
    {
        var player = session.CurrentPlayer;
        if (player.IsHuman) return;

        switch (session.Phase)
        {
            case GamePhase.TerritorySelection:
                TerritoryPicker.Pick(session);
                break;
            case GamePhase.Development:
                DoDevelopment(session);
                session.EndPlayerTurn();
                break;
            case GamePhase.Production:
                if (player.StockpileTerritoryId == null)
                {
                    PlaceStockpile(session);
                }
                break;
            case GamePhase.Shipment when !session.SkipShipment:
                MaybeMoveStockpile(session);
                session.EndPlayerTurn();
                break;
            case GamePhase.Conquest:
                DoConquest(session);
                session.EndPlayerTurn();
                break;
            default:
                session.EndPlayerTurn();
                break;
        }
    }

    private static void PlaceStockpile(GameSession session)
    {
        var owned = session.Map.OwnedBy(session.CurrentPlayer.Id).ToList();
        var best = owned
            .OrderByDescending(t => ForceCalculator.DefensiveForce(session.Map, t, session.CurrentPlayer.Id))
            .First();
        session.TryPlaceStockpile(best.Id);
    }

    private static void DoDevelopment(GameSession session)
    {
        var player = session.CurrentPlayer;
        var owned = session.Map.OwnedBy(player.Id).ToList();

        if (session.CanDevelopCity())
        {
            var site = owned.Where(t => !t.HasCity).OrderByDescending(t => t.NeighborIds.Count).FirstOrDefault();
            if (site != null)
            {
                session.BuyCity(site.Id);
                return;
            }
        }

        if (session.CanDevelopWeapon())
        {
            var site = owned.Where(t => !t.HasWeapon).OrderByDescending(t => t.NeighborIds.Count).FirstOrDefault();
            if (site != null)
            {
                session.BuyWeapon(site.Id);
            }
        }
    }

    private static void MaybeMoveStockpile(GameSession session)
    {
        var player = session.CurrentPlayer;
        if (player.StockpileTerritoryId is not int currentId) return;

        var owned = session.Map.OwnedBy(player.Id).ToList();
        var current = session.Map.GetTerritory(currentId)!;
        int currentForce = ForceCalculator.DefensiveForce(session.Map, current, player.Id);

        var safer = owned
            .Where(t => t.Id != currentId)
            .OrderByDescending(t => ForceCalculator.DefensiveForce(session.Map, t, player.Id))
            .FirstOrDefault();

        if (safer != null)
        {
            int saferForce = ForceCalculator.DefensiveForce(session.Map, safer, player.Id);
            if (saferForce > currentForce + 1)
            {
                session.MoveStockpile(safer.Id);
            }
        }
    }

    private static void DoConquest(GameSession session)
    {
        var player = session.CurrentPlayer;
        var targets = session.Map.Territories
            .Where(t => t.OwnerId >= 0 && t.OwnerId != player.Id)
            .Where(t => t.NeighborIds.Any(nid => session.Map.GetTerritory(nid)!.OwnerId == player.Id))
            .OrderByDescending(t =>
                ForceCalculator.OffensiveForce(session.Map, t, player.Id) -
                ForceCalculator.DefensiveForce(session.Map, t, t.OwnerId))
            .ToList();

        foreach (var target in targets)
        {
            int atk = ForceCalculator.OffensiveForce(session.Map, target, player.Id);
            int def = ForceCalculator.DefensiveForce(session.Map, target, target.OwnerId);
            bool canWin = session.Config.Chance switch
            {
                ChanceLevel.High => atk > 0,
                ChanceLevel.Low => atk >= def,
                _ => atk >= def
            };

            if (!canWin) continue;

            session.PlanAttack(target.Id);
            session.HandleConquestAction("ATTACK");
            if (session.Phase == GamePhase.GameOver) return;
            break;
        }
    }
}
