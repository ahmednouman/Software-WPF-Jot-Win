using JotWin.View;
using JotWin.ViewModel.Helpers;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows;

namespace JotWin
{
    public partial class App : Application
    {
        public static string version = "1.2.2";

        public static string savedTabsDB = Path.Combine(savedCanvasDirectory, "JotTabsDB.db");

        private NamedPipeServer namedPipeServer;
        public static string savedCanvasDirectory
        {
            get
            {
                string folderPath = Path.Combine(Environment.CurrentDirectory, "savedCanvas");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            if (e.Args.Length > 0)
            {
                string argument = e.Args[0];
                HandleJumpTaskClick(argument);
            }
            else
            {
                MainAppWindow mainWindow = Current.Windows.OfType<MainAppWindow>().FirstOrDefault();

                namedPipeServer = new NamedPipeServer(mainWindow);
                namedPipeServer.StartServer("JmpListPipe");
            }
        }

        private static void HandleJumpTaskClick(string argument)
        {
            switch (argument)
            {
                case "/openMagic":
                    SendMessageToExistingInstance("Magic-Jot-opened");
                    break;
                case "/openScreenshot":
                    SendMessageToExistingInstance("Screen-Jot-opened");
                    break;
                case "/openBlankJot":
                    SendMessageToExistingInstance("Blank-Jot-opened");
                    break;
                default:
                    //MessageBox.Show($"Unknown action clicked: {argument}");
                    break;
            }
            Current.Shutdown();
        }

        private static void SendMessageToExistingInstance(string message)
        {
            try
            {
                using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", "JmpListPipe", PipeDirection.Out))
                {
                    clientStream.Connect();
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    clientStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

    }
}
