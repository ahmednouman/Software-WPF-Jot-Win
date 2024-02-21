using JotWin.ViewModel.Helpers;
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
    /// Interaction logic for LaunchMenu.xaml
    /// </summary>
    public partial class LaunchMenu : Window
    {
        public MainAppWindow parentWindow;
        public LaunchMenu(MainAppWindow mainWin)
        {
            InitializeComponent();

            parentWindow = mainWin;

            this.StateChanged += LaunchMenu_StateChanged;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            System.Windows.Forms.SendKeys.SendWait("^(c)");
        }

        private void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            MajicHelpers.RestoreOrBringToTop();
            MajicHelpers.moveMainWindowToClickedMonitor();
            ExtenalMonitorInfo.resizeAppWindow(parentWindow);
            parentWindow.mainTabsVM.AddTab();

            parentWindow.activate_screen_shot_window();
        }

        private void Blank_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            MajicHelpers.RestoreOrBringToTop();
            MajicHelpers.moveMainWindowToClickedMonitor();
            ExtenalMonitorInfo.resizeAppWindow(parentWindow);
            parentWindow.mainTabsVM.AddTab();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            System.Windows.Forms.SendKeys.SendWait("^(v)");
        }

        private void Magic_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            MajicHelpers.handleMajicClick();
        }

        private void penMenuBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image image)
            {
                double width = image.ActualWidth;
                double height = image.ActualHeight;

                image.Width = width + 5;
                image.Height = height + 5;
            }
        }

        private void penMenuBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Image image)
            {
                double width = image.ActualWidth;
                double height = image.ActualHeight;

                image.Width = width - 5;
                image.Height = height - 5;
            }
        }

        private void LaunchMenu_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                
            }
            else if (WindowState == WindowState.Maximized)
            {
                
            }
            else if (WindowState == WindowState.Normal)
            {
                MajicHelpers.updateMajicCoordinates();
                //parentWindow.triggerPenMenu(true);
            }
        }

        private void penMenu_LostFocus(object sender, RoutedEventArgs e)
        {
            //Hide();
        }

        private void Deactivate(object sender, EventArgs e)
        {
           // Hide();
        }
    }
}

