using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpriteEditor.ViewModels;

namespace SpriteEditor.Views
{
    /// <summary>
    /// Interaction logic for ScaleCanvasView.xaml
    /// </summary>
    public partial class ScaleCanvasView : Window
    {
        public ScaleCanvasView()
        {
            InitializeComponent();
            DataContext = new ScaleCanvasViewModel();
        }
    }
}
