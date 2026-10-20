using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Openmiks.Models;

namespace Openmiks.Services
{
    public class InstantReplayEngine : INotifyPropertyChanged
    {
        private bool _isRecording = false;
        private DateTime _sessionStartTime;
        private TimeSpan _sessionTime = TimeSpan.Zero;
        private TimeSpan _currentMarkIn = TimeSpan.Zero;
        private int _currentListIndex = 1;
        private readonly DispatcherTimer _replayTimer;

        public bool IsRecording
        {
            get => _isRecording;
            private set { _isRecording = value; OnPropertyChanged(); }
        }

        public TimeSpan SessionTime
        {
            get => _sessionTime;
            private set { _sessionTime = value; OnPropertyChanged(); }
        }

        public int CurrentListIndex
        {
            get => _currentListIndex;
            set { _currentListIndex = Math.Clamp(value, 1, 20); OnPropertyChanged(); }
        }

        public ObservableCollection<ReplayCamera> Cameras { get; } = new();
        public ObservableCollection<ReplayEvent> Events { get; } = new();
        public ReplayChannel ChannelA { get; } = new() { ChannelName = "A", ActiveCamera = 1 };
        public ReplayChannel ChannelB { get; } = new() { ChannelName = "B", ActiveCamera = 2 };

        public InstantReplayEngine()
        {
            // Initialize 8 Replay Cameras
            for (int i = 1; i <= 8; i++)
            {
                Cameras.Add(new ReplayCamera
                {
                    CameraNumber = i,
                    Name = $"Camera {i}",
                    AssignedInputNumber = Math.Min(i, 8)
                });
            }

            _replayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _replayTimer.Tick += OnReplayTick;
        }

        public void StartRecording()
        {
            if (_isRecording) return;
            _sessionStartTime = DateTime.UtcNow;
            SessionTime = TimeSpan.Zero;
            _currentMarkIn = TimeSpan.Zero;
            IsRecording = true;

            foreach (var cam in Cameras)
            {
                cam.IsRecording = true;
            }

            _replayTimer.Start();
        }

        public void StopRecording()
        {
            if (!_isRecording) return;
            _replayTimer.Stop();
            IsRecording = false;

            foreach (var cam in Cameras)
            {
                cam.IsRecording = false;
            }
        }

        private void OnReplayTick(object? sender, EventArgs e)
        {
            if (!_isRecording) return;

            SessionTime = DateTime.UtcNow - _sessionStartTime;

            // Advance playback channels if playing
            UpdateChannelPosition(ChannelA);
            UpdateChannelPosition(ChannelB);
        }

        private void UpdateChannelPosition(ReplayChannel ch)
        {
            if (!ch.IsPlaying) return;

            // Position increases by elapsed 50ms scaled by Speed (slow-mo)
            double stepMs = 50.0 * ch.Speed;
            ch.CurrentPosition += TimeSpan.FromMilliseconds(stepMs);

            if (ch.SelectedEvent != null && ch.CurrentPosition > ch.SelectedEvent.OutPoint)
            {
                // Reached end of event
                ch.CurrentPosition = ch.SelectedEvent.InPoint; // loop or stop
                ch.IsPlaying = false;
            }
        }

        public void MarkIn()
        {
            _currentMarkIn = SessionTime;
        }

        public void MarkOut(string comment = "")
        {
            var inPt = _currentMarkIn;
            var outPt = SessionTime;

            if (outPt <= inPt)
            {
                inPt = TimeSpan.FromSeconds(Math.Max(0, outPt.TotalSeconds - 5));
            }

            var ev = new ReplayEvent
            {
                Id = Events.Count + 1,
                ListNumber = CurrentListIndex,
                CameraNumber = ChannelA.ActiveCamera,
                InPoint = inPt,
                OutPoint = outPt,
                Speed = 0.5,
                Text = string.IsNullOrEmpty(comment) ? $"Replay {Events.Count + 1} (Cam {ChannelA.ActiveCamera})" : comment
            };

            Events.Add(ev);
        }

        public void LiveAutoMark(int secondsBack = 10)
        {
            var outPt = SessionTime;
            var inPt = TimeSpan.FromSeconds(Math.Max(0, outPt.TotalSeconds - secondsBack));

            var ev = new ReplayEvent
            {
                Id = Events.Count + 1,
                ListNumber = CurrentListIndex,
                CameraNumber = ChannelA.ActiveCamera,
                InPoint = inPt,
                OutPoint = outPt,
                Speed = 0.5,
                Text = $"Auto {secondsBack}s Highlight (Cam {ChannelA.ActiveCamera})"
            };

            Events.Add(ev);

            // Auto cue to Channel A
            PlayEvent(ev, "A");
        }

        public void PlayEvent(ReplayEvent ev, string channel = "A")
        {
            var ch = channel.Equals("B", StringComparison.OrdinalIgnoreCase) ? ChannelB : ChannelA;
            ch.SelectedEvent = ev;
            ch.ActiveCamera = ev.CameraNumber;
            ch.CurrentPosition = ev.InPoint;
            ch.Speed = ev.Speed;
            ch.IsLive = false;
            ch.IsPlaying = true;
        }

        public void TogglePlay(string channel = "A")
        {
            var ch = channel.Equals("B", StringComparison.OrdinalIgnoreCase) ? ChannelB : ChannelA;
            ch.IsPlaying = !ch.IsPlaying;
        }

        public void SetSpeed(double speed, string channel = "A")
        {
            var ch = channel.Equals("B", StringComparison.OrdinalIgnoreCase) ? ChannelB : ChannelA;
            ch.Speed = Math.Clamp(speed, 0.05, 1.0);
        }

        public void Jog(double seconds, string channel = "A")
        {
            var ch = channel.Equals("B", StringComparison.OrdinalIgnoreCase) ? ChannelB : ChannelA;
            ch.CurrentPosition += TimeSpan.FromSeconds(seconds);
            if (ch.CurrentPosition < TimeSpan.Zero) ch.CurrentPosition = TimeSpan.Zero;
        }

        public void JumpToLive(string channel = "A")
        {
            var ch = channel.Equals("B", StringComparison.OrdinalIgnoreCase) ? ChannelB : ChannelA;
            ch.IsLive = true;
            ch.IsPlaying = true;
            ch.Speed = 1.0;
            ch.CurrentPosition = SessionTime;
            ch.SelectedEvent = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
