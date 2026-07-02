using System.Numerics;
using Raylib_cs;

namespace Loc;

static class Program
{
    private const int ScreenWidth = 800;
    private const int ScreenHeight = 450;
    private const string Title = "Loc";

    static void Main()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, Title);
        Raylib.SetTargetFPS(60);
        UiText.Load();

        try
        {
            const int fontSize = 48;
            Vector2 textSize = UiText.MeasureTextSize(Title, fontSize);
            int textX = (ScreenWidth - (int)textSize.X) / 2;
            int textY = (ScreenHeight - (int)textSize.Y) / 2;

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(30, 30, 36, 255));
                UiText.DrawText(Title, textX, textY, fontSize, Color.RAYWHITE);
                Raylib.EndDrawing();
            }
        }
        finally
        {
            UiText.Unload();
            Raylib.CloseWindow();
        }
    }
}
