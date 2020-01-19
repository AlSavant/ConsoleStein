using SpriteEditor.Models;

namespace SpriteEditor.ViewModels
{
    public class PixelEntry : ViewModel
    {
        private char character;
        public char Character
        {
            get
            {
                return character;
            }
            set
            {
                SetProperty(ref character, value, "Character");
            }
        }

        private ColorEntry color;
        public ColorEntry Color
        {
            get
            {
                return color;
            }
            set
            {
                SetProperty(ref color, value, "Color");
            }
        }

        public PixelEntry Clone()
        {
            var clone = new PixelEntry();
            clone.Character = Character;
            clone.Color = Color;
            return clone;
        }
    }
}
