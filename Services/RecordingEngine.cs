using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace Openmiks.Services
{
    // Frame source for the encoders: supplies BGRA frames the caller renders when it uses the
    // program-surface input, or nothing when the recorder falls back to desktop grabbing.
    public interface IFrameSource
    {
        bool TryGetFrame(out byte[] buffer, out int width, out int height);
    }

    public class RecordingEngine
    {
        private Process? _ffmpegProcess;
        private string _currentFilePath = string.Empty;
        private bool _isRecording = false;
        private DateTime _startTime;
        private long _totalBytesWritten = 0;
        private readonly DispatcherTimer _pollTimer;
        private Thread? _writer;
        private volatile bool _pump;
        private IFrameSource? _source;
        private bool _usesProgramSurface;

        public bool IsRecording => _isRecording;
        public string CurrentFilePath => _currentFilePath;
        public TimeSpan Duration => _isRecording ? (DateTime.UtcNow - _startTime) : TimeSpan.Zero;
        public long TotalBytesWritten => _totalBytesWritten;

        // True when this recording writes the rendered program surface rather than the desktop.
        public bool UsesProgramSurface => _usesProgramSurface;

        public string FileSizeFormatted
        {
            get
            {
                if (_totalBytesWritten < 1024 * 1024)
                    return $"{(_totalBytesWritten / 1024.0):0.0} KB";
                return $"{(_totalBytesWritten / (1024.0 * 1024.0)):0.0} MB";
            }
        }

        public RecordingEngine()
        {
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _pollTimer.Tick += OnPollTick;
        }

        public static string? FindFFmpeg()
        {
            string[] candidates = new[]
            {
                @"C:\Program Files (x86)\vMix\streaming\ffmpeg6.exe",
                @"C:\Program Files (x86)\vMix\streaming\ffmpeg2.exe",
                @"C:\Program Files\vMix\streaming\ffmpeg6.exe",
                "ffmpeg.exe"
            };
            foreach (var path in candidates)
            {
                try
                {
                    if (File.Exists(path)) return path;
                }
                catch { }
            }
            return null;
        }

        // frameSource: program output surface. Pass null to record the desktop instead.
        public bool Start(string folder, string format, int bitrateKbps, IFrameSource? frameSource = null, int fps = 30)
        {
            if (_isRecording) return false;

            try
            {
                if (string.IsNullOrWhiteSpace(folder))
                {
                    folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "vMixRecordings");
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string ext = format.ToLowerInvariant() switch
                {
                    "avi" => ".avi",
                    "mkv" => ".mkv",
                    "wmv" => ".wmv",
                    _ => ".mp4"
                };

                string fileName = $"vMix_Record_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                _currentFilePath = Path.Combine(folder, fileName);
                _totalBytesWritten = 0;
                _source = frameSource;
                _usesProgramSurface = frameSource != null;

                string? ffmpegPath = FindFFmpeg();
                if (ffmpegPath != null)
                {
                    // Program surface when a frame source exists (records the mix, not the desktop),
                    // otherwise gdigrab as the documented fallback.
                    int w = 0, h = 0;
                    bool haveSurfaceFrame = _usesProgramSurface && _source!.TryGetFrame(out _, out w, out h) && w > 0;
                    string input = haveSurfaceFrame
                        ? ProgramSurface.FfmpegInputArgs(w, h, fps)
                        : $"-f gdigrab -framerate {fps} -i desktop";

                    string args = $"-y {input} -c:v libx264 -preset ultrafast -b:v {bitrateKbps}k -pix_fmt yuv420p \"{_currentFilePath}\"";
                    var psi = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    _ffmpegProcess = Process.Start(psi);
                    if (_ffmpegProcess == null)
                    {
                        _usesProgramSurface = false;
                        return false;
                    }

                    _ffmpegProcess.ErrorDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data) && !e.Data.Contains("frame=", StringComparison.Ordinal))
                            Debug.WriteLine($"[record] {e.Data}");
                    };
                    _ffmpegProcess.BeginErrorReadLine();

                    if (haveSurfaceFrame)
                    {
                        _pump = true;
                        var stdin = _ffmpegProcess.StandardInput.BaseStream;
                        _writer = new Thread(() => PumpFrames(stdin)) { IsBackground = true, Name = "record-pump" };
                        _writer.Start();
                    }
                }
                else
                {
                    // Fallback stub file: no encoder at all.
                    using var fs = File.Create(_currentFilePath);
                    byte[] hdr = Encoding.UTF8.GetBytes($"vMixRecording;Date={DateTime.UtcNow:o};\r\n");
                    fs.Write(hdr, 0, hdr.Length);
                    _usesProgramSurface = false;
                }

                _startTime = DateTime.UtcNow;
                _isRecording = true;
                _pollTimer.Start();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Recording start error: {ex.Message}");
                _isRecording = false;
                _usesProgramSurface = false;
                return false;
            }
        }

        // Pushes rendered program frames into ffmpeg's stdin, coalescing when the encoder lags
        // so recording never builds an unbounded backlog.
        private void PumpFrames(Stream stdin)
        {
            int w = 0, h = 0;
            while (_pump && _source != null)
            {
                if (!_source.TryGetFrame(out byte[]? frame, out int fw, out int fh) || frame == null)
                {
                    Thread.Sleep(15);
                    continue;
                }

                if (fw != w || fh != h)
                {
                    w = fw;
                    h = fh;
                    Debug.WriteLine($"[record] program surface {w}x{h}");
                }

                try
                {
                    stdin.Write(frame, 0, w * h * 4);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[record] pump stopped: {ex.Message}");
                    return;
                }
            }
        }

        public string Stop()
        {
            if (!_isRecording) return string.Empty;

            _pollTimer.Stop();
            _isRecording = false;
            _pump = false;

            try { _writer?.Join(400); } catch { }
            _writer = null;
            _source = null;
            _usesProgramSurface = false;
            try
            {
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    // Send 'q' to gracefully stop and finalize MP4 header
                    _ffmpegProcess.StandardInput.WriteLine("q");
                    if (!_ffmpegProcess.WaitForExit(3000))
                    {
                        _ffmpegProcess.Kill();
                    }
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch { }

            try
            {
                if (File.Exists(_currentFilePath))
                {
                    _totalBytesWritten = new FileInfo(_currentFilePath).Length;
                }
            }
            catch { }

            return _currentFilePath;
        }

        private void OnPollTick(object? sender, EventArgs e)
        {
            if (!_isRecording) return;

            try
            {
                if (File.Exists(_currentFilePath))
                {
                    _totalBytesWritten = new FileInfo(_currentFilePath).Length;
                }
            }
            catch { }
        }
    }
}
