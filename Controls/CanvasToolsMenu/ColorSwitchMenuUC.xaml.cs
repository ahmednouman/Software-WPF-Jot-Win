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
        public System.Windows.Media.Color SelectedColor
        {
            get { return (System.Windows.Media.Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(System.Windows.Media.Color), typeof(ColorSwitchMenuUC), new PropertyMetadata(Colors.Black));

        public ColorSwitchMenuUC()
        {
            InitializeComponent();
        }

        private void colorSelect_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rectangle)
            {
                SolidColorBrush brush = rectangle.Fill as SolidColorBrush;
                if (brush != null)
                {
                    System.Windows.Media.Color color = brush.Color;
                    SelectedColor = color;
                    string hexColor = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
                    MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
                    if (main != null)
                    {
                        main.drawingSetting.SelectedColor = SelectedColor;
                        SolidColorBrush selectedColorBrush = new SolidColorBrush(SelectedColor);
                        main.global_selected_color.Fill = selectedColorBrush;
                        main.updateDrawingSettings();
                    }
                    
                }
            }
        }
    }
}
