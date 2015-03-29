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

            UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().GetName().Name);

            // ensure we have AppDataDirectory
            if (!Directory.Exists(AppDataDirectory)) {
                throw new FileNotFoundException(string.Format("Failed to find App Data directory (expected: {0})", AppDataDirectory));
            }
            
            // create UserData directories if they do not exist
            CreateUserDataDirectories();

            Debug.Print("App Data Directory = {0}", AppDataDirectory);
            Debug.Print("User Data Directory = {0}", UserDataDirectory);
            
        }
        /// <summary>
        /// Ensures the user directory and sub directories exist, otherwise creates them
        /// </summary>
        private void CreateUserDataDirectories()
        {
            Directory.CreateDirectory(Path.Combine(UserDataDirectory, "Plugins"));
            Directory.CreateDirectory(Path.Combine(UserDataDirectory, "Themes"));
        }

        public string GetUserPath(string path)
        {
            return Path.Combine(UserDataDirectory, path);
        }
        public string GetAppPath(string path)
        {
            return Path.Combine(AppDataDirectory, path);
        }
    }
}
