using JotWin.View;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace JotWin.Controls.CanvasToolsMenu
{
    public partial class TextMenuUC : UserControl
    {
        public string selectedFont = "Georgia";
        private Rectangle? prevRectangle;

        public TextMenuUC()
        {
            InitializeComponent();
        }

        private void TextSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = e.NewValue;

            if (Window.GetWindow(this) is MainAppWindow main)
            {
                main.drawingSetting.TextSize = (int)sliderValue;
                main.FontSizeSelected();
            }
        }

        private void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                Rectangle rectangle = FindChild<Rectangle>(grid);
                TextBlock textBlock = FindChild<TextBlock>(grid);

                if (textBlock.Text != selectedFont)
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
                TextBlock textBlock = FindChild<TextBlock>(grid);

                if (textBlock.Text != selectedFont)
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
            if (sender is not Rectangle rectangle
                || rectangle.Parent is not Panel parentContainer)
            {
                return;
            }

            var textBlock = parentContainer.Children.OfType<TextBlock>().FirstOrDefault();

            if (prevRectangle != null)
            {
                prevRectangle.Fill = new SolidColorBrush(Colors.Transparent);
            }

            selectedFont = textBlock.FontFamily.ToString();
            rectangle.Fill = new BrushConverter().ConvertFrom("#E1E1E1") as Brush;
            Debug.WriteLine(selectedFont);
            if (textBlock != null)
            {
                string font = textBlock.FontFamily.ToString();
                if (Window.GetWindow(this) is MainAppWindow main)
                {
                    main.drawingSetting.selectedFont = font;
                    main.FontSelected();
                }
            }

            prevRectangle = rectangle;
        }
    }
}
