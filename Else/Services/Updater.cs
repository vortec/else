using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using Autofac.Extras.NLog;
using Else.Interop;
using Else.Views;
using Squirrel;

namespace Else.Services
{
    public class Updater : IDisposable
    {
        private const string UpdateUrl = @"http://otp.me.uk/~james/Else/Installer";
        private const string AppName = "Else";
        private readonly ILogger _logger;
        private readonly object _updateLock = new object();
        public UpdateManager UpdateManager;

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

                    // restart the app
                    RestartApp();
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