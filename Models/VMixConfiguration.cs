using System.IO;
using System.Text.Json;

namespace Openmiks.Models
{
    public class VMixConfiguration
    {
        // Capture / transport flags used by the ffmpeg-based engines
        public string FfmpegPath { get; set; } = string.Empty;      // empty = auto-detect (vMix\streaming\ffmpeg6.exe, then PATH)
        public bool EnableNdi { get; set; } = false;                // NDI runtime is third-party; off until installed
        public bool EnableSrt { get; set; } = true;                 // live SRT output rides on ffmpeg's srt:// writer

        // Display / Output
        public string OutputResolution { get; set; } = "1920x1080";
        public string FrameRate { get; set; } = "59.94p";
        public string AspectRatio { get; set; } = "16:9";
        public int FullscreenMonitorIndex { get; set; } = 0;
        public string VideoRenderer { get; set; } = "Direct3D 11";

        // Recording
        public string RecordingFolder { get; set; } = string.Empty;
        public string RecordingFormat { get; set; } = "MP4";
        public int RecordingBitrateKbps { get; set; } = 8000;
        public int RecordingAudioBitrateKbps { get; set; } = 192;
        public bool RecordingAutoSplit { get; set; } = false;

        // Streaming
        public string StreamingService { get; set; } = "Custom RTMP Server";
        public string StreamingUrl { get; set; } = "rtmp://a.rtmp.youtube.com/live2";
        public string StreamingKey { get; set; } = "";
        public int StreamingBitrateKbps { get; set; } = 6000;
        public string StreamingEncoder { get; set; } = "H.264 Software / NVENC";

        // Audio
        public string AudioMasterDevice { get; set; } = "Default Windows Playback";
        public string AudioBusADevice { get; set; } = "Default Communications Device";
        public string MicAuxDevice1 { get; set; } = "";
        public string MicAuxDevice2 { get; set; } = "";
        public int AudioSampleRate { get; set; } = 48000;
        public bool AutoAudioMixing { get; set; } = true;

        // Web Server
        public int WebServerPort { get; set; } = 8088;
        public bool WebServerEnabled { get; set; } = true;
        public string WebServerPassword { get; set; } = "";

        // Performance
        public bool HardwareAcceleration { get; set; } = true;
        public string ProcessPriority { get; set; } = "High";

        public VMixConfiguration()
        {
            try
            {
                string myVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                RecordingFolder = Path.Combine(myVideos, "vMixRecordings");
            }
            catch
            {
                RecordingFolder = "C:\\vMixRecordings";
            }
        }

        private static string GetConfigPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "Openmiks");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "vmix_settings.json");
        }

        public static VMixConfiguration Load()
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var cfg = JsonSerializer.Deserialize<VMixConfiguration>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { }
            return new VMixConfiguration();
        }

        public void Save()
        {
            try
            {
                string path = GetConfigPath();
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
        }
    }
}
