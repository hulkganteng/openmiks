using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Openmiks.Services
{
    public class NdiSourceInfo
    {
        public string Name { get; set; } = "NDI Source";
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5960;
    }

    public class SrtStreamConfig : INotifyPropertyChanged
    {
        private string _mode = "Caller"; // Caller or Listener
        private string _host = "127.0.0.1";
        private int _port = 9000;
        private int _latencyMs = 120;
        private string _passphrase = string.Empty;
        private bool _isActive = false;

        public string Mode
        {
            get => _mode;
            set { _mode = value; OnPropertyChanged(); }
        }

        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public int LatencyMs
        {
            get => _latencyMs;
            set { _latencyMs = Math.Clamp(value, 20, 8000); OnPropertyChanged(); }
        }

        public string Passphrase
        {
            get => _passphrase;
            set { _passphrase = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class NdiSrtService : INotifyPropertyChanged
    {
        public ObservableCollection<NdiSourceInfo> DiscoveredNdiSources { get; } = new();
        public SrtStreamConfig SrtConfig { get; } = new();

        private bool _isNdiOutput1Enabled = true;
        private bool _isNdiOutput2Enabled = false;

        public bool IsNdiOutput1Enabled
        {
            get => _isNdiOutput1Enabled;
            set { _isNdiOutput1Enabled = value; OnPropertyChanged(); }
        }

        public bool IsNdiOutput2Enabled
        {
            get => _isNdiOutput2Enabled;
            set { _isNdiOutput2Enabled = value; OnPropertyChanged(); }
        }

        public NdiSrtService()
        {
            RefreshNdiSources();
        }

        public void RefreshNdiSources()
        {
            // Honest addressable endpoints plus live mDNS discovery; nothing invented.
            DiscoveredNdiSources.Clear();

            if (!NdiDiscoveryService.RuntimePresent)
            {
                DiscoveredNdiSources.Add(new NdiSourceInfo
                {
                    Name = "NDI runtime not installed (install NewTek NDI Tools)",
                    IpAddress = "0.0.0.0",
                    Port = 0
                });
                return;
            }

            NdiDiscovery.Refresh();
            foreach (var src in NdiDiscovery.Sources)
            {
                DiscoveredNdiSources.Add(src);
            }
        }

        public NdiDiscoveryService NdiDiscovery { get; } = new();

        public string BuildSrtUrl()
        {
            // Standard SRT URI format: srt://host:port?mode=caller&latency=120&passphrase=xxx
            string url = $"srt://{SrtConfig.Host}:{SrtConfig.Port}?mode={SrtConfig.Mode.ToLowerInvariant()}&latency={SrtConfig.LatencyMs}";
            if (!string.IsNullOrEmpty(SrtConfig.Passphrase))
            {
                url += $"&passphrase={Uri.EscapeDataString(SrtConfig.Passphrase)}";
            }
            return url;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
