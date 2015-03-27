using System;
using System.IO;
using System.Reflection;


namespace Else.Lib
{
    /// <summary>
    /// Handles resource paths for the entire app.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// The users custom data path (%appdata%\Else)
        /// </summary>
        public static string UserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().GetName().Name);

        /// <summary>
        /// The directory that contains the application exe (c:\program files\Else)
        /// </summary>
        public static string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Ensures the user directory and sub directories exist, otherwise creates them
        /// </summary>
        public static void CreateUserDirectories()
        {
            Directory.CreateDirectory(Path.Combine(UserDirectory, "Plugins"));
            Directory.CreateDirectory(Path.Combine(UserDirectory, "Themes"));
        }

        public static string GetUserPath(string path)
        {
            return Path.Combine(UserDirectory, path);
        }
        public static string GetAppPath(string path)
        {
            return Path.Combine(AppDirectory, path);
        }
    }
}
