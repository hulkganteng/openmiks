using System.Diagnostics;
using System.Globalization;

namespace Openmiks.Services
{
    // Real peak levels from a DirectShow audio device: ffmpeg astats -> ametadata prints
    // "lavfi.astats.Overall.RMS_level=<dB>" on stderr; converted to a 0..1 linear value.
    // Replaces the simulated jitter meters in ProductionEngine.
    public sealed class AudioMeterService : IDisposable
    {
        private Process? _proc;
        private Thread? _reader;
        private volatile bool _stop;

        public string DeviceName { get; private set; } = string.Empty;
        public bool IsRunning => _reader is { IsAlive: true } && _proc is { HasExited: false };
        public double LevelL { get; private set; }
        public double LevelR { get; private set; }
        public string LastError { get; private set; } = string.Empty;

        private const string MeterKey = "lavfi.astats.Overall.RMS_level";

        public static double DbToLinear(string line)
        {
            int eq = line.IndexOf('=');
            if (eq < 0) return 0.0;
            string raw = line[(eq + 1)..].Trim();
            if (raw.StartsWith("-inf", StringComparison.OrdinalIgnoreCase)) return 0.0;
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double db)) return 0.0;
            double linear = Math.Pow(10.0, db / 20.0);
            return Math.Clamp(linear * 1.6, 0.0, 1.0); // meter headroom so 0 dBFS sits near full scale
        }

        public bool Start(string deviceName)
        {
            Stop();
            LastError = string.Empty;
            if (string.IsNullOrWhiteSpace(deviceName)) { LastError = "No audio device selected."; return false; }

            string? ffmpeg = RecordingEngine.FindFFmpeg();
            if (ffmpeg == null) { LastError = "ffmpeg not found."; return false; }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpeg,
                    Arguments = $"-hide_banner -loglevel error -f dshow -i \"audio={deviceName}\" " +
                                $"-af \"astats=metadata=1:reset=1,ametadata=print:key={MeterKey}\" -f null -",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                _proc = Process.Start(psi);
                if (_proc == null) { LastError = "Process start failed."; return false; }

                DeviceName = deviceName;
                _stop = false;
                _reader = new Thread(ReadLoop) { IsBackground = true, Name = $"astats:{deviceName}" };
                _reader.Start();
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
            try
            {
                string? line;
                while (!_stop && (line = _proc?.StandardError.ReadLine()) != null)
                {
                    if (line.Contains(MeterKey, StringComparison.Ordinal))
                    {
                        double level = DbToLinear(line);
                        LevelL = level;
                        LevelR = level;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        LastError = line;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"AudioMeterService: {ex.Message}"); }
        }

        public void Stop()
        {
            _stop = true;
            try { _reader?.Join(400); } catch { }
            _reader = null;
            try
            {
                if (_proc is { HasExited: false }) _proc.Kill(true);
                _proc?.Dispose();
            }
            catch { }
            _proc = null;
            LevelL = 0.0;
            LevelR = 0.0;
        }

        public void Dispose() => Stop();
    }
}
