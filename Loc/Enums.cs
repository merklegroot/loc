namespace Loc;

public enum GameLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public enum ChanceLevel
{
    Low,
    Medium,
    High
}

public enum ResourceAbundance
{
    Low,
    Medium,
    High
}

public enum ResourceType
{
    Gold,
    Horses,
    Iron,
    Coal,
    Timber
}

public enum GamePhase
{
    MainMenu,
    TerritorySelection,
    Development,
    Production,
    Trading,
    Shipment,
    Conquest,
    GameOver
}

public enum UiMode
{
    Map,
    Menu,
    Confirm,
    Message
}
