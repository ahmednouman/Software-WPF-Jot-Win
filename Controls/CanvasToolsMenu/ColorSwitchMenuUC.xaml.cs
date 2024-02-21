using JotWin.View;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace JotWin.Controls.CanvasToolsMenu
{
    public partial class ColorSwitchMenuUC : UserControl
    {
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSwitchMenuUC), new PropertyMetadata(Colors.Black));

        public ColorSwitchMenuUC()
        {
            InitializeComponent();
        }

        private void colorSelect_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Rectangle rectangle
                || rectangle.Fill is not SolidColorBrush brush
                || Window.GetWindow(this) is not MainAppWindow main)
            {
                return;
            }

            main.ColorSelected(brush.Color);
            main.drawingSetting.SelectedColor = brush.Color;
            main.global_selected_color.Fill = brush;
            main.updateDrawingSettings();
        }
    }
}
