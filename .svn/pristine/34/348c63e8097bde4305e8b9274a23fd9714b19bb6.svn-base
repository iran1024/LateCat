using LateCat.PoseidonEngine.Core;
using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LateCat.Views
{
    public partial class SettingsView : Window
    {
        private bool _isExit = false; 
        public SettingsView()
        {
            InitializeComponent();

            DataContext = App.Services.GetRequiredService<SettingsViewModel>();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
            }            
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _isExit = true;

            Close();
        }

        private void TabControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
