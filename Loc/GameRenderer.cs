using System.Numerics;
using Raylib_cs;

namespace Loc;

public sealed class GameRenderer
{
    private const int SidebarWidth = 280;
    private const int TopBarHeight = 36;
    private const int CellSize = 14;

    public int MapOffsetX { get; private set; } = 8;
    public int MapOffsetY { get; private set; } = TopBarHeight + 8;

    public void Draw(GameSession? session, GameConfig? menuConfig, bool inMenu)
    {
        Raylib.ClearBackground(new Color(20, 24, 32, 255));

        if (inMenu || session == null)
        {
            DrawMainMenu(menuConfig ?? new GameConfig());
            return;
        }

        DrawTopBar(session);
        DrawMap(session);
        DrawSidebar(session);
    }

    private void DrawTopBar(GameSession session)
    {
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), TopBarHeight, new Color(35, 42, 58, 255));
        string title = $"Lords of Conquest  |  Year {session.Year}  |  {session.Phase}  |  {session.StatusMessage}";
        UiText.DrawText(title, 10, 10, 16, Color.RAYWHITE);
    }

    private void DrawMap(GameSession session)
    {
        var map = session.Map;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                int px = MapOffsetX + x * CellSize;
                int py = MapOffsetY + y * CellSize;
                if (map.IsWater[x, y])
                {
                    Raylib.DrawRectangle(px, py, CellSize, CellSize, new Color(30, 70, 120, 255));
                    continue;
                }

                int tid = map.TerritoryGrid[x, y];
                var territory = map.GetTerritory(tid)!;
                Color fill = territory.OwnerId >= 0
                    ? session.Players[territory.OwnerId].Color
                    : new Color(90, 90, 90, 255);

                if (session.HoveredTerritoryId == tid)
                {
                    fill = new Color(
                        (byte)Math.Min(255, fill.R + 40),
                        (byte)Math.Min(255, fill.G + 40),
                        (byte)Math.Min(255, fill.B + 40),
                        (byte)255);
                }

                if (session.SelectedTerritoryId == tid)
                {
                    Raylib.DrawRectangleLines(px, py, CellSize, CellSize, Color.RAYWHITE);
                }

                Raylib.DrawRectangle(px + 1, py + 1, CellSize - 2, CellSize - 2, fill);
            }
        }

        foreach (var territory in map.Territories)
        {
            var (cx, cy) = territory.Center;
            int px = MapOffsetX + cx * CellSize;
            int py = MapOffsetY + cy * CellSize;

            if (territory.IsStockpile)
            {
                Raylib.DrawCircle(px + CellSize / 2, py + CellSize / 2, 4, Color.GOLD);
            }

            string icon = ResourceIcon(territory);
            if (territory.HasCity) icon += "C";
            if (territory.HasWeapon) icon += "W";
            if (territory.HasHorse) icon += "H";

            if (icon.Length > 0)
            {
                UiText.DrawText(icon, px - 2, py - 6, 10, Color.RAYWHITE);
            }
        }
    }

    private static string ResourceIcon(Territory territory) => territory.Resource switch
    {
        ResourceType.Gold => "$",
        ResourceType.Horses => "h",
        ResourceType.Iron => "I",
        ResourceType.Coal => "c",
        ResourceType.Timber => "T",
        _ => ""
    };

    private void DrawSidebar(GameSession session)
    {
        int x = Raylib.GetScreenWidth() - SidebarWidth;
        Raylib.DrawRectangle(x, TopBarHeight, SidebarWidth, Raylib.GetScreenHeight() - TopBarHeight, new Color(28, 32, 44, 255));

        int y = TopBarHeight + 12;
        UiText.DrawText(session.CurrentPlayer.Name, x + 12, y, 20, session.CurrentPlayer.Color);
        y += 28;

        UiText.DrawText("Stockpile", x + 12, y, 16, Color.LIGHTGRAY);
        y += 20;
        foreach (var (type, amount) in session.CurrentPlayer.Stockpile.Enumerate())
        {
            UiText.DrawText($"{type}: {amount}", x + 16, y, 14, Color.RAYWHITE);
            y += 18;
        }
        y += 8;

        UiText.DrawText($"Cities: {session.CountCities(session.CurrentPlayer.Id)} / {session.Config.CitiesToWin}", x + 12, y, 14, Color.RAYWHITE);
        y += 24;

        if (session.HoveredTerritoryId is int hoverId)
        {
            var t = session.Map.GetTerritory(hoverId);
            if (t != null)
            {
                UiText.DrawText(t.Name, x + 12, y, 16, Color.SKYBLUE);
                y += 20;
                if (t.OwnerId >= 0)
                {
                    UiText.DrawText($"Owner: {session.Players[t.OwnerId].Name}", x + 12, y, 13, Color.LIGHTGRAY);
                    y += 18;
                }
                if (session.PreviewCombat(hoverId) is (int atk, int def) && t.OwnerId != session.CurrentPlayer.Id && t.OwnerId >= 0)
                {
                    UiText.DrawText($"Attack {atk} vs {def}", x + 12, y, 13, Color.ORANGE);
                    y += 18;
                }
            }
        }

        y = Raylib.GetScreenHeight() - 12;
        foreach (var player in session.Players)
        {
            y -= 18;
            UiText.DrawText($"{player.Name}: {session.CountCities(player.Id)} cities", x + 12, y, 12, player.Color);
        }
    }

    public void DrawButtons(IReadOnlyList<(Rectangle Rect, string Label)> buttons)
    {
        foreach (var (rect, label) in buttons)
        {
            var mouse = Raylib.GetMousePosition();
            bool hover = Raylib.CheckCollisionPointRec(mouse, rect);
            Raylib.DrawRectangleRec(rect, hover ? new Color(70, 90, 130, 255) : new Color(50, 60, 80, 255));
            Raylib.DrawRectangleLinesEx(rect, 1, Color.RAYWHITE);
            Vector2 size = UiText.MeasureTextSize(label, 14);
            UiText.DrawText(label, (int)(rect.X + (rect.Width - size.X) / 2), (int)(rect.Y + (rect.Height - size.Y) / 2), 14, Color.RAYWHITE);
        }
    }

    private void DrawMainMenu(GameConfig config)
    {
        int cx = Raylib.GetScreenWidth() / 2;
        UiText.DrawText("Lords of Conquest", cx - 140, 80, 36, Color.GOLD);
        UiText.DrawText("A territorial strategy classic", cx - 120, 125, 16, Color.LIGHTGRAY);
        UiText.DrawText($"Level: {config.Level}  |  Chance: {config.Chance}  |  Players: {config.TotalPlayers}", cx - 180, 160, 14, Color.RAYWHITE);
        UiText.DrawText("Click New Game to begin", cx - 100, 200, 16, Color.SKYBLUE);
    }

    public int? TerritoryAt(GameSession session, Vector2 mouse)
    {
        var map = session.Map;
        int gx = (int)((mouse.X - MapOffsetX) / CellSize);
        int gy = (int)((mouse.Y - MapOffsetY) / CellSize);
        var territory = map.TerritoryAt(gx, gy);
        return territory?.Id;
    }
}
