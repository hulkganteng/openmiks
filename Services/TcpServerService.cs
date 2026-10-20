using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Openmiks.Models;

namespace Openmiks.Services
{
    public class TcpServerService
    {
        private TcpListener? _listener;
        private readonly ProductionEngine _engine;
        private readonly int _port;
        private bool _isRunning = false;
        private readonly List<StreamWriter> _tallySubscribers = new();
        private readonly object _lock = new();

        public int Port => _port;
        public bool IsRunning => _isRunning;

        // ponytail: Native vMix TCP API on port 8099 supporting TALLY, FUNCTION, XML, SUBSCRIBE; matches vMix.exe TCPAPI specification.
        public TcpServerService(ProductionEngine engine, int port = 8099)
        {
            _engine = engine;
            _port = port;
            _engine.PropertyChanged += OnEnginePropertyChanged;
        }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;
                _listener.BeginAcceptTcpClient(OnAcceptClient, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP API Server start failed on port {_port}: {ex.Message}");
                _isRunning = false;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                lock (_lock)
                {
                    foreach (var sub in _tallySubscribers)
                    {
                        try { sub.Close(); } catch { }
                    }
                    _tallySubscribers.Clear();
                }
                _listener?.Stop();
                _listener = null;
            }
            catch { }
        }

        private void OnAcceptClient(IAsyncResult ar)
        {
            if (!_isRunning || _listener == null) return;

            TcpClient client;
            try
            {
                client = _listener.EndAcceptTcpClient(ar);
                _listener.BeginAcceptTcpClient(OnAcceptClient, null);
            }
            catch
            {
                return;
            }

            Task.Run(() => HandleClientSession(client));
        }

        private void HandleClientSession(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            {
                // Send vMix welcome banner
                writer.WriteLine("VERSION OK 29.0.0.49");

                string? line;
                while (_isRunning && (line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string response = ProcessCommand(line, writer);
                    if (!string.IsNullOrEmpty(response))
                    {
                        writer.WriteLine(response);
                    }
                }
            }
        }

        private string ProcessCommand(string command, StreamWriter writer)
        {
            string[] parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string verb = parts[0].ToUpperInvariant();
            string arg = parts.Length > 1 ? parts[1] : string.Empty;

            switch (verb)
            {
                case "TALLY":
                    return $"TALLY OK {BuildTallyString()}";

                case "SUBSCRIBE":
                    if (arg.ToUpperInvariant().Contains("TALLY"))
                    {
                        lock (_lock)
                        {
                            if (!_tallySubscribers.Contains(writer))
                                _tallySubscribers.Add(writer);
                        }
                        return $"SUBSCRIBE TALLY OK {BuildTallyString()}";
                    }
                    return "SUBSCRIBE OK";

                case "UNSUBSCRIBE":
                    lock (_lock) { _tallySubscribers.Remove(writer); }
                    return "UNSUBSCRIBE OK";

                case "XML":
                    return $"XML {BuildXmlState()}";

                case "FUNCTION":
                    // Wire format: "FUNCTION Fade Duration=500 Input=2"
                    ExecuteFunctionCommand(arg);
                    return $"FUNCTION OK {arg}";

                case "QUIT":
                    return "QUIT OK";

                default:
                    // Bare function names (vMix TCP accepts "Cut", "Fade Duration=500", ...).
                    ExecuteFunctionCommand(command);
                    return $"FUNCTION OK {command}";
            }
        }

        private void ExecuteFunctionCommand(string arg)
        {
            // Parses e.g. "Cut" or "Fade Duration=500" or "ActiveInput Input=2"
            string[] parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string funcName = parts[0];
            string queryParams = parts.Length > 1 ? parts[1] : string.Empty;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.HasShutdownStarted)
            {
                dispatcher.BeginInvoke(() => _engine.ExecuteApiFunction(funcName, queryParams.Replace(' ', '&')));
            }
            else
            {
                _engine.ExecuteApiFunction(funcName, queryParams.Replace(' ', '&'));
            }
        }

        private string BuildTallyString()
        {
            var sb = new StringBuilder();
            foreach (var inp in _engine.Inputs)
            {
                if (inp == _engine.ProgramInput) sb.Append('1'); // 1 = Program / Red
                else if (inp == _engine.PreviewInput) sb.Append('2'); // 2 = Preview / Green
                else sb.Append('0'); // 0 = Off
            }
            return sb.ToString();
        }

        private string BuildXmlState()
        {
            return $"<vmix><version>29.0.0.49</version><active>{_engine.ProgramInput?.Number ?? 0}</active><preview>{_engine.PreviewInput?.Number ?? 0}</preview><recording>{_engine.IsRecording}</recording><streaming>{_engine.IsStreaming}</streaming></vmix>";
        }

        private void OnEnginePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductionEngine.ProgramInput) || e.PropertyName == nameof(ProductionEngine.PreviewInput))
            {
                BroadcastTally();
            }
        }

        private void BroadcastTally()
        {
            string tallyMsg = $"TALLY OK {BuildTallyString()}";
            lock (_lock)
            {
                for (int i = _tallySubscribers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _tallySubscribers[i].WriteLine(tallyMsg);
                    }
                    catch
                    {
                        _tallySubscribers.RemoveAt(i);
                    }
                }
            }
        }
    }
}
