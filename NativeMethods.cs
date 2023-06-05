using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SharpDX9App {
    public static class NativeMethods {

        [StructLayout(LayoutKind.Sequential)]
        public struct Message {
            public IntPtr hWnd;
            public int msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint mesageFilterMin, uint messageFilterMax, uint flags);

        [DllImport("kernel32.dll")]
        public static extern UInt32 GetTickCount();
    }
}
