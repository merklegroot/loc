using System.Numerics;
using Raylib_cs;

namespace Loc;

public sealed class Game
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const string Title = "Lords of Conquest";

    private readonly GameRenderer _renderer = new();
    private GameConfig _menuConfig = new();
    private GameSession? _session;
    private bool _inMenu = true;
    private float _aiTimer;

    public void Run()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, Title);
        Raylib.SetTargetFPS(60);
        UiText.Load();

        try
        {
            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Update(dt);
                Draw();
            }
        }
        finally
        {
            UiText.Unload();
            Raylib.CloseWindow();
        }
    }

    private void Update(float dt)
    {
        if (_inMenu)
        {
            UpdateMenu();
            return;
        }

        if (_session == null) return;

        UpdateHover();
        UpdateButtons();

        if (!_session.IsHumanTurn())
        {
            _aiTimer -= dt;
            if (_aiTimer <= 0)
            {
                ComputerPlayer.TakeTurn(_session);
                _aiTimer = 0.35f;
            }
        }
    }

    private void UpdateMenu()
    {
        var buttons = GetMenuButtons();
        if (!Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) return;

        var mouse = Raylib.GetMousePosition();
        foreach (var (rect, label) in buttons)
        {
            if (!Raylib.CheckCollisionPointRec(mouse, rect)) continue;
            HandleMenuClick(label);
            return;
        }
    }

    private void HandleMenuClick(string label)
    {
        switch (label)
        {
            case "New Game":
                _session = new GameSession(_menuConfig);
                _session.StartNewGame();
                _inMenu = false;
                _aiTimer = 0.5f;
                break;
            case "Beginner":
                _menuConfig = _menuConfig with { Level = GameLevel.Beginner };
                break;
            case "Intermediate":
                _menuConfig = _menuConfig with { Level = GameLevel.Intermediate };
                break;
            case "Low Chance":
                _menuConfig = _menuConfig with { Chance = ChanceLevel.Low };
                break;
            case "Med Chance":
                _menuConfig = _menuConfig with { Chance = ChanceLevel.Medium };
                break;
            case "1 Human":
                _menuConfig = _menuConfig with { HumanPlayerCount = 1, TotalPlayers = 2 };
                break;
            case "2 Human":
                _menuConfig = _menuConfig with { HumanPlayerCount = 2, TotalPlayers = 2 };
                break;
        }
    }

    private void UpdateHover()
    {
        if (_session == null) return;
        var mouse = Raylib.GetMousePosition();
        _session.HoveredTerritoryId = _renderer.TerritoryAt(_session, mouse);
    }

    private void UpdateButtons()
    {
        if (_session == null || !_session.IsHumanTurn()) return;

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mouse = Raylib.GetMousePosition();
            foreach (var (rect, label) in GetPhaseButtons())
            {
                if (Raylib.CheckCollisionPointRec(mouse, rect))
                {
                    HandlePhaseButton(label);
                    return;
                }
            }

            if (_session.HoveredTerritoryId is int tid)
            {
                _session.HandleTerritoryClick(tid);
            }
        }
    }

    private void HandlePhaseButton(string label)
    {
        if (_session == null) return;

        switch (label)
        {
            case "Weapon":
                _session.SetPendingDevelopment("weapon");
                break;
            case "City":
                _session.SetPendingDevelopment("city");
                break;
            case "End Turn":
            case "End Phase":
                _session.EndPlayerTurn();
                break;
            case "Main Menu":
                _inMenu = true;
                _session = null;
                break;
        }
    }

    private void Draw()
    {
        Raylib.BeginDrawing();
        _renderer.Draw(_session, _menuConfig, _inMenu);

        if (_inMenu)
        {
            _renderer.DrawButtons(GetMenuButtons());
        }
        else if (_session != null)
        {
            _renderer.DrawButtons(GetPhaseButtons());
            if (_session.Phase == GamePhase.GameOver)
            {
                DrawGameOver();
            }
        }

        Raylib.EndDrawing();
    }

    private void DrawGameOver()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, w, h, new Color(0, 0, 0, 160));
        string msg = $"{_session?.WinnerName} wins!";
        Vector2 size = UiText.MeasureTextSize(msg, 40);
        UiText.DrawText(msg, (int)((w - size.X) / 2), h / 2 - 40, 40, Color.GOLD);
        UiText.DrawText("Click Main Menu", (int)((w - 160) / 2), h / 2 + 20, 18, Color.RAYWHITE);
    }

    private List<(Rectangle Rect, string Label)> GetMenuButtons()
    {
        int x = ScreenWidth / 2 - 100;
        int y = 260;
        int w = 200;
        int h = 34;
        int gap = 8;
        return
        [
            (new Rectangle(x, y, w, h), "New Game"),
            (new Rectangle(x - 110, y + 50, 95, h), "Beginner"),
            (new Rectangle(x + 15, y + 50, 95, h), "Intermediate"),
            (new Rectangle(x - 110, y + 50 + h + gap, 95, h), "Low Chance"),
            (new Rectangle(x + 15, y + 50 + h + gap, 95, h), "Med Chance"),
            (new Rectangle(x - 110, y + 50 + 2 * (h + gap), 95, h), "1 Human"),
            (new Rectangle(x + 15, y + 50 + 2 * (h + gap), 95, h), "2 Human"),
        ];
    }

    private List<(Rectangle Rect, string Label)> GetPhaseButtons()
    {
        if (_session == null) return [];

        int sidebar = ScreenWidth - 280;
        int x = sidebar + 12;
        int y = 320;
        int w = 256;
        int h = 30;
        int gap = 6;
        var buttons = new List<(Rectangle, string)>();

        switch (_session.Phase)
        {
            case GamePhase.Development:
                if (_session.CanDevelopWeapon())
                    buttons.Add((new Rectangle(x, y, w, h), "Weapon"));
                y += h + gap;
                if (_session.CanDevelopCity())
                    buttons.Add((new Rectangle(x, y, w, h), "City"));
                y += h + gap;
                buttons.Add((new Rectangle(x, y, w, h), "End Turn"));
                break;
            case GamePhase.Production:
                break;
            case GamePhase.Shipment:
                buttons.Add((new Rectangle(x, y, w, h), "End Turn"));
                break;
            case GamePhase.Conquest:
                buttons.Add((new Rectangle(x, y, w, h), "End Turn"));
                break;
            case GamePhase.Trading:
                buttons.Add((new Rectangle(x, y, w, h), "End Turn"));
                break;
        }

        buttons.Add((new Rectangle(x, ScreenHeight - 44, w, h), "Main Menu"));
        return buttons;
    }
}
