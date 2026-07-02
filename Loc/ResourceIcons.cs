using Raylib_cs;

namespace Loc;

public static class ResourceIcons
{
    public static void Draw(ResourceType type, int cx, int cy, int scale = 1, Color? ink = null)
    {
        Color color = ink ?? ClassicPalette.Border;
        int s = Math.Max(1, scale);

        switch (type)
        {
            case ResourceType.Gold:
                DrawGold(cx, cy, s, color);
                break;
            case ResourceType.Horses:
                DrawHorse(cx, cy, s, color);
                break;
            case ResourceType.Iron:
                DrawIron(cx, cy, s, color);
                break;
            case ResourceType.Coal:
                DrawCoal(cx, cy, s, color);
                break;
            case ResourceType.Timber:
                DrawTimber(cx, cy, s, color);
                break;
        }
    }

    public static void DrawBoxed(ResourceType type, int cx, int cy, int boxSize = 40)
    {
        Color fill = type switch
        {
            ResourceType.Gold => new Color(120, 95, 40, 255),
            ResourceType.Horses => new Color(50, 100, 55, 255),
            ResourceType.Iron => new Color(90, 95, 105, 255),
            ResourceType.Coal => new Color(45, 48, 55, 255),
            ResourceType.Timber => new Color(100, 70, 40, 255),
            _ => new Color(60, 60, 60, 255)
        };

        int half = boxSize / 2;
        Raylib.DrawRectangle(cx - half, cy - half, boxSize, boxSize, fill);
        Raylib.DrawRectangleLines(cx - half, cy - half, boxSize, boxSize, ClassicPalette.PanelBorder);

        int scale = boxSize >= 36 ? 2 : 1;
        Draw(type, cx, cy, scale, ClassicPalette.Text);
    }

    private static void DrawGold(int cx, int cy, int s, Color c)
    {
        int r = 3 * s;
        Raylib.DrawLine(cx - r, cy - r, cx + r, cy + r, c);
        Raylib.DrawLine(cx + r, cy - r, cx - r, cy + r, c);
        Raylib.DrawCircle(cx, cy, s, c);
    }

    private static void DrawHorse(int cx, int cy, int s, Color c)
    {
        Raylib.DrawRectangle(cx - 3 * s, cy - s, 6 * s, 2 * s, c);
        Raylib.DrawRectangle(cx + 2 * s, cy - 2 * s, 2 * s, 2 * s, c);
        Raylib.DrawLine(cx - 3 * s, cy + s, cx - 4 * s, cy + 3 * s, c);
        Raylib.DrawLine(cx + 2 * s, cy + s, cx + 3 * s, cy + 3 * s, c);
    }

    private static void DrawIron(int cx, int cy, int s, Color c)
    {
        Raylib.DrawRectangle(cx - 3 * s, cy - 2 * s, 6 * s, 4 * s, c);
        Raylib.DrawLine(cx - 2 * s, cy - 3 * s, cx + 2 * s, cy - 3 * s, c);
    }

    private static void DrawCoal(int cx, int cy, int s, Color c)
    {
        Raylib.DrawCircle(cx - s, cy, 2 * s, c);
        Raylib.DrawCircle(cx + 2 * s, cy - s, 2 * s, c);
        Raylib.DrawCircle(cx + s, cy + s, 2 * s, c);
    }

    private static void DrawTimber(int cx, int cy, int s, Color c)
    {
        Raylib.DrawRectangle(cx - s, cy - 4 * s, 2 * s, 8 * s, c);
        Raylib.DrawLine(cx - 2 * s, cy - 4 * s, cx, cy - 6 * s, c);
        Raylib.DrawLine(cx, cy - 6 * s, cx + 2 * s, cy - 4 * s, c);
        Raylib.DrawCircle(cx, cy + 4 * s, s, c);
    }
}
