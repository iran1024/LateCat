using LateCat.PoseidonEngine.Core;
using System.Windows;
using System.Windows.Interop;

namespace LateCat.Views
{
    public partial class MonitorLabelView : Window
    {
        readonly int xPos = 0, yPos = 0;
        public MonitorLabelView(string scrrenLabel, int posLeft, int posTop)
        {
            InitializeComponent();
            MonitorLabel.Text = scrrenLabel;
            xPos = posLeft;
            yPos = posTop;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Win32.SetWindowPos(new WindowInteropHelper(this).Handle, 1, xPos, yPos, 0, 0,
                (int)Win32.SetWindowPosFlags.SWP_NOACTIVATE | (int)Win32.SetWindowPosFlags.SWP_NOSIZE);
        }
    }
}
