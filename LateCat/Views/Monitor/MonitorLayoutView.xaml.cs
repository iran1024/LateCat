using LateCat.PoseidonEngine.Core;
using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace LateCat.Views
{
    public partial class MonitorLayoutView : Window
    {
        private readonly List<MonitorLabelView> _monitorLabels = new();
        private readonly MonitorLayoutViewModel _monitorVm;

        public MonitorLayoutView()
        {
            InitializeComponent();
            _monitorVm = App.Services.GetRequiredService<MonitorLayoutViewModel>();

            DataContext = _monitorVm;
            Closing += _monitorVm.OnWindowClosing;
            CreateLabelWindows();

            MonitorHelper.MonitorUpdated += MonitorHelper_MonitorUpdated;
        }

        private void MonitorLayoutControl_ChildChanged(object sender, EventArgs e)
        {

        }

        private void MonitorHelper_MonitorUpdated(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CloseLabelWindows();
                CreateLabelWindows();
            }));
        }

        private void CreateLabelWindows()
        {
            var Monitors = MonitorHelper.GetMonitor();
            if (Monitors.Count > 1)
            {
                Monitors.ForEach(Monitor =>
                {
                    var labelWindow = new MonitorLabelView(Monitor.DeviceNumber, Monitor.Bounds.Left + 10, Monitor.Bounds.Top + 10);
                    labelWindow.Show();
                    _monitorLabels.Add(labelWindow);
                });
            }
        }

        private void CloseLabelWindows()
        {
            _monitorLabels.ForEach(x => x.Close());
            _monitorLabels.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MonitorHelper.MonitorUpdated -= MonitorHelper_MonitorUpdated;
            CloseLabelWindows();
        }

        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
