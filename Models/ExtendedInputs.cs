using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VMixClone.Models
{
    // Which list the playout engine advances.
    public enum PlayoutMode
    {
        VideoList,
        Slideshow
    }

    public class PlaylistItem : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Media Clip";
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(2);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class VideoListModel : INotifyPropertyChanged
    {
        public ObservableCollection<PlaylistItem> Items { get; } = new();
        private int _currentIndex = 0;
        private bool _isLoop = true;

        public int CurrentIndex
        {
            get => _currentIndex;
            set { _currentIndex = value; OnPropertyChanged(); }
        }

        public bool IsLoop
        {
            get => _isLoop;
            set { _isLoop = value; OnPropertyChanged(); }
        }

        public void Next()
        {
            if (Items.Count == 0) return;
            CurrentIndex = (CurrentIndex + 1) % Items.Count;
        }

        public void Previous()
        {
            if (Items.Count == 0) return;
            CurrentIndex = (CurrentIndex - 1 + Items.Count) % Items.Count;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PhotoSlideshowModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ImagePaths { get; } = new();
        private int _currentIndex = 0;
        private int _slideDurationSeconds = 5;

        public int CurrentIndex
        {
            get => _currentIndex;
            set { _currentIndex = value; OnPropertyChanged(); }
        }

        public int SlideDurationSeconds
        {
            get => _slideDurationSeconds;
            set { _slideDurationSeconds = Math.Max(1, value); OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
