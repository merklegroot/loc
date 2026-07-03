using Raylib_cs;

namespace Loc;

public sealed class GameSession
{
    private static readonly Color[] PlayerColors =
    [
        ClassicPalette.PlayerPink,
        ClassicPalette.PlayerCyan,
        ClassicPalette.PlayerGreen,
        ClassicPalette.PlayerYellow
    ];

    private readonly Random _rng;
    private readonly List<int> _turnOrder = [];
    private int _attacksRemaining;
    private int? _pendingAttackTarget;
    private int _attackBonus;
    private bool _waitingStockpilePlacement;
    private string? _winnerName;

    public GameConfig Config { get; }
    public WorldMap Map { get; }
    public IReadOnlyList<PlayerState> Players { get; }
    public int Year { get; private set; } = 1;
    public GamePhase Phase { get; private set; } = GamePhase.TerritorySelection;
    public int CurrentPlayerIndex { get; private set; }
    public PlayerState CurrentPlayer => Players[CurrentPlayerIndex];
    public string StatusMessage { get; private set; } = "Select territories.";
    public string? WinnerName => _winnerName;
    public int? HoveredTerritoryId { get; set; }
    public int? SelectedTerritoryId { get; private set; }
    public string? PendingDevelopment { get; private set; }
    public bool SkipProduction { get; private set; }
    public bool SkipShipment { get; private set; }
    public int AttacksRemaining => _attacksRemaining;
    public int? PendingAttackTarget => _pendingAttackTarget;
    public int ConquestMenuIndex { get; set; } = 1;

    public GameSession(GameConfig config)
    {
        Config = config;
        _rng = new Random(config.Seed);
        Map = MapGenerator.Generate(config);
        Players = CreatePlayers(config);
        RebuildTurnOrder();
    }

    public void StartNewGame()
    {
        Year = 1;
        Phase = GamePhase.TerritorySelection;
        CurrentPlayerIndex = 0;
        StatusMessage = $"{CurrentPlayer.Name}: pick a territory.";
        RebuildTurnOrder();
    }

    private List<PlayerState> CreatePlayers(GameConfig config)
    {
        var players = new List<PlayerState>();
        for (int i = 0; i < config.TotalPlayers; i++)
        {
            bool isHuman = i < config.HumanPlayerCount;
            players.Add(new PlayerState
            {
                Id = i,
                Name = isHuman ? $"Player {i + 1}" : $"Computer {i + 1}",
                Color = PlayerColors[i % PlayerColors.Length],
                IsHuman = isHuman
            });
        }
        return players;
    }

    private void RebuildTurnOrder()
    {
        _turnOrder.Clear();
        for (int i = 0; i < Players.Count; i++)
        {
            _turnOrder.Add(i);
        }
    }

    public void RotateTurnOrder()
    {
        if (_turnOrder.Count <= 1) return;
        int first = _turnOrder[0];
        _turnOrder.RemoveAt(0);
        _turnOrder.Add(first);
    }

    public int TurnOrderPosition(int playerId) => _turnOrder.IndexOf(playerId);

    public bool IsHumanTurn() => CurrentPlayer.IsHuman;

    public void SelectTerritory(int territoryId)
    {
        if (Phase != GamePhase.TerritorySelection) return;
        var territory = Map.GetTerritory(territoryId);
        if (territory == null || territory.OwnerId >= 0) return;

        territory.OwnerId = CurrentPlayer.Id;
        if (territory.Resource == ResourceType.Horses) territory.HasHorse = true;

        AdvanceTerritorySelection();
    }

    private void AdvanceTerritorySelection()
    {
        var unowned = Map.Unowned().ToList();
        int remainder = unowned.Count;
        if (remainder > 0)
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            StatusMessage = $"{CurrentPlayer.Name}: pick a territory.";
            return;
        }

        BeginYear();
    }

    private void BeginYear()
    {
        if (Year >= 2)
        {
            EnterPhase(GamePhase.Development);
            return;
        }

        EnterPhase(GamePhase.Production);
    }

    private void EnterPhase(GamePhase phase)
    {
        Phase = phase;
        CurrentPlayerIndex = _turnOrder[0];
        _attacksRemaining = 2;
        _pendingAttackTarget = null;
        _attackBonus = 0;
        SelectedTerritoryId = null;
        _waitingStockpilePlacement = false;

        switch (phase)
        {
            case GamePhase.Development:
                StatusMessage = $"{CurrentPlayer.Name}: development phase.";
                break;
            case GamePhase.Production:
                SkipProduction = Config.Chance != ChanceLevel.Low && _rng.Next(4) == 0;
                if (SkipProduction)
                {
                    StatusMessage = "Production skipped (chance).";
                    if (!Players.Any(p => p.StockpileTerritoryId == null))
                    {
                        EndPhase();
                        return;
                    }
                }
                else
                {
                    RunProduction();
                    _waitingStockpilePlacement = Players.Any(p => p.StockpileTerritoryId == null);
                    if (!_waitingStockpilePlacement)
                    {
                        StatusMessage = "Production complete.";
                        EndPhase();
                        return;
                    }
                    CurrentPlayerIndex = Players.ToList().FindIndex(p => p.StockpileTerritoryId == null);
                    if (CurrentPlayerIndex < 0) CurrentPlayerIndex = 0;
                    StatusMessage = $"{CurrentPlayer.Name}: place your stockpile.";
                }
                break;
            case GamePhase.Trading:
                if (!Config.TradingEnabled)
                {
                    EndPhase();
                    return;
                }
                StatusMessage = $"{CurrentPlayer.Name}: trading phase (press End Turn).";
                break;
            case GamePhase.Shipment:
                SkipShipment = Config.Chance != ChanceLevel.Low && _rng.Next(4) == 0;
                if (SkipShipment)
                {
                    StatusMessage = "Shipment skipped (chance).";
                    EndPhase();
                    return;
                }
                StatusMessage = $"{CurrentPlayer.Name}: shipment — click a territory to move stockpile, or End Turn.";
                break;
            case GamePhase.Conquest:
                StatusMessage = "Select a territory to attack.";
                ConquestMenuIndex = 1;
                break;
        }
    }

    private void RunProduction()
    {
        foreach (var player in Players)
        {
            ProduceForPlayer(player);
        }
        SpreadHorses();
    }

    private void ProduceForPlayer(PlayerState player)
    {
        foreach (var territory in Map.OwnedBy(player.Id))
        {
            if (territory.Resource is not ResourceType resource) continue;
            if (resource == ResourceType.Horses) continue;

            int amount = ProductionAmount(territory, resource);
            player.Stockpile.Add(resource, amount);
        }
    }

    private int ProductionAmount(Territory territory, ResourceType resource)
    {
        int amount = Config.Abundance switch
        {
            ResourceAbundance.Low => 1,
            ResourceAbundance.High => 2,
            _ => 1
        };

        if (HasCityBonus(territory)) amount *= 2;
        return amount;
    }

    private bool HasCityBonus(Territory territory)
    {
        if (territory.HasCity) return true;
        foreach (int nid in territory.NeighborIds)
        {
            if (Map.GetTerritory(nid)!.HasCity) return true;
        }
        return false;
    }

    private void SpreadHorses()
    {
        foreach (var territory in Map.Territories)
        {
            if (territory.Resource != ResourceType.Horses || territory.OwnerId < 0) continue;
            if (!territory.HasHorse && Config.Chance == ChanceLevel.Low)
            {
                territory.HasHorse = true;
                continue;
            }

            if (territory.HasHorse) continue;
            if (Config.Chance != ChanceLevel.Low && _rng.Next(3) != 0) continue;

            bool adjacentHorse = territory.NeighborIds
                .Select(id => Map.GetTerritory(id)!)
                .Any(n => n.OwnerId == territory.OwnerId && n.HasHorse);
            if (adjacentHorse) territory.HasHorse = true;
        }
    }

    public void TryPlaceStockpile(int territoryId)
    {
        if (Phase != GamePhase.Production || !_waitingStockpilePlacement) return;
        var territory = Map.GetTerritory(territoryId);
        if (territory == null || territory.OwnerId != CurrentPlayer.Id) return;
        if (CurrentPlayer.StockpileTerritoryId != null) return;

        foreach (var t in Map.Territories.Where(t => t.IsStockpile))
        {
            if (Map.GetTerritory(t.Id)!.OwnerId == CurrentPlayer.Id) return;
        }

        territory.IsStockpile = true;
        CurrentPlayer.StockpileTerritoryId = territory.Id;
        AdvanceAfterStockpilePlacement();
    }

    private void AdvanceAfterStockpilePlacement()
    {
        if (Players.All(p => p.StockpileTerritoryId != null))
        {
            _waitingStockpilePlacement = false;
            EndPhase();
            return;
        }

        int start = CurrentPlayerIndex;
        do
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
        } while (Players[CurrentPlayerIndex].StockpileTerritoryId != null && CurrentPlayerIndex != start);

        StatusMessage = $"{CurrentPlayer.Name}: place your stockpile.";
    }

    public void SetPendingDevelopment(string? kind)
    {
        PendingDevelopment = kind;
        StatusMessage = kind switch
        {
            "weapon" => "Click a territory to build a weapon.",
            "city" => "Click a territory to build a city.",
            _ => $"{CurrentPlayer.Name}: development phase."
        };
    }

    public void HandleDevelopmentClick(int territoryId)
    {
        if (PendingDevelopment == "weapon") BuyWeapon(territoryId);
        else if (PendingDevelopment == "city") BuyCity(territoryId);
        PendingDevelopment = null;
    }

    public bool CanDevelopWeapon() =>
        DevelopmentCosts.WeaponCost(Config.Level) is { } cost && CurrentPlayer.Stockpile.CanSpend(cost);

    public bool CanDevelopCity() =>
        DevelopmentCosts.CityCost(Config.Level) is { } cost && CurrentPlayer.Stockpile.CanSpend(cost);

    public void BuyWeapon(int territoryId)
    {
        if (Phase != GamePhase.Development) return;
        var territory = ValidateDevelopmentTarget(territoryId);
        if (territory == null || territory.HasWeapon) return;
        var cost = DevelopmentCosts.WeaponCost(Config.Level)!;
        if (!CurrentPlayer.Stockpile.Spend(cost)) return;
        territory.HasWeapon = true;
        StatusMessage = $"{CurrentPlayer.Name} built a weapon in {territory.Name}.";
    }

    public void BuyCity(int territoryId)
    {
        if (Phase != GamePhase.Development) return;
        var territory = ValidateDevelopmentTarget(territoryId);
        if (territory == null || territory.HasCity) return;
        var cost = DevelopmentCosts.CityCost(Config.Level)!;
        if (!CurrentPlayer.Stockpile.Spend(cost)) return;
        territory.HasCity = true;
        StatusMessage = $"{CurrentPlayer.Name} built a city in {territory.Name}.";
        CheckVictory();
    }

    private Territory? ValidateDevelopmentTarget(int territoryId)
    {
        var territory = Map.GetTerritory(territoryId);
        if (territory == null || territory.OwnerId != CurrentPlayer.Id) return null;
        return territory;
    }

    public void MoveStockpile(int territoryId)
    {
        if (Phase != GamePhase.Shipment || SkipShipment) return;
        var territory = Map.GetTerritory(territoryId);
        if (territory == null || territory.OwnerId != CurrentPlayer.Id) return;
        if (CurrentPlayer.StockpileTerritoryId is not int oldId) return;

        var old = Map.GetTerritory(oldId)!;
        old.IsStockpile = false;
        territory.IsStockpile = true;
        CurrentPlayer.StockpileTerritoryId = territory.Id;
        StatusMessage = $"{CurrentPlayer.Name} moved stockpile to {territory.Name}.";
        EndPlayerTurn();
    }

    public void PlanAttack(int territoryId)
    {
        if (Phase != GamePhase.Conquest || _attacksRemaining <= 0) return;
        var target = Map.GetTerritory(territoryId);
        if (target == null || target.OwnerId == CurrentPlayer.Id || target.OwnerId < 0) return;

        bool adjacent = target.NeighborIds
            .Select(id => Map.GetTerritory(id)!)
            .Any(n => n.OwnerId == CurrentPlayer.Id);
        if (!adjacent) return;

        _pendingAttackTarget = territoryId;
        SelectedTerritoryId = territoryId;
        ConquestMenuIndex = 2;
        int atk = ForceCalculator.OffensiveForce(Map, target, CurrentPlayer.Id) + _attackBonus;
        int def = ForceCalculator.DefensiveForce(Map, target, target.OwnerId);
        StatusMessage = atk >= def ? "Attack will succeed." : atk < def ? "Attack may fail." : "Forces are equal.";
    }

    public void ReplanAttack()
    {
        _pendingAttackTarget = null;
        SelectedTerritoryId = null;
        ConquestMenuIndex = 1;
        StatusMessage = "Select a territory to attack.";
    }

    public void ExitConquestMenu()
    {
        ReplanAttack();
    }

    public bool IsAutoWinTarget(int territoryId)
    {
        if (PendingAttackTarget != territoryId) return false;
        if (PreviewCombat(territoryId) is not (int atk, int def)) return false;
        return atk >= def;
    }

    public void HandleConquestAction(string action)
    {
        switch (action.ToUpperInvariant())
        {
            case "ATTACK":
                ConfirmAttack();
                break;
            case "REPLAN":
                ReplanAttack();
                break;
            case "EXIT":
                if (_pendingAttackTarget != null) ExitConquestMenu();
                else EndPlayerTurn();
                break;
            case "BRING FORCES":
                StatusMessage = "Bring forces — coming soon.";
                break;
            case "END TURN":
                ReplanAttack();
                EndPlayerTurn();
                break;
            case "PLAN ATTACK":
                StatusMessage = "Select a territory to attack.";
                break;
        }
    }

    public void ConfirmAttack()
    {
        if (Phase != GamePhase.Conquest || _pendingAttackTarget is not int targetId) return;
        var target = Map.GetTerritory(targetId)!;
        int defenderId = target.OwnerId;
        int attackForce = ForceCalculator.OffensiveForce(Map, target, CurrentPlayer.Id) + _attackBonus;
        int defendForce = ForceCalculator.DefensiveForce(Map, target, defenderId);

        bool won = Config.Chance == ChanceLevel.High
            ? ForceCalculator.AttackerWinsHighChance(attackForce, defendForce, _rng)
            : ForceCalculator.AttackerWins(attackForce, defendForce, Config.Chance, _rng);

        _attacksRemaining--;
        _pendingAttackTarget = null;
        SelectedTerritoryId = null;
        _attackBonus = 0;

        if (won)
        {
            ConquerTerritory(target, CurrentPlayer.Id, defenderId);
            StatusMessage = $"{CurrentPlayer.Name} conquered {target.Name}!";
            CheckVictory();
            if (_attacksRemaining > 0)
            {
                StatusMessage += " Plan another attack or end phase.";
            }
            else
            {
                EndPlayerTurn();
            }
        }
        else
        {
            StatusMessage = Config.Chance == ChanceLevel.Low
                ? "Attack failed."
                : "Attack repulsed! Turn over.";
            if (Config.Chance != ChanceLevel.Low)
            {
                _attacksRemaining = 0;
                EndPlayerTurn();
            }
        }
    }

    private void ConquerTerritory(Territory target, int newOwnerId, int oldOwnerId)
    {
        if (target.IsStockpile)
        {
            var oldOwner = Players[oldOwnerId];
            var newOwner = Players[newOwnerId];
            newOwner.Stockpile.Absorb(oldOwner.Stockpile);
            oldOwner.StockpileTerritoryId = null;
            target.IsStockpile = true;
            newOwner.StockpileTerritoryId = target.Id;
        }

        if (target.HasWeapon && target.HasHorse)
        {
            target.HasHorse = false;
        }

        target.OwnerId = newOwnerId;
    }

    public void UseAttackToMoveStockpile(int territoryId)
    {
        if (Phase != GamePhase.Conquest || _attacksRemaining != 2) return;
        MoveStockpile(territoryId);
        _attacksRemaining = 0;
    }

    public void EndPlayerTurn()
    {
        if (Phase is GamePhase.Development or GamePhase.Shipment or GamePhase.Conquest or GamePhase.Trading)
        {
            int idx = _turnOrder.IndexOf(CurrentPlayer.Id);
            if (idx < _turnOrder.Count - 1)
            {
                CurrentPlayerIndex = _turnOrder[idx + 1];
                _attacksRemaining = 2;
                _pendingAttackTarget = null;
                StatusMessage = $"{CurrentPlayer.Name}'s turn.";
                return;
            }
        }

        EndPhase();
    }

    public void EndPhase()
    {
        switch (Phase)
        {
            case GamePhase.Development:
                EnterPhase(GamePhase.Production);
                if (SkipProduction && Year == 1) AdvancePastSkippedProduction();
                break;
            case GamePhase.Production:
                if (Config.TradingEnabled) EnterPhase(GamePhase.Trading);
                else EnterPhase(GamePhase.Shipment);
                break;
            case GamePhase.Trading:
                EnterPhase(GamePhase.Shipment);
                break;
            case GamePhase.Shipment:
                EnterPhase(GamePhase.Conquest);
                break;
            case GamePhase.Conquest:
                EndYear();
                break;
        }
    }

    private void AdvancePastSkippedProduction()
    {
        if (Config.TradingEnabled) EnterPhase(GamePhase.Trading);
        else EnterPhase(GamePhase.Shipment);
    }

    private void EndYear()
    {
        if (CheckVictory()) return;

        Year++;
        RotateTurnOrder();
        CurrentPlayerIndex = _turnOrder[0];
        EnterPhase(Year >= 2 ? GamePhase.Development : GamePhase.Production);
    }

    private bool CheckVictory()
    {
        var cityCounts = Players.Select(p => (Player: p, Cities: Map.OwnedBy(p.Id).Count(t => t.HasCity))).ToList();
        int needed = Config.CitiesToWin;
        var contenders = cityCounts.Where(c => c.Cities >= needed).ToList();

        if (contenders.Count == 1)
        {
            _winnerName = contenders[0].Player.Name;
            Phase = GamePhase.GameOver;
            StatusMessage = $"{_winnerName} wins!";
            return true;
        }

        if (contenders.Count > 1)
        {
            int max = contenders.Max(c => c.Cities);
            var leaders = contenders.Where(c => c.Cities == max).ToList();
            if (leaders.Count == 1)
            {
                _winnerName = leaders[0].Player.Name;
                Phase = GamePhase.GameOver;
                StatusMessage = $"{_winnerName} wins in overtime!";
                return true;
            }
        }

        return false;
    }

    public int CountCities(int playerId) => Map.OwnedBy(playerId).Count(t => t.HasCity);

    public (int Attack, int Defense)? PreviewCombat(int territoryId)
    {
        var target = Map.GetTerritory(territoryId);
        if (target == null || target.OwnerId < 0) return null;
        int atk = ForceCalculator.OffensiveForce(Map, target, CurrentPlayer.Id) + _attackBonus;
        int def = ForceCalculator.DefensiveForce(Map, target, target.OwnerId);
        return (atk, def);
    }

    public void HandleTerritoryClick(int territoryId)
    {
        switch (Phase)
        {
            case GamePhase.TerritorySelection:
                SelectTerritory(territoryId);
                break;
            case GamePhase.Production when _waitingStockpilePlacement:
                TryPlaceStockpile(territoryId);
                break;
            case GamePhase.Shipment when !SkipShipment:
                MoveStockpile(territoryId);
                break;
            case GamePhase.Conquest when _attacksRemaining > 0 && _pendingAttackTarget == null:
                PlanAttack(territoryId);
                break;
            case GamePhase.Development when PendingDevelopment != null:
                HandleDevelopmentClick(territoryId);
                break;
        }
    }
}
