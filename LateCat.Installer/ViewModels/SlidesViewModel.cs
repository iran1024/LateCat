using LateCat.Common;
using LateCat.Installer.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace LateCat.Installer
{
    internal class SlidesViewModel : ObservableObject
    {
        private readonly ResourceMap _programResourceMap;

        public SlidesViewModel()
        {
            _programResourceMap = Constants.Resources.First(n => n.Name == "Program");

            HeadImage = new BitmapImage(new Uri(Path.Combine(Constants.InstallerTempDir, "head.jpg")));
            QRCode = new BitmapImage(new Uri(Path.Combine(Constants.InstallerTempDir, "qrcode.png")));
            InstallPath = _programResourceMap.DestinationDirectory;

            Progress = new Progress<double>();
        }

        private BitmapImage _headImage;
        public BitmapImage HeadImage
        {
            get => _headImage;
            set
            {
                _headImage = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _qrCode;
        public BitmapImage QRCode
        {
            get => _qrCode;
            set
            {
                _qrCode = value;
                OnPropertyChanged();
            }
        }

        private string _installPath;
        public string InstallPath
        {
            get => _installPath;
            set
            {
                _installPath = value;
                OnPropertyChanged();
            }
        }

        private bool _isInstallStart = false;
        public bool IsInstallStart
        {
            get => _isInstallStart;
            set
            {
                _isInstallStart = value;
                _programResourceMap.DestinationDirectory = _installPath;
                OnPropertyChanged();
            }
        }

        private Progress<double> _progress;
        public Progress<double> Progress
        {
            get => _progress;
            set
            {
                if (_progress is null)
                {
                    _progress = value;
                }
                OnPropertyChanged();
            }
        }

        private RelayCommand _changeInstallPathCommand;
        public RelayCommand ChangeInstallPathCommand
        {
            get
            {
                if (_changeInstallPathCommand == null)
                {
                    _changeInstallPathCommand = new RelayCommand(
                        param => ChangeInstallPath());
                }
                return _changeInstallPathCommand;
            }
        }

        private void ChangeInstallPath()
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = InstallPath
            };

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (string.Equals(folderBrowserDialog.SelectedPath, InstallPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                InstallPath = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
