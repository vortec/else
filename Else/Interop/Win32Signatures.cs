using System;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Else.Interop
{
    public static class Win32Signatures
    {
        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">Handle to the window that will receive WM_HOTKEY messages.</param>
        /// <param name="id">The identifier of the hotkey.</param>
        /// <param name="fsModifiers">The keys that must be pressed in combination with the key specified by the virtualKey.</param>
        /// <param name="vk">The vk.</param>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646309%28v=vs.85%29.aspx"/>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.The vk.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">A handle to the window associated with the hot key to be freed.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646327(v=vs.85).aspx"/>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.The vk.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, Win32.EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
    }
}
