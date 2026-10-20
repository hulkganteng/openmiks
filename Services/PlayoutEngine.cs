using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Openmiks.Models;

namespace Openmiks.Services
{
    // Drives the playlist and photo slideshow models that previously existed as data only:
    // real advance on elapsed time, real file resolution, and a real current item for the
    // InputDisplay to render.
    public sealed class PlayoutEngine : INotifyPropertyChanged, IDisposable
    {        private readonly DispatcherTimer _timer;
        private DateTime _itemStartedAt = DateTime.UtcNow;
        private readonly Random _rnd = new();
        private string _currentFile = string.Empty;
        private bool _isPlaying;

        public VideoListModel VideoList { get; } = new();
        public PhotoSlideshowModel Slideshow { get; } = new();

        public PlayoutMode Mode { get; set; } = PlayoutMode.VideoList;
        public bool IncludeSubfolders { get; set; }
        public bool Shuffle { get; set; }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set { _isPlaying = value; OnPropertyChanged(); }
        }

        // File the display should render right now; empty when the playout has no items.
        public string CurrentFile
        {
            get => _currentFile;
            private set { _currentFile = value; OnPropertyChanged(); }
        }

        public string CurrentTitle => Mode == PlayoutMode.Slideshow
            ? (Slideshow.ImagePaths.Count == 0 ? "-" : Path.GetFileName(Slideshow.ImagePaths[Slideshow.CurrentIndex]))
            : (VideoList.Items.Count == 0 ? "-" : VideoList.Items[VideoList.CurrentIndex].Title);

        public string ProgressText
        {
            get
            {
                double elapsed = (DateTime.UtcNow - _itemStartedAt).TotalSeconds;
                return $"{elapsed:0.0}s / {CurrentItemSeconds:0.0}s";
            }
        }

        public double CurrentItemSeconds => Mode == PlayoutMode.Slideshow
            ? Slideshow.SlideDurationSeconds
            : VideoList.Items[videoSafeIndex].Duration.TotalSeconds;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int videoSafeIndex => VideoList.Items.Count == 0 ? 0 : Math.Clamp(VideoList.CurrentIndex, 0, VideoList.Items.Count - 1);

        public PlayoutEngine()
        {
            // 200ms is fine for advance granularity and keeps the UI timer cheap.
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += OnTick;
        }

        public static readonly string[] VideoExtensions = { ".mp4", ".mkv", ".avi", ".wmv", ".mov", ".m4v" };
        public static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".webp" };

        // Loads real files from disk; non-media files are ignored rather than listed as broken entries.
        public int LoadFolder(string folder)
        {
            VideoList.Items.Clear();
            Slideshow.ImagePaths.Clear();
            if (!Directory.Exists(folder)) return 0;

            var opts = IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            int added = 0;

            foreach (string file in Directory.EnumerateFiles(folder, "*.*", opts))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (VideoExtensions.Contains(ext))
                {
                    VideoList.Items.Add(new PlaylistItem { Title = Path.GetFileNameWithoutExtension(file), FilePath = file });
                    added++;
                }
                else if (ImageExtensions.Contains(ext))
                {
                    Slideshow.ImagePaths.Add(file);
                    added++;
                }
            }

            if (added > 0 && Shuffle) ShuffleAll();
            OnPropertyChanged(nameof(CurrentTitle));
            return added;
        }

        public void ShuffleAll()
        {
            ShuffleList(VideoList.Items);
            var shuffledImages = Slideshow.ImagePaths.OrderBy(_ => _rnd.Next()).ToList();
            Slideshow.ImagePaths.Clear();
            foreach (string p in shuffledImages) Slideshow.ImagePaths.Add(p);
            VideoList.CurrentIndex = 0;
            Slideshow.CurrentIndex = 0;
        }

        private static void ShuffleList<T>(IList<T> list)
        {
            var rnd = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public bool Start()
        {
            if (VideoList.Items.Count == 0 && Slideshow.ImagePaths.Count == 0) return false;
            if (VideoList.Items.Count == 0) Mode = PlayoutMode.Slideshow;
            if (Slideshow.ImagePaths.Count == 0 && VideoList.Items.Count > 0) Mode = PlayoutMode.VideoList;

            IsPlaying = true;
            _itemStartedAt = DateTime.UtcNow;
            CurrentFile = ResolveCurrentFile();
            _timer.Start();
            return true;
        }

        public void Stop()
        {
            IsPlaying = false;
            _timer.Stop();
        }

        public void Next()
        {
            if (Mode == PlayoutMode.Slideshow)
            {
                if (Slideshow.ImagePaths.Count == 0) return;
                Slideshow.CurrentIndex = (Slideshow.CurrentIndex + 1) % Slideshow.ImagePaths.Count;
            }
            else
            {
                if (VideoList.Items.Count == 0) return;
                VideoList.Next();
            }
            _itemStartedAt = DateTime.UtcNow;
            CurrentFile = ResolveCurrentFile();
            OnPropertyChanged(nameof(CurrentTitle));
        }

        public void Previous()
        {
            if (Mode == PlayoutMode.Slideshow)
            {
                if (Slideshow.ImagePaths.Count == 0) return;
                Slideshow.CurrentIndex = (Slideshow.CurrentIndex - 1 + Slideshow.ImagePaths.Count) % Slideshow.ImagePaths.Count;
            }
            else
            {
                if (VideoList.Items.Count == 0) return;
                VideoList.Previous();
            }
            _itemStartedAt = DateTime.UtcNow;
            CurrentFile = ResolveCurrentFile();
            OnPropertyChanged(nameof(CurrentTitle));
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!IsPlaying) return;

            double elapsed = (DateTime.UtcNow - _itemStartedAt).TotalSeconds;
            if (elapsed >= CurrentItemSeconds)
            {
                Next();
            }
            else
            {
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private string ResolveCurrentFile()
        {
            if (Mode == PlayoutMode.Slideshow)
            {
                return Slideshow.ImagePaths.Count == 0 ? string.Empty : Slideshow.ImagePaths[Slideshow.CurrentIndex];
            }
            return VideoList.Items.Count == 0 ? string.Empty : VideoList.Items[videoSafeIndex].FilePath;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
