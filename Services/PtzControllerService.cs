using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Openmiks.Services
{
    public enum PtzProtocol
    {
        ViscaUdp,
        ViscaTcp,
        SonyCgi,
        PtzOpticsHttp
    }

    public class PtzControllerService
    {
        // Standard VISCA over IP uses UDP port 52381 (Sony / Panasonic / PTZOptics)
        public const int DefaultViscaPort = 52381;

        public static async Task SendViscaCommandAsync(string ip, byte[] command, int port = DefaultViscaPort)
        {
            if (string.IsNullOrWhiteSpace(ip)) return;

            try
            {
                using var udp = new UdpClient();
                // Standard VISCA over IP header: 01 00 00 (payload length) 00 00 00 01 + command
                byte[] packet = new byte[8 + command.Length];
                packet[0] = 0x01; // VISCA command
                packet[1] = 0x00;
                packet[2] = (byte)(command.Length >> 8);
                packet[3] = (byte)(command.Length & 0xFF);
                packet[7] = 0x01; // sequence
                Buffer.BlockCopy(command, 0, packet, 8, command.Length);

                await udp.SendAsync(packet, packet.Length, ip, port);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VISCA command error: {ex.Message}");
            }
        }

        // VISCA: Pan/Tilt Drive (81 01 06 01 pan_speed tilt_speed pan_dir tilt_dir FF)
        public static Task PanTiltAsync(string ip, int panSpeed, int tiltSpeed, int panDir, int tiltDir, int port = DefaultViscaPort)
        {
            byte[] cmd = new byte[] {
                0x81, 0x01, 0x06, 0x01,
                (byte)Math.Clamp(panSpeed, 1, 24),
                (byte)Math.Clamp(tiltSpeed, 1, 20),
                (byte)panDir, (byte)tiltDir,
                0xFF
            };
            return SendViscaCommandAsync(ip, cmd, port);
        }

        public static Task StopPanTiltAsync(string ip, int port = DefaultViscaPort) => PanTiltAsync(ip, 0, 0, 0x03, 0x03, port);
        public static Task PanLeftAsync(string ip, int speed = 12, int port = DefaultViscaPort) => PanTiltAsync(ip, speed, speed, 0x01, 0x03, port);
        public static Task PanRightAsync(string ip, int speed = 12, int port = DefaultViscaPort) => PanTiltAsync(ip, speed, speed, 0x02, 0x03, port);
        public static Task TiltUpAsync(string ip, int speed = 12, int port = DefaultViscaPort) => PanTiltAsync(ip, speed, speed, 0x03, 0x01, port);
        public static Task TiltDownAsync(string ip, int speed = 12, int port = DefaultViscaPort) => PanTiltAsync(ip, speed, speed, 0x03, 0x02, port);

        // Zoom: 81 01 04 07 2X FF (Tele / Zoom In), 81 01 04 07 3X FF (Wide / Zoom Out), 81 01 04 07 00 FF (Stop)
        public static Task ZoomInAsync(string ip, int speed = 5, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x07, (byte)(0x20 | (Math.Clamp(speed, 0, 7) & 0x07)), 0xFF }, port);

        public static Task ZoomOutAsync(string ip, int speed = 5, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x07, (byte)(0x30 | (Math.Clamp(speed, 0, 7) & 0x07)), 0xFF }, port);

        public static Task ZoomStopAsync(string ip, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x07, 0x00, 0xFF }, port);

        // Focus commands (81 01 04 38 02 FF Auto, 81 01 04 38 03 FF Manual)
        public static Task FocusAutoAsync(string ip, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x38, 0x02, 0xFF }, port);

        public static Task FocusManualAsync(string ip, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x38, 0x03, 0xFF }, port);

        public static Task FocusNearAsync(string ip, int speed = 2, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x08, (byte)(0x30 | (Math.Clamp(speed, 0, 7) & 0x07)), 0xFF }, port);

        public static Task FocusFarAsync(string ip, int speed = 2, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x08, (byte)(0x20 | (Math.Clamp(speed, 0, 7) & 0x07)), 0xFF }, port);

        public static Task FocusStopAsync(string ip, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x08, 0x00, 0xFF }, port);

        // Presets: Recall (0x02), Store (0x01), Clear (0x00)
        public static Task RecallPresetAsync(string ip, int presetNumber, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x02, (byte)Math.Clamp(presetNumber, 0, 255), 0xFF }, port);

        public static Task StorePresetAsync(string ip, int presetNumber, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x01, (byte)Math.Clamp(presetNumber, 0, 255), 0xFF }, port);

        public static Task ClearPresetAsync(string ip, int presetNumber, int port = DefaultViscaPort) =>
            SendViscaCommandAsync(ip, new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x00, (byte)Math.Clamp(presetNumber, 0, 255), 0xFF }, port);

        // HTTP / CGI Control for Sony and PTZOptics
        public static async Task SendCgiCommandAsync(string ip, string pathAndQuery, int port = 80)
        {
            if (string.IsNullOrWhiteSpace(ip)) return;
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                string url = $"http://{ip}:{port}/{pathAndQuery.TrimStart('/')}";
                await http.GetAsync(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CGI command error: {ex.Message}");
            }
        }
    }
}
