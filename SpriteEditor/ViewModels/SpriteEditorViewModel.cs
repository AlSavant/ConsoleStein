using System;
using System.Text;
using SpriteEditor.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SpriteEditor.Commands;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.Specialized;
using SpriteEditor.Util;
using SpriteEditor.Views;

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
                SetProperty(ref gridHeight, value, "GridHeight");
            }
        }

        public int PixelWidth 
        { 
            get 
            { 
                return GridWidth * 20; 
            } 
        }        

        private bool supportsTransparency;
        public bool SupportsTransparency
        {
            get
            {
                return supportsTransparency;
            }
            set
            {
                if(SetProperty(ref supportsTransparency, value, "SupportsTransparency"))
                {
                    IsDirty = true;                    
                }
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
                if (SetProperty(ref showGrid, value, "ShowGrid"))
                {                    
                    OnPropertyChanged("GridColor");
                }
            }
        }

        public SolidColorBrush GridColor 
        { 
            get
            {
                return ShowGrid ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
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

        public bool CanSave
        {
            get
            {
                return IsDirty && SavePath != string.Empty;
            }
        }

        private bool isDirty = true;
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                if (SetProperty(ref isDirty, value, "IsDirty"))
                {
                    OnPropertyChanged("CanSave");
                }
            }
        }

        private string savePath = string.Empty;
        public string SavePath
        {
            get
            {
                return savePath;
            }
            set
            {
                if(SetProperty(ref savePath, value, "SavePath"))
                {
                    OnPropertyChanged("SavePath");
                }
            }
        }

        private ObservableCollection<string> recentFiles;
        public ObservableCollection<string> RecentFiles
        {
            get
            {
                return recentFiles;
            }
            set
            {
                SetProperty(ref recentFiles, value, "RecentFiles");
            }
        }

        public bool CanBrowseRecents
        {
            get
            {
                return RecentFiles.Count > 0;
            }
        }                
        public SmartCollection<PixelEntry> Pixels { get; set; }

        public SpriteEditorViewModel()
        {
            Setup();
        }

        private Dictionary<char, byte> charLookup;

        public string UndoAction
        {
            get
            {
                return History.UndoAction;
            }
        }

        public string RedoAction
        {
            get
            {
                return History.RedoAction;
            }
        }

        private History History { get; set; }

        public void Setup()
        {
            CharacterList = new ObservableCollection<char>();
            charLookup = new Dictionary<char, byte>();
            var encoding = Encoding.GetEncoding("437");
            var space = encoding.GetString(new byte[] { 32 });
            CharacterList.Add(space[0]);
            charLookup.Add(space[0], 32);
            for(byte i = 0; i < 255; i++)
            {
                char c = (char)i;
                if (char.IsControl(c))
                    continue;
                if (char.IsWhiteSpace(c))
                    continue;                
                var s = encoding.GetString(new byte[] { i });
                charLookup.Add(s[0], i);
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
            Pixels = new SmartCollection<PixelEntry>();
            OnGridResized();
            History = new History(3);
            AddHistoryState("");
            RecentFiles = new ObservableCollection<string>();
            if(Properties.Settings.Default.RecentFiles == null)
            {
                Properties.Settings.Default.RecentFiles = new StringCollection();
                Properties.Settings.Default.Save();
            }
            for(int i = Properties.Settings.Default.RecentFiles.Count - 1; i >= 0; i--)
            {
                var file = Properties.Settings.Default.RecentFiles[i];
                if(!File.Exists(file))
                {
                    Properties.Settings.Default.RecentFiles.RemoveAt(i);
                    Properties.Settings.Default.Save();
                    continue;                    
                }
                RecentFiles.Add(file);
            }
            OnPropertyChanged("CanBrowseRecents");
            IsDirty = false;
        }

        private void AddHistoryState(string actionName)
        {
            if (History == null)
                return;
            History.AddState(new HistoryState(actionName, GridWidth, GridHeight, Pixels));
            OnPropertyChanged("UndoAction");
            OnPropertyChanged("RedoAction");
            OnPropertyChanged("CanUndo");
            OnPropertyChanged("CanRedo");
        }

        private ICommand undoCommand;
        public ICommand UndoCommand
        {
            get
            {
                if (undoCommand == null)
                {
                    undoCommand = new RelayCommand(
                        param => Undo()
                    );
                }
                return undoCommand;
            }
        }

        private ICommand redoCommand;
        public ICommand RedoCommand
        {
            get
            {
                if (redoCommand == null)
                {
                    redoCommand = new RelayCommand(
                        param => Redo()
                    );
                }
                return redoCommand;
            }
        }

        public bool CanUndo
        {
            get
            {
                if (History == null)
                    return false;
                return History.CanUndo;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (History == null)
                    return false;
                return History.CanRedo;
            }
        }

        public void Undo()
        {
            if (!History.CanUndo)
                return;
            var state = History.GetPreviousState();
            ApplyState(state);
        }

        public void Redo()
        {
            if (!History.CanRedo)
                return;
            var state = History.GetNextState();
            ApplyState(state);
        }

        private void ApplyState(HistoryState state)
        {
            gridWidth = state.GridWidth;
            gridHeight = state.GridHeight;
            Pixels.Reset(state.GetGridClone());
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");
            OnPropertyChanged("Pixels");
            OnPropertyChanged("UndoAction");
            OnPropertyChanged("RedoAction");
            OnPropertyChanged("CanUndo");
            OnPropertyChanged("CanRedo");
        }

        private void OnGridResized()
        {
            var pixels = new List<PixelEntry>(GridHeight * GridWidth);            
            for (int i = 0; i < GridHeight * GridWidth; i++)
            {
                pixels.Add(PixelEntry.Default);                
            }
            Pixels.Reset(pixels);
            AddHistoryState("Resize Grid");
            IsDirty = true;
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
            bool dirtyPixel = false;
            if(CanPaintCharacters)
            {
                if(pixel.Character != SelectedCharacter)
                {
                    dirtyPixel = true;
                    pixel.Character = SelectedCharacter;                    
                }
                
            }
            if(pixel.Color != SelectedColor)
            {
                dirtyPixel = true;
                pixel.Color = SelectedColor;               
            }            
            if(dirtyPixel)
            {
                AddHistoryState("Paint Pixel");
                IsDirty = true;
            }
                
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
            bool dirtyFill = false;
            for(int i = 0; i < Pixels.Count; i++)
            {
                if(CanPaintCharacters)
                {
                    if(Pixels[i].Character != SelectedCharacter)
                    {
                        Pixels[i].Character = SelectedCharacter;
                        dirtyFill = true;
                    }                    
                }
                if(Pixels[i].Color != SelectedColor)
                {
                    dirtyFill = true;
                    Pixels[i].Color = SelectedColor;
                }
                
            }
            if(dirtyFill)
            {
                AddHistoryState("Fill");
                IsDirty = true;
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
            bool dirtyClear = false;
            for (int i = 0; i < Pixels.Count; i++)
            {
                if(CanPaintCharacters)
                {
                    if(Pixels[i].Character != ' ')
                    {
                        dirtyClear = true;
                        Pixels[i].Character = ' ';
                    }                    
                }
                if(Pixels[i].Color.ConsoleColor != ConsoleColor.Black)
                {
                    dirtyClear = true;
                    Pixels[i].Color = ColorEntry.FromConsoleColor(ConsoleColor.Black);
                }
                
            }
            if(dirtyClear)
            {
                AddHistoryState("Clear");
                IsDirty = true;
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

        private char InvalidCharacter
        {
            get
            {
                var e = Encoding.GetEncoding("437");
                var s = e.GetString(new byte[] { 32 });
                return s[0];
            }
        }

        private void ImportArt()
        {
            if (string.IsNullOrEmpty(ImportedArt))
                return;
            var lines = ImportedArt.Split('\n');
            GridHeight = lines.Length;
            var ordered = lines.OrderByDescending(x => x.Length);
            int leftPad = lines.Length <= 1 ? 0 : -1;
            GridWidth = ordered.First().Length + leftPad;
            OnGridResized();
            for(int y = 0; y < lines.Length; y++)
            {
                var line = lines[y];
                for (int x = 0; x < GridWidth; x++)
                {
                    if (x >= line.Length)
                        continue;
                    var pixel = Pixels[y * GridWidth + x];
                    if(charLookup.ContainsKey(line[x]))
                    {
                        pixel.Character = line[x];
                        pixel.Color = SelectedColor;
                    }
                    else
                    {
                        pixel.Character = InvalidCharacter;
                        pixel.Color = ColorEntry.FromConsoleColor(ConsoleColor.Black);
                    }                    
                }
            }            
            ImportedArt = string.Empty;
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");
            AddHistoryState("Import Art");
            IsDirty = true;
        }

        private ICommand saveFileWithLocationCommand;
        public ICommand SaveFileWithLocationCommand
        {
            get
            {
                if (saveFileWithLocationCommand == null)
                {
                    saveFileWithLocationCommand = new RelayCommand(
                        param => SaveFileWithLocation()
                    );
                }
                return saveFileWithLocationCommand;
            }
        }

        private void SaveFileWithLocation()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Console Sprite (*.csp)|*.csp";
            if(SavePath == string.Empty)
            {
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(SavePath);
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                SavePath = saveFileDialog.FileName;
                SaveFile();
                if(!RecentFiles.Contains(SavePath))
                {
                    RecentFiles.Insert(0, SavePath);
                    Properties.Settings.Default.RecentFiles.Insert(0, SavePath);
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("RecentFiles");
                    OnPropertyChanged("CanBrowseRecents");
                }
                else
                {
                    int idx = RecentFiles.IndexOf(saveFileDialog.FileName);
                    if (idx > 0)
                    {
                        for (int i = 0; i < idx; i++)
                        {
                            string movedFile = RecentFiles[i];
                            RecentFiles[i + 1] = movedFile;
                            Properties.Settings.Default.RecentFiles[i + 1] = movedFile;
                        }
                        RecentFiles[0] = saveFileDialog.FileName;
                        Properties.Settings.Default.RecentFiles[0] = saveFileDialog.FileName;
                        Properties.Settings.Default.Save();
                        OnPropertyChanged("RecentFiles");
                    }
                }
            }
                
        }

        private ICommand saveFileCommand;
        public ICommand SaveFileCommand
        {
            get
            {
                if (saveFileCommand == null)
                {
                    saveFileCommand = new RelayCommand(
                        param => SaveFile()
                    );
                }
                return saveFileCommand;
            }
        }

        private void SaveFile()
        {
            var colors = new byte[GridWidth * GridHeight];
            var characters = new byte[colors.Length];
            for(int i = 0; i < Pixels.Count; i++)
            {
                if(Pixels[i].Color.ConsoleColor == ConsoleColor.Black || Pixels[i].Character == ' ')
                {
                    colors[i] = (byte)ConsoleColor.Black;
                    characters[i] = (byte)' ';
                    continue;
                }
                colors[i] = (byte)Pixels[i].Color.ConsoleColor;
                characters[i] = charLookup[Pixels[i].Character];
            }
            var sprite = new ConsoleSprite(GridWidth, GridHeight, characters, colors, supportsTransparency);
            var formatter = new BinaryFormatter();
            var stream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, sprite);
            stream.Close();
            IsDirty = false;
        }

        private ICommand newSpriteCommand;
        public ICommand NewSpriteCommand
        {
            get
            {
                if (newSpriteCommand == null)
                {
                    newSpriteCommand = new RelayCommand(
                        param => NewSprite()
                    );
                }
                return newSpriteCommand;
            }
        }

        private void NewSprite()
        {
            if (!DiscardChanges())
                return;
            if(GridWidth == 20 && GridHeight == 20)
            {
                Clear();
            }
            else
            {
                gridWidth = 20;
                gridHeight = 20;
                OnGridResized();
            }            
            ShowGrid = true;
            IsDirty = false;
        }

        private ICommand openWithLocationCommand;
        public ICommand OpenWithLocationCommand
        {
            get
            {
                if (openWithLocationCommand == null)
                {
                    openWithLocationCommand = new RelayCommand(
                        param => OpenFileWithLocation(param)
                    );
                }
                return openWithLocationCommand;
            }
        }

        private void OpenFileWithLocation(object param)
        {
            string path = param.ToString();
            if (!File.Exists(path))
            {
                Properties.Settings.Default.RecentFiles.Remove(path);
                RecentFiles.Remove(path);
                Properties.Settings.Default.Save();
                MessageBox.Show("The requested sprite file could not be found at the target location. Removing from list.", "Sprite not found", MessageBoxButton.OK);
                OnPropertyChanged("RecentFiles");
                OnPropertyChanged("CanBrowseRecents");
                return;
            }
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            var sprite = (ConsoleSprite)formatter.Deserialize(stream);
            stream.Close();
            ApplySprite(sprite);
            IsDirty = false;
            ImportedArt = string.Empty;
            SavePath = path;

            int idx = RecentFiles.IndexOf(path);
            if(idx > 0)
            {
                for (int i = 0; i < idx; i++)
                {
                    string movedFile = RecentFiles[i];
                    RecentFiles[i + 1] = movedFile;
                    Properties.Settings.Default.RecentFiles[i + 1] = movedFile;
                }
                RecentFiles[0] = path;
                Properties.Settings.Default.RecentFiles[0] = path;
                Properties.Settings.Default.Save();
                OnPropertyChanged("RecentFiles");
            }                      
        }

        private ICommand openSpriteCommand;
        public ICommand OpenSpriteCommand
        {
            get
            {
                if (openSpriteCommand == null)
                {
                    openSpriteCommand = new RelayCommand(
                        param => OpenSprite()
                    );
                }
                return openSpriteCommand;
            }
        }

        private void OpenSprite()
        {
            if (!DiscardChanges())
                return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Console Sprite (*.csp)|*.csp";
            if (SavePath == string.Empty)
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            }
            else
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(SavePath);
            }
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var formatter = new BinaryFormatter();
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                var sprite = (ConsoleSprite)formatter.Deserialize(stream);                
                stream.Close();
                ApplySprite(sprite);
                IsDirty = false;
                ImportedArt = string.Empty;
                SavePath = filePath;

                if (!RecentFiles.Contains(SavePath))
                {
                    RecentFiles.Insert(0, SavePath);
                    Properties.Settings.Default.RecentFiles.Insert(0, SavePath);
                    if(RecentFiles.Count > 10)
                    {
                        RecentFiles.RemoveAt(10);
                        Properties.Settings.Default.RecentFiles.RemoveAt(10);
                    }
                    Properties.Settings.Default.Save();
                }
                else
                {
                    int idx = RecentFiles.IndexOf(openFileDialog.FileName);
                    if (idx > 0)
                    {
                        for (int i = 0; i < idx; i++)
                        {
                            string movedFile = RecentFiles[i];
                            RecentFiles[i + 1] = movedFile;
                            Properties.Settings.Default.RecentFiles[i + 1] = movedFile;
                        }
                        RecentFiles[0] = openFileDialog.FileName;
                        Properties.Settings.Default.RecentFiles[0] = openFileDialog.FileName;
                        Properties.Settings.Default.Save();
                        OnPropertyChanged("RecentFiles");
                    }
                }
                OnPropertyChanged("RecentFiles");
                OnPropertyChanged("CanBrowseRecents");
            }
        }

        private void ApplySprite(ConsoleSprite sprite)
        {
            gridWidth = sprite.Width;
            gridHeight = sprite.Height;
            var pixels = new List<PixelEntry>(gridWidth * gridHeight);
            for (int i = 0; i < gridWidth * gridHeight; i++)
            {
                pixels.Add(new PixelEntry());
                var e = Encoding.GetEncoding("437");
                var s = e.GetString(new byte[] { sprite.Characters[i] });
                pixels[i].Character = s[0];
                pixels[i].Color = ColorEntry.FromConsoleColor((ConsoleColor)sprite.Colors[i]);
            }
            Pixels.Reset(pixels);
            SupportsTransparency = sprite.IsTransparent;
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");            
        }

        private bool DiscardChanges()
        {
            if (!IsDirty)
                return true;
            var messageBoxResult = MessageBox.Show("You have pending unsaved changes. Do you wish to discard them?", "Discard Changes", MessageBoxButton.YesNo);
            return messageBoxResult == MessageBoxResult.Yes;
        }

        private ICommand quitApplicationCommand;
        public ICommand QuitApplicationCommand
        {
            get
            {
                if (quitApplicationCommand == null)
                {
                    quitApplicationCommand = new RelayCommand(
                        param => QuitApplication()
                    );
                }
                return quitApplicationCommand;
            }
        }

        private ICommand rotateGrid90CWCommand;
        public ICommand RotateGrid90CWCommand
        {
            get
            {
                if (rotateGrid90CWCommand == null)
                {
                    rotateGrid90CWCommand = new RelayCommand(
                        param => RotateGrid90CW()
                    );
                }
                return rotateGrid90CWCommand;
            }
        }

        private void RotateGrid90CW()
        {
            Pixels.Reset(MatrixUtil.RotateMatrix90CW(Pixels.ToArray(), GridWidth, GridHeight));
            int w = gridWidth;
            int h = gridHeight;
            gridWidth = h;
            gridHeight = w;            
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");
            AddHistoryState("Rotate Grid 90° CW");
            IsDirty = true;
        }

        private ICommand rotateGrid90CCWCommand;
        public ICommand RotateGrid90CCWCommand
        {
            get
            {
                if (rotateGrid90CCWCommand == null)
                {
                    rotateGrid90CCWCommand = new RelayCommand(
                        param => RotateGrid90CCW()
                    );
                }
                return rotateGrid90CCWCommand;
            }
        }

        private void RotateGrid90CCW()
        {
            Pixels.Reset(MatrixUtil.RotateMatrix90CCW(Pixels.ToArray(), GridWidth, GridHeight));
            int w = gridWidth;
            int h = gridHeight;
            gridWidth = h;
            gridHeight = w;
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");
            AddHistoryState("Rotate Grid 90° CCW");
            IsDirty = true;
        }

        private ICommand rotateGrid180Command;
        public ICommand RotateGrid180Command
        {
            get
            {
                if (rotateGrid180Command == null)
                {
                    rotateGrid180Command = new RelayCommand(
                        param => RotateGrid180()
                    );
                }
                return rotateGrid180Command;
            }
        }

        private void RotateGrid180()
        {
            Pixels.Reset(MatrixUtil.RotateMatrix180(Pixels.ToArray(), GridWidth, GridHeight));            
            AddHistoryState("Rotate Grid 180°");
            IsDirty = true;
        }

        private ICommand flipGridVerticallyCommand;
        public ICommand FlipGridVerticallyCommand
        {
            get
            {
                if (flipGridVerticallyCommand == null)
                {
                    flipGridVerticallyCommand = new RelayCommand(
                        param => FlipGridVertically()
                    );
                }
                return flipGridVerticallyCommand;
            }
        }

        private void FlipGridVertically()
        {
            Pixels.Reset(MatrixUtil.FlipMatrixVertically(Pixels.ToArray(), GridWidth, GridHeight));
            AddHistoryState("Flip Grid Vertically");
            IsDirty = true;
        }

        private ICommand flipGridHorizontallyCommand;
        public ICommand FlipGridHorizontallyCommand
        {
            get
            {
                if (flipGridHorizontallyCommand == null)
                {
                    flipGridHorizontallyCommand = new RelayCommand(
                        param => FlipGridHorizontally()
                    );
                }
                return flipGridHorizontallyCommand;
            }
        }

        private void FlipGridHorizontally()
        {
            Pixels.Reset(MatrixUtil.FlipMatrixHorizontally(Pixels.ToArray(), GridWidth, GridHeight));
            AddHistoryState("Flip Grid Horizontally");
            IsDirty = true;
        }

        private ICommand openCanvasDialogCommand;
        public ICommand OpenCanvasDialogCommand
        {
            get
            {
                if (openCanvasDialogCommand == null)
                {
                    openCanvasDialogCommand = new RelayCommand(
                        param => OpenCanvasDialog()
                    );
                }
                return openCanvasDialogCommand;
            }
        }

        private void OpenCanvasDialog()
        {
            var view = new ScaleCanvasView();
            var viewModel = (ScaleCanvasViewModel)view.DataContext;
            viewModel.Setup(GridWidth, GridHeight);
            view.ShowDialog();
            if (!viewModel.ApplyChanges)
                return;
            if (GridWidth == viewModel.GridWidth && GridHeight == viewModel.GridHeight)
                return;
            int x = viewModel.PivotIndex % 3;
            int y = viewModel.PivotIndex / 3;
            Vector2Int normalizedPivot = new Vector2Int(x, y);            
            var newPixels = MatrixUtil.ResizeMatrix(Pixels.ToArray(), GridWidth, GridHeight, viewModel.GridWidth, viewModel.GridHeight, normalizedPivot);
            for(int i = 0; i < newPixels.Length; i++)
            {
                if (newPixels[i] == null)
                    newPixels[i] = PixelEntry.Default;
            }
            gridWidth = viewModel.GridWidth;
            gridHeight = viewModel.GridHeight;
            Pixels.Reset(newPixels);            
            OnPropertyChanged("GridHeight");
            OnPropertyChanged("GridWidth");
            OnPropertyChanged("PixelWidth");
            AddHistoryState("Resize Grid");
            IsDirty = true;
        }

        private void QuitApplication()
        {
            Application.Current.Shutdown();
        }        
    }
}
