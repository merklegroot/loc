using Raylib_cs;

namespace Loc;

/// <summary>Approximate C64 / Apple II Lords of Conquest palette.</summary>
public static class ClassicPalette
{
    public static readonly Color Background = new(0, 0, 0, 255);
    public static readonly Color Star = new(255, 255, 255, 255);
    public static readonly Color Border = new(0, 0, 0, 255);
    public static readonly Color PanelBorder = new(40, 56, 140, 255);
    public static readonly Color PanelFill = new(0, 0, 0, 255);
    public static readonly Color Text = new(255, 255, 255, 255);
    public static readonly Color Highlight = new(255, 255, 255, 255);
    public static readonly Color HighlightText = new(0, 0, 0, 255);
    public static readonly Color Unowned = new(72, 72, 72, 255);

    public static readonly Color PlayerPink = new(236, 148, 154, 255);
    public static readonly Color PlayerCyan = new(122, 214, 204, 255);
    public static readonly Color PlayerGreen = new(110, 200, 110, 255);
    public static readonly Color PlayerYellow = new(220, 200, 90, 255);

    public static Color PlayerColor(int index) => index switch
    {
        0 => PlayerPink,
        1 => PlayerCyan,
        2 => PlayerGreen,
        3 => PlayerYellow,
        _ => Text
    };
}
