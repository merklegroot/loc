using Raylib_cs;

namespace Loc;

public static class ResourceIcons
{
    public static void Draw(ResourceType type, int cx, int cy, int scale = 1, Color? ink = null)
    {
        Color color = ink ?? AccentColor(type);
        int s = Math.Max(1, scale);

        switch (type)
        {
            case ResourceType.Gold:
                DrawGold(cx, cy, s, color);
                break;
            case ResourceType.Horses:
                DrawHorseshoe(cx, cy, s, color);
                break;
            case ResourceType.Iron:
                DrawAnvil(cx, cy, s, color);
                break;
            case ResourceType.Coal:
                DrawCoal(cx, cy, s, color);
                break;
            case ResourceType.Timber:
                DrawTree(cx, cy, s, color);
                break;
        }
    }

    public static void DrawBoxed(ResourceType type, int cx, int cy, int boxSize = 40)
    {
        Color fill = type switch
        {
            ResourceType.Gold => new Color(90, 70, 25, 255),
            ResourceType.Horses => new Color(40, 85, 48, 255),
            ResourceType.Iron => new Color(70, 75, 85, 255),
            ResourceType.Coal => new Color(30, 32, 38, 255),
            ResourceType.Timber => new Color(55, 90, 45, 255),
            _ => new Color(60, 60, 60, 255)
        };

        int half = boxSize / 2;
        Raylib.DrawRectangle(cx - half, cy - half, boxSize, boxSize, fill);
        Raylib.DrawRectangleLines(cx - half, cy - half, boxSize, boxSize, ClassicPalette.PanelBorder);

        int scale = boxSize >= 36 ? 2 : 1;
        Draw(type, cx, cy, scale, AccentColor(type));
    }

    private static Color AccentColor(ResourceType type) => type switch
    {
        ResourceType.Gold => new Color(255, 215, 80, 255),
        ResourceType.Horses => new Color(240, 240, 230, 255),
        ResourceType.Iron => new Color(200, 210, 225, 255),
        ResourceType.Coal => new Color(150, 155, 165, 255),
        ResourceType.Timber => new Color(210, 170, 110, 255),
        _ => ClassicPalette.Text
    };

    private static void DrawGold(int cx, int cy, int s, Color c)
    {
        Raylib.DrawCircle(cx, cy, 4 * s, c);
        Raylib.DrawCircleLines(cx, cy, 4 * s, new Color((byte)(c.R / 2), (byte)(c.G / 2), (byte)(c.B / 2), c.A));
        Raylib.DrawCircle(cx, cy, 2 * s, new Color((byte)(c.R / 2), (byte)(c.G / 2), (byte)(c.B / 2), c.A));
    }

    private static void DrawHorseshoe(int cx, int cy, int s, Color c)
    {
        int t = Math.Max(1, s);
        for (int i = 0; i < t; i++)
        {
            Raylib.DrawLine(cx - 3 * s + i, cy - 3 * s, cx - 3 * s + i, cy + 2 * s, c);
            Raylib.DrawLine(cx + 3 * s - i, cy - 3 * s, cx + 3 * s - i, cy + 2 * s, c);
        }

        for (int x = -3 * s; x <= 3 * s; x++)
        {
            int archY = cy + 2 * s + (int)(s * (1 - (double)(x * x) / (9 * s * s)) * 2);
            Raylib.DrawPixel(cx + x, archY, c);
            Raylib.DrawPixel(cx + x, archY + 1, c);
        }
    }

    private static void DrawAnvil(int cx, int cy, int s, Color c)
    {
        Raylib.DrawRectangle(cx - 4 * s, cy + s, 8 * s, 2 * s, c);
        Raylib.DrawRectangle(cx - 2 * s, cy - 2 * s, 4 * s, 3 * s, c);
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(cx + 2 * s, cy - 2 * s),
            new System.Numerics.Vector2(cx + 5 * s, cy + s),
            new System.Numerics.Vector2(cx + 2 * s, cy + s),
            c);
        Raylib.DrawRectangle(cx - 3 * s, cy + 3 * s, 6 * s, 2 * s, c);
    }

    private static void DrawCoal(int cx, int cy, int s, Color c)
    {
        Raylib.DrawCircle(cx - 2 * s, cy, 3 * s, c);
        Raylib.DrawCircle(cx + 3 * s, cy - s, 2 * s, c);
        Raylib.DrawCircle(cx + s, cy + 2 * s, 3 * s, c);
    }

    private static void DrawTree(int cx, int cy, int s, Color c)
    {
        Raylib.DrawRectangle(cx - s, cy + s, 2 * s, 4 * s, new Color(100, 70, 40, 255));
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(cx, cy - 5 * s),
            new System.Numerics.Vector2(cx - 4 * s, cy + 2 * s),
            new System.Numerics.Vector2(cx + 4 * s, cy + 2 * s),
            c);
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(cx, cy - 2 * s),
            new System.Numerics.Vector2(cx - 3 * s, cy + 3 * s),
            new System.Numerics.Vector2(cx + 3 * s, cy + 3 * s),
            c);
    }
}
