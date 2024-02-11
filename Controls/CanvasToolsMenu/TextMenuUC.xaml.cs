using JotWin.View;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;



namespace JotWin.Controls.CanvasToolsMenu
{
    public partial class TextMenuUC : UserControl
    {
        public string selectedTextID = "Arial";
        public TextMenuUC()
        {
            InitializeComponent();
        }

        private void TextSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = e.NewValue;

            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            if (main != null)
            {

                main.drawingSetting.TextSize = (int)sliderValue;
            }
        }
        private void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                Rectangle rectangle = FindChild<Rectangle>(grid);

                if (rectangle != null && rectangle.Name != selectedTextID)
                {
                    rectangle.Fill = new BrushConverter().ConvertFrom("#F2F0F2") as Brush;
                }
            }
        }

        private void Rectangle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                Rectangle rectangle = FindChild<Rectangle>(grid);

                if (rectangle != null && rectangle.Name != selectedTextID)
                {
                    rectangle.Fill = new SolidColorBrush(Colors.Transparent);
                }
            }          
        }

        private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    return typedChild;
                }

                T? childItem = FindChild<T>(child);
                if (childItem != null)
                {
                    return childItem;
                }
            }

            return null;
        }

        private void selectFont_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                if (frameworkElement.Parent is Panel parentContainer)
                {
                    var textBlock = parentContainer.Children.OfType<TextBlock>().FirstOrDefault();
                    var rect = parentContainer.Children.OfType<Rectangle>().FirstOrDefault();

                    FrameworkElement? foundElement = FindName(selectedTextID) as FrameworkElement;

                    if (foundElement is Rectangle rectangle)
                    {
                        rectangle.Fill = new SolidColorBrush(Colors.Transparent);
                    }

                    if (rect != null)
                    {
                        selectedTextID = rect.Name;
                        rect.Fill = new BrushConverter().ConvertFrom("#E1E1E1") as Brush;
                    }

                    if (textBlock != null)
                    {
                        string textBlockText = textBlock.Text;
                        if (Window.GetWindow(this) is MainAppWindow main)
                        {
                            main.drawingSetting.selectedFont = textBlockText;
                        }
                    }
                }
            }
        }
    }
}
