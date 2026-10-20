using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VMixClone.Controls;
using VMixClone.Models;
using VMixClone.Services;
using VMixClone.Views;

namespace VMixClone
{
    public partial class MainWindow : Window
    {
        private readonly ProductionEngine _engine;
        private WebServerService _webServer;
        private TcpServerService _tcpServer;
        private bool _isFullscreen = false;
        private bool _isExternalActive = false;
        private WindowState _prevWindowState = WindowState.Normal;
        private WindowStyle _prevWindowStyle = WindowStyle.SingleBorderWindow;

        // Feeds the encoders from the rendered program canvas instead of the desktop, so
        // recording/streaming capture the actual mix (including overlays and FTB).
        private readonly ProgramFrameSource _programFrames;
        private readonly System.Windows.Threading.DispatcherTimer _programGrabTimer;

        internal sealed class ProgramFrameSource : IFrameSource
        {
            private readonly Func<(byte[]? Buffer, int Width, int Height)> _grab;
            private byte[] _lastBuffer = Array.Empty<byte>();
            private int _lastWidth;
            private int _lastHeight;
            private readonly object _lock = new();

            public ProgramFrameSource(Func<(byte[]? Buffer, int Width, int Height)> grab) => _grab = grab;

            public void UpdateFromUI()
            {
                var (buf, w, h) = _grab();
                if (buf != null && w > 0 && h > 0)
                {
                    lock (_lock)
                    {
                        if (_lastBuffer.Length != buf.Length)
                            _lastBuffer = new byte[buf.Length];
                        Buffer.BlockCopy(buf, 0, _lastBuffer, 0, buf.Length);
                        _lastWidth = w;
                        _lastHeight = h;
                    }
                }
            }

            public bool TryGetFrame(out byte[] buffer, out int width, out int height)
            {
                lock (_lock)
                {
                    width = _lastWidth;
                    height = _lastHeight;
                    buffer = _lastBuffer;
                    return _lastBuffer.Length > 0 && width > 0 && height > 0;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _engine = new ProductionEngine();
            _engine.PropertyChanged += Engine_PropertyChanged;

            _programFrames = new ProgramFrameSource(() =>
            {
                byte[]? buf = ProgramSurface.Grab(ProgramContent, out int w, out int h);
                return (buf, w, h);
            });
            _engine.ProgramFrames = _programFrames;

            _programGrabTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _programGrabTimer.Tick += (_, _) => _programFrames.UpdateFromUI();
            _programGrabTimer.Start();

            _webServer = new WebServerService(_engine, 8088);
            _webServer.Start();

            _tcpServer = new TcpServerService(_engine, 8099);
            _tcpServer.Start();

            WebUrlText.Text = $"http://localhost:{_webServer.Port}/controller";
            StatusLeftText.Text = $"Ready | Web API: http://localhost:{_webServer.Port}/api | TCP API: port {_tcpServer.Port} | Remote: http://localhost:{_webServer.Port}/controller";

            InputsList.ItemsSource = _engine.FilteredInputs;

            Closing += MainWindow_Closing;

            UpdateUI();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _programGrabTimer.Stop();
            _webServer.Stop();
            _tcpServer.Stop();
            _engine.Shutdown();
        }

        private void Engine_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(_engine.MasterLevelL) or nameof(_engine.MasterLevelR))
            {
                MasterMeterL.Value = _engine.MasterLevelL;
                MasterMeterR.Value = _engine.MasterLevelR;
                return;
            }
            if (e.PropertyName is nameof(_engine.BusALevelL) or nameof(_engine.BusALevelR))
            {
                BusAMeterL.Value = _engine.BusALevelL;
                BusAMeterR.Value = _engine.BusALevelR;
                return;
            }
            Dispatcher.Invoke(UpdateUI);
        }

        private void UpdateUI()
        {
            MasterClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            ResolutionFpsText.Text = $"{_engine.Configuration.OutputResolution} {_engine.Configuration.FrameRate}";

            // Update Monitors
            if (PreviewDisplay.Item != _engine.PreviewInput)
            {
                PreviewDisplay.Item = _engine.PreviewInput;
                PreviewGizmo.Item = _engine.PreviewInput;
            }
            if (ProgramDisplay.Item != _engine.ProgramInput)
            {
                ProgramDisplay.Item = _engine.ProgramInput;
            }

            PreviewDisplay.AttachPlayout(_engine.Playout);
            ProgramDisplay.AttachPlayout(_engine.Playout);
            TransitionTargetDisplay.AttachPlayout(_engine.Playout);
            Overlay3Display.AttachPlayout(_engine.Playout);

            PreviewInputName.Text = _engine.PreviewInput != null
                ? $"{_engine.PreviewInput.Number}. {_engine.PreviewInput.Title}"
                : "-";

            ProgramInputName.Text = _engine.ProgramInput != null
                ? $"{_engine.ProgramInput.Number}. {_engine.ProgramInput.Title}"
                : "-";

            // Transitions & Blending
            if (_engine.IsTransitioning || _engine.TransitionProgress > 0)
            {
                TransitionTargetDisplay.Item = _engine.TransitionTargetInput ?? _engine.PreviewInput;

                double w = Math.Max(1, TransitionTargetDisplay.ActualWidth);
                double h = Math.Max(1, TransitionTargetDisplay.ActualHeight);

                switch (_engine.CurrentTransition)
                {
                    case TransitionEffect.Fade:
                        TransitionTargetDisplay.Opacity = _engine.TransitionProgress;
                        TransitionTargetDisplay.Clip = null;
                        TransitionTargetDisplay.RenderTransform = null;
                        break;
                    case TransitionEffect.Merge:
                        TransitionTargetDisplay.Opacity = _engine.TransitionProgress;
                        double scaleM = 0.9 + (_engine.TransitionProgress * 0.1);
                        TransitionTargetDisplay.RenderTransform = new ScaleTransform(scaleM, scaleM, w / 2, h / 2);
                        TransitionTargetDisplay.Clip = null;
                        break;
                    case TransitionEffect.Wipe:
                        TransitionTargetDisplay.Opacity = 1.0;
                        TransitionTargetDisplay.Clip = new RectangleGeometry(new Rect(0, 0, w * _engine.TransitionProgress, h));
                        TransitionTargetDisplay.RenderTransform = null;
                        break;
                    case TransitionEffect.BarnDoor:
                        TransitionTargetDisplay.Opacity = 1.0;
                        double halfW = (w / 2) * _engine.TransitionProgress;
                        TransitionTargetDisplay.Clip = new RectangleGeometry(new Rect((w / 2) - halfW, 0, halfW * 2, h));
                        TransitionTargetDisplay.RenderTransform = null;
                        break;
                    case TransitionEffect.Slide:
                        TransitionTargetDisplay.Opacity = 1.0;
                        TransitionTargetDisplay.Clip = null;
                        TransitionTargetDisplay.RenderTransform = new TranslateTransform((1.0 - _engine.TransitionProgress) * w, 0);
                        break;
                    case TransitionEffect.Zoom:
                    case TransitionEffect.CubeZoom:
                    default:
                        TransitionTargetDisplay.Opacity = _engine.TransitionProgress;
                        double scaleZ = _engine.TransitionProgress;
                        TransitionTargetDisplay.RenderTransform = new ScaleTransform(scaleZ, scaleZ, w / 2, h / 2);
                        TransitionTargetDisplay.Clip = null;
                        break;
                }

                TBarValText.Text = $"{(int)(_engine.TransitionProgress * 100)}%";
            }
            else
            {
                TransitionTargetDisplay.Opacity = 0.0;
                TransitionTargetDisplay.Clip = null;
                TransitionTargetDisplay.RenderTransform = null;
                TBarValText.Text = "0%";
            }

            // Duration
            DurationText.Text = $"{_engine.TransitionDurationMs} ms";

            // Overlays 1-8 True Real-Time Compositing with Smooth Broadcast Transitions
            UpdateOverlayChannel(1, _engine.Overlay1, Overlay1Host, Overlay1Display, Overlay1Btn);
            UpdateOverlayChannel(2, _engine.Overlay2, Overlay2Host, Overlay2Display, Overlay2Btn);
            UpdateOverlayChannel(3, _engine.Overlay3, Overlay3Host, Overlay3Display, Overlay3Btn);
            UpdateOverlayChannel(4, _engine.Overlay4, Overlay4Host, Overlay4Display, Overlay4Btn);
            UpdateOverlayChannel(5, _engine.Overlay5, Overlay5Host, Overlay5Display, Overlay5Btn);
            UpdateOverlayChannel(6, _engine.Overlay6, Overlay6Host, Overlay6Display, Overlay6Btn);
            UpdateOverlayChannel(7, _engine.Overlay7, Overlay7Host, Overlay7Display, Overlay7Btn);
            UpdateOverlayChannel(8, _engine.Overlay8, Overlay8Host, Overlay8Display, Overlay8Btn);

            // FTB
            FtbMask.Opacity = _engine.IsFTB ? 1.0 : 0.0;
            if (_engine.IsFTB)
            {
                FtbBtn.Background = new SolidColorBrush(Color.FromRgb(255, 87, 34));
                FtbText.Foreground = Brushes.White;
            }
            else
            {
                FtbBtn.Background = new SolidColorBrush(Color.FromRgb(62, 39, 35));
                FtbText.Foreground = new SolidColorBrush(Color.FromRgb(255, 138, 101));
            }

            // REC & STREAM
            if (_engine.IsRecording)
            {
                RecDot.Fill = Brushes.Red;
                RecText.Text = $"REC {_engine.RecordDuration:hh\\:mm\\:ss} ({_engine.RecordFileSize})";
                RecordBtn.Background = new SolidColorBrush(Color.FromRgb(127, 29, 29));
            }
            else
            {
                RecDot.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                RecText.Text = "RECORD";
                RecordBtn.Background = new SolidColorBrush(Color.FromRgb(37, 37, 40));
            }

            if (_engine.IsStreaming)
            {
                StreamDot.Fill = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                StreamText.Text = $"LIVE {_engine.StreamDuration:hh\\:mm\\:ss} ({_engine.StreamBitrateKbps}k)";
                StreamBtn.Background = new SolidColorBrush(Color.FromRgb(6, 78, 59));
            }
            else
            {
                StreamDot.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                StreamText.Text = "STREAM";
                StreamBtn.Background = new SolidColorBrush(Color.FromRgb(37, 37, 40));
            }

            PgmStatsText.Text = $"REC: {(_engine.IsRecording ? $"{_engine.RecordDuration:hh\\:mm\\:ss} [{_engine.RecordFileSize}]{(_engine.Recording.UsesProgramSurface ? " PROG" : " SCRN")}" : "OFF")} • STREAM: {(_engine.IsStreaming ? $"{_engine.StreamDuration:hh\\:mm\\:ss} [{_engine.StreamBitrateKbps} kbps]{(_engine.Streaming.ReportedLive ? "" : " (no encoder)")}" : "OFF")}";

            // Audio Meters: Master
            MasterMeterL.Value = _engine.MasterLevelL;
            MasterMeterR.Value = _engine.MasterLevelR;
            if (_engine.MasterMuted)
            {
                MasterDbText.Text = "MUTED";
                MasterDbText.Foreground = Brushes.Red;
                MasterMuteBtn.Background = new SolidColorBrush(Color.FromRgb(185, 28, 28));
                MasterMuteText.Text = "MUTED";
            }
            else
            {
                double db = (_engine.MasterVolume - 100.0) * 0.4;
                MasterDbText.Text = $"{db:0.0} dB";
                MasterDbText.Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                MasterMuteBtn.Background = new SolidColorBrush(Color.FromRgb(45, 45, 50));
                MasterMuteText.Text = "MUTE";
            }

            // Audio Meters: Bus A
            BusAMeterL.Value = _engine.BusALevelL;
            BusAMeterR.Value = _engine.BusALevelR;
            if (_engine.BusAMuted)
            {
                BusADbText.Text = "MUTED";
                BusADbText.Foreground = Brushes.Red;
                BusAMuteBtn.Background = new SolidColorBrush(Color.FromRgb(185, 28, 28));
                BusAMuteText.Text = "MUTED";
            }
            else
            {
                double dbA = (_engine.BusAVolume - 100.0) * 0.4;
                BusADbText.Text = $"{dbA:0.0} dB";
                BusADbText.Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                BusAMuteBtn.Background = new SolidColorBrush(Color.FromRgb(45, 45, 50));
                BusAMuteText.Text = "MUTE A";
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e) => _engine.Cut();
        private void Fade_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.Fade);
        private void Merge_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.Merge);
        private void Zoom_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.Zoom);
        private void Wipe_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.Wipe);
        private void Slide_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.Slide);
        private void BarnDoor_Click(object sender, RoutedEventArgs e) => _engine.StartTransition(TransitionEffect.BarnDoor);

        private void DurationMinus_Click(object sender, RoutedEventArgs e) => _engine.TransitionDurationMs = Math.Max(100, _engine.TransitionDurationMs - 100);
        private void DurationPlus_Click(object sender, RoutedEventArgs e) => _engine.TransitionDurationMs = Math.Min(3000, _engine.TransitionDurationMs + 100);

        private void TBarSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && !_engine.IsTransitioning)
            {
                _engine.SetTBar(e.NewValue);
            }
        }

        private void Ftb_Click(object sender, RoutedEventArgs e) => _engine.ToggleFTB();

        private void RecordBtn_Click(object sender, RoutedEventArgs e) => _engine.ToggleRecord();
        private void StreamBtn_Click(object sender, RoutedEventArgs e) => _engine.ToggleStream();

        private void ExternalBtn_Click(object sender, RoutedEventArgs e)
        {
            _isExternalActive = !_isExternalActive;
            ExternalBtn.Background = _isExternalActive ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : new SolidColorBrush(Color.FromRgb(37, 37, 40));
        }

        private void FullscreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullscreen)
            {
                _prevWindowState = WindowState;
                _prevWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _isFullscreen = true;
            }
            else
            {
                WindowStyle = _prevWindowStyle;
                WindowState = _prevWindowState;
                _isFullscreen = false;
            }
        }

        private void Overlay1_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(1);
        private void Overlay2_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(2);
        private void Overlay3_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(3);
        private void Overlay4_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(4);
        private void Overlay5_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(5);
        private void Overlay6_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(6);
        private void Overlay7_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(7);
        private void Overlay8_Click(object sender, RoutedEventArgs e) => _engine.ToggleOverlay(8);

        private void Stinger1_Click(object sender, RoutedEventArgs e) => _ = _engine.PlayStingerAsync(1);
        private void Stinger2_Click(object sender, RoutedEventArgs e) => _ = _engine.PlayStingerAsync(2);
        private void OverlayAllOff_Click(object sender, RoutedEventArgs e) => _engine.OverlayAllOff();

        private void UpdateOverlayChannel(int channel, InputItem? targetInput, Grid hostLayer, InputDisplay display, Button btn)
        {
            if (targetInput != null)
            {
                if (display.Item != targetInput)
                {
                    display.Item = targetInput;
                    display.AttachPlayout(_engine.Playout);
                }

                if (hostLayer.Visibility != Visibility.Visible)
                {
                    hostLayer.Visibility = Visibility.Visible;
                    var anim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(220))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    hostLayer.BeginAnimation(UIElement.OpacityProperty, anim);
                }

                btn.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Amber active tally
                btn.Foreground = Brushes.Black;
                btn.FontWeight = FontWeights.Black;
            }
            else
            {
                if (hostLayer.Visibility == Visibility.Visible && hostLayer.Opacity > 0.05)
                {
                    var anim = new DoubleAnimation(hostLayer.Opacity, 0.0, TimeSpan.FromMilliseconds(180))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };
                    anim.Completed += (s, e) =>
                    {
                        if (_engine.GetOverlay(channel) == null)
                        {
                            hostLayer.Visibility = Visibility.Collapsed;
                            display.Item = null;
                        }
                    };
                    hostLayer.BeginAnimation(UIElement.OpacityProperty, anim);
                }
                else if (hostLayer.Opacity <= 0.05)
                {
                    hostLayer.Visibility = Visibility.Collapsed;
                    display.Item = null;
                }

                btn.Background = new SolidColorBrush(Color.FromRgb(32, 36, 45)); // Dark neutral
                btn.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                btn.FontWeight = FontWeights.Bold;
            }
        }

        private void MasterFader_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_engine != null) _engine.MasterVolume = e.NewValue;
        }

        private void BusAFader_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_engine != null) _engine.BusAVolume = e.NewValue;
        }

        private void MasterMute_Click(object sender, RoutedEventArgs e) => _engine.MasterMuted = !_engine.MasterMuted;
        private void BusAMute_Click(object sender, RoutedEventArgs e) => _engine.BusAMuted = !_engine.BusAMuted;

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string catStr)
            {
                if (Enum.TryParse<InputCategory>(catStr, out var cat))
                {
                    _engine.SelectedCategory = cat;
                }
            }
        }

        private void WebControllerLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = $"http://localhost:{_webServer.Port}/controller";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open browser: {ex.Message}", "Web Controller", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsWindow(_engine) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                ResolutionFpsText.Text = $"{_engine.Configuration.OutputResolution} {_engine.Configuration.FrameRate}";
                if (_webServer.Port != _engine.Configuration.WebServerPort)
                {
                    _webServer.Stop();
                    _webServer = new WebServerService(_engine, _engine.Configuration.WebServerPort);
                    _webServer.Start();
                }
                WebUrlText.Text = $"http://localhost:{_webServer.Port}/controller";
                StatusLeftText.Text = $"Ready | Web API active on http://localhost:{_webServer.Port}/api | Remote: http://localhost:{_webServer.Port}/controller";
                UpdateUI();
            }
        }

        private void NewPreset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Create a new blank preset?", "New Preset", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _engine.Inputs.Clear();
                _engine.Inputs.Add(new InputItem { Number = 1, Title = "Color Bars", Type = Models.InputType.ColorBars });
                _engine.Inputs.Add(new InputItem { Number = 2, Title = "Studio Clock", Type = Models.InputType.Clock });
                _engine.SetProgram(1);
                _engine.SetPreview(2);
                Title = "Openmix 29.0 Pro - Untitled Preset";
                UpdateUI();
            }
        }

        private void OpenPreset_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "vMix / Openmix Preset (*.vmix)|*.vmix|All Files (*.*)|*.*",
                Title = "Open Openmix Preset"
            };
            if (dlg.ShowDialog() == true)
            {
                if (PresetService.LoadPreset(dlg.FileName, _engine, _engine.Configuration))
                {
                    Title = $"Openmix 29.0 Pro - {System.IO.Path.GetFileNameWithoutExtension(dlg.FileName)}";
                    UpdateUI();
                }
                else
                {
                    MessageBox.Show("Failed to load preset file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "vMix / Openmix Preset (*.vmix)|*.vmix",
                Title = "Save Openmix Preset",
                FileName = "StudioProduction.vmix"
            };
            if (dlg.ShowDialog() == true)
            {
                PresetService.SavePreset(dlg.FileName, _engine, _engine.Configuration);
                Title = $"Openmix 29.0 Pro - {System.IO.Path.GetFileNameWithoutExtension(dlg.FileName)}";
                MessageBox.Show("Preset saved successfully!", "Preset Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void InputCardSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                var dlg = new InputSettingsWindow(item) { Owner = this };
                if (dlg.ShowDialog() == true && dlg.IsDeleted)
                {
                    _engine.Inputs.Remove(item);
                }
            }
        }

        private void PreviewObsEditToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isEditing = PreviewObsEditToggle.IsChecked == true;
            PreviewGizmo.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            if (isEditing)
            {
                PreviewGizmo.Item = _engine.PreviewInput;
                PreviewGizmo.UpdateGizmoLayout();
            }
        }

        private void OpenPreviewObjectEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.PreviewInput != null)
            {
                var editor = new ObjectEditWindow(_engine.PreviewInput) { Owner = this };
                editor.ShowDialog();
                PreviewGizmo.UpdateGizmoLayout();
            }
        }

        private void OpenMultiView_Click(object sender, RoutedEventArgs e)
        {
            var mv = new MultiViewWindow(_engine) { Owner = this };
            mv.Show();
        }

        private void OpenReplay_Click(object sender, RoutedEventArgs e)
        {
            var replayWin = new ReplayWindow(_engine.Replay, _engine) { Owner = this };
            replayWin.Show();
        }

        private void OpenVse_Click(object sender, RoutedEventArgs e)
        {
            var vse = new VirtualSetEditorWindow(null, _engine.Inputs) { Owner = this };
            vse.Show();
        }

        private void AddInput_Click(object sender, RoutedEventArgs e)
        {
            int nextNumber = _engine.Inputs.Count > 0 ? _engine.Inputs.Max(x => x.Number) + 1 : 1;
            var dlg = new AddInputDialog(nextNumber, _engine.Inputs) { Owner = this };
            if (dlg.ShowDialog() == true && dlg.ResultItem != null)
            {
                dlg.ResultItem.Number = nextNumber;
                _engine.Inputs.Add(dlg.ResultItem);
            }
        }

        private void InputCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is InputItem item)
            {
                _engine.SetPreview(item.Number);
            }
        }

        private void InputCardCut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                _engine.SetProgram(item.Number);
            }
        }

        private void InputCardPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                _engine.QuickPlay(item.Number);
            }
        }

        private void InputCardPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                item.IsPlaying = !item.IsPlaying;
            }
        }

        private void InputCardClose_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                if (MessageBox.Show($"Remove input {item.Number}: '{item.Title}'?", "Close Input", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _engine.Inputs.Remove(item);
                }
            }
        }

        private void InputCardAudio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is InputItem item)
            {
                item.IsMuted = !item.IsMuted;
            }
        }

        private void CardOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string chStr && int.TryParse(chStr, out int ch) && btn.DataContext is InputItem item)
            {
                _engine.ToggleOverlay(ch, item.Number);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _engine.Cut();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _engine.StartTransition(TransitionEffect.Fade);
                e.Handled = true;
            }
            else if (e.Key == Key.F1)
            {
                _engine.ToggleOverlay(1);
                e.Handled = true;
            }
            else if (e.Key == Key.F2)
            {
                _engine.ToggleOverlay(2);
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                _engine.ToggleOverlay(3);
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                _engine.ToggleOverlay(4);
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                _engine.ToggleOverlay(5);
                e.Handled = true;
            }
            else if (e.Key == Key.F6)
            {
                _engine.ToggleOverlay(6);
                e.Handled = true;
            }
            else if (e.Key == Key.F7)
            {
                _engine.ToggleOverlay(7);
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                _engine.ToggleOverlay(8);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                FullscreenBtn_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int inputNum = e.Key - Key.D0;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    _engine.SetProgram(inputNum);
                }
                else
                {
                    _engine.SetPreview(inputNum);
                }
                e.Handled = true;
            }
        }
    }
}
