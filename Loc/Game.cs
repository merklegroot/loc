using System.Numerics;
using Raylib_cs;

namespace Loc;

public sealed class Game
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const string Title = "Lords of Conquest";

    private readonly GameRenderer _renderer = new();
    private readonly TitleScreenAssets _titleScreen = new();
    private GameConfig _menuConfig = new();
    private GameSession? _session;
    private bool _inMenu = true;
    private bool _inSetup;
    private bool _viewingResources;
    private bool _shouldQuit;
    private float _aiTimer;
    private int _menuSelection;
    private bool _autoTerritorySelection;

    public void Run()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, Title);
        Raylib.SetExitKey(KeyboardKey.KEY_NULL);
        Raylib.SetTargetFPS(60);
        UiText.Load();
        ResourceIcons.Load();
        _titleScreen.Load();
        _renderer.SetTitleScreen(_titleScreen);

        try
        {
            while (!_shouldQuit && !Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Update(dt);
                Draw();
            }
        }
        finally
        {
            UiText.Unload();
            ResourceIcons.Unload();
            _titleScreen.Dispose();
            if (Raylib.IsWindowReady())
            {
                Raylib.CloseWindow();
            }
        }
    }

    private void Update(float dt)
    {
        if (_inMenu)
        {
            if (_inSetup)
                UpdateSetup();
            else
                UpdateMenu();
            return;
        }

        if (_session == null) return;

        UpdateHover();
        UpdateInput();

        if (_session == null || _inMenu) return;

        if (_session.Phase == GamePhase.TerritorySelection && _autoTerritorySelection)
        {
            _aiTimer -= dt;
            if (_aiTimer <= 0)
            {
                TerritoryPicker.Pick(_session);
                _aiTimer = 0.35f;
                if (_session.Phase != GamePhase.TerritorySelection)
                    _autoTerritorySelection = false;
            }
        }
        else if (!_session.IsHumanTurn())
        {
            _aiTimer -= dt;
            if (_aiTimer <= 0)
            {
                ComputerPlayer.TakeTurn(_session);
                _aiTimer = 0.35f;
            }
        }
    }

    private void UpdateSetup()
    {
        var rows = SetupScreen.Rows(_menuConfig, ScreenWidth, ScreenHeight);
        var (back, start) = SetupScreen.ActionButtons(ScreenWidth, ScreenHeight);

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
            _menuSelection = (_menuSelection - 1 + rows.Count) % rows.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
            _menuSelection = (_menuSelection + 1) % rows.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER) || Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
            _menuConfig = SetupScreen.CycleRow(_menuConfig, rows[_menuSelection].Label);

        if (!Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) return;

        var mouse = Raylib.GetMousePosition();
        if (Raylib.CheckCollisionPointRec(mouse, back))
        {
            _inSetup = false;
            _menuSelection = 0;
            return;
        }

        if (Raylib.CheckCollisionPointRec(mouse, start))
        {
            _session = new GameSession(_menuConfig);
            _session.StartNewGame();
            _inMenu = false;
            _inSetup = false;
            _autoTerritorySelection = false;
            _aiTimer = 0.5f;
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            if (!Raylib.CheckCollisionPointRec(mouse, rows[i].Rect)) continue;
            _menuSelection = i;
            _menuConfig = SetupScreen.CycleRow(_menuConfig, rows[i].Label);
            return;
        }
    }

    private void UpdateMenu()
    {
        var buttons = GetMenuButtons();
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
            _menuSelection = (_menuSelection - 1 + buttons.Count) % buttons.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
            _menuSelection = (_menuSelection + 1) % buttons.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            HandleMenuClick(buttons[_menuSelection].Label);

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && _viewingResources)
        {
            _viewingResources = false;
            _menuSelection = 0;
            return;
        }

        if (!Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) return;
        var mouse = Raylib.GetMousePosition();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (!Raylib.CheckCollisionPointRec(mouse, buttons[i].Rect)) continue;
            _menuSelection = i;
            HandleMenuClick(buttons[i].Label);
            return;
        }

        if (_viewingResources &&
            !Raylib.CheckCollisionPointRec(mouse, ResourcesPanel.PanelRect(ScreenWidth, ScreenHeight)))
        {
            _viewingResources = false;
            _menuSelection = 0;
        }
    }

    private void HandleMenuClick(string label)
    {
        switch (label)
        {
            case "NEW GAME":
                _inSetup = true;
                _menuSelection = 0;
                break;
            case "RESOURCES":
                _viewingResources = true;
                _menuSelection = 0;
                break;
            case "BACK":
                _viewingResources = false;
                _menuSelection = 0;
                break;
            case "EXIT":
                _shouldQuit = true;
                break;
            case "MAIN MENU":
                _inMenu = true;
                _viewingResources = false;
                _session = null;
                _menuSelection = 0;
                break;
        }
    }

    private void UpdateHover()
    {
        if (_session == null) return;
        var mouse = Raylib.GetMousePosition();
        _session.HoveredTerritoryId = _renderer.TerritoryAt(_session, mouse);
    }

    private void UpdateInput()
    {
        if (_session == null || !_session.IsHumanTurn()) return;

        if (_session.Phase == GamePhase.TerritorySelection && _autoTerritorySelection)
            return;

        if (_session.Phase == GamePhase.Conquest)
        {
            UpdateConquestInput();
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
        {
            HandlePhaseAction("END TURN");
        }

        if (!Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) return;

        foreach (var (rect, label) in GetPhaseButtons())
        {
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect))
            {
                HandlePhaseAction(label);
                return;
            }
        }

        if (_session.HoveredTerritoryId is int tid)
        {
            _session.HandleTerritoryClick(tid);
        }
    }

    private void UpdateConquestInput()
    {
        if (_session == null) return;

        var menu = _renderer.GetConquestMenuRects(_session);
        int count = menu.Count;

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
            _session.ConquestMenuIndex = (_session.ConquestMenuIndex - 1 + count) % count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
            _session.ConquestMenuIndex = (_session.ConquestMenuIndex + 1) % count;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            _session.HandleConquestAction(menu[_session.ConquestMenuIndex].Label);

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mouse = Raylib.GetMousePosition();
            for (int i = 0; i < menu.Count; i++)
            {
                if (!Raylib.CheckCollisionPointRec(mouse, menu[i].Rect)) continue;
                _session.ConquestMenuIndex = i;
                _session.HandleConquestAction(menu[i].Label);
                return;
            }

            if (_session.HoveredTerritoryId is int tid && _session.PendingAttackTarget == null)
            {
                _session.HandleTerritoryClick(tid);
            }
        }
    }

    private void HandlePhaseAction(string label)
    {
        if (_session == null) return;

        switch (label.ToUpperInvariant())
        {
            case "WEAPON":
                _session.SetPendingDevelopment("weapon");
                break;
            case "CITY":
                _session.SetPendingDevelopment("city");
                break;
            case "BOAT":
                _session.SetPendingDevelopment("boat");
                break;
            case "GIFT GOLD":
                _session.GiftTradeResource();
                break;
            case "END TURN":
            case "END PHASE":
                _session.EndPlayerTurn();
                break;
            case "AUTO TERRITORIES":
                if (_session.Phase == GamePhase.TerritorySelection)
                {
                    _autoTerritorySelection = true;
                    _aiTimer = 0;
                }
                break;
            case "MAIN MENU":
                _inMenu = true;
                _autoTerritorySelection = false;
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
            if (_inSetup)
            {
                _renderer.DrawMainMenuBackground();
                SetupScreen.Draw(_menuConfig, ScreenWidth, ScreenHeight);
                HighlightSetupRow();
            }
            else if (_viewingResources)
            {
                _renderer.DrawResourcesScreen();
                _renderer.DrawButtons(GetMenuButtons(), _menuSelection);
            }
            else
            {
                DrawMenuButtonBackdrop();
                _renderer.DrawButtons(GetMenuButtons(), _menuSelection);
            }
        }
        else if (_session != null)
        {
            if (_session.Phase != GamePhase.Conquest)
            {
                _renderer.DrawButtons(GetPhaseButtons());
            }

            if (_session.Phase == GamePhase.GameOver)
            {
                DrawGameOver();
            }
        }

        Raylib.EndDrawing();
    }

    private void HighlightSetupRow()
    {
        var rows = SetupScreen.Rows(_menuConfig, ScreenWidth, ScreenHeight);
        if (_menuSelection < 0 || _menuSelection >= rows.Count) return;
        Raylib.DrawRectangleLinesEx(rows[_menuSelection].Rect, 2, ClassicPalette.Highlight);
    }

    private void DrawGameOver()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, w, h, new Color(0, 0, 0, 180));
        string msg = $"{_session?.WinnerName?.ToUpperInvariant()} WINS!";
        Vector2 size = UiText.MeasureTextSize(msg, 40);
        UiText.DrawText(msg, (int)((w - size.X) / 2), h / 2 - 40, 40, ClassicPalette.PlayerPink);
        UiText.DrawText("MAIN MENU", (int)((w - 120) / 2), h / 2 + 20, 18, ClassicPalette.Text);
    }

    private void DrawMenuButtonBackdrop()
    {
        var buttons = GetMenuButtons();
        if (buttons.Count == 0) return;

        float minX = buttons.Min(b => b.Rect.X) - 16;
        float maxX = buttons.Max(b => b.Rect.X + b.Rect.Width) + 16;
        float minY = buttons.Min(b => b.Rect.Y) - 12;
        float maxY = buttons.Max(b => b.Rect.Y + b.Rect.Height) + 12;
        Raylib.DrawRectangleRec(new Rectangle(minX, minY, maxX - minX, maxY - minY), new Color(0, 0, 0, 150));
        Raylib.DrawRectangleLinesEx(new Rectangle(minX, minY, maxX - minX, maxY - minY), 1, new Color(255, 255, 255, 80));
    }

    private List<(Rectangle Rect, string Label)> GetMenuButtons()
    {
        int x = ScreenWidth / 2 - 100;
        int w = 200;
        int h = 34;
        int gap = 12;

        if (_viewingResources)
        {
            return [(ResourcesPanel.BackButtonRect(ScreenWidth, ScreenHeight), "BACK")];
        }

        int y = 220;
        return
        [
            (new Rectangle(x, y, w, h), "NEW GAME"),
            (new Rectangle(x, y + h + gap, w, h), "RESOURCES"),
            (new Rectangle(x, y + 2 * (h + gap), w, h), "EXIT"),
        ];
    }

    private List<(Rectangle Rect, string Label)> GetPhaseButtons()
    {
        if (_session == null) return [];

        int y = ScreenHeight - 130;
        int x = ScreenWidth / 2;
        int w = 180;
        int h = 28;
        int gap = 6;
        var buttons = new List<(Rectangle, string)>();

        switch (_session.Phase)
        {
            case GamePhase.TerritorySelection:
                buttons.Add((new Rectangle(x, y, w, h), "AUTO TERRITORIES"));
                break;
            case GamePhase.Development:
                if (_session.CanDevelopWeapon())
                    buttons.Add((new Rectangle(x, y, w, h), "WEAPON"));
                y += h + gap;
                if (_session.CanDevelopCity())
                    buttons.Add((new Rectangle(x, y, w, h), "CITY"));
                y += h + gap;
                if (_session.CanDevelopBoat())
                    buttons.Add((new Rectangle(x, y, w, h), "BOAT"));
                y += h + gap;
                buttons.Add((new Rectangle(x, y, w, h), "END TURN"));
                break;
            case GamePhase.Shipment:
                buttons.Add((new Rectangle(x, y, w, h), "END TURN"));
                break;
            case GamePhase.Trading:
                if (_session.CanGiftTrade())
                    buttons.Add((new Rectangle(x, y, w, h), "GIFT GOLD"));
                y += h + gap;
                buttons.Add((new Rectangle(x, y, w, h), "END TURN"));
                break;
        }

        buttons.Add((new Rectangle(x, ScreenHeight - 44, w, h), "MAIN MENU"));
        return buttons;
    }
}
