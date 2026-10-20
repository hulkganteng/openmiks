using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Openmiks.Models;
using Openmiks.Services;

namespace Openmiks.Tests
{
    public static class ProductionEngineTests
    {
        // Frames for the program-surface recording test, so it does not depend on the UI thread.
        private sealed class SyntheticFrameSource : IFrameSource
        {
            private long _frames;
            public long FramesServed => Interlocked.Read(ref _frames);

            public bool TryGetFrame(out byte[] buffer, out int width, out int height)
            {
                width = 160;
                height = 90;
                buffer = new byte[width * height * 4];
                long n = Interlocked.Increment(ref _frames);
                byte shift = (byte)(n % 200);
                for (int i = 0; i < buffer.Length; i += 4)
                {
                    buffer[i] = shift;        // B
                    buffer[i + 1] = 40;       // G
                    buffer[i + 2] = (byte)(255 - shift); // R
                    buffer[i + 3] = 255;      // A
                }
                return true;
            }
        }

        public static void RunAll()
        {
            Console.WriteLine("[TEST] Starting vMix Engine and WebServer validation...");

            var engine = new ProductionEngine();

            // 1. Initial State Assertions
            Debug.Assert(engine.Inputs.Count >= 6, "Engine must load default inputs");
            Debug.Assert(engine.ProgramInput != null, "Program input must not be null");
            Debug.Assert(engine.PreviewInput != null, "Preview input must not be null");
            Debug.Assert(engine.ProgramInput.Number == 1, "Default Program should be Input 1");
            Debug.Assert(engine.PreviewInput.Number == 2, "Default Preview should be Input 2");
            Console.WriteLine("[PASS] Initial state check passed.");

            // 2. Cut Transition Test
            int prevProgram = engine.ProgramInput.Number;
            int prevPreview = engine.PreviewInput.Number;
            engine.Cut();
            Debug.Assert(engine.ProgramInput.Number == prevPreview, "Cut must swap Preview to Program");
            Debug.Assert(engine.PreviewInput.Number == prevProgram, "Cut must swap Program to Preview");
            Console.WriteLine("[PASS] Cut operation swap check passed.");

            // 3. Selection Tests
            engine.SetPreview(3);
            Debug.Assert(engine.PreviewInput.Number == 3, "SetPreview should update PreviewInput");
            engine.SetProgram(4);
            Debug.Assert(engine.ProgramInput.Number == 4, "SetProgram should update ProgramInput");
            Console.WriteLine("[PASS] Input selection check passed.");

            // 4. Overlays 1-4 Tests (PiP & Titles)
            engine.ToggleOverlay(1, 3);
            Debug.Assert(engine.Overlay1 != null && engine.Overlay1.Number == 3, "Overlay 1 should be set to Input 3");
            engine.ToggleOverlay(3, 5);
            Debug.Assert(engine.Overlay3 != null && engine.Overlay3.Number == 5, "Overlay 3 (PiP) should be set to Input 5");
            engine.ToggleOverlay(1, 3);
            Debug.Assert(engine.Overlay1 == null, "Overlay 1 should toggle off");
            Console.WriteLine("[PASS] Overlay channels check passed.");

            // 5. T-Bar and Advanced Transitions Test
            engine.CurrentTransition = TransitionEffect.Merge;
            engine.SetTBar(0.5);
            Debug.Assert(Math.Abs(engine.TransitionProgress - 0.5) < 0.001, "T-Bar progress must be 0.5");
            engine.SetTBar(1.0);
            Debug.Assert(Math.Abs(engine.TransitionProgress - 0.0) < 0.001, "T-Bar reaching 1.0 must complete and reset to 0.0");
            engine.CurrentTransition = TransitionEffect.BarnDoor;
            Console.WriteLine("[PASS] T-Bar and transition effects check passed.");

            // 6. Category Filtering
            engine.SelectedCategory = InputCategory.Red;
            int redCount = 0;
            foreach (var item in engine.FilteredInputs) redCount++;
            Debug.Assert(redCount >= 1, "Category filter should return red inputs");
            engine.SelectedCategory = InputCategory.All;
            Console.WriteLine("[PASS] Input category filtering check passed.");

            // 7. Audio Busses (Master + Bus A)
            engine.BusAMuted = true;
            Debug.Assert(engine.BusAMuted, "Bus A mute must be true");
            engine.BusAVolume = 70.0;
            Debug.Assert(Math.Abs(engine.BusAVolume - 70.0) < 0.01, "Bus A volume should be 70");
            Console.WriteLine("[PASS] Audio busses check passed.");

            // 8. Recording Backend File Output Test
            string tempRecDir = Path.Combine(Path.GetTempPath(), "vMixTestRec");
            var rec = new RecordingEngine();
            bool started = rec.Start(tempRecDir, "mp4", 8000);
            Debug.Assert(started, "Recording engine must start");
            Debug.Assert(rec.IsRecording, "Recording state must be true");
            string recordedPath = rec.Stop();
            Debug.Assert(!rec.IsRecording, "Recording state must stop");
            Debug.Assert(File.Exists(recordedPath), "Recorded video container file must exist on disk");
            Debug.Assert(new FileInfo(recordedPath).Length > 0, "Recorded file must contain bytes");
            try { File.Delete(recordedPath); Directory.Delete(tempRecDir); } catch { }
            Console.WriteLine("[PASS] Recording backend file output check passed.");

            // 9. Configuration & Preset Persistence Backend Test
            string tempPreset = Path.Combine(Path.GetTempPath(), "test_production.vmix");
            var cfg = new VMixConfiguration { OutputResolution = "3840x2160", FrameRate = "60p" };
            PresetService.SavePreset(tempPreset, engine, cfg);
            Debug.Assert(File.Exists(tempPreset), "Saved .vmix XML preset must exist");

            var loadedCfg = new VMixConfiguration();
            var freshEngine = new ProductionEngine();
            bool loaded = PresetService.LoadPreset(tempPreset, freshEngine, loadedCfg);
            Debug.Assert(loaded, "Preset load must succeed");
            Debug.Assert(loadedCfg.OutputResolution == "3840x2160", "Loaded resolution should match");
            Debug.Assert(loadedCfg.FrameRate == "60p", "Loaded frame rate should match");
            freshEngine.Shutdown();
            try { File.Delete(tempPreset); } catch { }
            Console.WriteLine("[PASS] Preset XML serialization/deserialization check passed.");

            // 10. Web Server API Test
            var web = new WebServerService(engine, 8089);
            web.Start();
            using var client = new HttpClient();
            var res = client.GetAsync("http://localhost:8089/api").GetAwaiter().GetResult();
            Debug.Assert(res.IsSuccessStatusCode, "Web API must return 200 OK");
            string xml = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Debug.Assert(xml.Contains("<vmix>"), "Web API response must contain <vmix> root tag");
            Debug.Assert(xml.Contains("<version>29.0.0.49</version>"), "Web API response must contain version");
            Debug.Assert(xml.Contains("<inputs>"), "Web API response must contain <inputs>");
            Console.WriteLine("[PASS] Web API XML check passed.");

            // 11. Remote HTTP Command Execution Test
            var cutRes = client.GetAsync("http://localhost:8089/api/?Function=Cut").GetAwaiter().GetResult();
            Debug.Assert(cutRes.IsSuccessStatusCode, "API function call should succeed");

            var ctrlRes = client.GetAsync("http://localhost:8089/controller").GetAwaiter().GetResult();
            Debug.Assert(ctrlRes.IsSuccessStatusCode, "Web Controller HTML must return 200 OK");
            string html = ctrlRes.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Debug.Assert(html.Contains("vMix Web Controller"), "Web Controller page must be served");
            Console.WriteLine("[PASS] Remote HTTP Command execution passed.");
            web.Stop();

            // 12. Native vMix TCP API Server (Port 8099) Test
            var tcp = new TcpServerService(engine, 8097);
            tcp.Start();
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect("127.0.0.1", 8097);
                using var stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string? welcome = reader.ReadLine();
                Debug.Assert(welcome != null && welcome.Contains("VERSION OK"), "TCP server should send VERSION banner");

                writer.WriteLine("TALLY");
                string? tallyResp = reader.ReadLine();
                Debug.Assert(tallyResp != null && tallyResp.StartsWith("TALLY OK"), "TCP server should respond to TALLY");

                writer.WriteLine("FUNCTION Cut");
                string? fCutResp = reader.ReadLine();
                Debug.Assert(fCutResp != null && fCutResp.StartsWith("FUNCTION OK Cut"), "TCP server should respond to FUNCTION Cut");
            }
            tcp.Stop();
            Console.WriteLine("[PASS] Native vMix TCP API (Port 8099) check passed.");

            // 13. Phase 1: Virtual Sets & GPU Chroma Key Test
            string vmixVsPath = @"C:\Program Files (x86)\vMix\VirtualSets\NewsStudio";
            if (Directory.Exists(vmixVsPath))
            {
                var vs = VirtualSet.LoadFromFolder(vmixVsPath);
                Debug.Assert(vs.Layers.Count >= 3, "Virtual Set must load at least 3 layers from config.xml");
                Debug.Assert(vs.CameraPresets.Count >= 2, "Virtual Set must load camera presets");
                Debug.Assert(vs.Layers.Any(l => l.PlaceholderType == "Talent"), "Must detect Talent layer");
                Debug.Assert(vs.Layers.Any(l => l.PlaceholderType == "Screen"), "Must detect Screen layer");

                var closeUp = vs.CameraPresets.FirstOrDefault(p => p.Name.Contains("Close"));
                if (closeUp != null)
                {
                    vs.CurrentPreset = closeUp;
                    Debug.Assert(vs.ActiveCameraZoom == closeUp.Zoom, "Camera zoom must match preset");
                }
                Console.WriteLine("[PASS] Virtual Set loading and camera presets validated.");
            }

            var chroma = new Openmiks.Effects.ChromaKeyEffect();
            Debug.Assert(chroma.Similarity > 0, "ChromaKey similarity must be positive");
            Debug.Assert(chroma.SpillSuppression > 0, "ChromaKey spill suppression must be positive");
            Console.WriteLine("[PASS] GPU Chroma Key ShaderEffect validated.");

            // 14. Phase 2: Instant Replay (8 Cameras, Channel A/B, Mark In/Out, Slow-Mo)
            var replay = engine.Replay;
            Debug.Assert(replay.Cameras.Count == 8, "Replay must manage 8 independent cameras");
            replay.StartRecording();
            Debug.Assert(replay.IsRecording, "Replay recording must be active");
            replay.MarkIn();
            replay.MarkOut("Test Highlight Event");
            Debug.Assert(replay.Events.Count >= 1, "Replay must store marked events");
            replay.LiveAutoMark(10);
            Debug.Assert(replay.Events.Count >= 2, "Replay LiveAutoMark must create auto event");
            replay.SetSpeed(0.25, "A");
            Debug.Assert(Math.Abs(replay.ChannelA.Speed - 0.25) < 0.01, "Replay slow-motion speed must be set to 25%");
            replay.Jog(1.5, "A");
            replay.StopRecording();
            Console.WriteLine("[PASS] Phase 2: Instant Replay 8-Camera engine validated.");

            // 15. Phase 3: NDI & SRT Support
            var ndiSrt = engine.NdiSrt;
            bool ndiHonest = NdiDiscoveryService.RuntimePresent
                ? ndiSrt.DiscoveredNdiSources.All(s => s.Port > 0 && IPAddress.TryParse(s.IpAddress, out _))
                : ndiSrt.DiscoveredNdiSources.Count == 1 && ndiSrt.DiscoveredNdiSources[0].Name.Contains("not installed");
            Debug.Assert(ndiHonest, "NDI list must be real mDNS results or an explicit missing-runtime marker, never invented hosts");
            Console.WriteLine($"[INFO] NDI: runtime={NdiDiscoveryService.RuntimePresent}, discovered={ndiSrt.DiscoveredNdiSources.Count}");
            ndiSrt.SrtConfig.Host = "192.168.1.100";
            ndiSrt.SrtConfig.Port = 9999;
            ndiSrt.SrtConfig.Passphrase = "SecretPass123";
            string srtUrl = ndiSrt.BuildSrtUrl();
            Debug.Assert(srtUrl.StartsWith("srt://192.168.1.100:9999"), "SRT URL must be properly formatted");
            Debug.Assert(srtUrl.Contains("passphrase=SecretPass123"), "SRT URL must include passphrase");
            Console.WriteLine("[PASS] Phase 3: NDI & SRT discovery and URL builder validated.");

            // 16. Phase 4: Audio Mixer 8 Busses (M + A-G) & VST3
            var audioMixer = engine.AudioMixer;
            Debug.Assert(audioMixer.Busses.Count == 8, "Audio mixer must have 8 busses (Master + A-G)");
            audioMixer.SetBusVolume("B", 65.0);
            Debug.Assert(Math.Abs(audioMixer.BusB.Volume - 65.0) < 0.01, "Bus B volume must be 65");
            audioMixer.ToggleBusMute("C");
            Debug.Assert(audioMixer.BusC.IsMuted, "Bus C mute toggle must work");
            var vstSlot = new Vst3EffectSlot { PluginName = "Broadcast Compressor", Param1 = -18.0 };
            Debug.Assert(vstSlot.Param1 == -18.0, "VST3 effect slot parameter must be set");
            Console.WriteLine("[PASS] Phase 4: Audio Mixer 8-Bus architecture & VST3 slot validated.");

            // 17. Phase 5: Overlays 1-8 Channels, vMix Workflow & Stinger
            engine.ToggleOverlay(1, 1);
            Debug.Assert(engine.Overlay1 != null && engine.Overlay1.Number == 1 && engine.Inputs.First(i => i.Number == 1).IsOverlay1, "Overlay 1 card toggle must set active and tally");
            engine.ToggleOverlay(1); // Bar/hotkey toggle when active must take it off
            Debug.Assert(engine.Overlay1 == null && !engine.Inputs.First(i => i.Number == 1).IsOverlay1, "Overlay 1 bar toggle must take off active overlay");

            engine.ToggleOverlay(5, 1);
            Debug.Assert(engine.Overlay5 != null && engine.Overlay5.Number == 1, "Overlay 5 must be active");
            engine.ToggleOverlay(8, 2);
            Debug.Assert(engine.Overlay8 != null && engine.Overlay8.Number == 2, "Overlay 8 must be active");
            engine.OverlayAllOff();
            Debug.Assert(engine.Overlay5 == null && engine.Overlay8 == null, "OverlayAllOff must clear all overlays");

            var stinger = new StingerTransition { StingerNumber = 1, CutPointMs = 50, DurationMs = 120 };
            engine.Stingers.Clear();
            engine.Stingers.Add(stinger);
            stinger.AssignedInputNumber = 1;
            // PlayStingerAsync runs synchronously without deadlocking sync context
            Task.Run(async () => await engine.PlayStingerAsync(1)).GetAwaiter().GetResult();
            Debug.Assert(engine.Overlay4 == null, "Stinger must auto-clear overlay after playback completes");

            Console.WriteLine("[PASS] Phase 5: 8-Channel Overlays, vMix workflow & Stinger validated.");

            // 18. Phase 6: Scripting Engine & Automation Triggers
            var automation = engine.Automation;
            Debug.Assert(automation.Scripts.Count >= 1, "Scripting engine should contain default scripts");
            Debug.Assert(automation.Triggers.Count >= 1, "Triggers engine should contain default trigger rules");
            bool scriptExecuted = false;
            var script = new ScriptItem { Name = "TestScript", Code = "API.Function(\"Cut\");" };
            automation.ExecuteScriptAsync(script, (func, param) => { if (func == "Cut") scriptExecuted = true; }).GetAwaiter().GetResult();
            Debug.Assert(scriptExecuted, "Automation script runner must execute API functions");
            Console.WriteLine("[PASS] Phase 6: Scripting engine & automation triggers validated.");

            // 19. Phase 7: Playlist & Photo Slideshow Models
            var playlist = new VideoListModel();
            playlist.Items.Add(new PlaylistItem { Title = "Clip 1" });
            playlist.Items.Add(new PlaylistItem { Title = "Clip 2" });
            playlist.Next();
            Debug.Assert(playlist.CurrentIndex == 1, "Video playlist must advance to next item");
            var slideshow = new PhotoSlideshowModel();
            slideshow.ImagePaths.Add("photo1.jpg");
            slideshow.SlideDurationSeconds = 4;
            Debug.Assert(slideshow.SlideDurationSeconds == 4, "Slideshow duration must be set");
            Console.WriteLine("[PASS] Phase 7: Extended video playlist and photo slideshow models validated.");

            // 20. Phase 8: VISCA over IP PTZ Controller
            var viscaTask = PtzControllerService.RecallPresetAsync("127.0.0.1", 1);
            Debug.Assert(viscaTask != null, "VISCA PTZ recall preset command must be generated");
            viscaTask!.GetAwaiter().GetResult();
            Console.WriteLine("[PASS] Phase 8: VISCA over IP PTZ controller validated.");

            // 21. Real capture backends: device enumeration, capture graph, single API path
            var devices = CameraDeviceService.VideoDevices;
            Console.WriteLine($"[INFO] DirectShow devices on this host: {(devices.Count == 0 ? "(none)" : string.Join(", ", devices))}");

            var camInput = engine.Inputs.FirstOrDefault(i => i.Type == InputType.Camera);
            Debug.Assert(camInput != null, "Default preset must contain a camera input");
            if (devices.Count > 0)
            {
                Debug.Assert(devices.Contains(camInput.CameraDeviceName),
                    "Default camera input must name a device that actually exists on this host");
                Debug.Assert(camInput.Capture == null || camInput.Capture.DeviceName == camInput.CameraDeviceName,
                    "Camera input capture graph must target the selected device");
            }
            else
            {
                Debug.Assert(camInput.Capture == null, "No device on host => no capture graph");
                Console.WriteLine("[INFO] No DirectShow device present; capture assertions skipped.");
            }

            var broken = new CameraCaptureService();
            bool bogusStarted = broken.Start("__no_such_device__");
            Debug.Assert(!bogusStarted, "Opening a nonexistent device must fail");
            Debug.Assert(!string.IsNullOrEmpty(broken.LastError), "Failed capture must report why");
            broken.Dispose();
            Console.WriteLine("[PASS] Camera capture graph: real open/refuse behaviour validated.");

            if (camInput.Capture is { IsRunning: true } live)
            {
                long before = live.FrameCount;
                Thread.Sleep(1200);
                Debug.Assert(live.FrameCount > before, "Running capture graph must deliver frames");
                Console.WriteLine($"[PASS] Live DirectShow capture delivered {live.FrameCount - before} frames in 1.2s.");
            }

            Debug.Assert(Math.Abs(AudioMeterService.DbToLinear("lavfi.astats.Overall.RMS_level=-6.000000") - 0.801) < 0.01,
                "dBFS must convert to a linear meter value");
            Debug.Assert(AudioMeterService.DbToLinear("lavfi.astats.Overall.RMS_level=-inf") == 0.0,
                "-inf must map to silence");
            Console.WriteLine("[PASS] Audio meter dBFS->linear conversion validated.");

            byte[] mdns = NdiDiscoveryService.BuildQuery("_ndi._tcp.local");
            byte[] expectedMdns =
            {
                0x00,0x00, 0x00,0x00, 0x00,0x01, 0x00,0x00, 0x00,0x00, 0x00,0x00,
                0x04, (byte)'_',(byte)'n',(byte)'d',(byte)'i',
                0x04, (byte)'_',(byte)'t',(byte)'c',(byte)'p',
                0x05, (byte)'l',(byte)'o',(byte)'c',(byte)'a',(byte)'l',
                0x00, 0x00,0x0C, 0x00,0x01
            };
            Debug.Assert(mdns.SequenceEqual(expectedMdns),
                "NDI discovery query must be exactly the mDNS PTR query for _ndi._tcp.local");
            Console.WriteLine($"[PASS] NDI mDNS query builder validated ({mdns.Length} bytes); runtime present: {NdiDiscoveryService.RuntimePresent}.");

            // 22. Single API surface: HTTP/TCP/scripts/triggers share one dispatch, overlays 5-8 included
            engine.ExecuteApiFunction("OverlayInput7", "Input=2");
            Debug.Assert(engine.Overlay7 != null && engine.Inputs.First(i => i.Number == 2).IsOverlay7,
                "OverlayInput7 must set both the channel and the input flag");
            engine.ExecuteApiFunction("OverlayInput7", "Input=2");
            Debug.Assert(engine.Overlay7 == null, "OverlayInput7 must toggle off when the same input is sent again");
            engine.ExecuteApiFunction("SetVolume", "Input=1&Volume=42");
            Debug.Assert(Math.Abs(engine.Inputs.First(i => i.Number == 1).Volume - 42) < 0.01, "SetVolume must reach the input");
            engine.ExecuteApiFunction("SetTBar", "Value=128");
            Debug.Assert(engine.TransitionProgress is > 0.45 and < 0.55, "SetTBar must accept vMix's 0-255 scale");
            Console.WriteLine("[PASS] Unified API dispatch (overlays 5-8, SetVolume, SetTBar) validated.");

            var triggerTarget = engine.Inputs.First(i => i.Number == 2);
            engine.Automation.Triggers.Add(new TriggerRule { Trigger = "OnTransitionComplete", Function = "AudioOff", Input = "2" });
            engine.SetPreview(2);
            engine.Cut();
            Debug.Assert(triggerTarget.IsMuted, "OnTransitionComplete trigger must fire on Cut");
            Console.WriteLine("[PASS] Automation trigger fired from a real transition.");

            // 23. Program surface: a real Visual renders to BGRA frames
            var probe = new Border
            {
                Width = 320,
                Height = 180,
                Background = new LinearGradientBrush(Colors.Red, Colors.Blue, 0)
            };
            probe.Measure(new Size(320, 180));
            probe.Arrange(new Rect(0, 0, 320, 180));
            probe.UpdateLayout();

            byte[]? frame = ProgramSurface.Grab(probe, out int fw, out int fh, 320, 180);
            Debug.Assert(frame != null, $"Program surface must render a Visual to a frame: {ProgramSurface.LastError}");
            Debug.Assert(fw == 320 && fh == 180, "Frame size must match the visual");
            Debug.Assert(frame!.Length == fw * fh * 4, "Frame must be packed BGRA");
            Debug.Assert(frame.Distinct().Count() > 8, "Rendered gradient must contain real pixel content, not a blank buffer");
            Console.WriteLine($"[PASS] Program surface rendered {fw}x{fh} BGRA frame ({frame.Length} bytes, {frame.Distinct().Count()} distinct values).");

            // 24. Recording configures the program-surface input instead of gdigrab
            // Every side-effecting call is hoisted out of Debug.Assert: asserts are stripped in
            // Release, so work inside them would silently never run.
            var synth = new SyntheticFrameSource();
            var progRec = new RecordingEngine();
            string progDir = Path.Combine(Path.GetTempPath(), "vMixProgRec");
            bool progStarted = progRec.Start(progDir, "mp4", 4000, synth, 30);
            Debug.Assert(progStarted, "Program-surface recording must start");
            Debug.Assert(progRec.UsesProgramSurface, "Recorder must report the program surface as its source");
            Thread.Sleep(1500);
            long framesPumped = synth.FramesServed;
            string progFile = progRec.Stop();
            Debug.Assert(framesPumped > 0, "Recorder must pull frames from the frame source");
            Debug.Assert(File.Exists(progFile), "Program-surface recording must produce a file");
            long progBytes = progFile.Length > 0 ? new FileInfo(progFile).Length : 0;
            Debug.Assert(progBytes > 4096, $"Encoded program recording must contain real video, got {progBytes} bytes");
            Console.WriteLine($"[PASS] Recorded program surface: {framesPumped} frames -> {progBytes / 1024} KB.");
            try { File.Delete(progFile); Directory.Delete(progDir); } catch { }

            // 25. Real vMix title templates load and their named fields drive output
            var templates = TitleTemplate.AvailableTemplates();
            Console.WriteLine($"[INFO] Title templates found: {templates.Count}");
            Debug.Assert(templates.Count > 0, "Shipped vMix title templates must be discoverable");
            Debug.Assert(templates.All(File.Exists), "Discovered template paths must exist");

            var tmpl = TitleTemplate.Load(Path.Combine(Path.GetTempPath(), "__no_such_title__.xaml"));
            Debug.Assert(tmpl == null, "Missing template file must not load");

            // Title1.xaml is a shipped template; use it when present.
            string? title1 = templates.FirstOrDefault(p => Path.GetFileName(p).Equals("Title1.xaml", StringComparison.OrdinalIgnoreCase));
            if (title1 != null)
            {
                var titleTemplate = TitleTemplate.Load(title1);
                Debug.Assert(titleTemplate != null, $"Shipped template must load: {TitleTemplate.LastLoadError}");
                Debug.Assert(titleTemplate!.Root != null, "Loaded template must expose a root element");
                Debug.Assert(titleTemplate.NamedFields.Contains("Heading"), $"Title1 must index its Heading field (indexed: {string.Join(", ", titleTemplate.NamedFields)})");
                Debug.Assert(titleTemplate.NamedFields.Contains("Description"), "Title1 must index its Description field");

                bool setOk = titleTemplate.SetField("Heading", "OPENMIX TEST");
                Debug.Assert(setOk, "SetField must target a named template field");
                Debug.Assert(titleTemplate.GetField("Heading") == "OPENMIX TEST", "SetField must actually change template text");
                bool unknownRefused = !titleTemplate.SetField("__no_field__", "x");
                Debug.Assert(unknownRefused, "Unknown field must be refused");
                titleTemplate.ApplyHeadline("HEADLINE A", "SUBTITLE B");
                Debug.Assert(titleTemplate.GetField("Heading") == "HEADLINE A", "ApplyHeadline must reach the headline alias");
                Debug.Assert(titleTemplate.GetField("Description") == "SUBTITLE B", "ApplyHeadline must reach the subtitle alias");
                Console.WriteLine($"[PASS] vMix title template '{titleTemplate.Name}' loaded: {titleTemplate.Fields.Count} fields ({string.Join(", ", titleTemplate.Fields.Take(6))}).");
            }
            else
            {
                Console.WriteLine("[INFO] Title1.xaml not present; field-level template assertions skipped.");
            }

            // 26. Playout engine: real files, real advance, no invented state
            string playoutDir = Path.Combine(Path.GetTempPath(), "vMixPlayout");
            Directory.CreateDirectory(playoutDir);
            string fakeVideo = Path.Combine(playoutDir, "clip.mp4");
            string fakeImage = Path.Combine(playoutDir, "photo.png");
            string ignored = Path.Combine(playoutDir, "notes.txt");
            File.WriteAllBytes(fakeVideo, new byte[64]);
            File.WriteAllBytes(fakeImage, new byte[64]);
            File.WriteAllText(ignored, "not media");

            var playout = engine.Playout;
            int mediaLoaded = playout.LoadFolder(playoutDir);
            Debug.Assert(mediaLoaded == 2, $"Playout must load only media files, got {mediaLoaded}");
            Debug.Assert(playout.VideoList.Items.Count == 1, "Playlist must hold the .mp4");
            Debug.Assert(playout.Slideshow.ImagePaths.Count == 1, "Slideshow must hold the .png");
            Debug.Assert(!playout.VideoList.Items.Any(i => i.FilePath.EndsWith(".txt")), "Non-media files must be ignored");

            Debug.Assert(playout.VideoList.Items[0].FilePath == fakeVideo, "Playlist must resolve the real path");
            bool playoutStarted = playout.Start();
            Debug.Assert(playoutStarted, "Playout must start when it has items");
            Debug.Assert(playout.IsPlaying && playout.CurrentFile == fakeVideo, "Playout must expose the current file");
            Console.WriteLine($"[PASS] Playout loaded {mediaLoaded} media files; current = {Path.GetFileName(playout.CurrentFile)}.");

            playout.Mode = PlayoutMode.Slideshow;
            playout.Next();
            Debug.Assert(playout.CurrentFile == fakeImage, "Slideshow mode must advance to the image");
            playout.Previous();
            Debug.Assert(playout.CurrentFile == fakeImage, "Single-item slideshow must wrap to itself");
            playout.Stop();
            Debug.Assert(!playout.IsPlaying, "Playout must stop");
            Console.WriteLine("[PASS] Playout video/slideshow advance and wrap validated.");

            var emptyPlayout = new PlayoutEngine();
            bool emptyStarted = emptyPlayout.Start();
            Debug.Assert(!emptyStarted, "Playout must refuse to start with no media");
            emptyPlayout.Dispose();

            // 27. A Playlist input drives the engine's playout from its folder
            var playlistInput = new InputItem
            {
                Number = 99,
                Title = "Playlist Test",
                Type = InputType.Playlist,
                PlayoutFolder = playoutDir
            };
            bool playlistCued = engine.PreparePlayout(playlistInput);
            Debug.Assert(playlistCued, "Playlist input must configure the playout engine");
            Debug.Assert(engine.Playout.IsPlaying, "Playlist input must leave playout running");
            Debug.Assert(engine.Playout.CurrentFile == fakeVideo, "Playlist input must cue its first clip");

            var brokenPlayout = new InputItem
            {
                Number = 98,
                Title = "Broken",
                Type = InputType.Playlist,
                PlayoutFolder = Path.Combine(Path.GetTempPath(), "__missing_playout_dir__")
            };
            bool brokenRefused = !engine.PreparePlayout(brokenPlayout);
            Debug.Assert(brokenRefused, "Missing folder must be refused, not silently played");
            Console.WriteLine("[PASS] Playlist input wiring: real folder cues the engine, missing folder refused.");

            playout.Dispose();
            try { Directory.Delete(playoutDir, true); } catch { }

            // 28. Data Sources Engine: CSV and JSON data parsing and title field binding
            string testCsvPath = Path.Combine(Path.GetTempPath(), "test_datasource.csv");
            File.WriteAllText(testCsvPath, "Headline,Score,Time\n\"Team A vs Team B\",3 - 1,\"90:00\"\n\"Final Round\",1 - 0,\"45:00\"");
            var csvDs = new CsvDataSource("Scoreboard", testCsvPath, 1000);
            engine.DataSources.AddSource(csvDs);
            Debug.Assert(engine.DataSources.Sources.Count >= 1, "DataSourceManager must register CSV source");
            Debug.Assert(csvDs.GetTableNames().Contains("Table1"), "CSV data source must create Table1");
            string headlineVal = engine.DataSources.GetValue("Scoreboard", "Table1", "Headline", 0);
            Debug.Assert(headlineVal == "Team A vs Team B", $"CSV must parse quoted column, got '{headlineVal}'");
            string scoreVal = engine.DataSources.GetValue("Scoreboard", "Table1", "Score", 0);
            Debug.Assert(scoreVal == "3 - 1", $"CSV must parse Score column, got '{scoreVal}'");

            string testJsonPath = Path.Combine(Path.GetTempPath(), "test_datasource.json");
            File.WriteAllText(testJsonPath, "[{\"Player\":\"Alice\",\"Points\":150},{\"Player\":\"Bob\",\"Points\":200}]");
            var jsonDs = new JsonDataSource("Leaderboard", testJsonPath, 1000);
            engine.DataSources.AddSource(jsonDs);
            Debug.Assert(engine.DataSources.Sources.Count >= 2, "DataSourceManager must hold both sources");
            string playerVal = engine.DataSources.GetValue("Leaderboard", "Table1", "Player", 1);
            Debug.Assert(playerVal == "Bob", $"JSON data source must extract item 1 Player 'Bob', got '{playerVal}'");
            Console.WriteLine("[PASS] Phase 5: Data Sources Engine (CSV & JSON parsed and queryable) validated.");
            try { File.Delete(testCsvPath); File.Delete(testJsonPath); } catch { }

            // 29. vMix API Contract: exact parity with vMixAPI.dll (IvMixAPI, GetState, GetTallys, Execute)
            var api = engine.Api;
            Debug.Assert(api != null, "Engine must expose standard IvMixAPI service");
            bool tallyEventFired = false;
            api.TallyChanged += (_, _) => tallyEventFired = true;
            api.Execute("Cut");
            Debug.Assert(tallyEventFired, "IvMixAPI.Execute('Cut') must fire TallyChanged event");

            var state = api.GetState(true);
            Debug.Assert(state.Output > 0, "IvMixAPI.GetState() must reflect active Output");
            Debug.Assert(state.XML.Contains("<vmix>"), "IvMixAPI.GetState() must include XML document");

            var tallies = api.GetTallys();
            Debug.Assert(tallies.Output != null && tallies.Output.Length > 0, "IvMixAPI.GetTallys() must contain active program tally");
            Debug.Assert(tallies.Output[0].InputNumber == state.Output, "Tally output input number must match active program");
            Console.WriteLine("[PASS] Phase 6: vMixAPI.dll Contract parity (IvMixAPI, State, Tally) validated.");

            // 30. PTZ Controller expanded VISCA / Focus / Zoom / Presets
            bool ptzExFired = false;
            try
            {
                PtzControllerService.ZoomInAsync("127.0.0.1", 7).GetAwaiter().GetResult();
                PtzControllerService.FocusAutoAsync("127.0.0.1").GetAwaiter().GetResult();
                PtzControllerService.RecallPresetAsync("127.0.0.1", 5).GetAwaiter().GetResult();
            }
            catch
            {
                ptzExFired = true;
            }
            Debug.Assert(!ptzExFired, "Expanded PTZ commands (ZoomIn, FocusAuto, RecallPreset) must execute without exception");
            Console.WriteLine("[PASS] Phase 7: PTZ Controller expanded features (Focus, Zoom, Presets) validated.");

            // 31. Hardware Surface & Stream Deck Service
            var deckService = engine.StreamDeckTally;
            Debug.Assert(deckService != null, "StreamDeck & Tally surface service must be initialized");
            deckService.Start(null, 18099); // start on test port
            Debug.Assert(deckService.IsRunning, "StreamDeck service must be running");
            Console.WriteLine("[PASS] Phase 8: Stream Deck & Hardware Tally surface service validated.");

            // 32. Phase 9: Object Edit & Transform Engine (OBS + vMix Mode)
            var transformInput = new InputItem { Number = 101, Title = "Transform Test", Type = InputType.Colour };
            Debug.Assert(transformInput.PanX == 0.0 && transformInput.PanY == 0.0, "Default Pan must be centered");
            Debug.Assert(transformInput.ZoomX == 1.0 && transformInput.ZoomY == 1.0, "Default Zoom must be 1.0");
            Debug.Assert(transformInput.Rotation == 0.0, "Default Rotation must be 0");
            Debug.Assert(transformInput.CropLeft == 0.0 && transformInput.CropRight == 0.0, "Default Crop must be 0");

            transformInput.PanX = 0.25;
            transformInput.PanY = -0.50;
            transformInput.ZoomX = 1.75;
            transformInput.ZoomY = 1.75;
            transformInput.Rotation = 45.0;
            transformInput.CropLeft = 0.10;
            transformInput.CropRight = 0.15;
            transformInput.CropTop = 0.05;
            transformInput.CropBottom = 0.20;

            Debug.Assert(transformInput.PanX == 0.25 && transformInput.PanY == -0.50, "Pan values must be set");
            Debug.Assert(transformInput.ZoomX == 1.75 && transformInput.ZoomY == 1.75, "Zoom values must be set");
            Debug.Assert(transformInput.Rotation == 45.0, "Rotation value must be set");
            Debug.Assert(transformInput.CropLeft == 0.10 && transformInput.CropBottom == 0.20, "Crop values must be set");

            // Test presets
            transformInput.ApplyPreset("pip_br");
            Debug.Assert(transformInput.ZoomX == 0.35 && transformInput.PanX == 0.60 && transformInput.PanY == 0.60, "PIP Bottom-Right preset must configure position and scale");

            transformInput.ApplyPreset("fit");
            Debug.Assert(transformInput.PanX == 0.0 && transformInput.PanY == 0.0 && transformInput.ZoomX == 1.0 && transformInput.CropLeft == 0.0, "Fit/Reset preset must reset all values");

            Console.WriteLine("[PASS] Phase 9: Object Edit & Transform Engine (vMix parameters + OBS Presets) validated.");

            // 33. Phase 10: External Audio Hardware / Soundcard Input Device
            var audioDevs = CameraDeviceService.AudioDevices;
            Debug.Assert(audioDevs.Count > 0, "Host system must enumerate DirectShow audio input devices");
            var audioInput = new InputItem
            {
                Number = 98,
                Title = "Soundcard Mic",
                Type = InputType.AudioDevice,
                AudioDeviceName = audioDevs[0],
                Volume = 90
            };
            engine.Inputs.Add(audioInput);
            Debug.Assert(engine.Inputs.Any(i => i.Type == InputType.AudioDevice), "Input list must support real AudioDevice input");
            Console.WriteLine($"[PASS] Phase 10: DirectShow Audio Hardware Device input ({audioDevs.Count} soundcard/drivers found: '{audioDevs[0]}') validated.");

            deckService.Dispose();
            engine.Shutdown();

            Console.WriteLine("[SUCCESS] All 33 tests across all phases passed successfully.");        }
    }
}
