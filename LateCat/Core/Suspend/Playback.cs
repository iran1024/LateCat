﻿using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using LateCat.PoseidonEngine.Utilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace LateCat.Core
{
    public class Playback : IPlayback
    {
        private readonly string[] classWhiteList = new string[]
        {
            "Windows.UI.Core.CoreWindow",
            "MultitaskingViewFrame",
            "XamlExplorerHostIslandWindow",
            "WindowsDashboard",
            "Shell_TrayWnd",
            "Shell_SecondaryTrayWnd",
            "NotifyIconOverflowWindow",
            "RainmeterMeterWindow"
        };
        private IntPtr workerWOrig, progman;
        private PlaybackState _wallpaperPlayback;
        public PlaybackState WallpaperPlaybackState
        {
            get => _wallpaperPlayback;
            set
            {
                _wallpaperPlayback = value;
                PlaybackStateChanged?.Invoke(this, _wallpaperPlayback);
            }
        }
        public event EventHandler<PlaybackState> PlaybackStateChanged;

        private readonly DispatcherTimer _dispatcherTimer = new();
        private bool _isLockMonitor, _isRemoteSession;
        private bool _disposed;
        private int _pid = 0;

        private readonly ISettingsService _settings;
        private readonly IDesktopCore _desktopCore;

        public Playback(ISettingsService userSettings, IDesktopCore desktopCore)
        {
            _settings = userSettings;
            _desktopCore = desktopCore;

            Initialize();
            desktopCore.WallpaperReset += (s, e) => FindDesktopHandles();
        }

        private void FindDesktopHandles()
        {
            progman = Win32.FindWindow("Progman", string.Empty);

            var folderView = Win32.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);
            if (folderView == IntPtr.Zero)
            {
                do
                {
                    workerWOrig = Win32.FindWindowEx(Win32.GetDesktopWindow(), workerWOrig, "WorkerW", string.Empty);
                    folderView = Win32.FindWindowEx(workerWOrig, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);

                } while (folderView == IntPtr.Zero && workerWOrig != IntPtr.Zero);
            }
        }

        private void Initialize()
        {
            InitializeTimer();
            WallpaperPlaybackState = PlaybackState.Play;

            try
            {
                using Process process = Process.GetCurrentProcess();
                _pid = process.Id;
            }
            catch
            {

            }

            _isRemoteSession = System.Windows.Forms.SystemInformation.TerminalServerSession;

            if (_isRemoteSession)
            {

            }

            _isLockMonitor = IsSystemLocked();
            if (_isLockMonitor)
            {

            }
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void InitializeTimer()
        {
            _dispatcherTimer.Tick += new EventHandler(ProcessMonitor);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, _settings.Settings.ProcessTimerInterval);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.RemoteConnect)
            {
                _isRemoteSession = true;
            }
            else if (e.Reason == SessionSwitchReason.RemoteDisconnect)
            {
                _isRemoteSession = false;
            }
            else if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _isLockMonitor = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _isLockMonitor = false;
            }
        }

        public void Start()
        {
            _dispatcherTimer.Start();
        }

        public void Stop()
        {
            _dispatcherTimer.Stop();
        }

        private void ProcessMonitor(object? sender, EventArgs e)
        {
            if (WallpaperPlaybackState == PlaybackState.Paused || _isLockMonitor ||
                (_isRemoteSession && _settings.Settings.InRemoteDesktop == PerformanceStrategy.Pause))
            {
                PauseWallpapers();
            }
            else if (_settings.Settings.InBatteryMode == PerformanceStrategy.Pause &&
                BatteryChecker.GetACPowerStatus() == BatteryChecker.ACLineStatus.Offline)
            {
                PauseWallpapers();
            }
            else if (_settings.Settings.PowerSaveMode == PerformanceStrategy.Pause &&
                BatteryChecker.GetBatterySaverStatus() == BatteryChecker.SystemStatusFlag.On)
            {
                PauseWallpapers();
            }
            else
            {
                switch (_settings.Settings.ProcessMonitorAlgorithm)
                {
                    case ProcessMonitorAlgorithm.Foreground:
                        ForegroundAppMonitor();
                        break;
                    case ProcessMonitorAlgorithm.All:
                        break;
                    case ProcessMonitorAlgorithm.GameMode:
                        GameModeAppMonitor();
                        break;
                }
            }
        }

        private void GameModeAppMonitor()
        {
            if (Win32.SHQueryUserNotificationState(out Win32.QUERY_USER_NOTIFICATION_STATE state) == 0)
            {
                switch (state)
                {
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_NOT_PRESENT:
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_BUSY:
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_PRESENTATION_MODE:
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_ACCEPTS_NOTIFICATIONS:
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_QUIET_TIME:
                        break;
                    case Win32.QUERY_USER_NOTIFICATION_STATE.QUNS_RUNNING_D3D_FULL_SCREEN:
                        PauseWallpapers();
                        return;
                }
            }
            PlayWallpapers();
            SetWallpaperVolume(_settings.Settings.AudioVolumeGlobal);
        }

        private void ForegroundAppMonitor()
        {
            var isDesktop = false;
            var fHandle = Win32.GetForegroundWindow();

            if (IsWhitelistedClass(fHandle))
            {
                PlayWallpapers();
                SetWallpaperVolume(_settings.Settings.AudioVolumeGlobal);
                return;
            }

            try
            {
                _ = Win32.GetWindowThreadProcessId(fHandle, out int processID);
                using var fProcess = Process.GetProcessById(processID);

                if (string.IsNullOrEmpty(fProcess.ProcessName) || fHandle.Equals(IntPtr.Zero))
                {
                    PlayWallpapers();
                    return;
                }

                if (fProcess.Id == _pid || IsPlugin(fProcess.Id))
                {
                    PlayWallpapers();
                    SetWallpaperVolume(_settings.Settings.AudioVolumeGlobal);
                    return;
                }
            }
            catch
            {
                PlayWallpapers();
                return;
            }

            try
            {
                if (!(fHandle.Equals(Win32.GetDesktopWindow()) || fHandle.Equals(Win32.GetShellWindow())))
                {
                    if (!MonitorHelper.IsMultiMonitor() ||
                        _settings.Settings.MonitorPauseStrategy == MonitorPauseStrategy.All)
                    {
                        if (Equals(fHandle, workerWOrig) || Equals(fHandle, progman))
                        {
                            isDesktop = true;
                            PlayWallpapers();
                        }
                        else if (Win32.IsZoomed(fHandle) || IsZoomedCustom(fHandle))
                        {
                            if (_settings.Settings.OtherAppFullScreen == PerformanceStrategy.Keep)
                            {
                                PlayWallpapers();
                            }
                            else
                            {
                                PauseWallpapers();
                            }
                        }
                        else
                        {
                            if (_settings.Settings.OtherAppFocus == PerformanceStrategy.Pause)
                            {
                                PauseWallpapers();
                            }
                            else
                            {
                                PlayWallpapers();
                            }
                        }
                    }
                    else
                    {
                        PoseidonMonitor focusedMonitor;
                        if ((focusedMonitor = MapWindowToMonitor(fHandle)) != null)
                        {
                            foreach (var item in MonitorHelper.GetMonitor())
                            {
                                if (_settings.Settings.WallpaperArrangement != WallpaperArrangement.Span &&
                                    !MonitorHelper.Compare(item, focusedMonitor, MonitorIdentificationMode.DeviceId))
                                {
                                    PlayWallpaper(item);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }

                        if (Equals(fHandle, workerWOrig) || IntPtr.Equals(fHandle, progman))
                        {
                            isDesktop = true;
                            PlayWallpaper(focusedMonitor);
                        }
                        else if (_settings.Settings.WallpaperArrangement == WallpaperArrangement.Span)
                        {
                            if (IsZoomedSpan(fHandle))
                            {
                                PauseWallpapers();
                            }
                            else
                            {
                                if (_settings.Settings.OtherAppFocus == PerformanceStrategy.Pause)
                                {
                                    PauseWallpapers();
                                }
                                else
                                {
                                    PlayWallpapers();
                                }
                            }
                        }
                        else if (Win32.IsZoomed(fHandle) || IsZoomedCustom(fHandle))
                        {
                            if (_settings.Settings.OtherAppFullScreen == PerformanceStrategy.Keep)
                            {
                                PlayWallpaper(focusedMonitor);
                            }
                            else
                            {
                                PauseWallpaper(focusedMonitor);
                            }
                        }
                        else
                        {
                            if (_settings.Settings.OtherAppFocus == PerformanceStrategy.Pause)
                            {
                                PauseWallpaper(focusedMonitor);
                            }
                            else
                            {
                                PlayWallpaper(focusedMonitor);
                            }
                        }
                    }

                    if (isDesktop)
                    {
                        SetWallpaperVolume(_settings.Settings.AudioVolumeGlobal);
                    }
                    else
                    {
                        SetWallpaperVolume(_settings.Settings.AudioOnlyOnDesktop ? 0 : _settings.Settings.AudioVolumeGlobal);
                    }
                }
            }
            catch { }
        }

        private string GetClassName(IntPtr hwnd)
        {
            const int maxChars = 256;
            StringBuilder className = new StringBuilder(maxChars);
            return Win32.GetClassName((int)hwnd, className, maxChars) > 0 ? className.ToString() : string.Empty;
        }

        private bool IsWhitelistedClass(IntPtr hwnd)
        {
            const int maxChars = 256;
            var className = new StringBuilder(maxChars);
            return Win32.GetClassName((int)hwnd, className, maxChars) > 0 && classWhiteList.Any(x => x.Equals(className.ToString(), StringComparison.Ordinal));
        }

        private void PauseWallpapers()
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                x.Pause();
            }
        }

        private void PlayWallpapers()
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                x.Play();
            }
        }

        private void PauseWallpaper(IMonitor monitor)
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                if (x.Monitor.Equals(monitor))
                {
                    x.Pause();
                }
            }
        }

        private void PlayWallpaper(IMonitor monitor)
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                if (x.Monitor.Equals(monitor))
                {
                    x.Play();
                }
            }
        }

        private void SetWallpaperVolume(int volume)
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                x.SetVolume(volume);
            }
        }

        private void SetWallpaperVolume(int volume, IMonitor monitor)
        {
            foreach (var x in _desktopCore.Wallpapers)
            {
                if (x.Monitor.Equals(monitor))
                {
                    x.SetVolume(volume);
                }
            }
        }

        private bool IsPlugin(int pid)
        {
            return _desktopCore.Wallpapers.Any(x => x.Proc != null && x.Proc.Id == pid);
        }

        private static bool IsZoomedCustom(IntPtr hWnd)
        {
            try
            {
                System.Drawing.Rectangle MonitorBounds;
                _ = Win32.GetWindowRect(hWnd, out Win32.RECT appBounds);

                MonitorBounds = System.Windows.Forms.Screen.FromHandle(hWnd).Bounds;

                if ((appBounds.Bottom - appBounds.Top) >= MonitorBounds.Height * .95f && (appBounds.Right - appBounds.Left) >= MonitorBounds.Width * .95f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private PoseidonMonitor MapWindowToMonitor(IntPtr handle)
        {
            try
            {
                return new PoseidonMonitor(MonitorManager.Instance.GetMonitorFromHWnd(handle));
            }
            catch
            {
                return null;
            }
        }

        private static bool IsZoomedSpan(IntPtr hWnd)
        {
            _ = Win32.GetWindowRect(hWnd, out Win32.RECT appBounds);

            var MonitorArea = MonitorHelper.GetVirtualMonitorBounds();

            return ((appBounds.Bottom - appBounds.Top) >= MonitorArea.Height * .95f &&
               (appBounds.Right - appBounds.Left) >= MonitorArea.Width * .95f);
        }

        private static bool IsSystemLocked()
        {
            var result = false;
            var fHandle = Win32.GetForegroundWindow();
            try
            {
                _ = Win32.GetWindowThreadProcessId(fHandle, out int processID);
                using Process fProcess = Process.GetProcessById(processID);
                result = fProcess.ProcessName.Equals("LockApp", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {

            }

            return result;
        }

        private bool IsDesktop()
        {
            var hWnd = Win32.GetForegroundWindow();

            return Equals(hWnd, workerWOrig) || Equals(hWnd, progman);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dispatcherTimer.Stop();
                    SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
