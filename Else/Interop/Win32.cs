using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Else.Views;

// ReSharper disable InconsistentNaming

namespace Else.Interop
{
    /// <summary>
    /// Win32 API calls
    /// </summary>
    public class Win32 : Win32Signatures
    {
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int SWP_NOACTIVATE = 0x0010;
        private const uint WM_SETICON = 0x0080;

        /// <summary>
        /// Removes the window icon (the icon on the left of the window titlebar).
        /// <remarks>Has the side effect of removing the icon from alt-tab task switcher also</remarks>
        /// </summary>
        /// <param name="window">The window.</param>
        public static void RemoveWindowIcon(Window window)
        {
            // Get the window's handle
            var hwnd = new WindowInteropHelper(window).Handle;
            // Change the extended window style to not show a window icon
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE);
            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Enumerates the process window handles.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <returns></returns>
        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads) {
                EnumThreadWindows(thread.Id, (hWnd, lParam) =>
                {
                    handles.Add(hWnd);
                    return true;
                }, IntPtr.Zero);
            }

            return handles;
        }

        private const int WM_CLOSE = 0x0010;

        /// <summary>
        /// If there are 2 instances (processes) of this app running, kill the other one.
        /// </summary>
        public static void KillCurrentlyRunning()
        {
            // get current pid
            var assembly = Assembly.GetExecutingAssembly();
            var pid = Process.GetCurrentProcess().Id;

            // get all processes with this assembly name
            var processes = Process.GetProcessesByName(assembly.GetName().Name);

            for (var i = 0; i < processes.Count(); i++) {
                var process = processes[i];
                // exclude the current process..
                if (process.Id != pid) {
                    // get all window handles for the process
                    var handles = EnumerateProcessWindowHandles(process.Id);
                    
                    foreach (var handle in handles) {
                        try {
                            // try and find the "Else Launcher" window (main window that we can send messages to)
                            var length = GetWindowTextLength(handle);
                            var wtStr = new StringBuilder(length + 1);
                            GetWindowText(handle, wtStr, wtStr.Capacity);
                            if (wtStr.ToString() == LauncherWindow.WindowTitle) {
                                // we have found the window that we can send messages to..
                                // send WM_QUIT message
                                SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                // wait for the process to exit cleanly
                                var exited = process.WaitForExit(500);
                                // ugh, process didn't exit, we just kill it (bad because we will leave a tray icon)
                                if (!exited) {
                                    process.Kill();
                                    process.WaitForExit(500);
                                }
                            }

                        }
                        catch {
                            // not sure what went wrong really...
                            // lets hope there are no processes running
                        }
                    }
                }
            }
        }
    }
}