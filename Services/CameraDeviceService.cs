using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Openmiks.Services
{
    // Enumerates real DirectShow capture devices by asking the installed ffmpeg.
    // Evidence: vMix ships ffmpeg6.exe (dshow device available) and its own picker is
    // vMix.frmCameraDropdown + vMix.AVEnumerator; the device list never comes from a constant.
    public static class CameraDeviceService
    {
        private static List<string>? _videoCache;
        private static List<string>? _audioCache;

        private static readonly Regex DeviceRx = new("\"([^\"]+)\"\\s*\\((video|audio)\\)", RegexOptions.Compiled);

        public static IReadOnlyList<string> VideoDevices => _videoCache ??= Enumerate("video");
        public static IReadOnlyList<string> AudioDevices => _audioCache ??= Enumerate("audio");

        public static void Refresh()
        {
            _videoCache = null;
            _audioCache = null;
        }

        private static List<string> Enumerate(string kind)
        {
            var found = new List<string>();
            string? ffmpeg = RecordingEngine.FindFFmpeg();
            if (ffmpeg == null) return found;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpeg,
                    Arguments = "-hide_banner -list_devices true -f dshow -i dummy",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p == null) return found;
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(5000);

                foreach (Match m in DeviceRx.Matches(stderr))
                {
                    // Skip the "Alternative name \"@device_pnp_...\"" mirror lines and the dummy input echo.
                    if (stderr.Contains("Alternative name \"" + m.Groups[1].Value + "\"")) continue;
                    if (m.Groups[1].Value == "dummy") continue;
                    if (m.Groups[2].Value != kind) continue;
                    if (!found.Contains(m.Groups[1].Value)) found.Add(m.Groups[1].Value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CameraDeviceService: {ex.Message}");
            }
            return found;
        }
    }
}
