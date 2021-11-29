using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LateCat.Controls
{
    public class Loading : Control
    {
        public static readonly DependencyProperty FillColorProperty;

        static Loading()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Loading), new FrameworkPropertyMetadata(typeof(Loading)));

            FillColorProperty = DependencyProperty.Register(
                "FillColor",
                typeof(Color),
                typeof(Loading),
                new UIPropertyMetadata(Colors.DarkBlue, new PropertyChangedCallback(OnUriChanged)));
        }

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        [Description("背景色"), Category("个性配置"), DefaultValue("#FF668899")]
        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        public static Loading Create()
            => new()
            {
                Margin = new Thickness(0, 163, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
    }
}