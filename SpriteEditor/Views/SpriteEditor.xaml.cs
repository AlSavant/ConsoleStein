using System.Windows;
using SpriteEditor.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;

namespace SpriteEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new SpriteEditorViewModel();
        }

        //Collapse MenuItem when item is clicked
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Hyperlink;
            if (b == null)
                return;
            var mi = FindParent<MenuItem>(b.Parent);
            if (mi == null)
                return;
            mi.RaiseEvent(
               new MouseButtonEventArgs(
                  Mouse.PrimaryDevice, 0, MouseButton.Left
               )
               { RoutedEvent = Mouse.MouseUpEvent }
            );
        }

        private T FindParent<T>(DependencyObject child)
        where T : DependencyObject
        {
            if (child == null) return null;

            T foundParent = null;
            var currentParent = VisualTreeHelper.GetParent(child);

            do
            {
                var frameworkElement = currentParent as FrameworkElement;
                if (frameworkElement is T)
                {
                    foundParent = (T)currentParent;
                    break;
                }

                currentParent = VisualTreeHelper.GetParent(currentParent);

            } while (currentParent != null);

            return foundParent;
        }

        //Shortcut detection
        private void SpriteEditor_KeyDown(object sender, KeyEventArgs e)
        {
            var ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (!ctrl)
                return;
            var s = Keyboard.IsKeyDown(Key.S);
            var shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var context = (SpriteEditorViewModel)DataContext;
            if(s)
            {
                if(shift)
                {
                    //Save as...
                    if (context.IsDirty)
                        context.SaveFileWithLocationCommand.Execute(this);
                }
                else
                {
                    //Save
                    if(context.CanSave)
                        context.SaveFileCommand.Execute(this);
                    else if(context.IsDirty)
                        context.SaveFileWithLocationCommand.Execute(this);
                }
                return;
            }
            var n = Keyboard.IsKeyDown(Key.N);
            if(n)
            {
                context.NewSpriteCommand.Execute(this);
                return;
            }

            var o = Keyboard.IsKeyDown(Key.O);
            if(o)
            {
                context.OpenSpriteCommand.Execute(this);
                return;
            }

            var z = Keyboard.IsKeyDown(Key.Z);
            var y = Keyboard.IsKeyDown(Key.Y);
            if(z)
            {
                //Undo
                context.Undo();
                return;
            }
            if(y)
            {
                //Redo
                context.Redo();
                return;
            }

        }
    }
}
