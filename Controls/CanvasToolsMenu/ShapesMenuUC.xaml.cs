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
    /// Interaction logic for ShapesMenuUC.xaml
    /// </summary>
    public partial class ShapesMenuUC : UserControl
    {
        public ShapesMenuUC()
        {
            InitializeComponent();
        }

        private void rectShape_Click(object sender, MouseButtonEventArgs e)
        {
            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            main.drawingSetting.selectedShape = Model.DrawingShape.Rectangle;
            UpdateSelectedShapeButton();
        }

        private void ellipseShape_Click(object sender, MouseButtonEventArgs e)
        {
            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            main.drawingSetting.selectedShape = Model.DrawingShape.Ellipse;
            UpdateSelectedShapeButton();
        }

        public void UpdateSelectedShapeButton()
        {
            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            var sel_shape = main.drawingSetting.selectedShape;

            bool IsRectangleSelected = sel_shape == Model.DrawingShape.Rectangle;
            bool IsEllipseSelected = sel_shape == Model.DrawingShape.Ellipse;

            Color fillColor = (Color)ColorConverter.ConvertFromString("#E1E1E1");

            rectangleShape.Fill = new SolidColorBrush(IsRectangleSelected ? fillColor : Colors.Transparent);
            ellipseShape.Fill = new SolidColorBrush(IsEllipseSelected ? fillColor : Colors.Transparent);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            main.drawingSetting.isShapeFill = true;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MainAppWindow? main = Window.GetWindow(this) as MainAppWindow;
            main.drawingSetting.isShapeFill = false;
        }
    }
}
