using System.IO;
using Raylib_cs;

namespace Loc;

public sealed class TitleScreenAssets : IDisposable
{
    private Texture2D _texture;
    private bool _loaded;

    public bool IsLoaded => _loaded;

    public void Load()
    {
        if (_loaded) return;

        string path = Path.Combine(AppContext.BaseDirectory, "Assets", "title-screen.png");
        if (!File.Exists(path)) return;

        _texture = Raylib.LoadTexture(path);
        Raylib.SetTextureFilter(_texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
        _loaded = true;
    }

    public void DrawFullscreen()
    {
        if (!_loaded) return;

        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        Raylib.DrawTexturePro(
            _texture,
            new Rectangle(0, 0, _texture.Width, _texture.Height),
            new Rectangle(0, 0, w, h),
            System.Numerics.Vector2.Zero,
            0,
            Color.WHITE);
    }

    public void Dispose()
    {
        if (!_loaded) return;
        Raylib.UnloadTexture(_texture);
        _loaded = false;
    }
}
