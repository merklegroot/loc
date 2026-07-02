using System.Numerics;
using Raylib_cs;

namespace Loc;

public sealed class GameRenderer
{
    private const int PanelHeight = 190;
    private const int StarSpacing = 18;
    private int _checkerFrame;
    private TitleScreenAssets? _titleScreen;

    public int MapOffsetX { get; private set; }
    public int MapOffsetY { get; private set; }
    public int CellSize { get; private set; } = 12;

    public void SetTitleScreen(TitleScreenAssets titleScreen) => _titleScreen = titleScreen;

    public void Draw(GameSession? session, GameConfig? menuConfig, bool inMenu)
    {
        if (inMenu || session == null)
        {
            DrawMainMenu();
            return;
        }

        DrawStarfield();
        _checkerFrame++;

        ComputeMapLayout(session.Map);
        DrawMap(session);
        DrawBottomPanel(session);
    }

    private void DrawStarfield()
    {
        Raylib.ClearBackground(ClassicPalette.Background);
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        for (int y = 0; y < h; y += StarSpacing)
        {
            for (int x = 0; x < w; x += StarSpacing)
            {
                Raylib.DrawPixel(x + (y / StarSpacing) % 3, y, ClassicPalette.Star);
            }
        }
    }

    private void ComputeMapLayout(WorldMap map)
    {
        int availW = Raylib.GetScreenWidth() - 24;
        int availH = Raylib.GetScreenHeight() - PanelHeight - 24;
        CellSize = Math.Max(8, Math.Min(availW / map.Width, availH / map.Height));
        int mapW = map.Width * CellSize;
        int mapH = map.Height * CellSize;
        MapOffsetX = (Raylib.GetScreenWidth() - mapW) / 2;
        MapOffsetY = Math.Max(8, (Raylib.GetScreenHeight() - PanelHeight - mapH) / 2);
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

                if (map.IsWater[x, y]) continue;

                int tid = map.TerritoryGrid[x, y];
                var territory = map.GetTerritory(tid)!;
                Color fill = TerritoryFill(session, territory, tid, x, y);

                Raylib.DrawRectangle(px, py, CellSize, CellSize, fill);
            }
        }

        DrawTerritoryBorders(map);
        DrawTerritoryIcons(session);
    }

    private Color TerritoryFill(GameSession session, Territory territory, int tid, int x, int y)
    {
        Color fill = territory.OwnerId >= 0
            ? session.Players[territory.OwnerId].Color
            : ClassicPalette.Unowned;

        if (session.IsAutoWinTarget(tid))
        {
            bool checker = ((x + y + _checkerFrame / 8) % 2) == 0;
            return checker ? fill : ClassicPalette.Border;
        }

        if (session.SelectedTerritoryId == tid)
        {
            return Brighten(fill, 30);
        }

        if (session.HoveredTerritoryId == tid && session.PendingAttackTarget == null)
        {
            return Brighten(fill, 18);
        }

        return fill;
    }

    private static Color Brighten(Color c, int amount) => new(
        (byte)Math.Min(255, c.R + amount),
        (byte)Math.Min(255, c.G + amount),
        (byte)Math.Min(255, c.B + amount),
        (byte)255);

    private void DrawTerritoryBorders(WorldMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.IsWater[x, y]) continue;
                int tid = map.TerritoryGrid[x, y];
                int px = MapOffsetX + x * CellSize;
                int py = MapOffsetY + y * CellSize;

                if (x + 1 < map.Width)
                {
                    int right = map.IsWater[x + 1, y] ? -1 : map.TerritoryGrid[x + 1, y];
                    if (right != tid)
                    {
                        Raylib.DrawLine(px + CellSize, py, px + CellSize, py + CellSize, ClassicPalette.Border);
                    }
                }

                if (y + 1 < map.Height)
                {
                    int down = map.IsWater[x, y + 1] ? -1 : map.TerritoryGrid[x, y + 1];
                    if (down != tid)
                    {
                        Raylib.DrawLine(px, py + CellSize, px + CellSize, py + CellSize, ClassicPalette.Border);
                    }
                }
            }
        }
    }

    private void DrawTerritoryIcons(GameSession session)
    {
        foreach (var territory in session.Map.Territories)
        {
            var (cx, cy) = territory.Center;
            int px = MapOffsetX + cx * CellSize + CellSize / 2;
            int py = MapOffsetY + cy * CellSize + CellSize / 2;

            if (territory.Resource != null)
            {
                DrawResourceIcon(px, py, territory.Resource.Value);
            }

            if (territory.HasCity) DrawCityIcon(px + 6, py - 4);
            if (territory.HasWeapon) DrawWeaponIcon(px - 8, py - 2);
            if (territory.HasHorse) DrawHorseIcon(px, py + 5);
            if (territory.IsStockpile) DrawStockpileIcon(px - 4, py + 6);
        }
    }

    private static void DrawResourceIcon(int cx, int cy, ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold:
                Raylib.DrawLine(cx - 3, cy - 3, cx + 3, cy + 3, ClassicPalette.Border);
                Raylib.DrawLine(cx + 3, cy - 3, cx - 3, cy + 3, ClassicPalette.Border);
                break;
            case ResourceType.Horses:
                Raylib.DrawRectangle(cx - 2, cy - 1, 5, 3, ClassicPalette.Border);
                break;
            case ResourceType.Iron:
                Raylib.DrawRectangle(cx - 2, cy - 2, 4, 4, ClassicPalette.Border);
                break;
            case ResourceType.Coal:
                Raylib.DrawCircle(cx, cy, 2, ClassicPalette.Border);
                break;
            case ResourceType.Timber:
                Raylib.DrawRectangle(cx - 1, cy - 3, 2, 6, ClassicPalette.Border);
                break;
        }
    }

    private static void DrawWeaponIcon(int cx, int cy)
    {
        Raylib.DrawLine(cx, cy - 4, cx, cy + 4, ClassicPalette.Border);
        Raylib.DrawLine(cx - 3, cy - 2, cx + 3, cy - 2, ClassicPalette.Border);
        Raylib.DrawLine(cx, cy - 4, cx - 2, cy - 6, ClassicPalette.Border);
        Raylib.DrawLine(cx, cy - 4, cx + 2, cy - 6, ClassicPalette.Border);
    }

    private static void DrawCityIcon(int cx, int cy)
    {
        Raylib.DrawRectangle(cx - 3, cy, 6, 4, ClassicPalette.Border);
        Raylib.DrawLine(cx - 3, cy, cx, cy - 3, ClassicPalette.Border);
        Raylib.DrawLine(cx, cy - 3, cx + 3, cy, ClassicPalette.Border);
    }

    private static void DrawHorseIcon(int cx, int cy)
    {
        Raylib.DrawPixel(cx, cy, ClassicPalette.Border);
        Raylib.DrawPixel(cx + 1, cy, ClassicPalette.Border);
        Raylib.DrawPixel(cx + 2, cy - 1, ClassicPalette.Border);
    }

    private static void DrawStockpileIcon(int cx, int cy)
    {
        Raylib.DrawRectangle(cx, cy, 4, 4, ClassicPalette.Border);
    }

    private void DrawBottomPanel(GameSession session)
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        int py = sh - PanelHeight;

        Raylib.DrawRectangle(0, py, sw, PanelHeight, ClassicPalette.PanelFill);
        Raylib.DrawRectangleLines(0, py, sw, PanelHeight, ClassicPalette.PanelBorder);
        Raylib.DrawLine(0, py, sw, py, ClassicPalette.PanelBorder);

        if (session.Phase == GamePhase.Conquest)
        {
            DrawConquestPanel(session, py);
            return;
        }

        DrawPhasePanel(session, py);
    }

    private void DrawConquestPanel(GameSession session, int py)
    {
        int sw = Raylib.GetScreenWidth();
        int attackNum = 3 - session.AttacksRemaining;
        string header = $"-ATTACK #{attackNum}-CONQUEST-FORCES-";
        DrawClassicLabel(header, 24, py + 12, 18);

        int statY = py + 44;
        if (session.PendingAttackTarget is int targetId &&
            session.PreviewCombat(targetId) is (int atk, int def))
        {
            var target = session.Map.GetTerritory(targetId)!;
            Color atkColor = session.CurrentPlayer.Color;
            Color defColor = target.OwnerId >= 0
                ? session.Players[target.OwnerId].Color
                : ClassicPalette.Unowned;

            DrawForceBar(24, statY, atkColor);
            DrawForceBar(34, statY, defColor);
            DrawStatBox(56, statY - 4, $"ATT = {atk}", atkColor);
            DrawStatBox(156, statY - 4, $"DEF = {def}", defColor);
        }

        string[] items = session.PendingAttackTarget != null
            ? ["BRING FORCES", "REPLAN", "ATTACK", "EXIT"]
            : ["PLAN ATTACK", "END TURN", "EXIT"];

        int menuY = py + 78;
        for (int i = 0; i < items.Length; i++)
        {
            bool selected = session.ConquestMenuIndex == i;
            DrawMenuItem(items[i], 24, menuY + i * 26, selected);
        }

        int infoX = sw / 2 + 40;
        UiText.DrawText($"YEAR {session.Year}", infoX, py + 16, 16, ClassicPalette.Text);
        DrawStockpileList(session, infoX, py + 44);
    }

    private void DrawPhasePanel(GameSession session, int py)
    {
        int sw = Raylib.GetScreenWidth();
        string phase = session.Phase.ToString().ToUpperInvariant();
        DrawClassicLabel($"-{phase}-", 24, py + 12, 18);
        UiText.DrawText(session.StatusMessage.ToUpperInvariant(), 24, py + 40, 14, ClassicPalette.Text);

        int infoX = sw / 2;
        UiText.DrawText($"YEAR {session.Year}  |  {session.CurrentPlayer.Name.ToUpperInvariant()}", infoX, py + 16, 16, session.CurrentPlayer.Color);
        DrawStockpileList(session, infoX, py + 44);
        UiText.DrawText($"CITIES {session.CountCities(session.CurrentPlayer.Id)}/{session.Config.CitiesToWin}", infoX, py + 120, 14, ClassicPalette.Text);
    }

    private static void DrawStockpileList(GameSession session, int x, int y)
    {
        UiText.DrawText("STOCKPILE", x, y, 14, ClassicPalette.Text);
        int line = 0;
        foreach (var (type, amount) in session.CurrentPlayer.Stockpile.Enumerate())
        {
            UiText.DrawText($"{type.ToString().ToUpperInvariant()} {amount}", x, y + 20 + line * 16, 13, ClassicPalette.Text);
            line++;
        }
    }

    private static void DrawClassicLabel(string text, int x, int y, int size)
    {
        UiText.DrawText(text, x, y, size, ClassicPalette.Text);
    }

    private static void DrawStatBox(int x, int y, string text, Color bg)
    {
        Vector2 size = UiText.MeasureTextSize(text, 16);
        int w = (int)size.X + 16;
        int h = (int)size.Y + 8;
        Raylib.DrawRectangle(x, y, w, h, bg);
        Raylib.DrawRectangleLines(x, y, w, h, ClassicPalette.Border);
        UiText.DrawText(text, x + 8, y + 4, 16, ClassicPalette.Border);
    }

    private static void DrawForceBar(int x, int y, Color color)
    {
        Raylib.DrawRectangle(x, y, 6, 18, color);
        Raylib.DrawRectangleLines(x, y, 6, 18, ClassicPalette.Border);
        Raylib.DrawLine(x + 1, y + 4, x + 5, y + 14, ClassicPalette.Border);
        Raylib.DrawLine(x + 5, y + 4, x + 1, y + 14, ClassicPalette.Border);
    }

    private static void DrawMenuItem(string label, int x, int y, bool selected)
    {
        if (selected)
        {
            Vector2 size = UiText.MeasureTextSize(label, 18);
            Raylib.DrawRectangle(x - 4, y - 2, (int)size.X + 12, (int)size.Y + 6, ClassicPalette.Highlight);
            UiText.DrawText(label, x, y, 18, ClassicPalette.HighlightText);
        }
        else
        {
            UiText.DrawText(label, x, y, 18, ClassicPalette.Text);
        }
    }

    public void DrawButtons(IReadOnlyList<(Rectangle Rect, string Label)> buttons, int selectedIndex = -1)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var (rect, label) = buttons[i];
            bool selected = i == selectedIndex;
            if (selected)
            {
                Raylib.DrawRectangleRec(rect, ClassicPalette.Highlight);
                Vector2 size = UiText.MeasureTextSize(label, 16);
                UiText.DrawText(label, (int)(rect.X + 8), (int)(rect.Y + (rect.Height - size.Y) / 2), 16, ClassicPalette.HighlightText);
            }
            else
            {
                var mouse = Raylib.GetMousePosition();
                bool hover = Raylib.CheckCollisionPointRec(mouse, rect);
                if (hover) Raylib.DrawRectangleRec(rect, new Color(40, 40, 40, 255));
                Vector2 size = UiText.MeasureTextSize(label, 16);
                UiText.DrawText(label, (int)(rect.X + 8), (int)(rect.Y + (rect.Height - size.Y) / 2), 16, ClassicPalette.Text);
            }
        }
    }

    private void DrawMainMenu()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();

        if (_titleScreen?.IsLoaded == true)
        {
            _titleScreen.DrawFullscreen();
            DrawTitleBackdrop(w);
        }
        else
        {
            DrawStarfield();
        }

        int cx = w / 2;
        DrawCenteredLabelWithShadow("LORDS OF CONQUEST", cx, 100, 40, new Color(255, 228, 160, 255));
        DrawCenteredLabelWithShadow("A TERRITORIAL STRATEGY CLASSIC", cx, 152, 16, new Color(230, 230, 230, 255));
    }

    private static void DrawTitleBackdrop(int screenWidth)
    {
        const int bandHeight = 240;
        for (int y = 0; y < bandHeight; y++)
        {
            byte alpha = (byte)(160 * (1.0 - (double)y / bandHeight));
            Raylib.DrawLine(0, y, screenWidth, y, new Color((byte)0, (byte)0, (byte)0, alpha));
        }
    }

    private static void DrawCenteredLabelWithShadow(string text, int centerX, int y, int size, Color color)
    {
        Vector2 measured = UiText.MeasureTextSize(text, size);
        int x = centerX - (int)(measured.X / 2);
        UiText.DrawText(text, x + 2, y + 2, size, new Color(0, 0, 0, 220));
        UiText.DrawText(text, x - 1, y, size, new Color(0, 0, 0, 120));
        UiText.DrawText(text, x + 1, y, size, new Color(0, 0, 0, 120));
        UiText.DrawText(text, x, y, size, color);
    }

    private static void DrawCenteredLabel(string text, int centerX, int y, int size)
    {
        Vector2 measured = UiText.MeasureTextSize(text, size);
        UiText.DrawText(text, centerX - (int)(measured.X / 2), y, size, ClassicPalette.Text);
    }

    public int? TerritoryAt(GameSession session, Vector2 mouse)
    {
        var map = session.Map;
        int gx = (int)((mouse.X - MapOffsetX) / CellSize);
        int gy = (int)((mouse.Y - MapOffsetY) / CellSize);
        var territory = map.TerritoryAt(gx, gy);
        return territory?.Id;
    }

    public List<(Rectangle Rect, string Label)> GetConquestMenuRects(GameSession session)
    {
        int sh = Raylib.GetScreenHeight();
        int py = sh - PanelHeight + 78;
        string[] items = session.PendingAttackTarget != null
            ? ["BRING FORCES", "REPLAN", "ATTACK", "EXIT"]
            : ["PLAN ATTACK", "END TURN", "EXIT"];

        var rects = new List<(Rectangle, string)>();
        for (int i = 0; i < items.Length; i++)
        {
            Vector2 size = UiText.MeasureTextSize(items[i], 18);
            rects.Add((new Rectangle(20, py + i * 26 - 2, size.X + 16, size.Y + 6), items[i]));
        }
        return rects;
    }
}
