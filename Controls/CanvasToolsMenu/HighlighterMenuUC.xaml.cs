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

    public partial class HighlighterMenuUC : UserControl
    {


        public double HighlighterSize
        {
            get { return (double)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("HighlighterSize", typeof(double), typeof(HighlighterMenuUC), new PropertyMetadata(20.0));


        public HighlighterMenuUC()
        {
            InitializeComponent();
        }

        private void HighlighterSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double HighlighterSize = e.NewValue;

            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            if (main != null)
            {
                main.DrawingCanvas.DefaultDrawingAttributes.Width = HighlighterSize;
                main.DrawingCanvas.DefaultDrawingAttributes.Height = HighlighterSize;

                main.drawingSetting.HighlighterSize = (int)HighlighterSize;
            }
        }
    }
}
