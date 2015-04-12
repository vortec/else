using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Else.Services
{
    /// <summary>
    /// Handles resource paths for the entire app.
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// The user data directory (e.g. %appdata%\Else)
        /// </summary>
        public string UserDataDirectory;

        /// <summary>
        /// Our app data directory
        /// </summary>
        public string AppDataDirectory;

        /// <summary>
        /// We were able to find paths
        /// </summary>
        public bool PathsOk;

        public void Setup()
        {
            // try and find our app data directory
            if (!Debugger.IsAttached && ApplicationDeployment.IsNetworkDeployed) {
                // is deployed via click-once
                AppDataDirectory = Application.UserAppDataPath;
            }
            else {
                // otherwise use the same directory as the .exe
                AppDataDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            //UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().GetName().Name);
            UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetExecutingAssembly().GetName().Name);

            // ensure we have AppDataDirectory
            if (!Directory.Exists(AppDataDirectory)) {
                throw new FileNotFoundException(string.Format("Failed to find App Data directory (expected: {0})", AppDataDirectory));
            }

            // create default user directories (more to check that we can create them)
            GetUserPath("Plugins");
            GetUserPath("Themes");

            PathsOk = true;
        }

        public string GetUserPath(string path="")
        {
            path = Path.Combine(UserDataDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
        public string GetAppPath(string path="")
        {
            return Path.Combine(AppDataDirectory, path);
        }
    }
}
