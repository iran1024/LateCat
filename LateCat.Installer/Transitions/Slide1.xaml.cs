using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace LateCat.Installer.Transitions
{
    public partial class Slide1 : UserControl
    {
        public Slide1()
        {
            InitializeComponent();

            DataContext = App.Services.GetRequiredService<SlidesViewModel>();
        }
    }
}