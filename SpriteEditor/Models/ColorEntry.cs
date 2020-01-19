using System;
using System.Windows.Media;


namespace SpriteEditor.Models
{
    public struct ColorEntry : IEquatable<ColorEntry>
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

        public bool Equals(ColorEntry other)
        {
            return Equals(other, this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var objectToCompareWith = (ColorEntry)obj;
            return objectToCompareWith.ConsoleColor == ConsoleColor;
        }

        public override int GetHashCode()
        {
            var calculation = ConsoleColor;
            return calculation.GetHashCode();
        }

        public static bool operator ==(ColorEntry c1, ColorEntry c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(ColorEntry c1, ColorEntry c2)
        {
            return !c1.Equals(c2);
        }
    }
}
