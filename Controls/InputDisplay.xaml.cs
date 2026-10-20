using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VMixClone.Models;
using VMixClone.Services;

namespace VMixClone.Controls
{
    public partial class InputDisplay : UserControl
    {
        private InputItem? _item;
        private readonly DispatcherTimer _clockTimer;
        private string _lastLoadedVideoPath = string.Empty;
        private TitleTemplate? _template;
        private string _templatePath = string.Empty;
        private PlayoutEngine? _playout;

        // The engine pushes its playout here so the display renders the currently playing file.
        public void AttachPlayout(PlayoutEngine? playout) => _playout = playout;

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                nameof(Item),
                typeof(InputItem),
                typeof(InputDisplay),
                new PropertyMetadata(null, OnItemChanged));

        public static readonly DependencyProperty IsOverlayModeProperty =
            DependencyProperty.Register(
                nameof(IsOverlayMode),
                typeof(bool),
                typeof(InputDisplay),
                new PropertyMetadata(false, (d, e) =>
                {
                    if (d is InputDisplay display && display.RootContainer != null)
                    {
                        display.RootContainer.Background = (bool)e.NewValue
                            ? Brushes.Transparent
                            : new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x10));
                    }
                }));

        public bool IsOverlayMode
        {
            get => (bool)GetValue(IsOverlayModeProperty);
            set => SetValue(IsOverlayModeProperty, value);
        }

        public InputItem? Item
        {
            get => (InputItem?)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public InputDisplay()
        {
            InitializeComponent();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();

            SizeChanged += (s, e) => UpdateTransform();
            UpdateDisplay();
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputDisplay display)
            {
                if (e.OldValue is InputItem oldItem)
                {
                    oldItem.PropertyChanged -= display.OnItemPropertyChanged;
                }
                if (e.NewValue is InputItem newItem)
                {
                    newItem.PropertyChanged += display.OnItemPropertyChanged;
                }
                display._item = e.NewValue as InputItem;
                display.UpdateDisplay();
            }
        }

        private void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InputItem.DesktopCaptureSource))
            {
                if (_item?.Type == InputType.DesktopCapture)
                {
                    DesktopImage.Source = _item.DesktopCaptureSource;
                }
            }
            else if (e.PropertyName is nameof(InputItem.PanX) or nameof(InputItem.PanY)
                     or nameof(InputItem.ZoomX) or nameof(InputItem.ZoomY)
                     or nameof(InputItem.Rotation)
                     or nameof(InputItem.CropLeft) or nameof(InputItem.CropRight)
                     or nameof(InputItem.CropTop) or nameof(InputItem.CropBottom))
            {
                UpdateTransform();
            }
            else if (e.PropertyName == nameof(InputItem.Volume) || e.PropertyName == nameof(InputItem.IsMuted))
            {
                if (_item != null)
                {
                    double vol = _item.IsMuted ? 0.0 : (_item.Volume / 100.0);
                    if (VideoPlayer.Source != null) VideoPlayer.Volume = vol;
                    if (AudioPlayer.Source != null) AudioPlayer.Volume = vol;
                }
            }
            else if (e.PropertyName == nameof(InputItem.Capture))
            {
                UpdateDisplay();
            }
            else if (e.PropertyName == nameof(InputItem.IsPlaying))
            {
                if (_item != null)
                {
                    if (VideoPlayer.Source != null)
                    {
                        if (_item.IsPlaying) VideoPlayer.Play();
                        else VideoPlayer.Pause();
                    }
                    if (AudioPlayer.Source != null)
                    {
                        if (_item.IsPlaying) AudioPlayer.Play();
                        else AudioPlayer.Pause();
                    }
                }
            }
            else
            {
                UpdateDisplay();
            }
        }

        private void OnClockTick(object? sender, EventArgs e)
        {
            if (_item?.Type == InputType.Clock && ClockRoot.Visibility == Visibility.Visible)
            {
                var now = DateTime.Now;
                ClockText.Text = now.ToString("HH:mm:ss:ff");
                DateText.Text = now.ToString("yyyy-MM-dd");
            }
            else if (_item?.Type == InputType.Camera && CameraRoot.Visibility == Visibility.Visible)
            {
                var capture = _item.Capture;
                CameraImage.Source = capture?.Bitmap;
                CameraPlaceholder.Visibility = capture is { IsRunning: true } ? Visibility.Collapsed : Visibility.Visible;
                CameraSourceBadge.Text = capture is { IsRunning: true } ? "DIRECTSHOW LIVE" : "NO SIGNAL";
                CameraSpecText.Text = capture is { IsRunning: true }
                    ? $"{capture.Width}x{capture.Height} • {capture.FrameCount} frames • DirectShow"
                    : $"{_item.CameraResolution} @ {_item.CameraFps} • capture not running";
            }
            else if (_item?.Type == InputType.Video && VideoRoot.Visibility == Visibility.Visible && VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                var cur = VideoPlayer.Position;
                var total = VideoPlayer.NaturalDuration.TimeSpan;
                VideoTimeText.Text = $"{cur:mm\\:ss} / {total:mm\\:ss}";
            }
            else if ((_item?.Type == InputType.AudioFile || _item?.Type == InputType.AudioDevice) && AudioRoot.Visibility == Visibility.Visible)
            {
                if (_item.Type == InputType.AudioFile && AudioPlayer.NaturalDuration.HasTimeSpan)
                {
                    var cur = AudioPlayer.Position;
                    var total = AudioPlayer.NaturalDuration.TimeSpan;
                    AudioTimeText.Text = $"{cur:mm\\:ss} / {total:mm\\:ss}";
                }
                else if (_item.Type == InputType.AudioDevice)
                {
                    AudioTimeText.Text = _item.AudioCapture is { IsRunning: true } ? "LIVE INPUT • 48kHz" : "DIRECTSHOW AUDIO";
                }

                // Animate EQ spectrum bars based on real-time audio levels
                double lvl = Math.Max(_item.AudioLevelL, _item.AudioLevelR);
                if (_item.IsMuted || lvl < 0.01) lvl = 0.05;
                EqBar1.Height = Math.Max(4, lvl * 30 * 0.7);
                EqBar2.Height = Math.Max(4, lvl * 42 * 0.9);
                EqBar3.Height = Math.Max(4, lvl * 26 * 0.6);
                EqBar4.Height = Math.Max(4, lvl * 38 * 0.85);
                EqBar5.Height = Math.Max(4, lvl * 48 * 1.0);
                EqBar6.Height = Math.Max(4, lvl * 34 * 0.8);
                EqBar7.Height = Math.Max(4, lvl * 40 * 0.9);
                EqBar8.Height = Math.Max(4, lvl * 20 * 0.5);
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_item?.IsLoop == true)
            {
                VideoPlayer.Position = TimeSpan.Zero;
                VideoPlayer.Play();
            }
        }

        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_item?.IsLoop == true)
            {
                AudioPlayer.Position = TimeSpan.Zero;
                AudioPlayer.Play();
            }
        }

        public void UpdateDisplay()
        {
            VideoRoot.Visibility = Visibility.Collapsed;
            CameraRoot.Visibility = Visibility.Collapsed;
            DesktopRoot.Visibility = Visibility.Collapsed;
            ImageRoot.Visibility = Visibility.Collapsed;
            AudioRoot.Visibility = Visibility.Collapsed;
            ColorBarsRoot.Visibility = Visibility.Collapsed;
            ClockRoot.Visibility = Visibility.Collapsed;
            TitleRoot.Visibility = Visibility.Collapsed;
            ColorRoot.Visibility = Visibility.Collapsed;
            WebRoot.Visibility = Visibility.Collapsed;
            NoSignalRoot.Visibility = Visibility.Collapsed;

            if (_item == null)
            {
                NoSignalRoot.Visibility = Visibility.Visible;
                VideoPlayer.Stop();
                VideoPlayer.Source = null;
                _lastLoadedVideoPath = string.Empty;
                return;
            }

            switch (_item.Type)
            {
                case InputType.Video:
                    VideoRoot.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(_item.FilePath) && File.Exists(_item.FilePath))
                    {
                        if (_lastLoadedVideoPath != _item.FilePath)
                        {
                            _lastLoadedVideoPath = _item.FilePath;
                            VideoPlayer.Source = new Uri(_item.FilePath);
                            VideoPlayer.Volume = _item.IsMuted ? 0.0 : (_item.Volume / 100.0);
                            VideoPlayer.Play();
                        }
                    }
                    else
                    {
                        VideoTimeText.Text = "VIDEO DEMO (READY)";
                    }
                    break;

                case InputType.Camera:
                    CameraRoot.Visibility = Visibility.Visible;
                    CameraNameText.Text = string.IsNullOrEmpty(_item.CameraDeviceName) ? "NO CAMERA" : _item.CameraDeviceName.ToUpperInvariant();
                    var capture = _item.Capture;
                    CameraSpecText.Text = capture is { IsRunning: true }
                        ? $"{capture.Width}x{capture.Height} @ {capture.FrameCount} frames • DirectShow"
                        : $"{_item.CameraResolution} @ {_item.CameraFps} • capture not running";
                    CameraImage.Source = capture?.Bitmap;
                    CameraPlaceholder.Visibility = capture is { IsRunning: true } ? Visibility.Collapsed : Visibility.Visible;
                    CameraSourceBadge.Text = capture is { IsRunning: true } ? "DIRECTSHOW LIVE" : "NO SIGNAL";
                    break;

                case InputType.DesktopCapture:
                    DesktopRoot.Visibility = Visibility.Visible;
                    DesktopImage.Source = _item.DesktopCaptureSource;
                    break;

                case InputType.Playlist:
                case InputType.PhotoSlideshow:
                    // Playout inputs render the engine's resolved current file.
                    var playout = _playout;
                    if (_item.Type == InputType.Playlist)
                    {
                        VideoRoot.Visibility = Visibility.Visible;
                        if (playout != null && !string.IsNullOrEmpty(playout.CurrentFile) && File.Exists(playout.CurrentFile))
                        {
                            if (_lastLoadedVideoPath != playout.CurrentFile)
                            {
                                _lastLoadedVideoPath = playout.CurrentFile;
                                VideoPlayer.Source = new Uri(playout.CurrentFile);
                                VideoPlayer.Volume = _item.IsMuted ? 0.0 : (_item.Volume / 100.0);
                                VideoPlayer.Play();
                            }
                        }
                    }
                    else
                    {
                        ImageRoot.Visibility = Visibility.Visible;
                        string current = playout?.CurrentFile ?? _item.FilePath;
                        if (!string.IsNullOrEmpty(current) && File.Exists(current))
                        {
                            try { StaticImage.Source = new BitmapImage(new Uri(current)); } catch { }
                        }
                    }
                    break;

                case InputType.Image:
                    ImageRoot.Visibility = Visibility.Visible;
                    if (!string.IsNullOrEmpty(_item.FilePath) && File.Exists(_item.FilePath))
                    {
                        try
                        {
                            StaticImage.Source = new BitmapImage(new Uri(_item.FilePath));
                        }
                        catch { }
                    }
                    break;

                case InputType.AudioFile:
                case InputType.AudioDevice:
                    AudioRoot.Visibility = Visibility.Visible;
                    if (_item.Type == InputType.AudioDevice)
                    {
                        AudioTitleText.Text = string.IsNullOrEmpty(_item.AudioDeviceName) ? _item.Title : _item.AudioDeviceName;
                        AudioTimeText.Text = _item.AudioCapture is { IsRunning: true } ? "LIVE INPUT • 48kHz" : "DIRECTSHOW AUDIO";
                    }
                    else
                    {
                        AudioTitleText.Text = string.IsNullOrEmpty(_item.FilePath) ? _item.Title : Path.GetFileName(_item.FilePath);
                        if (!string.IsNullOrEmpty(_item.FilePath) && File.Exists(_item.FilePath))
                        {
                            if (_lastLoadedVideoPath != _item.FilePath)
                            {
                                _lastLoadedVideoPath = _item.FilePath;
                                AudioPlayer.Source = new Uri(_item.FilePath);
                                AudioPlayer.Volume = _item.IsMuted ? 0.0 : (_item.Volume / 100.0);
                                AudioPlayer.Play();
                            }
                        }
                        else
                        {
                            AudioTimeText.Text = "READY";
                        }
                    }
                    break;

                case InputType.ColorBars:
                    ColorBarsRoot.Visibility = Visibility.Visible;
                    break;

                case InputType.Clock:
                    ClockRoot.Visibility = Visibility.Visible;
                    var now = DateTime.Now;
                    ClockText.Text = now.ToString("HH:mm:ss:ff");
                    DateText.Text = now.ToString("yyyy-MM-dd");
                    break;

                case InputType.Title:
                    TitleRoot.Visibility = Visibility.Visible;
                    // Loads the real vMix template once per path and drives its named fields.
                    if (!string.IsNullOrEmpty(_item.TitleTemplatePath) && _templatePath != _item.TitleTemplatePath)
                    {
                        _templatePath = _item.TitleTemplatePath;
                        _template = TitleTemplate.Load(_item.TitleTemplatePath);
                    }
                    else if (string.IsNullOrEmpty(_item.TitleTemplatePath))
                    {
                        _template = null;
                        _templatePath = string.Empty;
                    }

                    // Real vMix template when the input carries one, else the built-in lower third.
                    if (_template != null)
                    {
                        _template.ApplyHeadline(_item.Headline, _item.Subtitle);
                        TitleTemplateHost.Child = _template.Root;
                        TitleTemplateHost.Visibility = Visibility.Visible;
                        BuiltInTitle.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TitleTemplateHost.Child = null;
                        TitleTemplateHost.Visibility = Visibility.Collapsed;
                        BuiltInTitle.Visibility = Visibility.Visible;
                        HeadlineText.Text = string.IsNullOrEmpty(_item.Headline) ? "LIVE" : _item.Headline;
                        SubtitleText.Text = string.IsNullOrEmpty(_item.Subtitle) ? "vMix Clone" : _item.Subtitle;
                    }
                    break;

                case InputType.WebBrowser:
                    WebRoot.Visibility = Visibility.Visible;
                    WebUrlDisplay.Text = string.IsNullOrEmpty(_item.WebUrl) ? "https://vmix.com" : _item.WebUrl;
                    break;

                case InputType.VirtualSet:
                    VirtualSetRoot.Visibility = Visibility.Visible;
                    if (_item.VirtualSet != null)
                    {
                        VsView.VirtualSet = _item.VirtualSet;
                    }
                    break;

                case InputType.Colour:
                default:
                    ColorRoot.Visibility = Visibility.Visible;
                    ColorBorder.Background = new SolidColorBrush(_item.SolidColor);
                    ColorInputTitle.Text = _item.Title;
                    break;
            }

            UpdateTransform();
        }

        public void UpdateTransform()
        {
            if (_item == null)
            {
                HostScale.ScaleX = 1;
                HostScale.ScaleY = 1;
                HostRotate.Angle = 0;
                HostTranslate.X = 0;
                HostTranslate.Y = 0;
                TransformHost.Clip = null;
                return;
            }

            HostScale.ScaleX = _item.ZoomX;
            HostScale.ScaleY = _item.ZoomY;
            HostRotate.Angle = _item.Rotation;

            double w = ActualWidth > 0 ? ActualWidth : 1920;
            double h = ActualHeight > 0 ? ActualHeight : 1080;

            HostTranslate.X = _item.PanX * (w / 2.0);
            HostTranslate.Y = _item.PanY * (h / 2.0);

            if (_item.CropLeft > 0 || _item.CropRight > 0 || _item.CropTop > 0 || _item.CropBottom > 0)
            {
                double left = _item.CropLeft * w;
                double top = _item.CropTop * h;
                double rw = Math.Max(0, w - left - (_item.CropRight * w));
                double rh = Math.Max(0, h - top - (_item.CropBottom * h));
                TransformHost.Clip = new RectangleGeometry(new Rect(left, top, rw, rh));
            }
            else
            {
                TransformHost.Clip = null;
            }
        }
    }
}
