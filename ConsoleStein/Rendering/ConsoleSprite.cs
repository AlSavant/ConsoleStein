using System;
using ConsoleStein.Maths;

namespace ConsoleStein.Rendering
{
    [Serializable]
    public class ConsoleSprite
    {
        public bool IsTransparent { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Characters { get; set; }
        public byte[] Colors { get; set; }

        public ConsoleSprite() { }
        public ConsoleSprite(int width, int height, byte[] characters, byte[] colors, bool isTransparent)
        {
            Width = width;
            Height = height;
            Characters = characters;
            Colors = colors;
            IsTransparent = isTransparent;
        }

        public byte[] SamplePixel(Vector2 uv)
        {
            int x = (int)(uv.x * Width);
            int y = (int)(uv.y * Height);
            int index = y * Width + x;
            if (index < 0 || index >= Characters.Length || index >= Colors.Length)
                return null;
            return new byte[] { Characters[index], Colors[index] };
        }
    }
}
