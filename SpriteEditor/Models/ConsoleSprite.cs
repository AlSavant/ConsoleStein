using System;

namespace SpriteEditor.Models
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
    }
}
