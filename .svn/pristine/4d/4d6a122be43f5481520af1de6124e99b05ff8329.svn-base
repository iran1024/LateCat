using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Windows;

namespace LateCat.PoseidonEngine.Core
{
    public class SystemMonitor : ObservableObject
    {
        internal bool _isStale;

        #region Properties

        private Rect _bounds = Rect.Empty;

        public Rect Bounds
        {
            get => _bounds;
            internal set => SetProperty(ref _bounds, value);
        }

        private string _deviceId;

        public string DeviceId
        {
            get => _deviceId;
            internal set => SetProperty(ref _deviceId, value);
        }

        private string _deviceName;

        public string DeviceName
        {
            get => _deviceName;
            internal set => SetProperty(ref _deviceName, value);
        }

        private string _monitorName = string.Empty;

        public string MonitorName
        {
            get => _monitorName;
            internal set => SetProperty(ref _monitorName, value);
        }

        private IntPtr _hMonitor = IntPtr.Zero;

        public IntPtr HMonitor
        {
            get => _hMonitor;
            internal set => SetProperty(ref _hMonitor, value);
        }

        private int _index;

        public int Index
        {
            get => _index;
            internal set => SetProperty(ref _index, value);
        }

        private bool _isPrimary;

        public bool IsPrimary
        {
            get => _isPrimary;
            internal set => SetProperty(ref _isPrimary, value);
        }

        private Rect _workingArea = Rect.Empty;

        public Rect WorkingArea
        {
            get => _workingArea;
            internal set => SetProperty(ref _workingArea, value);
        }

        #endregion

        public SystemMonitor(string deviceName)
        {
            DeviceName = deviceName;
        }
    }
}