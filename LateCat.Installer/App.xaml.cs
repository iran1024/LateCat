using LateCat.Installer.Abstractions;
using LateCat.Installer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

namespace LateCat.Installer
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public static IServiceProvider Services
        {
            get
            {
                return ((App)Current)._serviceProvider ?? throw new InvalidOperationException("尚未初始化服务提供程序");
            }
        }

        public App()
        {
            _serviceProvider = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<IResourceExtractor, ResourceExtractor>()
                .AddSingleton<SlidesViewModel>()

                .BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Constants.InstallerTempDir);
            }
            catch
            {

            }

            Initialize();

            base.OnStartup(e);
        }

        private static void Initialize()
        {
            var extractor = Services.GetRequiredService<IResourceExtractor>();

            var headImage = extractor.GetResource("LateCat.Installer.latecat.head.jpg");
            var qrCode = extractor.GetResource("LateCat.Installer.latecat.qrcode.png");

            FileOperator.Save(headImage, Path.Combine(Constants.InstallerTempDir, "head.jpg"));
            FileOperator.Save(qrCode, Path.Combine(Constants.InstallerTempDir, "qrcode.png"));
        }
    }
}
