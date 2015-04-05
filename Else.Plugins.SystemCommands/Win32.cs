using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Else.Plugins.SystemCommands
{
    public static class Win32
    {
        /// <summary>
        /// Logs off the interactive user, shuts down the system, or shuts down and restarts the system.
        /// </summary>
        /// <param name="uFlags">The shutdown type.</param>
        /// <param name="dwReason">The reason for initiating the shutdown.</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa376868%28v=vs.85%29.aspx"/>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        /// <summary>
        /// Locks the work station.
        /// </summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa376875%28v=vs.85%29.aspx"/>
        [DllImport("user32")]
        public static extern void LockWorkStation();

        /// <summary>
        /// Sets the state of the suspend.
        /// </summary>
        /// <param name="hiberate">If this parameter is TRUE, the system hibernates. If the parameter is FALSE, the system is suspended.</param>
        /// <param name="forceCritical">This parameter has no effect.</param>
        /// <param name="disableWakeEvent">If this parameter is TRUE, the system disables all wake events. If the parameter is FALSE, any system wake events remain enabled.</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/aa373201%28v=vs.85%29.aspx"/>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
    }
}
