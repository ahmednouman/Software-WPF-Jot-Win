using System;
using System.Windows;
using System.Windows.Media;
using System.IO.Pipes;
using System.Text;
using System.IO;
using JotWin.ViewModel.Helpers.MajicJot;
using JotWin.View;

namespace JotWin.ViewModel.Helpers
{
    public class GenericHelpers
    {
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null && !(obj is T))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            return obj as T;
        }
    }


    public class NamedPipeServer
    {
        public MainAppWindow mainWindow;
        private NamedPipeServerStream serverStream;

        private string _pipeName;

        public NamedPipeServer(MainAppWindow mainWin)
        {
            mainWindow = mainWin;
        }

        public void StartServer(string pipeName)
        {
            serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            serverStream.BeginWaitForConnection(ConnectionCallback, null);

            _pipeName = pipeName;
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            try
            {
                serverStream.EndWaitForConnection(ar);

                byte[] buffer = new byte[1024];
                int bytesRead = serverStream.Read(buffer, 0, buffer.Length);

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainAppWindow mainWindow = ExtenalMonitorInfo.GetMainWindowInstance();
                    switch (message)
                    {
                        case "Magic-Jot-opened":
                            MajicHelpers.handleMajicClick();
                            break;
                        case "Screen-Jot-opened":
                            MajicHelpers.RestoreOrBringToTop();
                            MajicHelpers.moveMainWindowToClickedMonitor();
                            ExtenalMonitorInfo.resizeAppWindow(mainWindow);
                            mainWindow.mainTabsVM.AddTab();
                            mainWindow.activate_screen_shot_window();
                            break;
                        case "Blank-Jot-opened":
                            MajicHelpers.RestoreOrBringToTop();
                            MajicHelpers.moveMainWindowToClickedMonitor();
                            ExtenalMonitorInfo.resizeAppWindow(mainWindow);
                            mainWindow.mainTabsVM.AddTab();
                            break;
                        default:
                            //MessageBox.Show($"Unknown action clicked: {argument}");
                            break;
                    }
                });
            }
            catch (IOException ex)
            {
                MessageBox.Show($"IOException: {ex.Message}");
            }
            finally
            {
                serverStream.Close();
                serverStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                serverStream.BeginWaitForConnection(ConnectionCallback, null);
            }
        }

    }


}
