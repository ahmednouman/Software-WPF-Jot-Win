using JotWin.View;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using JotWin.ViewModel.Helpers.MajicJot;
using System.Threading.Tasks;

namespace JotWin.ViewModel.Helpers
{
    public static class WindowsLowLevelHelpers
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LEFT = 0x25;
        private const int VK_RIGHT = 0x27;
        private const int VK_J = 0x4A;

        private static IntPtr hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc? keyboardProc;
        private static MainAppWindow? mainWindowInstance;

        public static void InitializeWindowHooks(MainAppWindow mainWindow)
        {
            mainWindowInstance = mainWindow;

            keyboardProc = HookCallback;
            hookId = SetHook(keyboardProc);
        }

        public static void UnhookWindowsHookEx()
        {
            UnhookWindowsHookEx(hookId);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule == null || curModule.ModuleName == null)
            {
                return IntPtr.Zero;
            }
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (mainWindowInstance == null)
            {
                throw new Exception("HookCallback mainWindowInstance == null");
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) &&
                    (vkCode == VK_LEFT || vkCode == VK_RIGHT))
                {
                    mainWindowInstance.triggerAdjustWinSize();
                    return IntPtr.Zero; 
                }
                else if (Keyboard.IsKeyDown(Key.LWin) && vkCode == VK_J)
                {
                    mainWindowInstance.triggerPenMenu();
                }
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public static void AdjustWindowSize(MainAppWindow mainWindow)
        {
            Screen currentScreen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);

            Matrix matrix = PresentationSource.FromVisual(mainWindow).CompositionTarget.TransformToDevice;
            double dpiScalingX = matrix.M11;

            double taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;
            double newHeight = (currentScreen.Bounds.Height - taskbarHeight) / dpiScalingX;

            mainWindow.Width = currentScreen.Bounds.Width / dpiScalingX;
            mainWindow.Height = newHeight;
            mainWindow.Left = currentScreen.Bounds.Left / dpiScalingX;
            mainWindow.Top = currentScreen.Bounds.Top / dpiScalingX;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class MouseHook
    {
        public MainAppWindow? mainWindowInstance;

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelMouseProc? _proc;
        public IntPtr _hookID = IntPtr.Zero;

        public void InitializeMouseHooks(MainAppWindow mainWindow)
        {
            mainWindowInstance = mainWindow;

            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule == null || curModule.ModuleName == null)
            {
                return IntPtr.Zero;
            }
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                var hook = Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT))
                           ?? throw new Exception("MSLLHOOKSTRUCT null");
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)hook;
                Point clickPoint = new(hookStruct.pt.x, hookStruct.pt.y);

                Task.Run(async () =>
                {
                    await Task.Delay(100);
                    if (!IsClickInsideWindow())
                    {
                        string? foregroundProcess = MajicHelpers.GetForegroundWindowProcessName();
                        mainWindowInstance?.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                Point clientPoint = mainWindowInstance.PointFromScreen(clickPoint);
                                bool clickInside = (clientPoint.X >= 0 && clientPoint.X < mainWindowInstance.ActualWidth &&
                                                   clientPoint.Y >= 0 && clientPoint.Y < mainWindowInstance.ActualHeight);
                                if (foregroundProcess != "JotWin" || !clickInside)
                                {
                                    MajicHelpers.foregroundProccessName = foregroundProcess;
                                    MajicHelpers.updateMajicCoordinates();
                                    //System.Windows.MessageBox.Show($"Global Mouse Left Button Down Event. Foreground Process: {foregroundProcess}");
                                }
                            }
                            catch (InvalidOperationException e) {
                                Debug.WriteLine(e);
                            }
                        });
                    }
                });
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool IsClickInsideWindow()
        {
            return MajicHelpers.GetForegroundWindowProcessName() == "JotWin";
        }

        #region Native methods

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }
}
