using JotWin.View;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JotWin.ViewModel.Helpers
{
    public class HandleWindows
    {

        public void MoveWindowToCursorScreen(MainAppWindow window)
        {
            System.Drawing.Point screenMousePosition = Cursor.Position;

            Screen wpfScreen = Screen.FromPoint(screenMousePosition);

            IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            NativeMethods.SetWindowPos(windowHandle, IntPtr.Zero, wpfScreen.Bounds.Left, wpfScreen.Bounds.Top, 0, -1000, SWP.NOMOVE | SWP.NOSIZE | SWP.SHOWWINDOW);
        }


        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        }

        internal static class SWP
        {
            internal const int NOMOVE = 0x0002;
            internal const int NOSIZE = 0x0001;
            internal const int SHOWWINDOW = 0x0040;
        }

    }

}
