using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Openmiks.Services
{
    // Real DirectShow capture: ffmpeg dshow -> rawvideo bgr24 on stdout -> WriteableBitmap.
    // ponytail: pipe + 1 decode thread instead of DirectShow COM interop; swap to a native
    // filter graph only if per-frame latency of the pipe ever shows up in profiling.
    public sealed class CameraCaptureService : IDisposable
    {
        private Process? _proc;
        private Thread? _reader;
        private volatile bool _stop;
        private int _busy;
        private byte[]? _frame;
        private WriteableBitmap? _bitmap;

        public string DeviceName { get; private set; } = string.Empty;
        public int Width { get; private set; } = 1280;
        public int Height { get; private set; } = 720;
        public string LastError { get; private set; } = string.Empty;
        public long FrameCount;
        public WriteableBitmap? Bitmap => _bitmap;

        public bool IsRunning => _reader is { IsAlive: true } && _proc is { HasExited: false };

        public bool Start(string deviceName, int width = 1280, int height = 720, int fps = 30)
        {
            Stop();
            LastError = string.Empty;
            if (string.IsNullOrWhiteSpace(deviceName)) { LastError = "No capture device selected."; return false; }

            string? ffmpeg = RecordingEngine.FindFFmpeg();
            if (ffmpeg == null) { LastError = "ffmpeg not found (looked for vMix\\streaming\\ffmpeg6.exe and PATH)."; return false; }

            string input = $"-f dshow -framerate {fps} -video_size {width}x{height} -i \"video={deviceName}\"";
            if (!Launch(ffmpeg, input, width, height))
            {
                // Device may not accept the requested mode; retry with driver defaults.
                if (!Launch(ffmpeg, $"-f dshow -i \"video={deviceName}\"", width, height))
                {
                    LastError = LastError.Length > 0 ? LastError : "Device did not open.";
                    return false;
                }
            }

            DeviceName = deviceName;
            Width = width;
            Height = height;
            _stop = false;
            _reader = new Thread(ReadLoop) { IsBackground = true, Name = $"dshow:{deviceName}" };
            _reader.Start();
            return true;
        }

        private bool Launch(string ffmpeg, string input, int width, int height)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpeg,
                    Arguments = $"-hide_banner -loglevel error {input} -f rawvideo -pix_fmt bgr24 -",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                var proc = Process.Start(psi);
                if (proc == null) { LastError = "Process start failed."; return false; }

                _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
                _frame = new byte[width * height * 3];
                proc.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data)) LastError = e.Data!;
                };
                proc.BeginErrorReadLine();

                if (proc.WaitForExit(1500) && proc.ExitCode != 0)
                {
                    _proc = proc;
                    Stop();
                    return false;
                }

                _proc = proc;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        private void ReadLoop()
        {
            var stream = _proc?.StandardOutput.BaseStream;
            var buf = _frame;
            if (stream == null || buf == null) return;

            try
            {
                while (!_stop)
                {
                    int read = 0;
                    while (read < buf.Length)
                    {
                        int n = stream.Read(buf, read, buf.Length - read);
                        if (n <= 0) return;
                        read += n;
                    }
                    Interlocked.Increment(ref FrameCount);
                    Push();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CameraCaptureService read: {ex.Message}");
            }
        }

        // Coalesced UI push: drop frames instead of queueing them so the switcher stays live.
        private void Push()
        {
            if (Interlocked.Exchange(ref _busy, 1) == 1) return;

            var bmp = _bitmap;
            var buf = _frame;
            if (bmp == null || buf == null) { Interlocked.Exchange(ref _busy, 0); return; }

            try
            {
                bmp.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        bmp.Lock();
                        bmp.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), buf, Width * 3, 0);
                        bmp.Unlock();
                    }
                    catch (Exception ex) { Debug.WriteLine($"CameraCaptureService blit: {ex.Message}"); }
                    finally { Interlocked.Exchange(ref _busy, 0); }
                });
            }
            catch { Interlocked.Exchange(ref _busy, 0); }
        }

        public void Stop()
        {
            _stop = true;
            try { _reader?.Join(500); } catch { }
            _reader = null;

            try
            {
                if (_proc is { HasExited: false }) _proc.Kill(true);
                _proc?.Dispose();
            }
            catch { }
            _proc = null;
        }

        public void Dispose() => Stop();
    }
}
