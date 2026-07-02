using System.IO;
using System.Numerics;
using Raylib_cs;

namespace Loc;

public static class ResourceIcons
{
    private const int IconCount = 5;
    private static Texture2D _sheet;
    private static bool _loaded;

    public static bool IsLoaded => _loaded;

    public static void Load()
    {
        if (_loaded) return;

        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "resource-icons.png");
        if (!File.Exists(path)) return;

        Image img = Raylib.LoadImage(path);
        Raylib.ImageColorReplace(ref img, new Color(0, 0, 0, 255), new Color(0, 0, 0, 0));
        _sheet = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);
        Raylib.SetTextureFilter(_sheet, TextureFilter.TEXTURE_FILTER_POINT);
        _loaded = true;
    }

    public static void Unload()
    {
        if (!_loaded) return;
        Raylib.UnloadTexture(_sheet);
        _loaded = false;
    }

    public static void Draw(ResourceType type, int cx, int cy, int size = 14)
    {
        if (!_loaded) return;

        int index = (int)type;
        int cellW = _sheet.Width / IconCount;
        int crop = Math.Min(cellW, _sheet.Height);
        int srcX = index * cellW + (cellW - crop) / 2;
        int srcY = (_sheet.Height - crop) / 2;

        var source = new Rectangle(srcX, srcY, crop, crop);
        var dest = new Rectangle(cx - size / 2f, cy - size / 2f, size, size);
        Raylib.DrawTexturePro(_sheet, source, dest, Vector2.Zero, 0, Color.WHITE);
    }

    public static void DrawBoxed(ResourceType type, int cx, int cy, int boxSize = 48)
    {
        int half = boxSize / 2;
        Raylib.DrawRectangle(cx - half, cy - half, boxSize, boxSize, new Color(20, 24, 36, 220));
        Raylib.DrawRectangleLines(cx - half, cy - half, boxSize, boxSize, ClassicPalette.PanelBorder);
        Draw(type, cx, cy, boxSize - 8);
    }
}
