using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Openmiks.Models;
using Openmiks.Services;

namespace Openmiks.Views
{
    public partial class ReplayWindow : Window
    {
        private readonly InstantReplayEngine _replay;
        private readonly ProductionEngine _engine;
        private readonly DispatcherTimer _uiTimer;

        public ReplayWindow(InstantReplayEngine replay, ProductionEngine engine)
        {
            InitializeComponent();
            _replay = replay;
            _engine = engine;

            CameraAnglesList.ItemsSource = _replay.Cameras;
            EventsDataGrid.ItemsSource = _replay.Events;

            // Connect monitors to live camera / channel inputs
            UpdateMonitorFeeds();

            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _uiTimer.Tick += OnUiTick;
            _uiTimer.Start();

            Closing += (s, e) => _uiTimer.Stop();
        }

        private void OnUiTick(object? sender, EventArgs e)
        {
            SessionTimeText.Text = $"SESSION: {_replay.SessionTime:hh\\:mm\\:ss\\.ff}";

            ChanASpeedText.Text = $"{(_replay.ChannelA.Speed * 100):0}% SPEED";
            ChanAPosText.Text = $"POS: {_replay.ChannelA.CurrentPosition:hh\\:mm\\:ss\\.ff} {(_replay.ChannelA.IsLive ? "[LIVE]" : "[CUE]")}";

            ChanBSpeedText.Text = $"{(_replay.ChannelB.Speed * 100):0}% SPEED";
            ChanBPosText.Text = $"POS: {_replay.ChannelB.CurrentPosition:hh\\:mm\\:ss\\.ff} {(_replay.ChannelB.IsLive ? "[LIVE]" : "[CUE]")}";

            if (_replay.IsRecording)
            {
                ReplayDot.Fill = Brushes.Red;
                ReplayRecText.Text = "STOP REC";
                RecReplayBtn.Background = new SolidColorBrush(Color.FromRgb(153, 27, 27));
            }
            else
            {
                ReplayDot.Fill = Brushes.White;
                ReplayRecText.Text = "REC REPLAY";
                RecReplayBtn.Background = new SolidColorBrush(Color.FromRgb(40, 40, 45));
            }

            PlayPauseBtn.Content = _replay.ChannelA.IsPlaying ? "⏸ PAUSE" : "▶ PLAY";
        }

        private void UpdateMonitorFeeds()
        {
            ChanADisplay.Item = _engine.Inputs.FirstOrDefault(x => x.Number == _replay.ChannelA.ActiveCamera) 
                              ?? _engine.ProgramInput;
            ChanBDisplay.Item = _engine.Inputs.FirstOrDefault(x => x.Number == _replay.ChannelB.ActiveCamera) 
                              ?? _engine.PreviewInput;

            ChanATitle.Text = $"REPLAY CHANNEL A (CAM {_replay.ChannelA.ActiveCamera})";
            ChanBTitle.Text = $"REPLAY CHANNEL B (CAM {_replay.ChannelB.ActiveCamera})";
        }

        private void RecReplay_Click(object sender, RoutedEventArgs e)
        {
            if (_replay.IsRecording) _replay.StopRecording();
            else _replay.StartRecording();
        }

        private void MarkIn_Click(object sender, RoutedEventArgs e) => _replay.MarkIn();
        private void MarkOut_Click(object sender, RoutedEventArgs e) => _replay.MarkOut();

        private void Quick5s_Click(object sender, RoutedEventArgs e)
        {
            _replay.LiveAutoMark(5);
            UpdateMonitorFeeds();
        }

        private void Quick10s_Click(object sender, RoutedEventArgs e)
        {
            _replay.LiveAutoMark(10);
            UpdateMonitorFeeds();
        }

        private void Quick20s_Click(object sender, RoutedEventArgs e)
        {
            _replay.LiveAutoMark(20);
            UpdateMonitorFeeds();
        }

        private void LiveJump_Click(object sender, RoutedEventArgs e)
        {
            _replay.JumpToLive("A");
            UpdateMonitorFeeds();
        }

        private void Speed_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && double.TryParse(btn.Tag?.ToString(), out double s))
            {
                _replay.SetSpeed(s, "A");
            }
        }

        private void Jog_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && double.TryParse(btn.Tag?.ToString(), out double delta))
            {
                _replay.Jog(delta, "A");
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e) => _replay.TogglePlay("A");

        private void CameraAngle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is ReplayCamera cam)
            {
                _replay.ChannelA.ActiveCamera = cam.CameraNumber;
                UpdateMonitorFeeds();
            }
        }

        private void EventsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EventsDataGrid.SelectedItem is ReplayEvent ev)
            {
                _replay.PlayEvent(ev, "A");
                UpdateMonitorFeeds();
            }
        }
    }
}
