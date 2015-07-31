using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Autofac.Extras.NLog;
using Else.Helpers;
using Else.Interop;
using Else.Properties;
using Squirrel;
using Timer = System.Timers.Timer;

namespace Else.Services
{
    /// <summary>
    /// 
    /// Manages auto update and restart of the app.
    /// An initial update check is done 2 minutes after app start.
    /// If not successful, we check for for updates every 30 minutes, indefinately.
    /// 
    /// Once an update is installed, this class will monitor user activity for a good opportunity to restart 
    /// the app (e.g. windows all closed and no recent UI interaction)
    /// 
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

        /// <summary>
        /// the last known UI activity (updated by various app windows)
        /// </summary>
        private static DateTime _lastActivity;

        private readonly ILogger _logger;
        private readonly Settings _settings;

        /// <summary>
        /// The minimum UI idle time before restart
        /// </summary>
        private readonly TimeSpan _minimumUserIdleBeforeRestart = TimeSpan.FromSeconds(30);

        /// <summary>
        /// To prevent simultaenous updates.
        /// </summary>
        private readonly Semaphore _updateLock = new Semaphore(1, 1);

        /// <summary>
        /// The initial update delay
        /// </summary>
        private TimeSpan _initialUpdateDelay = TimeSpan.FromMinutes(1);

        /// <summary>
        /// The repeating update delay
        /// </summary>
        private TimeSpan _repeatingUpdateDelay = TimeSpan.FromMinutes(30);

        private TimeSpan _restartAttemptDelay = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Timer used for auto restarting the app.
        /// </summary>
        private Timer _restartTimer;

        /// <summary>
        /// Timer used to trigger update checks.
        /// </summary>
        private Timer _updateTimer;

        /// <summary>
        /// An update has been applied and we are waiting for restart.
        /// </summary>
        public bool RestartPending;

        /// <summary>
        /// the Squirrel update manager.
        /// </summary>
        public UpdateManager UpdateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class and begins auto-update timer.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public Updater(ILogger logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
            UpdateManager = new UpdateManager(UpdateUrl, AppName);
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
        /// Begins the automatic update timer.
        /// </summary>
        public void BeginAutoUpdates()
        {
            _updateTimer = new Timer(_initialUpdateDelay.TotalMilliseconds) {AutoReset = false};
            _updateTimer.Elapsed += UpdateTimerElapsed;
            _updateTimer.Enabled = true;
        }

        private async void UpdateTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _updateTimer.Enabled = false;

            // attempt initial app update
            await UpdateApp();

            if (RestartPending) {
                // app was successfully updated
                return;
            }
            _updateTimer.Enabled = true;

            // start a new repeating timer
            if (!_updateTimer.AutoReset) {
                // timer is not repeating so this is the first check
                // so we increase timer interval and make it repeat
                _updateTimer.Interval = _repeatingUpdateDelay.TotalMilliseconds;
                _updateTimer.AutoReset = true;
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
        /// Check if an update is available and install it.
        /// If update is successfully installed, schedule an app restart in the future.
        /// </summary>
        public async Task UpdateApp()
        {
            if (!_settings.User.AutoUpdate) {
                // automatic updates are disabled.
                return;
            }
            // lock, otherwise multiple updates could happen
            if (_updateLock.WaitOne(200)) {
                try {
                    _logger.Debug("Checking for updates");

                    // check for updates
                    var updateInfo = await UpdateManager.CheckForUpdate();
                    if (updateInfo != null && updateInfo.ReleasesToApply.Any()) {
                        //var latest = updateInfo.ReleasesToApply.OrderByDescending(x => x.Version).First().Version;
                        _logger.Debug("found an update, downloading..");

                        // download it
                        await UpdateManager.DownloadReleases(updateInfo.ReleasesToApply);
                        _logger.Debug("finished downloading, installing..");

                        // install it
                        var path = await UpdateManager.ApplyReleases(updateInfo);
                        _logger.Debug("update installed {0}", path);

                        // update is installed, we need to restart in the future to apply the update.
                        RestartPending = true;
                        // begin timer that will check for good opportunity to restart
                        _restartTimer = new Timer(_restartAttemptDelay.TotalMilliseconds);
                        _restartTimer.Elapsed += AttemptSilentRestart;
                        _restartTimer.Enabled = true;
                        _restartTimer.AutoReset = true;
                    }
                    else {
                        // no updates available
                        _logger.Debug("No updates available");
                    }
                }
                catch (Exception e) {
                    _logger.Error("error occurred while downloading or installing update", e);
                }
                finally {
                    // release lock
                    _updateLock.Release();
                }
            }
            else {
                _logger.Debug("Failed to acquire updateLock");
            }
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
                _restartTimer.Enabled = false;
                return;
            }

            // don't restart if UI has been recently used
            var delta = DateTime.Now - _lastActivity;
            if (delta < _minimumUserIdleBeforeRestart) {
                return;
            }
            // don't restart if any windows are open
            var windowsAreOpen = false;
            UI.UiInvoke(() =>
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
            try {
                RestartApp();
            }
            catch (Exception exception) {
                _logger.Error("failed to restart app", exception);
                // we failed to restart the app, nothing more we can do
                // so we cancel the restart timer
                _restartTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Starts the updated instance of the app, and kills this one.  Should be smooth.
        /// </summary>
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
            Thread.Sleep(500);
            UI.UiInvoke((() => { Application.Current.Shutdown(); }));
        }
    }
}