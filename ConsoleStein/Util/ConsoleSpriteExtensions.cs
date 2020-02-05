using ConsoleStein.Assets;
using ConsoleStein.Maths;
using System;

namespace ConsoleStein.Util
{
    public static class ConsoleSpriteExtensions
    {
        public static byte[] SamplePixel(this ConsoleSprite sprite, Vector2 uv)
        {
            int x = (int)Math.Round(uv.x * sprite.Width);
            int y = (int)Math.Round(uv.y * sprite.Height);
            int index = y * sprite.Width + x;
            if (index < 0 || index >= sprite.Characters.Length || index >= sprite.Colors.Length)
                return null;
            return new byte[] { sprite.Characters[index], sprite.Colors[index] };
        }
    }
}
