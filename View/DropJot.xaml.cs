using JotWin.ViewModel.Helpers.MajicJot;
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

namespace JotWin.View
{
    /// <summary>
    /// Interaction logic for DropJot.xaml
    /// </summary>
    public partial class DropJot : Window
    {
        public MainAppWindow mainWindow;
        public bool cursorTriggered = false;

        public bool pasteTriggered = false;
        public DropJot(MainAppWindow mainWin)
        {
            InitializeComponent();

            mainWindow = mainWin;

            Topmost = true;
            Left = 0;
            Top = 0;

            Hide();
        }

        private void textBoxPos(Point stylusPos)
        {
            if (!cursorTriggered)
            {
                dropJotText.Visibility = Visibility.Collapsed;
                dropJotText.VerticalAlignment = VerticalAlignment.Top;
                dropJotText.HorizontalAlignment = HorizontalAlignment.Left;
                dropJotText.Margin = new Thickness(0, 0, 0, 0);
                cursorTriggered = true;
            }
            else
            {
                
                dropJotText.Margin = new Thickness(stylusPos.X, stylusPos.Y, 0, 0);
                dropJotText.Visibility = Visibility.Visible;
            }
        }


        private void Grid_PreviewStylusInAirMove(object sender, StylusEventArgs e)
        {
            Point stylusPosition = e.GetPosition(this);
            textBoxPos(stylusPosition);
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point stylusPosition = e.GetPosition(this);
            textBoxPos(stylusPosition);
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!pasteTriggered)
                MajicHelpers.pasteAndRemove();
        }

        private void Grid_PreviewStylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            if (!pasteTriggered)
                MajicHelpers.pasteAndRemove();
        }

        private void Grid_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (!pasteTriggered)
                MajicHelpers.pasteAndRemove();
        }
    }
}
