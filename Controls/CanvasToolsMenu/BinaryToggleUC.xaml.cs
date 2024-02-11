using JotWin.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JotWin.Controls.CanvasToolsMenu
{
    /// <summary>
    /// Interaction logic for BinaryToggleUC.xaml
    /// </summary>
    public partial class BinaryToggleUC : UserControl
    {
        public BinaryToggleUC()
        {
            InitializeComponent();
        }

        private void binary_toggle_tracing_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                Slider slider = (Slider)sender;
                double newValue = (e.Key == Key.Left || e.Key == Key.Down) ? 0 : 1;

                slider.Value = newValue;

                e.Handled = true;
            }
        }


        private void binary_toggle_tracing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double toggleState = e.NewValue;

            if (Window.GetWindow(this) is MainAppWindow main)
            {
                if (toggleState == 1)
                {
                    main.DrawingCanvas.Background = new SolidColorBrush(Colors.Transparent);
                    bg_clr.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60269E"));
                }
                else if (toggleState == 0)
                {
                    main.DrawingCanvas.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/JotWin;component/Resources/Assets/canvasBG.png")));
                    bg_clr.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6E6E6"));
                }

            }
        }
    }
}
