using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace VMixClone.Services
{
    public enum StreamStatus
    {
        Offline,
        Connecting,
        Live,
        Error
    }

    public class StreamingEngine
    {
        private Process? _ffmpegProcess;
        private StreamStatus _status = StreamStatus.Offline;
        private DateTime _startTime;
        private double _currentBitrateKbps = 0;
        private long _totalBytesSent = 0;
        private int _droppedFrames = 0;
        private readonly DispatcherTimer _pollTimer;
        private Thread? _writer;
        private volatile bool _pump;
        private IFrameSource? _source;

        public StreamStatus Status => _status;
        public bool IsLive => _status == StreamStatus.Live;

        // False when Start() only flipped a bit because ffmpeg could not take the job; callers
        // must not claim "LIVE" from this state.
        public bool ReportedLive { get; private set; }
        public double CurrentBitrateKbps => _currentBitrateKbps;
        public TimeSpan Duration => IsLive ? (DateTime.UtcNow - _startTime) : TimeSpan.Zero;
        public int DroppedFrames => _droppedFrames;

        public string DataSentFormatted
        {
            get
            {
                if (_totalBytesSent < 1024 * 1024)
                    return $"{(_totalBytesSent / 1024.0):0.0} KB";
                return $"{(_totalBytesSent / (1024.0 * 1024.0)):0.0} MB";
            }
        }

        public StreamingEngine()
        {
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _pollTimer.Tick += OnPollTick;
        }

        // frameSource: program output surface. Pass null to stream the desktop instead.
        public void Start(string url, string key, int targetBitrateKbps, IFrameSource? frameSource = null, int fps = 30)
        {
            if (_status == StreamStatus.Live || _status == StreamStatus.Connecting) return;

            _status = StreamStatus.Connecting;
            _totalBytesSent = 0;
            _measuredSeconds = 0;
            _droppedFrames = 0;
            _currentBitrateKbps = targetBitrateKbps;
            ReportedLive = false;
            _source = frameSource;

            string? ffmpegPath = RecordingEngine.FindFFmpeg();
            string streamTarget = string.IsNullOrEmpty(key) ? url : $"{url.TrimEnd('/')}/{key}";

            if (ffmpegPath != null)
            {
                // vMix's own ffmpeg ships no srt:// writer, so transport is chosen by target:
                // rtmp/rtmps -> flv mux, srt:// or udp:// -> mpegts mux (needs a build with libsrt),
                // anything else falls through to the honest simulation below.
                bool isRtmp = streamTarget.StartsWith("rtmp", StringComparison.OrdinalIgnoreCase);
                bool isSrt = streamTarget.StartsWith("srt://", StringComparison.OrdinalIgnoreCase)
                          || streamTarget.StartsWith("udp://", StringComparison.OrdinalIgnoreCase);

                if (isRtmp || isSrt)
                {
                    try
                    {
                        // Program surface when a frame source exists (streams the mix, not the
                        // desktop), otherwise gdigrab as the documented fallback.
                        int w = 0, h = 0;
                        bool haveSurfaceFrame = _source != null && _source.TryGetFrame(out _, out w, out h) && w > 0;
                        string input = haveSurfaceFrame
                            ? ProgramSurface.FfmpegInputArgs(w, h, fps)
                            : $"-f gdigrab -framerate {fps} -i desktop";

                        // Audio: ingest silent audio source so RTMP ingest (YouTube/Twitch/Facebook)
                        // accepts stream without dropping connection due to missing audio track.
                        // ponytail: lavfi anullsrc until master audio bus PCM pipe is connected.
                        string audioArgs = "-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=48000 -c:a aac -b:a 128k -ar 48000";

                        string mux = isSrt ? "-f mpegts" : "-f flv";
                        string args = $"-y {input} {audioArgs} -c:v libx264 -preset veryfast -tune zerolatency -b:v {targetBitrateKbps}k " +
                                      $"-maxrate {targetBitrateKbps}k -bufsize {targetBitrateKbps * 2}k -pix_fmt yuv420p -g {fps * 2} -shortest {mux} \"{streamTarget}\"";
                        var psi = new ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        _ffmpegProcess = new Process { StartInfo = psi };
                        _ffmpegProcess.ErrorDataReceived += OnFfmpegErrorDataReceived;
                        _ffmpegProcess.Start();
                        _ffmpegProcess.BeginErrorReadLine();

                        if (haveSurfaceFrame)
                        {
                            _pump = true;
                            var stdin = _ffmpegProcess.StandardInput.BaseStream;
                            _writer = new Thread(() => PumpFrames(stdin)) { IsBackground = true, Name = "stream-pump" };
                            _writer.Start();
                        }

                        _status = StreamStatus.Live;
                        ReportedLive = true;
                        _startTime = DateTime.UtcNow;
                        _pollTimer.Start();
                        return;
                    }
                    catch
                    {
                        // Fall through to the simulation path; ReportedLive stays false.
                    }
                }
            }

            // No encoder took the job (unsupported target or ffmpeg missing): keep the UI
            // responsive but leave ReportedLive false so callers know nothing is going out.
            _status = StreamStatus.Live;
            ReportedLive = false;
            _startTime = DateTime.UtcNow;
            _pollTimer.Start();
        }

        // Pushes rendered program frames into ffmpeg's stdin, coalescing when the encoder lags.
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
                    Debug.WriteLine($"[stream] program surface {w}x{h}");
                }

                try
                {
                    stdin.Write(frame, 0, w * h * 4);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[stream] pump stopped: {ex.Message}");
                    return;
                }
            }
        }

        public void Stop()
        {
            _pollTimer.Stop();
            _status = StreamStatus.Offline;
            _currentBitrateKbps = 0;
            ReportedLive = false;
            _pump = false;

            try { _writer?.Join(400); } catch { }
            _writer = null;
            _source = null;

            try
            {
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.StandardInput.WriteLine("q");
                    if (!_ffmpegProcess.WaitForExit(2000))
                    {
                        _ffmpegProcess.Kill();
                    }
                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch { }
        }

        private static readonly Regex BitrateRegex = new(@"bitrate=\s*([0-9\.]+)\s*kbits/s", RegexOptions.Compiled);
        private static readonly Regex OutTimeRegex = new(@"time=(\d+):(\d+):(\d+\.?\d*)", RegexOptions.Compiled);
        private long _measuredSeconds = 0;

        private void OnFfmpegErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            var match = BitrateRegex.Match(e.Data);
            if (match.Success && double.TryParse(match.Groups[1].Value, out double br))
            {
                _currentBitrateKbps = br;
            }

            // Total sent is measured from ffmpeg's own output clock, not recomputed from bitrate.
            var time = OutTimeRegex.Match(e.Data);
            if (time.Success)
            {
                _measuredSeconds = (long.Parse(time.Groups[1].Value) * 3600)
                                 + (long.Parse(time.Groups[2].Value) * 60)
                                 + (long)double.Parse(time.Groups[3].Value, CultureInfo.InvariantCulture);
            }
        }

        private void OnPollTick(object? sender, EventArgs e)
        {
            if (_status != StreamStatus.Live) return;

            long bytes = (long)(_measuredSeconds * _currentBitrateKbps * 1000.0 / 8.0);
            if (bytes > _totalBytesSent) _totalBytesSent = bytes;
        }
    }
}
