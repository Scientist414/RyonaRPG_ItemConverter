using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices; // DLL Import

namespace RyonaRPG_ItemDataConverter
{
    class ClipboardManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        internal static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("Kernel32.dll")]
        internal static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("Kernel32.dll")]
        internal static extern int GlobalSize(IntPtr hMem);
    }
}
