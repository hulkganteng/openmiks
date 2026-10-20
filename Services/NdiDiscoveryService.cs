using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace VMixClone.Services
{
    // Real NDI discovery: NDI sources announce over mDNS as _ndi._tcp.local, so browse that
    // service instead of listing invented hosts. Receive/send needs the NDI runtime
    // (vMix ships ndi\x64\Processing.NDI.Lib.dll); that path is not implemented here.
    public sealed class NdiDiscoveryService : INotifyPropertyChanged
    {
        private const string Service = "_ndi._tcp.local";
        private static readonly IPAddress Multicast = IPAddress.Parse("224.0.0.251");

        public ObservableCollection<NdiSourceInfo> Sources { get; } = new();
        public bool IsScanning { get; private set; }
        public string LastError { get; private set; } = string.Empty;
        public int QueryBytes { get; private set; }

        public static bool RuntimePresent =>
            File.Exists(@"C:\Program Files (x86)\vMix\ndi\x64\Processing.NDI.Lib.dll") ||
            Directory.Exists(@"C:\Program Files\NewTek");

        public void Refresh(int listenMs = 1200)
        {
            Sources.Clear();
            LastError = string.Empty;
            IsScanning = true;
            try
            {
                byte[] query = BuildQuery(Service);
                QueryBytes = query.Length;

                using var udp = new UdpClient();
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                udp.Client.SendTimeout = 500;
                udp.Client.ReceiveTimeout = 300;
                udp.Send(query, query.Length, new IPEndPoint(Multicast, 5353));

                var deadline = DateTime.UtcNow.AddMilliseconds(listenMs);
                var names = new Dictionary<string, string>();

                while (DateTime.UtcNow < deadline)
                {
                    try
                    {
                        var from = new IPEndPoint(IPAddress.Any, 0);
                        byte[] packet = udp.Receive(ref from);
                        ParseAnswers(packet, names, from.Address);
                    }
                    catch (SocketException) { }
                }

                foreach (var kv in names)
                {
                    Sources.Add(new NdiSourceInfo { Name = kv.Key, IpAddress = kv.Value, Port = 5960 });
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Debug.WriteLine($"NdiDiscoveryService: {ex.Message}");
            }
            finally { IsScanning = false; }
        }

        public static byte[] BuildQuery(string service)
        {
            var ms = new MemoryStream();
            var header = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            ms.Write(header);
            foreach (var label in service.Split('.'))
            {
                byte[] bytes = Encoding.ASCII.GetBytes(label);
                ms.WriteByte((byte)bytes.Length);
                ms.Write(bytes);
            }
            ms.WriteByte(0x00);
            ms.Write(new byte[] { 0x00, 0x0C, 0x00, 0x01 }); // PTR IN
            return ms.ToArray();
        }

        // Minimal DNS answer reader: enough for PTR/SRV/A names in NDI announcements.
        private static void ParseAnswers(byte[] packet, Dictionary<string, string> names, IPAddress from)
        {
            if (packet.Length < 12) return;
            int qd = (packet[4] << 8) | packet[5];
            int an = (packet[6] << 8) | packet[7];
            int offset = 12;
            for (int i = 0; i < qd; i++) offset = SkipName(packet, offset) + 4;
            if (offset < 0) return;

            string pending = string.Empty;
            for (int i = 0; i < an && offset + 10 <= packet.Length; i++)
            {
                string name = ReadName(packet, ref offset);
                if (offset + 10 > packet.Length) return;
                int type = (packet[offset] << 8) | packet[offset + 1];
                int rdLength = (packet[offset + 8] << 8) | packet[offset + 9];
                offset += 10;
                int rdStart = offset;
                offset += rdLength;

                if (type == 12) // PTR
                {
                    int tmp = rdStart;
                    pending = ReadName(packet, ref tmp);
                    if (!string.IsNullOrEmpty(pending)) names[pending] = from.ToString();
                }
                else if (type == 1 && rdLength == 4 && !string.IsNullOrEmpty(pending)) // A
                {
                    names[pending] = new IPAddress(new[] { packet[rdStart], packet[rdStart + 1], packet[rdStart + 2], packet[rdStart + 3] }).ToString();
                }
            }
        }

        private static int SkipName(byte[] packet, int offset)
        {
            while (offset < packet.Length)
            {
                int len = packet[offset];
                if (len == 0) return offset + 1;
                if ((len & 0xC0) == 0xC0) return offset + 2;
                offset += len + 1;
            }
            return -1;
        }

        private static string ReadName(byte[] packet, ref int offset)
        {
            var sb = new StringBuilder();
            int guard = 0;
            while (offset < packet.Length && guard++ < 64)
            {
                int len = packet[offset];
                if (len == 0) { offset++; break; }
                if ((len & 0xC0) == 0xC0)
                {
                    int ptr = ((len & 0x3F) << 8) | packet[offset + 1];
                    offset += 2;
                    int saved = offset;
                    offset = ptr;
                    sb.Append(ReadName(packet, ref offset));
                    offset = saved;
                    break;
                }
                offset++;
                sb.Append(Encoding.ASCII.GetString(packet, offset, len));
                offset += len;
                sb.Append('.');
            }
            return sb.ToString().TrimEnd('.');
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
