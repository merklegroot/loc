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
        int gapHalf = Math.Max(2, CellSize / 4);

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.IsWater[x, y]) continue;
                DrawTerritoryCellFill(session, map, x, y, gapHalf);
            }
        }

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.IsWater[x, y]) continue;
                DrawTerritoryCellBorder(map, x, y, gapHalf);
            }
        }

        DrawTerritoryIcons(session);
    }

    private readonly struct TerritoryCellShape
    {
        public required Rectangle Fill { get; init; }
        public required int Radius { get; init; }
        public required float Roundness { get; init; }
        public required int Segments { get; init; }
        public required bool BorderNorth { get; init; }
        public required bool BorderSouth { get; init; }
        public required bool BorderWest { get; init; }
        public required bool BorderEast { get; init; }
        public required bool RoundTopLeft { get; init; }
        public required bool RoundTopRight { get; init; }
        public required bool RoundBottomLeft { get; init; }
        public required bool RoundBottomRight { get; init; }

        public bool AnyRoundedCorner =>
            RoundTopLeft || RoundTopRight || RoundBottomLeft || RoundBottomRight;

        public bool FullyEnclosed =>
            BorderNorth && BorderSouth && BorderEast && BorderWest;
    }

    private static TerritoryCellShape ComputeTerritoryCellShape(
        WorldMap map, int gx, int gy, int pixelX, int pixelY, int cellSize, int gapHalf)
    {
        bool borderNorth = !SameRegion(map, gx, gy, gx, gy - 1);
        bool borderSouth = !SameRegion(map, gx, gy, gx, gy + 1);
        bool borderWest = !SameRegion(map, gx, gy, gx - 1, gy);
        bool borderEast = !SameRegion(map, gx, gy, gx + 1, gy);

        int fillX = pixelX + (borderWest ? gapHalf : 0);
        int fillY = pixelY + (borderNorth ? gapHalf : 0);
        int fillW = cellSize - (borderWest ? gapHalf : 0) - (borderEast ? gapHalf : 0);
        int fillH = cellSize - (borderNorth ? gapHalf : 0) - (borderSouth ? gapHalf : 0);

        bool roundTopLeft = borderNorth && borderWest;
        bool roundTopRight = borderNorth && borderEast;
        bool roundBottomLeft = borderSouth && borderWest;
        bool roundBottomRight = borderSouth && borderEast;

        int radius = ComputeCornerRadius(fillW, fillH, gapHalf);

        // Only attempt rounding on a corner if there's enough room.
        if (radius < 2)
        {
            roundTopLeft = roundTopRight = roundBottomLeft = roundBottomRight = false;
            radius = 0;
        }

        float roundness = radius > 0
            ? Math.Clamp(radius / (Math.Min(fillW, fillH) * 0.5f), 0.1f, 1f)
            : 0f;
        int segments = radius > 0 ? Math.Max(24, radius * 10) : 8;

        return new TerritoryCellShape
        {
            Fill = new Rectangle(fillX, fillY, fillW, fillH),
            Radius = radius,
            Roundness = roundness,
            Segments = segments,
            BorderNorth = borderNorth,
            BorderSouth = borderSouth,
            BorderWest = borderWest,
            BorderEast = borderEast,
            RoundTopLeft = roundTopLeft,
            RoundTopRight = roundTopRight,
            RoundBottomLeft = roundBottomLeft,
            RoundBottomRight = roundBottomRight
        };
    }

    private void DrawTerritoryCellFill(GameSession session, WorldMap map, int x, int y, int gapHalf)
    {
        int px = MapOffsetX + x * CellSize;
        int py = MapOffsetY + y * CellSize;
        var shape = ComputeTerritoryCellShape(map, x, y, px, py, CellSize, gapHalf);
        if (shape.Fill.Width <= 0 || shape.Fill.Height <= 0) return;

        int tid = map.TerritoryGrid[x, y];
        var territory = map.GetTerritory(tid)!;
        Color fill = TerritoryFill(session, territory, tid, x, y);
        DrawTerritoryFill(shape, fill);
    }

    private void DrawTerritoryCellBorder(WorldMap map, int x, int y, int gapHalf)
    {
        int px = MapOffsetX + x * CellSize;
        int py = MapOffsetY + y * CellSize;
        var shape = ComputeTerritoryCellShape(map, x, y, px, py, CellSize, gapHalf);
        if (shape.Fill.Width <= 0 || shape.Fill.Height <= 0) return;
        DrawTerritoryBorder(shape);
    }

    private const float BorderThickness = 1.5f;

    private static int ComputeCornerRadius(int w, int h, int gapHalf)
    {
        if (w < 5 || h < 5) return 0;
        int maxByDim = Math.Min(w, h) / 2 - 2;
        if (maxByDim < 2) return 0;
        // Bias toward a pleasantly round but not overly bulbous corner.
        // Tie it to the gap so the rounding lives nicely in the separation zone.
        int pref = Math.Min(gapHalf + 1, Math.Min(w, h) / 3);
        return Math.Min(pref, maxByDim);
    }

    private static void DrawTerritoryFill(TerritoryCellShape shape, Color fill)
    {
        var rect = shape.Fill;
        if (rect.Width <= 0 || rect.Height <= 0) return;

        int r = shape.Radius;
        if (r < 2 || !shape.AnyRoundedCorner)
        {
            Raylib.DrawRectangleRec(rect, fill);
            return;
        }

        int L = (int)rect.X;
        int T = (int)rect.Y;
        int W = (int)rect.Width;
        int H = (int)rect.Height;
        int R = L + W;
        int B = T + H;

        // Amounts to reserve on each side for rounding (only if that side participates in a rounded corner).
        int leftR = (shape.RoundTopLeft || shape.RoundBottomLeft) ? r : 0;
        int rightR = (shape.RoundTopRight || shape.RoundBottomRight) ? r : 0;
        int topR = (shape.RoundTopLeft || shape.RoundTopRight) ? r : 0;
        int bottomR = (shape.RoundBottomLeft || shape.RoundBottomRight) ? r : 0;

        // For completely rounded cells, use the high-quality path.
        if (shape.RoundTopLeft && shape.RoundTopRight && shape.RoundBottomLeft && shape.RoundBottomRight)
        {
            Raylib.DrawRectangleRounded(rect, shape.Roundness, shape.Segments, fill);
            return;
        }

        // Partial rounding: build the shape from rects + sectors so only desired corners are rounded.
        // Center body (inset by the side reserves).
        int cx = L + leftR;
        int cy = T + topR;
        int cw = W - leftR - rightR;
        int ch = H - topR - bottomR;
        if (cw > 0 && ch > 0)
        {
            Raylib.DrawRectangle(cx, cy, cw, ch, fill);
        }

        // Top ledge (the straight part of the top edge, between any rounded corners).
        // Slight inward overlap helps avoid seams with the sector radii.
        if (topR > 0 && cw > 0)
        {
            Raylib.DrawRectangle(cx, T, cw, topR + 1, fill);
        }

        // Bottom ledge.
        if (bottomR > 0 && cw > 0)
        {
            Raylib.DrawRectangle(cx, B - bottomR - 1, cw, bottomR + 1, fill);
        }

        // Left ledge (middle vertical strip on left, between top/bottom reserves).
        int ly0 = T + topR - 1;
        if (ly0 < T) ly0 = T;
        int ly1 = B - bottomR;
        int lh = ly1 - ly0;
        if (leftR > 0 && lh > 0)
        {
            Raylib.DrawRectangle(L, ly0, leftR, lh + 1, fill);
        }

        // Right ledge.
        int ry0 = T + topR - 1;
        if (ry0 < T) ry0 = T;
        int ry1 = B - bottomR;
        int rh = ry1 - ry0;
        if (rightR > 0 && rh > 0)
        {
            Raylib.DrawRectangle(R - rightR, ry0, rightR, rh + 1, fill);
        }

        // Corner sectors for the rounded corners only. These add the curved fill exactly where needed
        // and do not color the "ears" beyond the arc (those stay as background gap).
        int segs = shape.Segments;
        if (shape.RoundTopLeft)
        {
            Raylib.DrawCircleSector(new Vector2(L + r, T + r), r, 180f, 270f, segs, fill);
        }
        if (shape.RoundTopRight)
        {
            Raylib.DrawCircleSector(new Vector2(R - r, T + r), r, 270f, 360f, segs, fill);
        }
        if (shape.RoundBottomLeft)
        {
            Raylib.DrawCircleSector(new Vector2(L + r, B - r), r, 90f, 180f, segs, fill);
        }
        if (shape.RoundBottomRight)
        {
            Raylib.DrawCircleSector(new Vector2(R - r, B - r), r, 0f, 90f, segs, fill);
        }
    }

    private static void DrawTerritoryBorder(TerritoryCellShape shape)
    {
        Color border = ClassicPalette.Border;
        var rect = shape.Fill;
        float x = rect.X;
        float y = rect.Y;
        float w = rect.Width;
        float h = rect.Height;
        int ri = shape.Radius;
        float r = ri; // for convenience in float math below
        bool useArcs = ri >= 2 && shape.AnyRoundedCorner;

        if (shape.FullyEnclosed && useArcs &&
            shape.RoundTopLeft && shape.RoundTopRight && shape.RoundBottomLeft && shape.RoundBottomRight)
        {
            Raylib.DrawRectangleRoundedLines(rect, shape.Roundness, shape.Segments, BorderThickness, border);
            return;
        }

        if (shape.BorderNorth)
        {
            float x0 = x + (useArcs && shape.RoundTopLeft ? r : 0f);
            float x1 = x + w - (useArcs && shape.RoundTopRight ? r : 0f);
            if (x1 > x0) DrawBorderLine(x0, y, x1, y, border);
        }

        if (shape.BorderSouth)
        {
            float x0 = x + (useArcs && shape.RoundBottomLeft ? r : 0f);
            float x1 = x + w - (useArcs && shape.RoundBottomRight ? r : 0f);
            if (x1 > x0) DrawBorderLine(x0, y + h, x1, y + h, border);
        }

        if (shape.BorderWest)
        {
            float y0 = y + (useArcs && shape.RoundTopLeft ? r : 0f);
            float y1 = y + h - (useArcs && shape.RoundBottomLeft ? r : 0f);
            if (y1 > y0) DrawBorderLine(x, y0, x, y1, border);
        }

        if (shape.BorderEast)
        {
            float y0 = y + (useArcs && shape.RoundTopRight ? r : 0f);
            float y1 = y + h - (useArcs && shape.RoundBottomRight ? r : 0f);
            if (y1 > y0) DrawBorderLine(x + w, y0, x + w, y1, border);
        }

        if (!useArcs) return;

        if (shape.RoundTopLeft) DrawBorderArc(x + r, y + r, r, 180f, 270f, shape.Segments, border);
        if (shape.RoundTopRight) DrawBorderArc(x + w - r, y + r, r, 270f, 360f, shape.Segments, border);
        if (shape.RoundBottomLeft) DrawBorderArc(x + r, y + h - r, r, 90f, 180f, shape.Segments, border);
        if (shape.RoundBottomRight) DrawBorderArc(x + w - r, y + h - r, r, 0f, 90f, shape.Segments, border);
    }

    private static void DrawBorderLine(float x0, float y0, float x1, float y1, Color color) =>
        Raylib.DrawLineEx(new Vector2(x0, y0), new Vector2(x1, y1), BorderThickness, color);

    private static void DrawBorderArc(
        float cx, float cy, float radius, float startDegrees, float endDegrees, int segments, Color color)
    {
        float halfThick = BorderThickness * 0.5f;
        Raylib.DrawRingLines(
            new Vector2(cx, cy),
            Math.Max(0.1f, radius - halfThick),
            radius + halfThick,
            startDegrees,
            endDegrees,
            segments,
            color);
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

    private static bool SameRegion(WorldMap map, int x1, int y1, int x2, int y2) =>
        RegionAt(map, x1, y1) == RegionAt(map, x2, y2);

    private static int RegionAt(WorldMap map, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return -1;
        if (map.IsWater[x, y]) return -1;
        return map.TerritoryGrid[x, y];
    }

    private void DrawTerritoryIcons(GameSession session)
    {
        foreach (var territory in session.Map.Territories)
        {
            var (cx, cy) = territory.GetDisplayCell(session.Map);
            int px = MapOffsetX + cx * CellSize + CellSize / 2;
            int py = MapOffsetY + cy * CellSize + CellSize / 2;

            if (territory.Resource != null)
            {
                ResourceIcons.Draw(territory.Resource.Value, px, py, Math.Max(10, CellSize - 2));
            }

            if (territory.HasCity) DrawCityIcon(px + 6, py - 4);
            if (territory.HasWeapon) DrawWeaponIcon(px - 8, py - 2);
            if (territory.HasHorse) DrawHorseIcon(px, py + 5);
            if (territory.IsStockpile) DrawStockpileIcon(px - 4, py + 6);
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
            Vector2 size = UiText.MeasureTextSize(label, 16);
            int textX = (int)(rect.X + (rect.Width - size.X) / 2);
            int textY = (int)(rect.Y + (rect.Height - size.Y) / 2);

            if (selected)
            {
                Raylib.DrawRectangleRec(rect, ClassicPalette.Highlight);
                Raylib.DrawRectangleLinesEx(rect, 1, ClassicPalette.PanelBorder);
                UiText.DrawText(label, textX, textY, 16, ClassicPalette.HighlightText);
            }
            else
            {
                var mouse = Raylib.GetMousePosition();
                bool hover = Raylib.CheckCollisionPointRec(mouse, rect);
                Raylib.DrawRectangleRec(rect, hover ? new Color(50, 60, 80, 255) : new Color(28, 34, 50, 255));
                Raylib.DrawRectangleLinesEx(rect, 1, ClassicPalette.PanelBorder);
                UiText.DrawText(label, textX, textY, 16, ClassicPalette.Text);
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

    public void DrawResourcesScreen()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        (int px, int py) = ResourcesPanel.Origin(w, h);
        int panelW = ResourcesPanel.Width;
        int panelH = ResourcesPanel.Height;

        Raylib.DrawRectangle(0, 0, w, h, new Color(0, 0, 0, 180));
        Raylib.DrawRectangle(px, py, panelW, panelH, new Color(16, 20, 32, 250));
        Raylib.DrawRectangleLines(px, py, panelW, panelH, ClassicPalette.PanelBorder);

        DrawCenteredLabelWithShadow("RESOURCES", w / 2, py + 20, 28, new Color(255, 228, 160, 255));

        int y = py + 52;
        const int rowHeight = 62;
        int textX = px + 88;
        int iconX = px + 44;

        foreach (var entry in ResourceGuide.All)
        {
            int rowCenterY = y + rowHeight / 2;
            ResourceIcons.DrawBoxed(entry.Type, iconX, rowCenterY);

            string name = entry.Type.ToString().ToUpperInvariant();
            UiText.DrawText(name, textX, y, 18, ClassicPalette.PlayerCyan);
            UiText.DrawText(entry.Site.ToUpperInvariant(), textX, y + 20, 13, ClassicPalette.Text);

            DrawWrappedText(entry.Description, textX, y + 38, panelW - textX + px - 24, 13, new Color(200, 200, 200, 255));
            y += rowHeight;
        }

        DrawCenteredLabel("BEGINNER GAMES USE GOLD AND HORSES ONLY", w / 2, py + panelH - 62, 12);
    }

    private static void DrawWrappedText(string text, int x, int y, int maxWidth, int size, Color color)
    {
        var words = text.Split(' ');
        string line = "";
        int lineY = y;

        foreach (string word in words)
        {
            string trial = line.Length == 0 ? word : $"{line} {word}";
            if (UiText.MeasureTextSize(trial, size).X > maxWidth && line.Length > 0)
            {
                UiText.DrawText(line, x, lineY, size, color);
                lineY += size + 4;
                line = word;
            }
            else
            {
                line = trial;
            }
        }

        if (line.Length > 0)
        {
            UiText.DrawText(line, x, lineY, size, color);
        }
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
