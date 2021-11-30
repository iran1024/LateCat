using LateCat.Helpers;
using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace LateCat.Core
{
    internal class TaskbarOperator : ITaskbarOperator
    {
        public bool IsRunning { get; private set; } = false;

        private Color _accentColor = Color.FromArgb(0, 0, 0);
        private TaskbarTheme _taskbarTheme = TaskbarTheme.None;
        private AccentPolicy _accentPolicyRegular = new();
        private bool _disposed;


        private readonly Timer _timer = new();
        private readonly static IDictionary<string, string> _incompatiblePrograms = new Dictionary<string, string>()
        {
            {"TranslucentTB", "344635E9-9AE4-4E60-B128-D53E25AB70A7"},
            {"TaskbarX", null}
        };

        public TaskbarOperator()
        {
            _timer.Interval = 500;
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start(TaskbarTheme theme)
        {
            if (theme == TaskbarTheme.None)
            {
                Stop();
            }
            else
            {
                _timer.Stop();
                SetTheme(theme);
                ResetTaskbar();
                _timer.Start();
                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                _timer.Stop();
                ResetTaskbar();
                IsRunning = false;
            }
        }

        private void SetTheme(TaskbarTheme theme)
        {
            _taskbarTheme = theme;
            switch (_taskbarTheme)
            {
                case TaskbarTheme.None:
                    break;
                case TaskbarTheme.Clear:
                    _accentPolicyRegular.GradientColor = 16777215;
                    _accentPolicyRegular.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                    break;
                case TaskbarTheme.Blur:
                    _accentPolicyRegular.GradientColor = 0;
                    _accentPolicyRegular.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                    break;
                case TaskbarTheme.Color:
                    break;
                case TaskbarTheme.Fluent:
                    _accentPolicyRegular.GradientColor = 167772160;
                    _accentPolicyRegular.AccentState = AccentState.ACCENT_ENABLE_FLUENT;
                    break;
                case TaskbarTheme.Wallpaper:
                    _accentPolicyRegular.GradientColor = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", 200, _accentColor.B, _accentColor.G, _accentColor.R), 16);
                    _accentPolicyRegular.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                    break;
                case TaskbarTheme.WallpaperFluent:
                    _accentPolicyRegular.GradientColor = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", 125, _accentColor.B, _accentColor.G, _accentColor.R), 16);
                    _accentPolicyRegular.AccentState = AccentState.ACCENT_ENABLE_FLUENT;
                    break;
            }
        }

        public void SetAccentColor(Color color)
        {
            _accentColor = color;
            Start(_taskbarTheme);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetTaskbarTransparent(_taskbarTheme);
        }

        private void SetTaskbarTransparent(TaskbarTheme theme)
        {
            if (theme == TaskbarTheme.None)
            {
                return;
            }

            var taskbars = GetTaskbars();
            if (taskbars.Count != 0)
            {
                var accentPtr = IntPtr.Zero;
                try
                {
                    var accentStructSize = Marshal.SizeOf(_accentPolicyRegular);
                    accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(_accentPolicyRegular, accentPtr, false);
                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    foreach (var taskbar in taskbars)
                    {
                        SetWindowCompositionAttribute(taskbar, ref data);
                    }
                }
                catch
                {
                    Stop();
                }
                finally
                {
                    //not required for this structure..
                    //Marshal.DestroyStructure(accentPtr, typeof(AccentPolicy));
                    Marshal.FreeHGlobal(accentPtr);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransparentTbService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #region helpers

        private List<IntPtr> GetTaskbars()
        {
            IntPtr taskbar;
            var taskbars = new List<IntPtr>(2);
            //main taskbar..
            if ((taskbar = Win32.FindWindow("Shell_TrayWnd", null)) != IntPtr.Zero)
            {
                taskbars.Add(taskbar);
            }
            //secondary taskbar(s)..
            if ((taskbar = Win32.FindWindow("Shell_SecondaryTrayWnd", null)) != IntPtr.Zero)
            {
                taskbars.Add(taskbar);
                while ((taskbar = Win32.FindWindowEx(IntPtr.Zero, taskbar, "Shell_SecondaryTrayWnd", IntPtr.Zero)) != IntPtr.Zero)
                {
                    taskbars.Add(taskbar);
                }
            }
            return taskbars;
        }

        private void ResetTaskbar()
        {
            foreach (var taskbar in GetTaskbars())
            {
                Win32.SendMessage(taskbar, (int)Win32.WM.DWMCOMPOSITIONCHANGED, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public string CheckIncompatiblePrograms()
        {
            foreach (var item in _incompatiblePrograms)
            {
                if (item.Value != null)
                {
                    try
                    {
                        System.Threading.Mutex mutex = null;
                        try
                        {
                            if (System.Threading.Mutex.TryOpenExisting(item.Value, out mutex))
                            {
                                return item.Key;
                            }
                        }
                        finally
                        {
                            mutex?.Dispose();
                        }
                    }
                    catch { } //skipping
                }
                else
                {
                    try
                    {
                        var proc = Process.GetProcessesByName(item.Key);
                        if (proc.Count() != 0)
                        {
                            return item.Key;
                        }
                    }
                    catch { } //skipping
                }
            }
            return null;
        }

        public Color GetAverageColor(string imgPath)
        {
            using var originalBmp = new Bitmap(imgPath);

            var targetWidth = originalBmp.Width;
            var targetHeight = 75;

            using var cutBmp = new Bitmap(targetWidth, targetHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var g = Graphics.FromImage(cutBmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(originalBmp, new Rectangle(0, 0, targetWidth, targetHeight), new Rectangle(0, originalBmp.Height - targetHeight, targetWidth, targetHeight), GraphicsUnit.Pixel);
            g.Dispose();

            var avgColor = AverageColorCalculator.Analyze(cutBmp);

            return Color.FromArgb(avgColor.R, avgColor.G, avgColor.B);
        }

        #endregion //helpers

        #region pinvoke {undocumented}

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_FLUENT = 4 //don't like alpha = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public uint GradientColor; //AABBGGRR
            public int AnimationId;
        }

        #endregion //pinvoke {undocumented}
    }
}
