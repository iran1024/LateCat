using LateCat.Installer.Abstractions;
using LateCat.Installer.Services;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace LateCat.Installer
{
    internal class SlidesViewModel : ObservableObject
    {
        public SlidesViewModel()
        {
            HeadImage = new BitmapImage(new Uri(Path.Combine(Constants.InstallerTempDir, "head.jpg")));
            QRCode = new BitmapImage(new Uri(Path.Combine(Constants.InstallerTempDir, "qrcode.png")));
            InstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Late Cat");

            Progress = new Progress<int>();           
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
                OnPropertyChanged();
            }
        }

        private Progress<int> _progress;
        public Progress<int> Progress
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

        private RelayCommand _completedCommand;
        public RelayCommand CompletedCommand
        {
            get
            {
                if (_completedCommand == null)
                {
                    _completedCommand = new RelayCommand(
                        param => Completed());
                }
                return _completedCommand;
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

        private void Completed()
        {

        }
    }
}
