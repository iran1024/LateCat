using LateCat.PoseidonEngine.Abstractions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LateCat.Views
{
    public partial class WallpaperPreviewer : UserControl
    {
        #region Element Definition
        static WallpaperPreviewer()
        {
            var style = CreateDefaultStyles();
            StyleProperty.OverrideMetadata(typeof(WallpaperPreviewer), new FrameworkPropertyMetadata(style));

            StretchProperty.OverrideMetadata(
                typeof(WallpaperPreviewer),
                new FrameworkPropertyMetadata(
                    Stretch.Uniform,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        private static Style CreateDefaultStyles()
        {
            var style = new Style(typeof(WallpaperPreviewer), null);
            style.Setters.Add(new Setter(FlowDirectionProperty, FlowDirection.LeftToRight));
            style.Seal();

            return style;
        }

        public static readonly DependencyProperty StretchProperty =
                Viewbox.StretchProperty.AddOwner(typeof(WallpaperPreviewer));

        public static readonly DependencyProperty SourceProperty =
                DependencyProperty.Register(
                    "Source",
                    typeof(IWallpaperMetadata),
                    typeof(WallpaperPreviewer),
                    new FrameworkPropertyMetadata(
                            null,
                            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(SourcePropertyChangedCallback)));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        public IWallpaperMetadata Source
        {
            get => (IWallpaperMetadata)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly RoutedEvent SourceChangedEvent =
            EventManager.RegisterRoutedEvent(
                "SourceChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(WallpaperPreviewer));

        public event RoutedEventHandler SourceChanged
        {
            add { AddHandler(SourceChangedEvent, value); }
            remove { RemoveHandler(SourceChangedEvent, value); }
        }

        #endregion

        public WallpaperPreviewer()
        {
            InitializeComponent();
        }

        #region Internal Methods
        private static void SourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d != null && d is WallpaperPreviewer)
            {
                GetInstance(d).RaiseEvent(new RoutedEventArgs(SourceChangedEvent, d));
            }
        }

        private static WallpaperPreviewer GetInstance(DependencyObject d)
        {
            if (d is WallpaperPreviewer previewer)
            {
                return previewer;
            }
            else
            {
                throw new ArgumentException(null, nameof(d));
            }
        }

        #endregion
    }
}
