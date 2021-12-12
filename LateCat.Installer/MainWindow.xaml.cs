using LateCat.Installer.Abstractions;
using LateCat.Installer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LateCat.Installer
{
    public partial class MainWindow : Window
    {
        private readonly IResourceExtractor _extractor;
        private readonly SlidesViewModel _slidesVm;

        public MainWindow()
        {
            InitializeComponent();

            _slidesVm = App.Services.GetRequiredService<SlidesViewModel>();
            _slidesVm.Progress.ProgressChanged += Progress_ProgressChanged;

            _extractor = App.Services.GetRequiredService<IResourceExtractor>();

            StartPage.BtnInstall.Click += BtnInstall_Click;
            EndPage.BtnComplete.Click += BtnComplete_Click;
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_slidesVm.IsInstallStart)
            {
                return;
            }

            _slidesVm.IsInstallStart = true;
            ProgressBar.Visibility = Visibility.Visible;
            EndPage.BtnComplete.IsEnabled = false;
            BtnClose.IsEnabled = false;

            Task.Run(() =>
            {
                foreach (var map in Constants.Resources)
                {
                    map.ResourceStream = _extractor.GetResource(map.Path);
                }

                FileOperator.ExtractorAll(Constants.Resources, _slidesVm.Progress);

                var lateCatExe = Path.Combine(_slidesVm.InstallPath, "LateCat.exe");

                WindowsUtils.CreateShortcutOnDesktop("Late Cat", lateCatExe, "Late Cat Mooooooooo", lateCatExe);
                WindowsUtils.AddToStartMenu("Late Cat", lateCatExe, "Late Cat Mooooooooo", lateCatExe);

            }).ContinueWith(task =>
            {
                Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    EndPage.BtnComplete.IsEnabled = true;
                    BtnClose.IsEnabled = true;

                    var doubleAnimation = new DoubleAnimation(1.0, 0, TimeSpan.FromSeconds(1));
                    ProgressBar.BeginAnimation(OpacityProperty, doubleAnimation);

                    await Task.Delay(1000);

                    ProgressBar.Visibility = Visibility.Hidden;
                });
            });
        }

        private void Progress_ProgressChanged(object? sender, double e)
        {
            ProgressBar.Value = e;
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
