using LateCat.Installer.Abstractions;
using LateCat.Installer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LateCat.Installer.Transitions
{
    public partial class Slide1 : UserControl
    {
        private readonly IResourceExtractor _extractor;

        public Slide1()
        {
            InitializeComponent();

            DataContext = App.Services.GetRequiredService<SlidesViewModel>();

            _extractor = App.Services.GetRequiredService<IResourceExtractor>();

            (DataContext as SlidesViewModel).Progress.ProgressChanged += Progress_ProgressChanged;
        }

        private void Progress_ProgressChanged(object? sender, int e)
        {
            (Application.Current.MainWindow as MainWindow).ProgressBar.Value = e;
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SlidesViewModel).IsInstallStart = true;

            Task.Run(() =>
            {
                foreach (var map in Constants.Resources)
                {
                    var stream = _extractor.GetResource(map.Name);

                    FileOperator.Extractor(stream, map.DestinationDirectory, (DataContext as SlidesViewModel).Progress);
                }
            });
        }
    }
}
