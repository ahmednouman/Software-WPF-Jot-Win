
using JotWin.View;
using System.Windows;
using System.Windows.Controls;


namespace JotWin.Controls.CanvasToolsMenu
{
    /// <summary>
    /// Interaction logic for DrawMenuUC.xaml
    /// </summary>
    public partial class DrawMenuUC : UserControl
    {

        public double SliderValue
        {
            get { return (double)GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }

        public static readonly DependencyProperty SliderValueProperty =
            DependencyProperty.Register("SliderValue", typeof(double), typeof(DrawMenuUC), new PropertyMetadata(0.0));
        public DrawMenuUC()
        {
            InitializeComponent();
        }

        private void PenSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = e.NewValue;

            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            if (main != null)
            {
                main.DrawingCanvas.DefaultDrawingAttributes.Width = SliderValue;
                main.DrawingCanvas.DefaultDrawingAttributes.Height = SliderValue;
                main.drawingSetting.PenSize = (int)SliderValue;
            }
        }

    }
}
