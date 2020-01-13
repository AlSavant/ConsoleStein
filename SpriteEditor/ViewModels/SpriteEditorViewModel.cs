using System;
using System.Text;
using SpriteEditor.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SpriteEditor.Commands;
using System.Linq;

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

        private string importedArt;
        public string ImportedArt
        {
            get
            {
                return importedArt;
            }
            set
            {
                SetProperty(ref importedArt, value, "ImportedArt");
            }
        }

        private bool canPaintCharacters = true;
        public bool CanPaintCharacters
        {
            get
            {
                return canPaintCharacters;
            }
            set
            {
                SetProperty(ref canPaintCharacters, value, "CanPaintCharacters");
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
                var entry = ColorEntry.FromConsoleColor(consoleColors[i]);                
                ColorList.Add(entry);
            }
            var last = ColorList[ColorList.Count - 1];
            ColorList[ColorList.Count - 1] = ColorList[0];
            ColorList[0] = last;
            SelectedCharacter = CharacterList[0];
            SelectedColor = ColorList[0];
            Pixels = new ObservableCollectionEx<PixelEntry>();
            Pixels.ItemPropertyChanged += (o, e) => { OnPropertyChanged("Pixels"); };
            OnGridResized();
        }

        private void OnGridResized()
        {
            Pixels.Clear();

            for (int i = 0; i < GridHeight * GridWidth; i++)
            {
                Pixels.Add(new PixelEntry());
                Pixels[i].Character = ' ';
                Pixels[i].Color = ColorEntry.FromConsoleColor(ConsoleColor.Black);
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
            if(CanPaintCharacters)
                pixel.Character = SelectedCharacter;
            pixel.Color = SelectedColor;                       
            OnPropertyChanged("Pixels");
        }

        private ICommand fillCommand;
        public ICommand FillCommand
        {
            get
            {
                if (fillCommand == null)
                {
                    fillCommand = new RelayCommand(
                        param => Fill()
                    );
                }
                return fillCommand;
            }
        }

        private void Fill()
        {
            for(int i = 0; i < Pixels.Count; i++)
            {
                if(CanPaintCharacters)
                    Pixels[i].Character = SelectedCharacter;
                Pixels[i].Color = SelectedColor;
            }
        }

        private ICommand clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (clearCommand == null)
                {
                    clearCommand = new RelayCommand(
                        param => Clear()
                    );
                }
                return clearCommand;
            }
        }

        private void Clear()
        {
            for (int i = 0; i < Pixels.Count; i++)
            {
                if(CanPaintCharacters)
                    Pixels[i].Character = ' ';
                Pixels[i].Color = ColorEntry.FromConsoleColor(ConsoleColor.Black);
            }
        }

        private ICommand importArtCommand;
        public ICommand ImportArtCommand
        {
            get
            {
                if (importArtCommand == null)
                {
                    importArtCommand = new RelayCommand(
                        param => ImportArt()
                    );
                }
                return importArtCommand;
            }
        }

        private void ImportArt()
        {
            if (string.IsNullOrEmpty(ImportedArt))
                return;
            var lines = ImportedArt.Split('\n');
            GridHeight = lines.Length;
            var ordered = lines.OrderByDescending(x => x.Length);
            GridWidth = ordered.First().Length;
            OnGridResized();
            for(int y = 0; y < lines.Length; y++)
            {
                var line = lines[y];
                for (int x = 0; x < GridWidth; x++)
                {
                    if (x >= line.Length)
                        continue;
                    var pixel = Pixels[y * GridWidth + x];
                    pixel.Character = line[x];
                    pixel.Color = SelectedColor;
                }
            }
            ImportedArt = string.Empty;
            OnPropertyChanged("Pixels");
        }
    }
}
