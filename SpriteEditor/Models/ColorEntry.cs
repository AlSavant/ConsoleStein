using System;
using System.Windows.Media;


namespace SpriteEditor.Models
{
    public struct ColorEntry
    {
        public SolidColorBrush MediaColor { get; set; }
        public ConsoleColor ConsoleColor { get; set; }

        public static ColorEntry FromConsoleColor(ConsoleColor color)
        {
            var entry = new ColorEntry();
            entry.ConsoleColor = color;
            try
            {
                entry.MediaColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(entry.ConsoleColor.ToString()));
            }
            catch
            {
                if (entry.ConsoleColor == ConsoleColor.DarkYellow)
                {
                    entry.MediaColor = new SolidColorBrush(Colors.Khaki);
                }
                else
                {
                    Console.WriteLine($"Could not resolve color {entry.ConsoleColor.ToString()}!");
                }

            }
            return entry;
        }
    }
}
