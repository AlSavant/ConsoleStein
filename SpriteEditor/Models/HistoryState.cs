using SpriteEditor.ViewModels;
using System.Collections.Generic;

namespace SpriteEditor.Models
{
    public struct HistoryState
    {
        public string StateName { get; set; }
        public List<PixelEntry> Grid { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }

        public HistoryState(string name, int width, int height, SmartCollection<PixelEntry> grid)
        {
            StateName = name;            
            GridWidth = width;
            GridHeight = height;
            Grid = new List<PixelEntry>(grid.Count);
            for(int i = 0; i < grid.Count; i++)
            {
                Grid.Add(grid[i].Clone());
            }
        }
        
        public List<PixelEntry> GetGridClone()
        {
            var grid = new List<PixelEntry>(Grid.Count);
            for (int i = 0; i < Grid.Count; i++)
            {
                grid.Add(Grid[i].Clone());
            }
            return grid;
        }
    }
}
