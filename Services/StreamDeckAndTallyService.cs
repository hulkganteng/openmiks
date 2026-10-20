using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using VMixClone.Models;

namespace VMixClone.Services
{
    public class StreamDeckAndTallyService : IDisposable
    {
        private readonly ProductionEngine _engine;
        private readonly IvMixAPI _api;
        private Stream? _serialStream;
        private StreamWriter? _serialWriter;
        private TcpListener? _deckListener;
        private CancellationTokenSource? _cts;
        private readonly List<TcpClient> _deckClients = new();
        private readonly object _lock = new();

        public string ActiveComPort { get; private set; } = string.Empty;
        public int DeckPort { get; } = 8099;
        public bool IsRunning { get; private set; }

        public StreamDeckAndTallyService(ProductionEngine engine, IvMixAPI api)
        {
            _engine = engine;
            _api = api;
        }

        public void Start(string? comPort = null, int port = 8099)
        {
            if (IsRunning) return;
            IsRunning = true;
            _cts = new CancellationTokenSource();

            if (!string.IsNullOrWhiteSpace(comPort))
            {
                ConfigureSerialTally(comPort);
            }

            _api.TallyChanged += OnTallyChanged;
            _api.StateChanged += OnStateChanged;

            StartDeckServer(port);
        }

        public void ConfigureSerialTally(string portName)
        {
            try
            {
                _serialWriter?.Dispose();
                _serialStream?.Dispose();

                string fullPortName = portName.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase)
                    ? portName
                    : $@"\\.\{portName}";

                // Stdlib native Windows file stream for COM port devices without external NuGet dependency
                _serialStream = new FileStream(fullPortName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                _serialWriter = new StreamWriter(_serialStream, Encoding.ASCII) { AutoFlush = true };
                ActiveComPort = portName;
                SendCurrentTallyToSerial();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Tally COM] Failed to open {portName}: {ex.Message}");
                _serialStream = null;
                _serialWriter = null;
                ActiveComPort = string.Empty;
            }
        }

        private void StartDeckServer(int port)
        {
            try
            {
                _deckListener = new TcpListener(IPAddress.Any, port);
                _deckListener.Start();
                _ = AcceptDeckClientsAsync(_cts?.Token ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamDeck] Server start failed on {port}: {ex.Message}");
            }
        }

        private async Task AcceptDeckClientsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _deckListener != null)
            {
                try
                {
                    var client = await _deckListener.AcceptTcpClientAsync(ct);
                    lock (_lock)
                    {
                        _deckClients.Add(client);
                    }
                    _ = HandleDeckClientAsync(client, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StreamDeck] Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleDeckClientAsync(TcpClient client, CancellationToken ct)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            await writer.WriteLineAsync("VERSION 29.0.0.49");
            BroadcastStateToClient(writer);

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    string? line = await reader.ReadLineAsync(ct);
                    if (line == null) break;

                    string cmd = line.Trim();
                    if (cmd.StartsWith("FUNCTION ", StringComparison.OrdinalIgnoreCase))
                    {
                        string fn = cmd.Substring(9);
                        _api.Execute(fn);
                        await writer.WriteLineAsync("FUNCTION OK");
                    }
                    else if (cmd.Equals("TALLY", StringComparison.OrdinalIgnoreCase))
                    {
                        var tallies = _api.GetTallys();
                        string tallyStr = BuildTallyString();
                        await writer.WriteLineAsync($"TALLY OK {tallyStr}");
                    }
                    else if (cmd.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    {
                        var state = _api.GetState(true);
                        await writer.WriteLineAsync($"XML {state.XML.Length}");
                        await writer.WriteLineAsync(state.XML);
                    }
                    else if (cmd.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }
            catch { }
            finally
            {
                lock (_lock)
                {
                    _deckClients.Remove(client);
                }
                client.Dispose();
            }
        }

        private void OnTallyChanged(object? sender, EventArgs e)
        {
            SendCurrentTallyToSerial();
            BroadcastDeckMessage($"TALLY OK {BuildTallyString()}");
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            var state = _api.GetState(false);
            BroadcastDeckMessage($"ACTS OK Output={state.Output} Preview={state.Preview}");
        }

        private void SendCurrentTallyToSerial()
        {
            if (_serialWriter == null) return;
            try
            {
                // Protocol: "TALLY:P=<program_num>,V=<preview_num>\n"
                int p = _engine.ProgramInput?.Number ?? 0;
                int v = _engine.PreviewInput?.Number ?? 0;
                _serialWriter.WriteLine($"TALLY:P={p},V={v}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Tally COM] Write error: {ex.Message}");
            }
        }

        private string BuildTallyString()
        {
            var sb = new StringBuilder();
            foreach (var inp in _engine.Inputs)
            {
                if (inp == _engine.ProgramInput) sb.Append('1');
                else if (inp == _engine.PreviewInput) sb.Append('2');
                else sb.Append('0');
            }
            return sb.ToString();
        }

        private void BroadcastStateToClient(StreamWriter writer)
        {
            try
            {
                writer.WriteLine($"TALLY OK {BuildTallyString()}");
            }
            catch { }
        }

        private void BroadcastDeckMessage(string msg)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg + "\r\n");
            lock (_lock)
            {
                for (int i = _deckClients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (_deckClients[i].Connected)
                        {
                            _deckClients[i].GetStream().Write(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            _deckClients.RemoveAt(i);
                        }
                    }
                    catch
                    {
                        _deckClients.RemoveAt(i);
                    }
                }
            }
        }

        public void Dispose()
        {
            IsRunning = false;
            _cts?.Cancel();
            _deckListener?.Stop();

            _api.TallyChanged -= OnTallyChanged;
            _api.StateChanged -= OnStateChanged;

            _serialWriter?.Dispose();
            _serialWriter = null;
            _serialStream?.Dispose();
            _serialStream = null;

            lock (_lock)
            {
                foreach (var c in _deckClients) c.Dispose();
                _deckClients.Clear();
            }
        }
    }
}
