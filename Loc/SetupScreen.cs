using System.Numerics;
using Raylib_cs;

namespace Loc;

public static class SetupScreen
{
    public sealed record SetupRow(string Label, string Value, Rectangle Rect);

    public static IReadOnlyList<SetupRow> Rows(GameConfig config, int screenWidth, int screenHeight)
    {
        int panelW = 420;
        int x = (screenWidth - panelW) / 2;
        int y = 200;
        int h = 30;
        int gap = 6;

        return
        [
            new SetupRow("PLAYERS", $"{config.TotalPlayers}", new Rectangle(x, y, panelW, h)),
            new SetupRow("HUMANS", $"{config.HumanPlayerCount}", new Rectangle(x, y + (h + gap), panelW, h)),
            new SetupRow("LEVEL", config.Level.ToString().ToUpperInvariant(), new Rectangle(x, y + 2 * (h + gap), panelW, h)),
            new SetupRow("CHANCE", config.Chance.ToString().ToUpperInvariant(), new Rectangle(x, y + 3 * (h + gap), panelW, h)),
            new SetupRow("RESOURCES", config.Abundance.ToString().ToUpperInvariant(), new Rectangle(x, y + 4 * (h + gap), panelW, h)),
            new SetupRow("CITIES TO WIN", $"{config.CitiesToWin}", new Rectangle(x, y + 5 * (h + gap), panelW, h)),
            new SetupRow("WATER", $"{(int)(config.WaterRatio * 100)}%", new Rectangle(x, y + 6 * (h + gap), panelW, h)),
            new SetupRow("AI STYLE", config.AiPersonality.ToString().ToUpperInvariant(), new Rectangle(x, y + 7 * (h + gap), panelW, h)),
        ];
    }

    public static (Rectangle Back, Rectangle Start) ActionButtons(int screenWidth, int screenHeight)
    {
        int w = 160;
        int h = 34;
        int y = screenHeight - 90;
        int gap = 24;
        int totalW = w * 2 + gap;
        int x = (screenWidth - totalW) / 2;
        return (new Rectangle(x, y, w, h), new Rectangle(x + w + gap, y, w, h));
    }

    public static void Draw(GameConfig config, int screenWidth, int screenHeight)
    {
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 140));

        int cx = screenWidth / 2;
        Vector2 titleSize = UiText.MeasureTextSize("NEW GAME SETUP", 28);
        UiText.DrawText("NEW GAME SETUP", cx - (int)(titleSize.X / 2), 130, 28, new Color(255, 228, 160, 255));

        foreach (var row in Rows(config, screenWidth, screenHeight))
        {
            Raylib.DrawRectangleRec(row.Rect, new Color(28, 34, 50, 230));
            Raylib.DrawRectangleLinesEx(row.Rect, 1, ClassicPalette.PanelBorder);
            UiText.DrawText(row.Label, (int)row.Rect.X + 12, (int)row.Rect.Y + 7, 14, ClassicPalette.Text);
            Vector2 valueSize = UiText.MeasureTextSize(row.Value, 14);
            UiText.DrawText(row.Value, (int)(row.Rect.X + row.Rect.Width - valueSize.X - 12), (int)row.Rect.Y + 7, 14, ClassicPalette.PlayerCyan);
        }

        var (back, start) = ActionButtons(screenWidth, screenHeight);
        DrawActionButton(back, "BACK");
        DrawActionButton(start, "START GAME");
        UiText.DrawText("CLICK A ROW TO CYCLE  |  ARROW KEYS + ENTER", cx - 200, screenHeight - 120, 12, new Color(180, 180, 180, 255));
    }

    private static void DrawActionButton(Rectangle rect, string label)
    {
        var mouse = Raylib.GetMousePosition();
        bool hover = Raylib.CheckCollisionPointRec(mouse, rect);
        Raylib.DrawRectangleRec(rect, hover ? new Color(50, 60, 80, 255) : new Color(28, 34, 50, 255));
        Raylib.DrawRectangleLinesEx(rect, 1, ClassicPalette.PanelBorder);
        Vector2 size = UiText.MeasureTextSize(label, 16);
        UiText.DrawText(label, (int)(rect.X + (rect.Width - size.X) / 2), (int)(rect.Y + 8), 16, ClassicPalette.Text);
    }

    public static GameConfig CycleRow(GameConfig config, string label)
    {
        if (label == "PLAYERS")
        {
            int nextPlayers = Cycle(config.TotalPlayers, 2, 4);
            return config with
            {
                TotalPlayers = nextPlayers,
                HumanPlayerCount = Math.Min(config.HumanPlayerCount, nextPlayers)
            };
        }

        return label switch
        {
            "HUMANS" => config with
            {
                HumanPlayerCount = Cycle(config.HumanPlayerCount, 1, config.TotalPlayers)
            },
            "LEVEL" => config with { Level = CycleEnum(config.Level) },
            "CHANCE" => config with { Chance = CycleEnum(config.Chance) },
            "RESOURCES" => config with { Abundance = CycleEnum(config.Abundance) },
            "CITIES TO WIN" => config with { CitiesToWin = Cycle(config.CitiesToWin, 3, 8) },
            "WATER" => config with { WaterRatio = CycleWater(config.WaterRatio) },
            "AI STYLE" => config with { AiPersonality = CycleEnum(config.AiPersonality) },
            _ => config
        };
    }

    private static int Cycle(int value, int min, int max) =>
        value >= max ? min : value + 1;

    private static T CycleEnum<T>(T value) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        int idx = Array.IndexOf(values, value);
        return values[(idx + 1) % values.Length];
    }

    private static double CycleWater(double ratio) => ratio switch
    {
        < 0.16 => 0.16,
        < 0.22 => 0.22,
        < 0.30 => 0.30,
        _ => 0.10
    };
}
