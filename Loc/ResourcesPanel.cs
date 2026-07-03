using System.Numerics;
using Raylib_cs;

namespace Loc;

public static class ResourcesPanel
{
    public const int Width = 560;
    public const int Height = 440;

    public static (int X, int Y) Origin(int screenWidth, int screenHeight)
    {
        return ((screenWidth - Width) / 2, (screenHeight - Height) / 2);
    }

    public static Rectangle PanelRect(int screenWidth, int screenHeight)
    {
        (int px, int py) = Origin(screenWidth, screenHeight);
        return new Rectangle(px, py, Width, Height);
    }

    public static Rectangle BackButtonRect(int screenWidth, int screenHeight)
    {
        (int px, int py) = Origin(screenWidth, screenHeight);
        const int buttonWidth = 140;
        const int buttonHeight = 34;
        return new Rectangle(px + (Width - buttonWidth) / 2, py + Height - buttonHeight - 16, buttonWidth, buttonHeight);
    }
}
