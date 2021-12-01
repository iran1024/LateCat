using LateCat.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;

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
