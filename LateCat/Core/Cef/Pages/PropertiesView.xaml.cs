﻿using LateCat.PoseidonEngine.Abstractions;
using LateCat.PoseidonEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LateCat.Core.Cef
{
    public partial class PropertiesView : Page
    {
        #region init

        private readonly string _propertyCopyPath;
        private JObject _propertyCopyData;

        private readonly IWallpaperMetadata _metadata;
        private readonly IMonitor _monitor;

        private readonly Thickness _margin = new(0, 10, 0, 0);
        private readonly double _maxWidth = 200;

        private readonly ISettingsService _userSettings;
        private readonly IDesktopCore _desktopCore;

        public PropertiesView(IWallpaperMetadata model)
        {
            _userSettings = App.Services.GetRequiredService<ISettingsService>();
            _desktopCore = App.Services.GetRequiredService<IDesktopCore>();

            InitializeComponent();
            _metadata = model;
            try
            {
                var wpInfo = GetPropertyDetails(model, _userSettings.Settings.WallpaperArrangement, _userSettings.Settings.SelectedMonitor);
                _propertyCopyPath = wpInfo.Item1;
                _monitor = wpInfo.Item2;
            }
            catch
            {
                return;
            }
            LoadUI();
        }

        private void LoadUI()
        {
            try
            {
                _propertyCopyData = PropertiesJsonHelper.LoadProperties(_propertyCopyPath);
                GenerateUIElements();
            }
            catch (Exception e)
            {
                _ = Task.Run(() => (MessageBox.Show(e.ToString(), Properties.Resources.TitleAppName)));
            }
        }

        private void GenerateUIElements()
        {
            #region Refactor
            /*
            if (_propertyCopyData == null)
            {
                var msg = "Property file not found!";
                if (_metadata.WallpaperInfo.Type == WallpaperType.Video ||
                    _metadata.WallpaperInfo.Type == WallpaperType.VideoStream ||
                    _metadata.WallpaperInfo.Type == WallpaperType.Gif ||
                    _metadata.WallpaperInfo.Type == WallpaperType.Picture)
                {
                    msg += "\nMpv player is required...";
                }

                AddUIElement(new TextBlock
                {
                    Text = msg,
                    Background = Brushes.Red,
                    Foreground = Brushes.Yellow,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 50, 0, 0)
                });

                return;
            }
            else if (_propertyCopyData.Count == 0)
            {
                AddUIElement(new TextBlock
                {
                    Text = "El Psy Congroo",
                    Foreground = Brushes.Yellow,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = _margin
                });

                return;
            }

            dynamic obj;

            foreach (var item in _propertyCopyData)
            {
                string uiElementType = item.Value["type"].ToString();
                if (uiElementType.Equals("slider", StringComparison.OrdinalIgnoreCase))
                {
                    WindowsXamlHost xamlSlider = new WindowsXamlHost()
                    {
                        Name = item.Key,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        InitialTypeName = "Windows.UI.Xaml.Controls.Slider",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    xamlSlider.ChildChanged += XamlSlider_ChildChanged;
                    obj = xamlSlider;
                }
                else if (uiElementType.Equals("textbox", StringComparison.OrdinalIgnoreCase))
                {
                    var tb = new TextBox
                    {
                        Name = item.Key,
                        Text = item.Value["value"].ToString(),
                        AcceptsReturn = true,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    tb.TextChanged += Textbox_TextChanged;
                    obj = tb;
                }
                else if (uiElementType.Equals("button", StringComparison.OrdinalIgnoreCase))
                {
                    var btn = new Button
                    {
                        Name = item.Key,
                        Content = item.Value["value"].ToString(),
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    btn.Click += Button_Click;
                    obj = btn;
                }
                else if (uiElementType.Equals("color", StringComparison.OrdinalIgnoreCase))
                {
                    var pb = new System.Windows.Shapes.Rectangle
                    {
                        Name = item.Key,
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(item.Value["value"].ToString()),
                        Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                        StrokeThickness = 0.25,
                        MinWidth = _maxWidth,
                        MaxWidth = _maxWidth,
                        MaxHeight = 15,
                        MinHeight = 15,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin,
                        RadiusX = 5,
                        RadiusY = 5,
                    };
                    pb.MouseUp += Rectangle_Click;
                    obj = pb;
                }
                else if (uiElementType.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                {
                    var chk = new CheckBox
                    {
                        Name = item.Key,
                        Content = item.Value["text"].ToString(),
                        IsChecked = (bool)item.Value["value"],
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        Margin = _margin
                    };
                    chk.Checked += Checkbox_CheckedChanged;
                    chk.Unchecked += Checkbox_CheckedChanged;
                    obj = chk;
                }
                else if (uiElementType.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
                {

                    WindowsXamlHost xamlCmbBox = new WindowsXamlHost()
                    {
                        Name = item.Key,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        InitialTypeName = "Windows.UI.Xaml.Controls.ComboBox",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    xamlCmbBox.ChildChanged += XmlCmbBox_ChildChanged;
                    obj = xamlCmbBox;
                }
                else if (uiElementType.Equals("folderDropdown", StringComparison.OrdinalIgnoreCase))
                {
                    WindowsXamlHost xamlFolderCmbBox = new WindowsXamlHost()
                    {
                        Name = item.Key,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        InitialTypeName = "Windows.UI.Xaml.Controls.ComboBox",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    xamlFolderCmbBox.ChildChanged += XamlFolderCmbBox_ChildChanged;
                    obj = xamlFolderCmbBox;
                }
                else if (uiElementType.Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    var label = new Label
                    {
                        Name = item.Key,
                        Content = item.Value["value"].ToString(),
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = _margin
                    };
                    obj = label;
                }
                else
                {
                    continue;
                }

                //Title
                if (item.Value["text"] != null &&
                    !uiElementType.Equals("checkbox", StringComparison.OrdinalIgnoreCase) &&
                    !uiElementType.Equals("label", StringComparison.OrdinalIgnoreCase))
                {

                    AddUIElement(new Label
                    {
                        Content = item.Value["text"].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        Margin = _margin
                    });
                }

                AddUIElement(obj);
                //File browser for folderdropdown.
                if (uiElementType.Equals("folderDropdown", StringComparison.OrdinalIgnoreCase))
                {
                    var folderDropDownOpenFileBtn = new Button()
                    {
                        Tag = item.Key,
                        Content = Properties.Resources.TextBrowse,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        MaxWidth = _maxWidth,
                        MinWidth = _maxWidth,
                        Margin = new Thickness(0, 5, 0, 0),
                    };
                    folderDropDownOpenFileBtn.Click += FolderDropDownOpenFileBtn_Click;
                    AddUIElement(folderDropDownOpenFileBtn);
                }
            }

            //restore-default btn.
            var defaultBtn = new Button
            {
                Name = "defaultBtn",
                Content = "Restore Default",
                MaxWidth = _maxWidth,
                MinWidth = _maxWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = _margin
            };
            defaultBtn.Click += DefaultBtn_Click;
            AddUIElement(defaultBtn);*/
            #endregion
        }


        private void AddUIElement(dynamic obj)
        {
            uiPanel.Children.Add(obj);
        }

        #endregion //init

        #region dropdown
        private static string[] GetFileNames(string path, string searchPattern, SearchOption searchOption)
        {
            var searchPatterns = searchPattern.Split('|');
            var files = new List<string>();

            foreach (string sp in searchPatterns)
            {
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            }

            files.Sort();

            var tmp = new List<string>();

            foreach (var item in files)
            {
                tmp.Add(Path.GetFileName(item));
            }
            return tmp.ToArray();
        }

        #endregion //dropdown

        #region button

        private void DefaultBtn_Click(object sender, EventArgs e)
        {
            if (RestoreOriginalPropertyFile(_metadata, _propertyCopyPath))
            {
                uiPanel.Children.Clear();
                LoadUI();
                WallpaperSendMsg(new IPCButton() { Name = "LateCat_default_settings_reload", IsDefault = true });
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                var item = (Button)sender;
                WallpaperSendMsg(new IPCButton() { Name = item.Name });
            }
            catch { }
        }

        #endregion //button

        #region checkbox

        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var item = (CheckBox)sender;
                WallpaperSendMsg(new IPCCheckbox() { Name = item.Name, Value = (item.IsChecked == true) });
                _propertyCopyData[item.Name]["value"] = item.IsChecked == true;
                UpdatePropertyFile();
            }
            catch { }
        }

        #endregion //checkbox

        #region textbox

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var item = (TextBox)sender;
                WallpaperSendMsg(new IPCTextBox() { Name = item.Name, Value = item.Text });
                _propertyCopyData[item.Name]["value"] = item.Text;
                UpdatePropertyFile();
            }
            catch { }
        }

        #endregion //textbox

        #region helpers

        private void UpdatePropertyFile()
        {
            Cef.PropertiesJsonHelper.SaveProperties(_propertyCopyPath, _propertyCopyData);
        }

        private void WallpaperSendMsg(IPCMessage msg)
        {
            switch (_userSettings.Settings.WallpaperArrangement)
            {
                case WallpaperArrangement.Per:
                    _desktopCore.SendMessage(_monitor, _metadata, msg);
                    break;
                case WallpaperArrangement.Span:
                case WallpaperArrangement.Duplicate:
                    _desktopCore.SendMessage(_metadata, msg);
                    break;
            }
        }

        internal static bool RestoreOriginalPropertyFile(IWallpaperMetadata metadata, string propertyCopyPath)
        {
            bool status = false;
            try
            {
                if (metadata.WallpaperInfo.Type == WallpaperType.Video ||
                    metadata.WallpaperInfo.Type == WallpaperType.VideoStream ||
                    metadata.WallpaperInfo.Type == WallpaperType.Gif ||
                    metadata.WallpaperInfo.Type == WallpaperType.Picture)
                {

                    var lpp = Path.Combine(metadata.InfoFolderPath, "Properties.json");

                    var dlpp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                "plugins", "mpv", "api", "Properties.json");
                    if (File.Exists(lpp))
                    {

                        if (!string.Equals(metadata.PropertyPath, lpp, StringComparison.OrdinalIgnoreCase))
                        {
                            metadata.PropertyPath = lpp;
                        }
                    }
                    else
                    {

                        if (!string.Equals(metadata.PropertyPath, dlpp, StringComparison.OrdinalIgnoreCase))
                        {
                            metadata.PropertyPath = dlpp;
                        }
                    }
                }

                File.Copy(metadata.PropertyPath, propertyCopyPath, true);
                status = true;
            }
            catch
            {

            }
            return status;
        }

        internal Tuple<string, IMonitor> GetPropertyDetails(IWallpaperMetadata metadata, WallpaperArrangement arrangement, IMonitor selectedMonitor)
        {
            if (metadata.PropertyPath == null)
            {
                throw new ArgumentException("Non-customizable wallpaper.");
            }

            var propertyCopy = string.Empty;
            IMonitor monitor = null;

            var items = _desktopCore.Wallpapers.ToList().FindAll(x => x.Metadata == metadata);
            if (items.Count == 0)
            {
                try
                {
                    monitor = selectedMonitor;
                    var dataFolder = Program.WallpaperDataDir;
                    if (monitor.DeviceNumber != null)
                    {
                        var wpdataFolder = string.Empty;
                        switch (arrangement)
                        {
                            case WallpaperArrangement.Per:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, monitor.DeviceNumber);
                                break;
                            case WallpaperArrangement.Span:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, "span");
                                break;
                            case WallpaperArrangement.Duplicate:
                                wpdataFolder = Path.Combine(dataFolder, new DirectoryInfo(metadata.InfoFolderPath).Name, "duplicate");
                                break;
                        }
                        Directory.CreateDirectory(wpdataFolder);

                        propertyCopy = Path.Combine(wpdataFolder, "Properties.json");
                        if (!File.Exists(propertyCopy))
                        {
                            File.Copy(metadata.PropertyPath, propertyCopy);
                        }
                    }
                    else
                    {

                    }
                }
                catch
                {

                }
            }
            else if (items.Count == 1)
            {
                propertyCopy = items[0].PropertyCopyPath;
                monitor = items[0].Monitor;
            }
            else
            {
                switch (arrangement)
                {
                    case WallpaperArrangement.Per:
                        {
                            int index = items.FindIndex(x => MonitorHelper.Compare(selectedMonitor, x.Monitor, MonitorIdentificationMode.DeviceId));
                            propertyCopy = index != -1 ? items[index].PropertyCopyPath : items[0].PropertyCopyPath;
                            monitor = index != -1 ? items[index].Monitor : items[0].Monitor;
                        }
                        break;
                    case WallpaperArrangement.Span:
                    case WallpaperArrangement.Duplicate:
                        {
                            propertyCopy = items[0].PropertyCopyPath;
                            monitor = items[0].Monitor;
                        }
                        break;
                }
            }
            return new Tuple<string, IMonitor>(propertyCopy, monitor!);
        }

        #endregion //helpers
    }
}
