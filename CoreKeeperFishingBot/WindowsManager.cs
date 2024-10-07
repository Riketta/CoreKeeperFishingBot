using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreKeeperFishingBot
{
    internal class WindowsManager
    {
        static public Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = NativeMethods.GetWindowDC(hwnd);
            uint pixel = NativeMethods.GetPixel(hdc, x, y);
            NativeMethods.ReleaseDC(hwnd, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        public static void RightMouseClickSendMessage(IntPtr window)
        {
            NativeMethods.SendMessage(window, NativeMethods.WM_RBUTTONDOWN, (uint)NativeMethods.VirtualKeys.RightButton, IntPtr.Zero);
            Thread.Sleep(50);
            NativeMethods.SendMessage(window, NativeMethods.WM_RBUTTONUP, (uint)NativeMethods.VirtualKeys.RightButton, IntPtr.Zero);
        }

        public static void RightMouseClickMouseEvent(IntPtr window)
        {
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static Point GetMousePosition()
        {
            return System.Windows.Forms.Cursor.Position;
        }

        public static void MoveMouseTo(Point position)
        {
            System.Windows.Forms.Cursor.Position = position;
        }

        public static void SetWindowActive(IntPtr handle)
        {
            NativeMethods.SetForegroundWindow(handle);
            NativeMethods.ShowWindowAsync(handle, 9); // SW_RESTORE = 9
        }

        public static NativeMethods.Rect GetMonitorSize(IntPtr handle)
        {
            var monitor = NativeMethods.MonitorFromWindow(handle, NativeMethods.MONITOR_DEFAULTTONEAREST);

            NativeMethods.Rect monitorBounds = new NativeMethods.Rect();

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new NativeMethods.MonitorInfo();
                NativeMethods.GetMonitorInfo(monitor, monitorInfo);

                monitorBounds = monitorInfo.Monitor;
            }

            return monitorBounds;
        }
    }
}
