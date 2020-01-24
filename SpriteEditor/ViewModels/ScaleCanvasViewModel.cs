namespace SpriteEditor.ViewModels
{
    public class ScaleCanvasViewModel : ViewModel
    {
        private int currentWidth;
        private int CurrentWidth
        {
            get
            {
                return currentWidth;
            }
            set
            {
                SetProperty(ref currentWidth, value, "CurrentWidth");
            }
        }

        private int currentHeight;
        private int CurrentHeight
        {
            get
            {
                return currentWidth;
            }
            set
            {
                SetProperty(ref currentHeight, value, "CurrentHeight");
            }
        }

        private int gridWidth;
        public int GridWidth
        {
            get
            {
                return gridWidth;
            }
            set
            {
                SetProperty(ref gridWidth, value, "GridWidth");
            }
        }

        private int gridHeight;
        public int GridHeight
        {
            get
            {
                return gridHeight;
            }
            set
            {
                SetProperty(ref gridHeight, value, "GridHeight");
            }
        }

        public void Setup(int width, int height)
        {
            CurrentWidth = width;
            CurrentHeight = height;
            GridWidth = CurrentWidth;
            GridHeight = CurrentHeight;
        }
    }
}
