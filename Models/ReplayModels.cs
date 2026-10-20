using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Openmiks.Models
{
    public class ReplayCamera : INotifyPropertyChanged
    {
        private int _cameraNumber = 1;
        private string _name = "Camera 1";
        private int _assignedInputNumber = 4;
        private bool _isRecording = false;

        public int CameraNumber
        {
            get => _cameraNumber;
            set { _cameraNumber = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int AssignedInputNumber
        {
            get => _assignedInputNumber;
            set { _assignedInputNumber = value; OnPropertyChanged(); }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set { _isRecording = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ReplayEvent : INotifyPropertyChanged
    {
        private int _id;
        private int _listNumber = 1;
        private int _cameraNumber = 1;
        private TimeSpan _inPoint;
        private TimeSpan _outPoint;
        private double _speed = 0.5; // 50% slow-mo by default
        private string _text = string.Empty;
        private DateTime _timestamp = DateTime.UtcNow;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public int ListNumber
        {
            get => _listNumber;
            set { _listNumber = value; OnPropertyChanged(); }
        }

        public int CameraNumber
        {
            get => _cameraNumber;
            set { _cameraNumber = value; OnPropertyChanged(); }
        }

        public TimeSpan InPoint
        {
            get => _inPoint;
            set { _inPoint = value; OnPropertyChanged(); OnPropertyChanged(nameof(Duration)); }
        }

        public TimeSpan OutPoint
        {
            get => _outPoint;
            set { _outPoint = value; OnPropertyChanged(); OnPropertyChanged(nameof(Duration)); }
        }

        public TimeSpan Duration => OutPoint > InPoint ? (OutPoint - InPoint) : TimeSpan.Zero;

        public double Speed
        {
            get => _speed;
            set { _speed = Math.Clamp(value, 0.05, 1.0); OnPropertyChanged(); }
        }

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ReplayChannel : INotifyPropertyChanged
    {
        private string _channelName = "A";
        private int _activeCamera = 1;
        private bool _isPlaying = false;
        private double _speed = 1.0;
        private TimeSpan _currentPosition = TimeSpan.Zero;
        private bool _isLive = true;
        private ReplayEvent? _selectedEvent;

        public string ChannelName
        {
            get => _channelName;
            set { _channelName = value; OnPropertyChanged(); }
        }

        public int ActiveCamera
        {
            get => _activeCamera;
            set { _activeCamera = value; OnPropertyChanged(); }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        public double Speed
        {
            get => _speed;
            set { _speed = Math.Clamp(value, 0.0, 1.0); OnPropertyChanged(); }
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set { _currentPosition = value; OnPropertyChanged(); }
        }

        public bool IsLive
        {
            get => _isLive;
            set { _isLive = value; OnPropertyChanged(); }
        }

        public ReplayEvent? SelectedEvent
        {
            get => _selectedEvent;
            set { _selectedEvent = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
