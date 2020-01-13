using System;
using System.Windows.Media;


namespace SpriteEditor.Models
{
    public class ColorEntry
    {
        public SolidColorBrush MediaColor { get; set; } = new SolidColorBrush(Colors.White);
        public ConsoleColor ConsoleColor { get; set; } = ConsoleColor.White;
    }
}
