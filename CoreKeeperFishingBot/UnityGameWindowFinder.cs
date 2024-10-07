using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreKeeperFishingBot
{
    internal class UnityGameWindowFinder
    {
        private IntPtr bestHandle = IntPtr.Zero;
        private int processId;

        public IntPtr FindMainWindow(int processId)
        {
            this.processId = processId;

            NativeMethods.EnumThreadWindowsCallback enumThreadWindowsCallback = EnumWindowsCallback;
            NativeMethods.EnumWindows(enumThreadWindowsCallback, IntPtr.Zero);
            GC.KeepAlive(enumThreadWindowsCallback);

            return bestHandle;
        }

        private bool IsMainWindow(IntPtr handle)
        {
            NativeMethods.Rect rect = new NativeMethods.Rect();
            NativeMethods.GetWindowRect(handle, out rect);

            StringBuilder windowTitleBuilder = new StringBuilder(256);
            NativeMethods.GetWindowText(handle, windowTitleBuilder, windowTitleBuilder.Capacity);
            string windowTitle = windowTitleBuilder.ToString();

            StringBuilder windowClassBuilder = new StringBuilder(256);
            NativeMethods.GetClassName(handle, windowClassBuilder, windowTitleBuilder.Capacity);
            string windowClass = windowClassBuilder.ToString();

            if (rect.Right - rect.Left == 0 || rect.Bottom - rect.Top == 0)
                return false;

            if (NativeMethods.GetWindow(handle, 4) != IntPtr.Zero || !NativeMethods.IsWindowVisible(handle))
                return false;

            return true;
        }

        private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            NativeMethods.GetWindowThreadProcessId(handle, out var num);
            if (num == processId && IsMainWindow(handle))
            {
                bestHandle = handle;
                return false;
            }

            return true;
        }
    }
}
