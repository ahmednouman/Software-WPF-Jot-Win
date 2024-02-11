using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using JotWin.View;
using System.Threading;

namespace JotWin.ViewModel.Helpers.MajicJot
{
    public static class MajicHelpers
    {
#pragma warning disable CA2211
        public static MainAppWindow? mainWindow;

        public static int lastXClick;
        public static int lastYClick;

        public static int lastXClickRelative;
        public static int lastYClickRelative;

        public static string? foregroundProccessName;
        public static string? clickedMonitor;
#pragma warning restore CA2211

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int SWP_NOSIZE = 0x0001;
        //private const int SWP_NOMOVE = 0x0002;
        private const int SWP_SHOWWINDOW = 0x0040;

        public static void SetMainWindow(MainAppWindow mainWindow)
        {
            MajicHelpers.mainWindow = mainWindow;
        }

        public static void updateMajicCoordinates()
        {
            ExtenalMonitorInfo.LoadData();
            lastXClick = ExtenalMonitorInfo.CursorX;
            lastYClick = ExtenalMonitorInfo.CursorY;

            lastXClickRelative = ExtenalMonitorInfo.monitorInfo.cursorRelative.Xcoordinates;
            lastYClickRelative = ExtenalMonitorInfo.monitorInfo.cursorRelative.Ycoordinates;

            clickedMonitor = FindMonitorByCoordinates();
           // moveMainWindowToClickedMonitor();
        }

        public static string? FindMonitorByCoordinates()
        {
            foreach (var displayInfo in ExtenalMonitorInfo.DisplayInfos)
            {
                if (IsPointInsideRectangle(lastXClick, lastYClick, displayInfo.Resolution))
                {
                    return displayInfo.DeviceName;
                }
            }

            return null;
        }

        private static bool IsPointInsideRectangle(int x, int y, Resolution resolution)
        {
            return x >= resolution.X && x < resolution.X + resolution.Width
                && y >= resolution.Y && y < resolution.Y + resolution.Height;
        }

        public static string? GetForegroundWindowProcessName()
        {
            IntPtr hWnd = GetForegroundWindow();

            if (GetWindowThreadProcessId(hWnd, out uint processId) == 0)
            {
                Debug.WriteLine("GetForegroundWindowProcessName failed to get thread id");
                return null;
            }

            Process process = Process.GetProcessById((int)processId);

            return process.ProcessName;
        }

        private static void MoveWindowToMonitor(Window window)
        {
            if (clickedMonitor == null)
            {
                Debug.WriteLine("MoveWindowToMonitor clickedMonitor is null");
                return;
            }

            try
            {
                Screen targetScreen = Screen.AllScreens.FirstOrDefault(screen => screen.DeviceName.Contains(clickedMonitor, StringComparison.OrdinalIgnoreCase))
                                      ?? throw new Exception("MoveWindowToMonitor null screen");

                var hwnd = new WindowInteropHelper(window).Handle;

                SetWindowPos(hwnd, IntPtr.Zero, targetScreen.Bounds.Left, targetScreen.Bounds.Top, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"MoveWindowToMonitor {e}");
            }
        }

        public static void moveMainWindowToClickedMonitor()
        {
            if (mainWindow == null)
            {
                Debug.WriteLine("moveMainWindowToClickedMonitor mainWindow is null");
                return;
            }
            MoveWindowToMonitor(mainWindow); 
        }

        public static void RestoreOrBringToTop()
        {
            if (mainWindow == null)
            {
                Debug.WriteLine("RestoreOrBringToTop mainWindow is null");
                return;
            }

            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
            }
        }

        private static int GetScreenIndex()
        {
            if (clickedMonitor == null)
            {
                Debug.WriteLine("GetScreenIndex clickedMonitor is null");
                return -1;
            }

            Screen[] screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i].DeviceName.Contains(clickedMonitor, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static Resolution? GetResolution()
        {
            if (clickedMonitor == null)
            {
                Debug.WriteLine("GetResolution clickedMonitor is null");
                return null;
            }

            DisplayInfo? displayInfo = ExtenalMonitorInfo.DisplayInfos.FirstOrDefault(
                info => info.DeviceName.Contains(clickedMonitor, StringComparison.OrdinalIgnoreCase));

            return displayInfo?.Resolution;
        }

        public static Resolution? GetPrimaryDisplayResolution()
        {
            for(int i = 0; i < ExtenalMonitorInfo.monitorInfo.DisplayCount; i++)
            {
                if (ExtenalMonitorInfo.monitorInfo.Displays[i].isPrimary)
                {
                    return ExtenalMonitorInfo.monitorInfo.Displays[i].Resolution;
                }
            }
            return null;
        }


        public static void moveAndTriggerCursor()
        {
            int screenIndex = GetScreenIndex();
            Resolution? displayResolution = GetResolution();
            Resolution? primaryDisplayResolutionScaled = GetPrimaryDisplayResolution();
            Screen? primaryScreen = Screen.AllScreens.FirstOrDefault(screen => screen.Primary);
       
            if (screenIndex == -1 ||
                displayResolution == null ||
                primaryDisplayResolutionScaled == null ||
                primaryScreen == null)
            {
                Debug.WriteLine("moveAndTriggerCursor null index/res/screen");
                return;
            }

            double scale_factor = primaryScreen.Bounds.Width / primaryDisplayResolutionScaled.Width;

            Screen screen = Screen.AllScreens[screenIndex];

            int absoluteX =  (int)((displayResolution.X + lastXClickRelative) * scale_factor);
            int absoluteY = (int)((displayResolution.Y + lastYClickRelative) * scale_factor);

            SetCursorPos(absoluteX, absoluteY);


            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void handleMajicClick()
        {
            if (mainWindow == null)
            {
                Debug.WriteLine("handleMajicClick mainWindow is null");
                return;
            }

            RestoreOrBringToTop();
            moveMainWindowToClickedMonitor();
            ExtenalMonitorInfo.resizeAppWindow(mainWindow);
            mainWindow.mainTabsVM.AddTab();
            mainWindow.canvasTemplate = CanvasTemplate.Tracing;
            mainWindow.tabDataList[mainWindow.mainTabsVM.SelectedTab].canvasBackground = CanvasTemplate.Tracing;
            mainWindow.tabDataList[mainWindow.mainTabsVM.SelectedTab].isForMajicJot = true;
        }

        public static void finishMajicJot()
        {
            if (mainWindow == null)
            {
                Debug.WriteLine("finishMajicJot mainWindow is null");
                return;
            }

            SystemFileControl.CopyCanvasToClipboard(mainWindow.DrawingCanvas);
            mainWindow.WindowState = WindowState.Minimized;

            updateMajicCoordinates();

            moveAndTriggerCursor();

            SendKeys.SendWait("^(v)");
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
