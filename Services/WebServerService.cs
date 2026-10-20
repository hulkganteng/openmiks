using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Openmiks.Models;

namespace Openmiks.Services
{
    public class WebServerService
    {
        private HttpListener? _listener;
        private readonly ProductionEngine _engine;
        private bool _isRunning = false;
        private readonly int _port;

        public int Port => _port;
        public bool IsRunning => _isRunning;

        // ponytail: Standard HTTP vMix XML API & HTML5 Web Controller on port 8088; upgrade to full vMix TCP port 8099 protocol if Companion TCP feedback driver needed.
        public WebServerService(ProductionEngine engine, int port = 8088)
        {
            _engine = engine;
            _port = port;
        }

        public void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{_port}/");
                _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
                _listener.Start();
                _isRunning = true;
                _listener.BeginGetContext(OnRequestReceived, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start HTTP server on port {_port}: {ex.Message}");
                // Fallback to port 8089 if 8088 needs admin or is occupied
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://localhost:8089/");
                    _listener.Prefixes.Add($"http://127.0.0.1:8089/");
                    _listener.Start();
                    _isRunning = true;
                    _listener.BeginGetContext(OnRequestReceived, null);
                }
                catch
                {
                    _isRunning = false;
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
        }

        private void OnRequestReceived(IAsyncResult result)
        {
            if (!_isRunning || _listener == null) return;

            HttpListenerContext context;
            try
            {
                context = _listener.EndGetContext(result);
                _listener.BeginGetContext(OnRequestReceived, null);
            }
            catch
            {
                return;
            }

            var request = context.Request;
            var response = context.Response;

            try
            {
                string rawUrl = request.RawUrl ?? "/";
                string path = request.Url?.AbsolutePath.ToLowerInvariant() ?? "/";

                if (path == "/api" || path == "/api/")
                {
                    HandleApi(context);
                }
                else if (path == "/controller" || path == "/" || path == "/index.html")
                {
                    ServeWebController(context);
                }
                else
                {
                    response.StatusCode = 404;
                    byte[] notFound = Encoding.UTF8.GetBytes("Not Found");
                    response.OutputStream.Write(notFound, 0, notFound.Length);
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    response.StatusCode = 500;
                    byte[] err = Encoding.UTF8.GetBytes(ex.Message);
                    response.OutputStream.Write(err, 0, err.Length);
                    response.Close();
                }
                catch { }
            }
        }

        private void HandleApi(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            string? function = query["Function"];

            if (!string.IsNullOrEmpty(function))
            {
                var disp = Application.Current?.Dispatcher;
                if (disp != null && !disp.HasShutdownStarted)
                {
                    disp.BeginInvoke(() => ExecuteFunction(function, query));
                }
                else
                {
                    ExecuteFunction(function, query);
                }
            }

            // Return XML state response matching vMix format
            string xml = BuildVmixXml();
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.Close();
        }

        private void ExecuteFunction(string function, System.Collections.Specialized.NameValueCollection query)
        {
            // Everything funnels into the engine's single API surface so HTTP, TCP, scripts and
            // triggers cannot drift apart.
            var sb = new StringBuilder();
            foreach (string? key in query.AllKeys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (key.Equals("Function", StringComparison.OrdinalIgnoreCase)) continue;
                if (sb.Length > 0) sb.Append('&');
                sb.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(query[key] ?? string.Empty));
            }

            _engine.ExecuteApiFunction(function, sb.ToString());
        }

        private string BuildVmixXml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<vmix>");
            sb.AppendLine("  <version>29.0.0.49</version>");
            sb.AppendLine("  <edition>Pro</edition>");
            sb.AppendLine("  <preset>Default</preset>");
            sb.AppendLine("  <inputs>");
            foreach (var inp in _engine.Inputs)
            {
                sb.AppendLine($"    <input key=\"{inp.Key}\" number=\"{inp.Number}\" type=\"{inp.Type}\" title=\"{System.Security.SecurityElement.Escape(inp.Title)}\" state=\"Running\" position=\"0\" duration=\"0\" loop=\"True\" muted=\"{inp.IsMuted}\" volume=\"{inp.Volume:0}\" balance=\"0\" solo=\"{inp.IsSolo}\" audiobusses=\"M\">{System.Security.SecurityElement.Escape(inp.Title)}</input>");
            }
            sb.AppendLine("  </inputs>");
            sb.AppendLine("  <overlays>");
            sb.AppendLine($"    <overlay number=\"1\" key=\"{_engine.Overlay1?.Key ?? ""}\" />");
            sb.AppendLine($"    <overlay number=\"2\" key=\"{_engine.Overlay2?.Key ?? ""}\" />");
            sb.AppendLine($"    <overlay number=\"3\" key=\"{_engine.Overlay3?.Key ?? ""}\" />");
            sb.AppendLine($"    <overlay number=\"4\" key=\"{_engine.Overlay4?.Key ?? ""}\" />");
            sb.AppendLine("  </overlays>");
            sb.AppendLine($"  <preview>{_engine.PreviewInput?.Number ?? 0}</preview>");
            sb.AppendLine($"  <active>{_engine.ProgramInput?.Number ?? 0}</active>");
            sb.AppendLine($"  <recording>{_engine.IsRecording}</recording>");
            sb.AppendLine($"  <streaming>{_engine.IsStreaming}</streaming>");
            sb.AppendLine($"  <external>False</external>");
            sb.AppendLine($"  <fullscreen>False</fullscreen>");
            sb.AppendLine($"  <fadeToBlack>{_engine.IsFTB}</fadeToBlack>");
            sb.AppendLine("  <transitions>");
            sb.AppendLine($"    <transition number=\"1\" name=\"Fade\" duration=\"{_engine.TransitionDurationMs}\" />");
            sb.AppendLine("    <transition number=\"2\" name=\"Zoom\" duration=\"500\" />");
            sb.AppendLine("    <transition number=\"3\" name=\"Wipe\" duration=\"500\" />");
            sb.AppendLine("    <transition number=\"4\" name=\"Slide\" duration=\"500\" />");
            sb.AppendLine("  </transitions>");
            sb.AppendLine("  <audio>");
            sb.AppendLine($"    <master volume=\"{_engine.MasterVolume:0}\" muted=\"{_engine.MasterMuted}\" meterF1=\"{_engine.MasterLevelL:0.00}\" meterF2=\"{_engine.MasterLevelR:0.00}\" />");
            sb.AppendLine("  </audio>");
            sb.AppendLine("</vmix>");
            return sb.ToString();
        }

        private void ServeWebController(HttpListenerContext context)
        {
            string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>vMix Web Controller</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; user-select: none; }
    body { background: #181818; color: #ececec; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; padding: 12px; }
    header { display: flex; justify-content: space-between; align-items: center; padding-bottom: 12px; border-bottom: 1px solid #333; margin-bottom: 16px; }
    h1 { font-size: 1.1rem; color: #00ff88; letter-spacing: 0.5px; }
    .badge { padding: 4px 10px; border-radius: 4px; font-size: 0.75rem; font-weight: bold; }
    .badge-rec { background: #441111; color: #ff5555; border: 1px solid #772222; }
    .badge-rec.active { background: #dd1111; color: #fff; animation: pulse 1.5s infinite; }
    .badge-stream { background: #113322; color: #44dd88; border: 1px solid #226644; }
    .badge-stream.active { background: #00aa55; color: #fff; }
    @keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

    .monitors { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 16px; }
    .mon { background: #222; border-radius: 6px; padding: 10px; text-align: center; }
    .mon.pv { border: 2px solid #00ff66; }
    .mon.pgm { border: 2px solid #ff2222; }
    .mon-label { font-size: 0.75rem; font-weight: bold; letter-spacing: 1px; margin-bottom: 4px; }
    .mon-val { font-size: 1.4rem; font-weight: bold; }

    .transitions { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin-bottom: 16px; }
    button { background: #2c2c30; color: #eee; border: 1px solid #444; border-radius: 5px; padding: 12px 6px; font-size: 0.9rem; font-weight: bold; cursor: pointer; transition: all 0.1s; }
    button:active { transform: scale(0.96); }
    .btn-cut { background: #c52222; border-color: #f33; color: white; }
    .btn-fade { background: #0b7285; border-color: #15aabf; color: white; }
    .btn-action { background: #333; }

    .section-title { font-size: 0.75rem; text-transform: uppercase; color: #888; margin-bottom: 8px; font-weight: bold; }
    .inputs-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 10px; margin-bottom: 18px; }
    .card { background: #242426; border: 2px solid #3c3c40; border-radius: 6px; padding: 10px; cursor: pointer; text-align: center; }
    .card.is-pv { border-color: #00ff66; box-shadow: 0 0 8px rgba(0,255,100,0.3); }
    .card.is-pgm { border-color: #ff2222; box-shadow: 0 0 10px rgba(255,34,34,0.4); }
    .card-num { font-size: 0.75rem; color: #aaa; margin-bottom: 4px; }
    .card-title { font-size: 0.9rem; font-weight: bold; margin-bottom: 8px; }
    .card-btns { display: flex; gap: 4px; }
    .card-btns button { flex: 1; padding: 4px; font-size: 0.75rem; }

    .overlays { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; margin-bottom: 18px; }
    .btn-overlay.active { background: #e67700; border-color: #ff922b; color: #fff; }

    .tbar-box { background: #222; padding: 12px; border-radius: 6px; margin-bottom: 18px; text-align: center; }
    input[type=range] { width: 100%; height: 28px; accent-color: #ff6b00; cursor: pointer; }
  </style>
</head>
<body>
  <header>
    <h1>vMix Remote Controller</h1>
    <div style=""display:flex;gap:6px;"">
      <span id=""recBadge"" class=""badge badge-rec"" onclick=""cmd('StartStopRecord')"">REC</span>
      <span id=""streamBadge"" class=""badge badge-stream"" onclick=""cmd('StartStopStreaming')"">STREAM</span>
      <span id=""ftbBadge"" class=""badge"" style=""background:#333;"" onclick=""cmd('FadeToBlack')"">FTB</span>
    </div>
  </header>

  <div class=""monitors"">
    <div class=""mon pv"">
      <div class=""mon-label"" style=""color:#00ff66;"">PREVIEW</div>
      <div class=""mon-val"" id=""pvTitle"">-</div>
    </div>
    <div class=""mon pgm"">
      <div class=""mon-label"" style=""color:#ff4444;"">PROGRAM (LIVE)</div>
      <div class=""mon-val"" id=""pgmTitle"">-</div>
    </div>
  </div>

  <div class=""transitions"">
    <button class=""btn-cut"" onclick=""cmd('Cut')"">CUT</button>
    <button class=""btn-fade"" onclick=""cmd('Fade')"">FADE</button>
    <button class=""btn-action"" onclick=""cmd('Zoom')"">ZOOM</button>
    <button class=""btn-action"" onclick=""cmd('Wipe')"">WIPE</button>
  </div>

  <div class=""tbar-box"">
    <div style=""display:flex;justify-content:space-between;font-size:0.75rem;color:#aaa;margin-bottom:6px;"">
      <span>T-BAR TRANSITION</span>
      <span id=""tbarLabel"">0%</span>
    </div>
    <input type=""range"" id=""tbar"" min=""0"" max=""255"" value=""0"" oninput=""onTBar(this.value)"" onchange=""onTBarEnd()"" />
  </div>

  <div class=""section-title"">Overlays</div>
  <div class=""overlays"">
    <button id=""ov1"" class=""btn-overlay"" onclick=""cmd('OverlayInput1')"">1</button>
    <button id=""ov2"" class=""btn-overlay"" onclick=""cmd('OverlayInput2')"">2</button>
    <button id=""ov3"" class=""btn-overlay"" onclick=""cmd('OverlayInput3')"">3</button>
    <button id=""ov4"" class=""btn-overlay"" onclick=""cmd('OverlayInput4')"">4</button>
  </div>

  <div class=""section-title"">Inputs</div>
  <div class=""inputs-grid"" id=""inputsGrid""></div>

  <script>
    let pollInterval = null;
    async function fetchState() {
      try {
        const res = await fetch('/api');
        const text = await res.text();
        const parser = new DOMParser();
        const xml = parser.parseFromString(text, 'text/xml');
        render(xml);
      } catch (err) {
        console.error('State poll failed', err);
      }
    }

    function cmd(func, params = '') {
      fetch('/api/?Function=' + func + params).then(() => fetchState());
    }

    function onTBar(val) {
      document.getElementById('tbarLabel').innerText = Math.round((val/255)*100) + '%';
      fetch('/api/?Function=SetTBar&Value=' + val);
    }
    function onTBarEnd() {
      setTimeout(() => {
        document.getElementById('tbar').value = 0;
        document.getElementById('tbarLabel').innerText = '0%';
        fetch('/api/?Function=SetTBar&Value=0');
      }, 300);
    }

    function render(xml) {
      const activeNum = xml.querySelector('active')?.textContent || '0';
      const previewNum = xml.querySelector('preview')?.textContent || '0';
      const isRec = xml.querySelector('recording')?.textContent === 'True';
      const isStream = xml.querySelector('streaming')?.textContent === 'True';
      const isFtb = xml.querySelector('fadeToBlack')?.textContent === 'True';

      const recBadge = document.getElementById('recBadge');
      recBadge.className = 'badge badge-rec ' + (isRec ? 'active' : '');
      const streamBadge = document.getElementById('streamBadge');
      streamBadge.className = 'badge badge-stream ' + (isStream ? 'active' : '');
      const ftbBadge = document.getElementById('ftbBadge');
      ftbBadge.style.background = isFtb ? '#ffaa00' : '#333';

      const inputs = xml.querySelectorAll('inputs > input');
      let pgmName = 'Input ' + activeNum;
      let pvName = 'Input ' + previewNum;

      const container = document.getElementById('inputsGrid');
      let html = '';
      inputs.forEach(inp => {
        const num = inp.getAttribute('number');
        const title = inp.getAttribute('title') || inp.textContent;
        const isPgm = (num === activeNum);
        const isPv = (num === previewNum);
        if (isPgm) pgmName = num + '. ' + title;
        if (isPv) pvName = num + '. ' + title;

        const cls = 'card ' + (isPgm ? 'is-pgm' : (isPv ? 'is-pv' : ''));
        html += `
          <div class=""${cls}"" onclick=""cmd('PreviewInput', '&Input=${num}')"">
            <div class=""card-num"">INPUT ${num}</div>
            <div class=""card-title"">${title}</div>
            <div class=""card-btns"">
              <button onclick=""event.stopPropagation();cmd('ActiveInput','&Input=${num}')"" style=""background:#c52222;color:#fff;"">CUT</button>
              <button onclick=""event.stopPropagation();cmd('QuickPlay','&Input=${num}')"">PLAY</button>
            </div>
          </div>
        `;
      });
      container.innerHTML = html;

      document.getElementById('pgmTitle').innerText = pgmName;
      document.getElementById('pvTitle').innerText = pvName;

      for (let i = 1; i <= 4; i++) {
        const ov = xml.querySelector(`overlays > overlay[number=""${i}""]`);
        const btn = document.getElementById('ov' + i);
        if (btn) {
          const hasKey = ov && ov.getAttribute('key') && ov.getAttribute('key').length > 0;
          btn.className = 'btn-overlay ' + (hasKey ? 'active' : '');
        }
      }
    }

    fetchState();
    setInterval(fetchState, 1000);
  </script>
</body>
</html>";

            byte[] bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.Close();
        }
    }
}
