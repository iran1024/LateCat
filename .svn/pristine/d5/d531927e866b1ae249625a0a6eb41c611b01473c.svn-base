using LateCat.PoseidonEngine.Abstractions;
using System.Windows;
using System.Windows.Input;

namespace LateCat.Core.Cef
{
    public partial class PropertiesTrayWindow : Window
    {
        public PropertiesTrayWindow(IWallpaperMetadata metadata)
        {
            InitializeComponent();
            PreviewKeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };

            WindowStartupLocation = WindowStartupLocation.Manual;
            Height = (int)(SystemParameters.WorkArea.Height / 1.1f);
            Top = SystemParameters.WorkArea.Bottom - Height - 10;
            Left = SystemParameters.WorkArea.Right - Width - 5;
            Title = metadata.Title;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Win32.SetWindowPos(new WindowInteropHelper(this).Handle, 1, xPos, yPos, width, height, 0 | 0x0010);
        }
    }
}
