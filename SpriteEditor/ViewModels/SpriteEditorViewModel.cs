using System;
using System.Text;
using SpriteEditor.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;
using SpriteEditor.Commands;

namespace SpriteEditor.ViewModels
{
    public class SpriteEditorViewModel : ViewModel
    {
        public ObservableCollection<char> CharacterList { get; set; }
        public ObservableCollection<ColorEntry> ColorList { get; set; }

        private int gridWidth = 20;
        public int GridWidth
        {
            get
            {
                return gridWidth;
            }
            set
            {                
                if(SetProperty(ref gridWidth, value, "GridWidth"))
                {
                    OnGridResized();
                    OnPropertyChanged("PixelWidth");
                }
            }
        }
        private int gridHeight = 20;
        public int GridHeight
        {
            get
            {
                return gridHeight;
            }
            set
            {
                if(SetProperty(ref gridHeight, value, "GridHeight"))
                {
                    OnGridResized();
                    OnPropertyChanged("PixelHeight");
                }
            }
        }

        public int PixelWidth 
        { 
            get 
            { 
                return GridWidth * 22; 
            } 
        }
        public int PixelHeight 
        { 
            get 
            { 
                return GridHeight * 22; 
            } 
        }

        private bool showGrid = true;
        public bool ShowGrid
        {
            get
            {
                return showGrid;
            }
            set
            {
                SetProperty(ref showGrid, value, "ShowGrid");
            }
        }

        private int selectedCharacterIndex = 0;
        public int SelectedCharacterIndex
        {
            get
            {
                return selectedCharacterIndex;
            }
            set
            {
                if (SetProperty(ref selectedCharacterIndex, value, "SelectedCharacterIndex"))
                {
                    SelectedCharacter = CharacterList[value];
                }
            }
        }

        private char SelectedCharacter { get; set; }

        private int selectedColorIndex = 0;
        public int SelectedColorIndex
        {
            get
            {
                return selectedColorIndex;
            }
            set
            {
                if (SetProperty(ref selectedColorIndex, value, "SelectedColorIndex"))
                {
                    SelectedColor = ColorList[value];

                }
            }
        }
        private ColorEntry SelectedColor { get; set; }

        private int tabIndex = 0;
        public int TabIndex
        {
            get
            {
                return tabIndex;
            }
            set
            {
                SetProperty(ref tabIndex, value, "TabIndex");
            }
        }
        
        public ObservableCollectionEx<PixelEntry> Pixels { get; set; }         

        public SpriteEditorViewModel()
        {
            Setup();
        }

        public void Setup()
        {
            CharacterList = new ObservableCollection<char>();
            for(byte i = 0; i < 255; i++)
            {
                char c = (char)i;
                if (char.IsControl(c))
                    continue;
                if (char.IsWhiteSpace(c))
                    continue;
                var e = Encoding.GetEncoding("437");
                var s = e.GetString(new byte[] { i });
                CharacterList.Add(s[0]);
            }            
            ColorList = new ObservableCollection<ColorEntry>();
            var consoleColors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            for(int i = 0; i < consoleColors.Length; i++)
            {
                var entry = new ColorEntry();
                entry.ConsoleColor = consoleColors[i];
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
                ColorList.Add(entry);
            }
            Pixels = new ObservableCollectionEx<PixelEntry>();
            Pixels.ItemPropertyChanged += (o, e) => { OnPropertyChanged("Pixels"); };
            OnGridResized();
        }

        private void OnGridResized()
        {
            Pixels.Clear();
            for(int i = 0; i < GridHeight * GridWidth; i++)
            {
                Pixels.Add(new PixelEntry());
                Pixels[i].Character = ' ';
                Pixels[i].Color = new ColorEntry();
                Pixels[i].Color.ConsoleColor = ConsoleColor.Black;
                Pixels[i].Color.MediaColor = new SolidColorBrush(Colors.Black);
            }
        }

        private ICommand selectPixelCommand;
        public ICommand SelectPixelCommand
        {
            get
            {
                if (selectPixelCommand == null)
                {
                    selectPixelCommand = new RelayCommand(
                        param => SelectPixel(param)
                    );
                }
                return selectPixelCommand;
            }
        }

        private void SelectPixel(object param)
        {
            PixelEntry pixel = (PixelEntry)param;
            switch(TabIndex)
            {
                case 0:
                    pixel.Character = SelectedCharacter;
                    break;

                case 1:
                    pixel.Color = SelectedColor;
                    break;
            }
            OnPropertyChanged("Pixels");
        }

    }
}
