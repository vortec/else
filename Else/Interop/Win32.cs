

using System;
using System.Runtime.InteropServices;

namespace Else.Interop
{
    /// <summary>
    /// Win32 API calls
    /// </summary>
    public static class Win32
    {
        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">Handle to the window that will receive WM_HOTKEY messages.</param>
        /// <param name="id">The identifier of the hotkey.</param>
        /// <param name="fsModifiers">The keys that must be pressed in combination with the key specified by the virtualKey.</param>
        /// <param name="vk">The vk.</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646309%28v=vs.85%29.aspx"/>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.The vk.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">A handle to the window associated with the hot key to be freed.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646327(v=vs.85).aspx"/>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.The vk.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);



    }
}