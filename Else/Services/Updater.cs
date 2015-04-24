using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Autofac.Extras.NLog;
using Else.Interop;
using Else.Views;
using Squirrel;
using Timer = System.Timers.Timer;

namespace Else.Services
{
    /// <summary>
    /// Manages auto update and restart of the app.
    /// </summary>
    public class Updater : IDisposable
    {
        /// <summary>
        /// The application name (as defined in the nuget package)
        /// </summary>
        private const string AppName = "Else";
        /// <summary>
        /// URL of the installer directory.
        /// </summary>
        private const string UpdateUrl = @"http://otp.me.uk/~james/Else/Installer";

        private readonly ILogger _logger;
        public UpdateManager UpdateManager;

        /// <summary>
        /// To prevent simultaenous updates.
        /// </summary>
        private readonly object _updateLock = new object();

        /// <summary>
        /// An update has been applied and we are waiting for restart.
        /// </summary>
        public bool RestartPending;
        
        /// <summary>
        /// The minimum UI idle time before restart.
        /// </summary>
        private const int MinimumUserIdleBeforeRestart = 30;

        /// <summary>
        /// Timer used for auto restarting the app.
        /// </summary>
        private Timer _restartTimer;

        private static DateTime _lastActivity;

        public Updater(ILogger logger)
        {
            _logger = logger;
            UpdateManager = new UpdateManager(UpdateUrl, AppName, FrameworkVersion.Net45);
        }

        public void Dispose()
        {
            // take extra fuckign special care to dispose the squirrel UpdateManager, else we get exceptions on process exit.....
            if (UpdateManager != null) {
                UpdateManager.Dispose();
                UpdateManager = null;
            }
        }

        /// <summary>
        /// Handles squirrel events.
        /// <remarks>This method should be called early in app startup</remarks>
        /// </summary>
        public void HandleEvents()
        {
            SquirrelAwareApp.HandleEvents(
                onAppUpdate: version =>
                {
                    UpdateManager.CreateShortcutForThisExe();
                    // update uninstall entry (because version changes), untested.
                    UpdateManager.RemoveUninstallerRegistryEntry();
                    UpdateManager.CreateUninstallerRegistryEntry();
                },
                onInitialInstall: version => UpdateManager.CreateShortcutForThisExe(),
                onAppUninstall: version =>
                {
                    UpdateManager.RemoveShortcutForThisExe();
                    // kill app if currently running
                    Win32.KillCurrentlyRunning();
                }
            );
            //onFirstRun: () => { });
        }

        /// <summary>
        /// Check if an update is available and install it.
        /// If update is successfully installed, schedule an app restart in the future.
        /// </summary>
        public async void UpdateApp()
        {
            // lock, otherwise multiple updates could happen
            if (Monitor.TryEnter(_updateLock)) {
                _logger.Debug("Checking for updates");
                
                // check for updates
                var updateInfo = await UpdateManager.CheckForUpdate();
                if (updateInfo != null && updateInfo.ReleasesToApply.Any()) {
                    // update available, get the most recent
                    var latest = updateInfo.ReleasesToApply.OrderByDescending(x => x.Version).First().Version;

                    Debug.Print("found update: {0}", latest);
                    Debug.Print("downloading..");

                    // download it
                    await UpdateManager.DownloadReleases(updateInfo.ReleasesToApply);
                    Debug.Print("finished downloading, installing..");

                    // install it
                    var path = await UpdateManager.ApplyReleases(updateInfo);
                    Debug.Print("update installed ({0}), restarting", path);

                    // update is installed, we need to restart in the future to apply the update.
                    RestartPending = true;
                    // begin timer that will check for good opportunity to restart
                    _restartTimer = new Timer(5000);
                    _restartTimer.Elapsed += AttemptSilentRestart;
                    _restartTimer.Start();
                }
                else {
                    // no updates available
                    _logger.Debug("No updates available");
                }
                // release lock
                Monitor.Exit(_updateLock);
            }
            else {
                _logger.Debug("Failed to acquire updateLock");
            }
        }

        /// <summary>
        /// When UI is interacted with (e.g. launcher is used, or settings dialog is used), this method is called
        /// We store this so we can silently update when the app is not being actively used by the user.
        /// </summary>
        public static void OnUserActivity()
        {
            _lastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Attempts the silent restart.
        /// Will only restart if 2 conditions are met (no UI windows are open, and there has been no UI activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void AttemptSilentRestart(object sender, ElapsedEventArgs e)
        {
            if (!RestartPending) {
                _restartTimer.Stop();
                return;
            }

            if (RestartPending) {
                // minimum UI idle before restart..
                var delta = DateTime.Now - _lastActivity;
                if (delta.TotalSeconds < MinimumUserIdleBeforeRestart) {
                    return;
                }
                // ensure no windows open before restart..
                bool windowsAreOpen = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var windows = Application.Current.Windows.OfType<Window>().Where(w => w.Visibility == Visibility.Visible);
                    if (windows.Any()) {
                        windowsAreOpen = true;
                    }
                });
                if (windowsAreOpen) {
                    return; 
                }

                // otherwise, all good to restart
                RestartApp();
            }
        }

        /// <summary>
        /// Restarts the application (used during in-app update)
        /// </summary>
        /// <exception cref="System.Exception">update.exe not found, not a squirrel installed app?</exception>
        public void RestartApp()
        {
            var exeToStart = Path.GetFileName(Assembly.GetEntryAssembly().Location);

            // determine path to update.exe
            var assembly = Assembly.GetExecutingAssembly();
            var updateDotExe = Path.Combine(Path.GetDirectoryName(assembly.Location), "..\\Update.exe");
            var target = new FileInfo(updateDotExe);
            if (!target.Exists) {
                throw new Exception("update.exe not found, not a squirrel installed app?");
            }

            // start update.exe, it will wait for our process to exit.
            Process.Start(updateDotExe, $"--processStartAndWait {exeToStart}");

            // NB: We have to give update.exe some time to grab our PID, but
            // we can't use WaitForInputIdle because we probably don't have
            // whatever WaitForInputIdle considers a message loop.
            // edit: we probably can
            Thread.Sleep(500);
            Application.Current.Shutdown();
        }


        
    }
}