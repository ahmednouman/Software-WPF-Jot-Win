using JotWin.ViewModel.Helpers;
using JotWin.ViewModel.Helpers.MajicJot;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JotWin.View
{
    public partial class PenMenu : Window
    {
        MainAppWindow parentWindow;

        public PenMenu(MainAppWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            MajicHelpers.SetMainWindow(parentWindow);
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

        private void penMenu_LostFocus(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Deactivate(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
