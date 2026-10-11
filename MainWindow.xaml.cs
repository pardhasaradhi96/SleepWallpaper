using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SleepWallpaper
{
    public class Settings
    {
        public string Mode { get; set; } = "color";   // color | image | video
        public string? Path { get; set; }
        public string Color { get; set; } = "#000000";
        public double Zoom { get; set; } = 1.0;
    }

    public partial class MainWindow : Window
    {
        static readonly string SettingsFile = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SleepWallpaper", "settings.json");

        static readonly string[] ImageExt = { ".jpg", ".jpeg", ".png", ".bmp" };
        static readonly string[] VideoExt = { ".mp4", ".wmv", ".avi", ".mov", ".mkv", ".webm" };

        Settings cfg = new Settings();
        readonly DispatcherTimer cursorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

        public MainWindow()
        {
            InitializeComponent();
            cursorTimer.Tick += (s, e) =>
            {
                if (!Menu.IsOpen) Cursor = Cursors.None;
                cursorTimer.Stop();
            };
        }

        // ---------- startup / shutdown ----------
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            Apply();
            cursorTimer.Start();
        }

        void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => SaveSettings();

        void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                    cfg = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFile)) ?? new Settings();
            }
            catch { cfg = new Settings(); }
        }

        void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SettingsFile)!);
                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(cfg));
            }
            catch { /* ignore */ }
        }

        // ---------- apply current setting ----------
        void Apply()
        {
            Vid.Stop();
            Vid.Source = null;
            Vid.Visibility = Visibility.Collapsed;
            Pic.Source = null;
            Pic.Visibility = Visibility.Collapsed;

            if (cfg.Mode != "color" && (cfg.Path == null || !File.Exists(cfg.Path)))
                cfg.Mode = "color";

            // Image/video: always black behind it. Saved color is used only in color mode.
            Background = Brushes.Black;
            if (cfg.Mode == "color")
            {
                try { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cfg.Color)); }
                catch { }
            }

            if (cfg.Mode == "image")
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;   // don't lock the file
                bmp.UriSource = new Uri(cfg.Path!);
                bmp.EndInit();
                Pic.Source = bmp;
                Pic.Visibility = Visibility.Visible;
            }
            else if (cfg.Mode == "video")
            {
                Vid.Source = new Uri(cfg.Path!);
                Vid.Visibility = Visibility.Visible;
                Vid.Volume = 0;
                Vid.IsMuted = true;
                Vid.Play();
            }
            SetZoom(cfg.Zoom);
        }

        // ---------- video loop ----------
        void Vid_MediaEnded(object sender, RoutedEventArgs e)
        {
            Vid.Position = TimeSpan.Zero;
            Vid.Play();
        }

        void Vid_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            cfg.Mode = "color";
            Apply();
        }

        // ---------- actions ----------
        void PickFile()
        {
            bool wasTop = Topmost; Topmost = false;
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choose image or video",
                Filter = "Images and videos|*.jpg;*.jpeg;*.png;*.bmp;*.mp4;*.wmv;*.avi;*.mov;*.mkv;*.webm"
                       + "|Images|*.jpg;*.jpeg;*.png;*.bmp"
                       + "|Videos|*.mp4;*.wmv;*.avi;*.mov;*.mkv;*.webm"
            };
            if (dlg.ShowDialog(this) == true)
            {
                string ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                cfg.Path = dlg.FileName;
                cfg.Mode = Array.IndexOf(VideoExt, ext) >= 0 ? "video" : "image";
                cfg.Zoom = 1.0;
                Apply();
                SaveSettings();
            }
            Topmost = wasTop;
        }

        void PickColor()
        {
            bool wasTop = Topmost; Topmost = false;
            using (var dlg = new System.Windows.Forms.ColorDialog { FullOpen = true, AnyColor = true })
            {
                try
                {
                    var c = (Color)ColorConverter.ConvertFromString(cfg.Color);
                    dlg.Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
                }
                catch { }

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    cfg.Color = $"#{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
                    cfg.Mode = "color";
                    cfg.Zoom = 1.0;
                    Apply();
                    SaveSettings();
                }
            }
            Topmost = wasTop;
        }

        async Task PickScreenColor()
        {
            Menu.IsOpen = false;
            Hide();                       // reveal the app underneath (e.g. VS Code)
            await Task.Delay(450);        // let the screen repaint

            var area = System.Windows.Forms.SystemInformation.VirtualScreen;
            System.Drawing.Color? picked = null;
            using (var bmp = new System.Drawing.Bitmap(area.Width, area.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                    g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);

                using (var f = new ScreenPicker(bmp, area))
                {
                    f.ShowDialog();
                    picked = f.Picked;
                }
            }

            Show();
            WindowState = WindowState.Maximized;
            Topmost = true;
            Activate();

            if (picked.HasValue)
            {
                var c = picked.Value;
                cfg.Color = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
                cfg.Mode = "color";
                cfg.Zoom = 1.0;
                Apply();
                SaveSettings();
            }
        }

        void SetZoom(double z)
        {
            cfg.Zoom = Math.Max(0.1, Math.Min(10.0, z));
            Scale.ScaleX = Scale.ScaleY = cfg.Zoom;
            SaveSettings();
        }

        void ZoomIn()  => SetZoom(cfg.Zoom * 1.1);
        void ZoomOut() => SetZoom(cfg.Zoom / 1.1);

        void ShowNewMenu()
        {
            Cursor = null;
            Menu.Placement = PlacementMode.MousePoint;
            Menu.IsOpen = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NewItem.IsSubmenuOpen = true;
                NewItem.Focus();
            }), DispatcherPriority.Background);
        }

        // ---------- menu handlers ----------
        void FilePicker_Click(object s, RoutedEventArgs e)  => PickFile();
        void ColorPicker_Click(object s, RoutedEventArgs e) => PickColor();
        async void ScreenColor_Click(object s, RoutedEventArgs e) => await PickScreenColor();
        void ZoomIn_Click(object s, RoutedEventArgs e)      => ZoomIn();
        void ZoomOut_Click(object s, RoutedEventArgs e)     => ZoomOut();
        void Close_Click(object s, RoutedEventArgs e)       => Close();

        // ---------- keyboard / mouse ----------
        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            switch (e.Key)
            {
                case Key.N: ShowNewMenu(); e.Handled = true; break;
                case Key.OemPlus:
                case Key.Add: ZoomIn(); e.Handled = true; break;
                case Key.OemMinus:
                case Key.Subtract: ZoomOut(); e.Handled = true; break;
                case Key.D0:
                case Key.NumPad0: SetZoom(1.0); e.Handled = true; break;
                case Key.W: Close(); e.Handled = true; break;
            }
        }

        void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = null;
            cursorTimer.Stop();
            cursorTimer.Start();
        }
    }
}
