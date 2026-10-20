using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Threading;
using VMixClone.Models;

namespace VMixClone.Services
{
    public class ProductionEngine : INotifyPropertyChanged
    {
        private InputItem? _previewInput;
        private InputItem? _programInput;
        private InputItem? _transitionTargetInput;
        private InputItem? _overlay1;
        private InputItem? _overlay2;
        private InputItem? _overlay3;
        private InputItem? _overlay4;
        private InputItem? _overlay5;
        private InputItem? _overlay6;
        private InputItem? _overlay7;
        private InputItem? _overlay8;

        private bool _isTransitioning = false;
        private double _transitionProgress = 0.0;
        private TransitionEffect _currentTransition = TransitionEffect.Fade;
        private int _transitionDurationMs = 500;

        private double _masterVolume = 85.0;
        private bool _masterMuted = false;
        private double _masterLevelL = 0.75;
        private double _masterLevelR = 0.75;

        private double _busAVolume = 80.0;
        private bool _busAMuted = false;
        private double _busALevelL = 0.6;
        private double _busALevelR = 0.6;

        private bool _isRecording = false;
        private TimeSpan _recordDuration = TimeSpan.Zero;
        private bool _isStreaming = false;
        private TimeSpan _streamDuration = TimeSpan.Zero;
        private double _renderFps = 59.94;
        private bool _isFTB = false;

        private InputCategory _selectedCategory = InputCategory.All;

        public VMixConfiguration Configuration { get; set; } = VMixConfiguration.Load();
        public RecordingEngine Recording { get; } = new();
        public StreamingEngine Streaming { get; } = new();
        public InstantReplayEngine Replay { get; } = new();

        public string RecordFileSize => Recording.FileSizeFormatted;
        public int StreamBitrateKbps => (int)Streaming.CurrentBitrateKbps;
        public string StreamDataSent => Streaming.DataSentFormatted;

        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _transitionTimer;
        private readonly DispatcherTimer _desktopCaptureTimer;
        private readonly List<CameraCaptureService> _startedCameras = new();
        private readonly List<AudioMeterService> _startedAudioInputs = new();
        private DateTime _transitionStartTime;

        public ObservableCollection<InputItem> Inputs { get; } = new();
        public ICollectionView FilteredInputs { get; }

        // Playlist / photo slideshow playout with real files and real advance timing.
        public PlayoutEngine Playout { get; } = new();
        public DataSourceManager DataSources { get; } = new();
        public IvMixAPI Api { get; }
        public StreamDeckAndTallyService StreamDeckTally { get; }

        // Loads a playout input's folder into the engine and starts advancing it. Called when a
        // Playlist/PhotoSlideshow input is added so its files are only read once per folder.
        public bool PreparePlayout(InputItem item)
        {
            if (item.Type is not (InputType.Playlist or InputType.PhotoSlideshow)) return false;
            if (string.IsNullOrWhiteSpace(item.PlayoutFolder) || !Directory.Exists(item.PlayoutFolder))
            {
                System.Diagnostics.Debug.WriteLine($"[playout] folder missing: '{item.PlayoutFolder}'");
                return false;
            }

            Playout.IncludeSubfolders = false;
            Playout.Shuffle = item.ShufflePlayout;
            Playout.Slideshow.SlideDurationSeconds = item.SlideshowIntervalSeconds;
            int loaded = Playout.LoadFolder(item.PlayoutFolder);
            if (loaded == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[playout] no media in '{item.PlayoutFolder}'");
                return false;
            }

            Playout.Mode = item.Type == InputType.PhotoSlideshow ? PlayoutMode.Slideshow : PlayoutMode.VideoList;
            Playout.Start();
            return true;
        }

        public InputItem? PreviewInput
        {
            get => _previewInput;
            private set
            {
                if (_previewInput != value)
                {
                    if (_previewInput != null) _previewInput.IsPreview = false;
                    _previewInput = value;
                    if (_previewInput != null) _previewInput.IsPreview = true;
                    OnPropertyChanged();
                }
            }
        }

        public InputItem? ProgramInput
        {
            get => _programInput;
            private set
            {
                if (_programInput != value)
                {
                    if (_programInput != null) _programInput.IsProgram = false;
                    _programInput = value;
                    if (_programInput != null) _programInput.IsProgram = true;
                    OnPropertyChanged();
                }
            }
        }

        public InputItem? TransitionTargetInput
        {
            get => _transitionTargetInput;
            private set { _transitionTargetInput = value; OnPropertyChanged(); }
        }

        public InputItem? Overlay1
        {
            get => _overlay1;
            set
            {
                if (_overlay1 != null) _overlay1.IsOverlay1 = false;
                _overlay1 = value;
                if (_overlay1 != null) _overlay1.IsOverlay1 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay2
        {
            get => _overlay2;
            set
            {
                if (_overlay2 != null) _overlay2.IsOverlay2 = false;
                _overlay2 = value;
                if (_overlay2 != null) _overlay2.IsOverlay2 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay3
        {
            get => _overlay3;
            set
            {
                if (_overlay3 != null) _overlay3.IsOverlay3 = false;
                _overlay3 = value;
                if (_overlay3 != null) _overlay3.IsOverlay3 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay4
        {
            get => _overlay4;
            set
            {
                if (_overlay4 != null) _overlay4.IsOverlay4 = false;
                _overlay4 = value;
                if (_overlay4 != null) _overlay4.IsOverlay4 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay5
        {
            get => _overlay5;
            set
            {
                if (_overlay5 != null) _overlay5.IsOverlay5 = false;
                _overlay5 = value;
                if (_overlay5 != null) _overlay5.IsOverlay5 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay6
        {
            get => _overlay6;
            set
            {
                if (_overlay6 != null) _overlay6.IsOverlay6 = false;
                _overlay6 = value;
                if (_overlay6 != null) _overlay6.IsOverlay6 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay7
        {
            get => _overlay7;
            set
            {
                if (_overlay7 != null) _overlay7.IsOverlay7 = false;
                _overlay7 = value;
                if (_overlay7 != null) _overlay7.IsOverlay7 = true;
                OnPropertyChanged();
            }
        }

        public InputItem? Overlay8
        {
            get => _overlay8;
            set
            {
                if (_overlay8 != null) _overlay8.IsOverlay8 = false;
                _overlay8 = value;
                if (_overlay8 != null) _overlay8.IsOverlay8 = true;
                OnPropertyChanged();
            }
        }

        public AudioMixerEngine AudioMixer { get; } = new();
        public ScriptingAndTriggersEngine Automation { get; } = new();
        public NdiSrtService NdiSrt { get; } = new();
        public AudioMeterService AudioMeter { get; } = new();
        public NdiDiscoveryService NdiDiscovery { get; } = new();

        // Program output surface supplied by the shell; when null, record/stream fall back to
        // grabbing the desktop.
        public IFrameSource? ProgramFrames { get; set; }
        public ObservableCollection<StingerTransition> Stingers { get; } = new();

        public bool IsTransitioning
        {
            get => _isTransitioning;
            private set { _isTransitioning = value; OnPropertyChanged(); }
        }

        public double TransitionProgress
        {
            get => _transitionProgress;
            set
            {
                _transitionProgress = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged();
            }
        }

        public TransitionEffect CurrentTransition
        {
            get => _currentTransition;
            set { _currentTransition = value; OnPropertyChanged(); }
        }

        public int TransitionDurationMs
        {
            get => _transitionDurationMs;
            set { _transitionDurationMs = Math.Max(50, value); OnPropertyChanged(); }
        }

        public double MasterVolume
        {
            get => _masterVolume;
            set { _masterVolume = Math.Clamp(value, 0.0, 100.0); OnPropertyChanged(); }
        }

        public bool MasterMuted
        {
            get => _masterMuted;
            set { _masterMuted = value; OnPropertyChanged(); }
        }

        public double MasterLevelL
        {
            get => _masterLevelL;
            set { _masterLevelL = value; OnPropertyChanged(); }
        }

        public double MasterLevelR
        {
            get => _masterLevelR;
            set { _masterLevelR = value; OnPropertyChanged(); }
        }

        public double BusAVolume
        {
            get => _busAVolume;
            set { _busAVolume = Math.Clamp(value, 0.0, 100.0); OnPropertyChanged(); }
        }

        public bool BusAMuted
        {
            get => _busAMuted;
            set { _busAMuted = value; OnPropertyChanged(); }
        }

        public double BusALevelL
        {
            get => _busALevelL;
            set { _busALevelL = value; OnPropertyChanged(); }
        }

        public double BusALevelR
        {
            get => _busALevelR;
            set { _busALevelR = value; OnPropertyChanged(); }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set { _isRecording = value; OnPropertyChanged(); }
        }

        public TimeSpan RecordDuration
        {
            get => _recordDuration;
            private set { _recordDuration = value; OnPropertyChanged(); }
        }

        public bool IsStreaming
        {
            get => _isStreaming;
            set { _isStreaming = value; OnPropertyChanged(); }
        }

        public TimeSpan StreamDuration
        {
            get => _streamDuration;
            private set { _streamDuration = value; OnPropertyChanged(); }
        }

        public double RenderFps
        {
            get => _renderFps;
            set { _renderFps = value; OnPropertyChanged(); }
        }

        public bool IsFTB
        {
            get => _isFTB;
            set { _isFTB = value; OnPropertyChanged(); }
        }

        public InputCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    FilteredInputs.Refresh();
                    OnPropertyChanged();
                }
            }
        }

        // ponytail: Software mixing and simulated audio meters in DispatcherTimer; upgrade to hardware capture (DeckLink/DirectShow) and WASAPI native buffer when physical SDI/HDMI cards are attached.
        public ProductionEngine()
        {
            Api = new VMixApiService(this);
            StreamDeckTally = new StreamDeckAndTallyService(this, Api);
            FilteredInputs = CollectionViewSource.GetDefaultView(Inputs);
            FilteredInputs.Filter = FilterByCategory;

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _clockTimer.Tick += OnClockTick;
            _clockTimer.Start();

            _transitionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _transitionTimer.Tick += OnTransitionTick;

            _desktopCaptureTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(66) };
            _desktopCaptureTimer.Tick += OnDesktopCaptureTick;
            _desktopCaptureTimer.Start();

            InitDefaultInputs();

            Inputs.CollectionChanged += (_, e) =>
            {
                foreach (var added in e.NewItems?.Cast<InputItem>() ?? Enumerable.Empty<InputItem>())
                {
                    PreparePlayout(added);
                }
                SyncCameraCaptures();
            };
            SyncCameraCaptures();
        }

        // Opens a real DirectShow graph per camera input through ffmpeg and closes the ones
        // whose input was removed; the audio meter follows the first available capture device.
        public void SyncCameraCaptures()
        {
            foreach (var inp in Inputs)
            {
                if (inp.Type != InputType.Camera) continue;
                if (inp.Capture is { IsRunning: true } running && running.DeviceName == inp.CameraDeviceName) continue;

                inp.Capture?.Dispose();
                inp.Capture = null;

                var svc = new CameraCaptureService();
                var (w, h) = ParseResolution(inp.CameraResolution);
                if (svc.Start(inp.CameraDeviceName, w, h, ParseFps(inp.CameraFps)))
                {
                    inp.Capture = svc;
                    _startedCameras.Add(svc);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[camera] {inp.CameraDeviceName}: {svc.LastError}");
                }
            }

            for (int i = _startedCameras.Count - 1; i >= 0; i--)
            {
                if (!Inputs.Any(x => ReferenceEquals(x.Capture, _startedCameras[i])))
                {
                    _startedCameras[i].Dispose();
                    _startedCameras.RemoveAt(i);
                }
            }

            // Sync per-input live DirectShow Audio Devices (soundcards, mic, audio interfaces)
            foreach (var inp in Inputs)
            {
                if (inp.Type != InputType.AudioDevice) continue;
                if (string.IsNullOrWhiteSpace(inp.AudioDeviceName)) continue;
                if (inp.AudioCapture is { IsRunning: true } running && running.DeviceName == inp.AudioDeviceName) continue;

                inp.AudioCapture?.Dispose();
                inp.AudioCapture = null;

                var meter = new AudioMeterService();
                if (meter.Start(inp.AudioDeviceName))
                {
                    inp.AudioCapture = meter;
                    _startedAudioInputs.Add(meter);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[audio-input] {inp.AudioDeviceName}: {meter.LastError}");
                }
            }

            for (int i = _startedAudioInputs.Count - 1; i >= 0; i--)
            {
                if (!Inputs.Any(x => ReferenceEquals(x.AudioCapture, _startedAudioInputs[i])))
                {
                    _startedAudioInputs[i].Dispose();
                    _startedAudioInputs.RemoveAt(i);
                }
            }

            bool anyCapture = Inputs.Any(x => x.Capture is { IsRunning: true });
            // Master Audio Meter: use configured MicAuxDevice1 from Settings if specified,
            // otherwise fallback to auto-detected default Realtek/Microphone.
            string preferredMic = !string.IsNullOrWhiteSpace(Configuration.MicAuxDevice1)
                ? Configuration.MicAuxDevice1
                : string.Empty;

            if (!AudioMeter.IsRunning || (!string.IsNullOrEmpty(preferredMic) && AudioMeter.DeviceName != preferredMic))
            {
                var audioDevs = CameraDeviceService.AudioDevices;
                var device = (!string.IsNullOrEmpty(preferredMic) && audioDevs.Contains(preferredMic))
                             ? preferredMic
                             : audioDevs.FirstOrDefault(d => d.Contains("Realtek", StringComparison.OrdinalIgnoreCase))
                               ?? audioDevs.FirstOrDefault(d => d.StartsWith("Microphone", StringComparison.OrdinalIgnoreCase))
                               ?? audioDevs.FirstOrDefault(d => d.Contains("vMix Audio", StringComparison.OrdinalIgnoreCase))
                               ?? audioDevs.FirstOrDefault();

                if (device != null && !AudioMeter.Start(device))
                {
                    System.Diagnostics.Debug.WriteLine($"[audio] {device}: {AudioMeter.LastError}");
                }
            }
        }

        private static (int Width, int Height) ParseResolution(string? resolution)
        {
            var parts = (resolution ?? "1280x720").Split('x');
            int w = 1280, h = 720;
            if (parts.Length == 2)
            {
                int.TryParse(parts[0].Trim(), out w);
                int.TryParse(parts[1].Trim(), out h);
            }
            if (w <= 0) w = 1280;
            if (h <= 0) h = 720;
            return (w, h);
        }

        private static int ParseFps(string? fps)
        {
            string digits = new string((fps ?? "30").TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            return double.TryParse(digits, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v) && v > 0
                ? (int)Math.Round(v)
                : 30;
        }

        private bool FilterByCategory(object item)
        {
            if (_selectedCategory == InputCategory.All) return true;
            if (item is InputItem input)
            {
                return input.Category == _selectedCategory;
            }
            return true;
        }

        private void InitDefaultInputs()
        {
            Inputs.Add(new InputItem
            {
                Number = 1,
                Title = "Color Bars",
                Type = InputType.ColorBars,
                Category = InputCategory.All,
                AudioLevelL = 0.65,
                AudioLevelR = 0.65
            });

            Inputs.Add(new InputItem
            {
                Number = 2,
                Title = "Studio Clock",
                Type = InputType.Clock,
                Category = InputCategory.Orange,
                AudioLevelL = 0.4,
                AudioLevelR = 0.4
            });

            Inputs.Add(new InputItem
            {
                Number = 3,
                Title = "Lower Third Title",
                Type = InputType.Title,
                Category = InputCategory.Green,
                Headline = "BREAKING NEWS",
                Subtitle = "vMix 29 Studio Production Online",
                AudioLevelL = 0.0,
                AudioLevelR = 0.0,
                IsMuted = true
            });

            Inputs.Add(new InputItem
            {
                Number = 4,
                Title = "Camera 1",
                Type = InputType.Camera,
                CameraDeviceName = CameraDeviceService.VideoDevices.FirstOrDefault() ?? string.Empty,
                CameraResolution = "1280x720",
                CameraFps = "30p",
                Category = InputCategory.Red,
                AudioLevelL = 0.8,
                AudioLevelR = 0.85
            });

            Inputs.Add(new InputItem
            {
                Number = 5,
                Title = "Desktop Display",
                Type = InputType.DesktopCapture,
                Category = InputCategory.Green,
                AudioLevelL = 0.5,
                AudioLevelR = 0.5
            });

            Inputs.Add(new InputItem
            {
                Number = 6,
                Title = "Video Player",
                Type = InputType.Video,
                Category = InputCategory.Blue,
                AudioLevelL = 0.75,
                AudioLevelR = 0.75
            });

            Inputs.Add(new InputItem
            {
                Number = 7,
                Title = "Channel Logo Bug",
                Type = InputType.Title,
                Category = InputCategory.Purple,
                Headline = "LIVE HD",
                Subtitle = "vMix Studio Network",
                AudioLevelL = 0.0,
                AudioLevelR = 0.0,
                IsMuted = true
            });

            // Virtual Set Input
            string vsPath = @"C:\Program Files (x86)\vMix\VirtualSets\NewsStudio";
            var vs = VirtualSet.LoadFromFolder(Directory.Exists(vsPath) ? vsPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VirtualSets", "NewsStudio"));
            var talentL = vs.Layers.FirstOrDefault(l => l.PlaceholderType == "Talent");
            if (talentL != null) talentL.AssignedInputNumber = 4; // Map Camera 1
            var screenL = vs.Layers.FirstOrDefault(l => l.PlaceholderType == "Screen");
            if (screenL != null) screenL.AssignedInputNumber = 6; // Map Video Player

            Inputs.Add(new InputItem
            {
                Number = 8,
                Title = $"VS - {vs.Name}",
                Type = InputType.VirtualSet,
                Category = InputCategory.Aqua,
                VirtualSet = vs
            });

            PreviewInput = Inputs[1];
            ProgramInput = Inputs[0];
            Overlay1 = null;
        }

        private readonly Random _rnd = new();

        private void OnDesktopCaptureTick(object? sender, EventArgs e)
        {
            var desktopInput = Inputs.FirstOrDefault(x => x.Type == InputType.DesktopCapture);
            if (desktopInput != null && desktopInput.IsVisible)
            {
                var bmp = ScreenCaptureService.CapturePrimaryScreen();
                if (bmp != null)
                {
                    desktopInput.DesktopCaptureSource = bmp;
                }
            }
        }

        private void OnClockTick(object? sender, EventArgs e)
        {
            if (IsRecording) RecordDuration = Recording.Duration;
            if (IsStreaming) StreamDuration = Streaming.Duration;

            // Real capture levels when a DirectShow audio device is attached, jitter metrics otherwise.
            // ponytail: jitter fallback only for demo inputs (colour bars/clock) with no audio device; drop it once every bus has a real meter.
            bool liveMeters = AudioMeter.IsRunning;
            double baseMaster = MasterMuted ? 0.0 : (MasterVolume / 100.0);
            if (liveMeters)
            {
                MasterLevelL = Math.Clamp(AudioMeter.LevelL * baseMaster, 0.0, 1.0);
                MasterLevelR = Math.Clamp(AudioMeter.LevelR * baseMaster, 0.0, 1.0);
            }
            else if (ProgramInput != null && !ProgramInput.IsMuted)
            {
                double inputScale = ProgramInput.Volume / 100.0;
                double jitterL = (_rnd.NextDouble() * 0.14 - 0.07);
                double jitterR = (_rnd.NextDouble() * 0.14 - 0.07);
                MasterLevelL = Math.Clamp(baseMaster * (0.7 + jitterL) * inputScale, 0.0, 1.0);
                MasterLevelR = Math.Clamp(baseMaster * (0.7 + jitterR) * inputScale, 0.0, 1.0);
            }
            else
            {
                MasterLevelL = 0.0;
                MasterLevelR = 0.0;
            }

            double baseBusA = BusAMuted ? 0.0 : (BusAVolume / 100.0);
            if (liveMeters)
            {
                BusALevelL = Math.Clamp(AudioMeter.LevelL * baseBusA, 0.0, 1.0);
                BusALevelR = Math.Clamp(AudioMeter.LevelR * baseBusA, 0.0, 1.0);
            }
            else
            {
                BusALevelL = Math.Clamp(baseBusA * (0.6 + (_rnd.NextDouble() * 0.1 - 0.05)), 0.0, 1.0);
                BusALevelR = Math.Clamp(baseBusA * (0.6 + (_rnd.NextDouble() * 0.1 - 0.05)), 0.0, 1.0);
            }

            foreach (var inp in Inputs)
            {
                if (inp.Type == InputType.AudioDevice && inp.AudioCapture is { IsRunning: true } devCapture)
                {
                    if (!inp.IsMuted && inp.Volume > 0)
                    {
                        double scale = inp.Volume / 100.0;
                        inp.AudioLevelL = Math.Clamp(devCapture.LevelL * scale, 0.0, 1.0);
                        inp.AudioLevelR = Math.Clamp(devCapture.LevelR * scale, 0.0, 1.0);
                    }
                    else
                    {
                        inp.AudioLevelL = 0.0;
                        inp.AudioLevelR = 0.0;
                    }
                }
                else if (!inp.IsMuted && inp.Volume > 0)
                {
                    double jL = (_rnd.NextDouble() * 0.1 - 0.05);
                    double jR = (_rnd.NextDouble() * 0.1 - 0.05);
                    inp.AudioLevelL = Math.Clamp((inp.Volume / 100.0) * (0.65 + jL), 0.0, 1.0);
                    inp.AudioLevelR = Math.Clamp((inp.Volume / 100.0) * (0.65 + jR), 0.0, 1.0);
                }
                else
                {
                    inp.AudioLevelL = 0.0;
                    inp.AudioLevelR = 0.0;
                }
            }
        }

        public void Cut()
        {
            if (PreviewInput == null || ProgramInput == null) return;
            if (IsTransitioning)
            {
                _transitionTimer.Stop();
                IsTransitioning = false;
            }

            var temp = ProgramInput;
            ProgramInput = PreviewInput;
            PreviewInput = temp;
            TransitionProgress = 0.0;
            Automation.FireTrigger("OnTransitionComplete", ExecuteApiFunction);
        }

        public void StartTransition(TransitionEffect effect, int durationMs = -1)
        {
            if (PreviewInput == null || ProgramInput == null || IsTransitioning) return;

            CurrentTransition = effect;
            int dur = durationMs > 0 ? durationMs : TransitionDurationMs;
            TransitionTargetInput = PreviewInput;
            TransitionProgress = 0.0;
            IsTransitioning = true;
            _transitionStartTime = DateTime.UtcNow;
            _transitionTimer.Start();
        }

        private void OnTransitionTick(object? sender, EventArgs e)
        {
            double elapsed = (DateTime.UtcNow - _transitionStartTime).TotalMilliseconds;
            double progress = elapsed / TransitionDurationMs;

            if (progress >= 1.0)
            {
                _transitionTimer.Stop();
                TransitionProgress = 1.0;
                IsTransitioning = false;

                // Complete swap
                var temp = ProgramInput;
                ProgramInput = PreviewInput;
                PreviewInput = temp;
                TransitionProgress = 0.0;
                TransitionTargetInput = null;
                Automation.FireTrigger("OnTransitionComplete", ExecuteApiFunction);
            }
            else
            {
                TransitionProgress = progress;
            }
        }

        public void SetTBar(double progress)
        {
            if (IsTransitioning)
            {
                _transitionTimer.Stop();
                IsTransitioning = false;
            }

            TransitionProgress = Math.Clamp(progress, 0.0, 1.0);
            if (TransitionProgress >= 1.0)
            {
                var temp = ProgramInput;
                ProgramInput = PreviewInput;
                PreviewInput = temp;
                TransitionProgress = 0.0;
            }
        }

        public void SetPreview(int number)
        {
            var target = Inputs.FirstOrDefault(x => x.Number == number);
            if (target != null && target != PreviewInput)
            {
                PreviewInput = target;
            }
        }

        public void SetProgram(int number)
        {
            var target = Inputs.FirstOrDefault(x => x.Number == number);
            if (target != null && target != ProgramInput)
            {
                ProgramInput = target;
            }
        }

        public void QuickPlay(int number)
        {
            SetPreview(number);
            Cut();
        }

        // Single entry point every surface (UI, HTTP, TCP, scripts, triggers) routes through, so
        // automation added here works identically for Stream Deck/Companion callers.
        public void ExecuteApiFunction(string function, string parameters = "")
        {
            var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in parameters.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = kv.IndexOf('=');
                if (eq > 0) args[Uri.UnescapeDataString(kv[..eq])] = Uri.UnescapeDataString(kv[(eq + 1)..]);
            }

            int input = args.TryGetValue("Input", out string? inStr) && int.TryParse(inStr, out int n) ? n : 0;
            int duration = args.TryGetValue("Duration", out string? dStr) && int.TryParse(dStr, out int d) ? d : 0;
            string value = args.TryGetValue("Value", out string? v) ? v
                         : args.TryGetValue("Name", out string? nm) ? nm : string.Empty;

            switch (function.ToLowerInvariant())
            {
                case "cut": Cut(); break;
                case "fade": StartTransition(TransitionEffect.Fade, duration > 0 ? duration : TransitionDurationMs); break;
                case "zoom": StartTransition(TransitionEffect.Zoom, duration > 0 ? duration : TransitionDurationMs); break;
                case "wipe": StartTransition(TransitionEffect.Wipe, duration > 0 ? duration : TransitionDurationMs); break;
                case "slide": StartTransition(TransitionEffect.Slide, duration > 0 ? duration : TransitionDurationMs); break;
                case "merge": StartTransition(TransitionEffect.Merge, duration > 0 ? duration : TransitionDurationMs); break;
                case "fly": StartTransition(TransitionEffect.Fly, duration > 0 ? duration : TransitionDurationMs); break;
                case "verticalwipe": StartTransition(TransitionEffect.VerticalWipe, duration > 0 ? duration : TransitionDurationMs); break;
                case "barndoor": StartTransition(TransitionEffect.BarnDoor, duration > 0 ? duration : TransitionDurationMs); break;
                case "cubezoom": StartTransition(TransitionEffect.CubeZoom, duration > 0 ? duration : TransitionDurationMs); break;
                case "activeinput": if (input > 0) SetProgram(input); break;
                case "previewinput": if (input > 0) SetPreview(input); break;
                case "quickplay": if (input > 0) QuickPlay(input); break;
                case "overlayinput1": ToggleOverlay(1, input > 0 ? input : null); break;
                case "overlayinput2": ToggleOverlay(2, input > 0 ? input : null); break;
                case "overlayinput3": ToggleOverlay(3, input > 0 ? input : null); break;
                case "overlayinput4": ToggleOverlay(4, input > 0 ? input : null); break;
                case "overlayinput5": ToggleOverlay(5, input > 0 ? input : null); break;
                case "overlayinput6": ToggleOverlay(6, input > 0 ? input : null); break;
                case "overlayinput7": ToggleOverlay(7, input > 0 ? input : null); break;
                case "overlayinput8": ToggleOverlay(8, input > 0 ? input : null); break;
                case "overlayinput1in": OverlayIn(1, input > 0 ? input : null); break;
                case "overlayinput2in": OverlayIn(2, input > 0 ? input : null); break;
                case "overlayinput3in": OverlayIn(3, input > 0 ? input : null); break;
                case "overlayinput4in": OverlayIn(4, input > 0 ? input : null); break;
                case "overlayinput1out":
                case "overlayinput1off": OverlayOut(1); break;
                case "overlayinput2out":
                case "overlayinput2off": OverlayOut(2); break;
                case "overlayinput3out":
                case "overlayinput3off": OverlayOut(3); break;
                case "overlayinput4out":
                case "overlayinput4off": OverlayOut(4); break;
                case "overlayalloff":
                case "overlayinputalloff": OverlayAllOff(); break;
                case "stinger1": _ = PlayStingerAsync(1); break;
                case "stinger2": _ = PlayStingerAsync(2); break;
                case "stinger3": _ = PlayStingerAsync(3); break;
                case "stinger4": _ = PlayStingerAsync(4); break;
                case "startstoprecord": ToggleRecord(); break;
                case "startstopstreaming": ToggleStream(); break;
                case "fadetoblack": ToggleFTB(); break;
                case "settbar":
                    if (args.TryGetValue("Value", out string? tbarStr) && double.TryParse(tbarStr, out double tbar))
                    {
                        if (tbar > 1.0) tbar /= 255.0;
                        SetTBar(tbar);
                    }
                    break;
                case "setvolume":
                    if (input > 0 && args.TryGetValue("Volume", out string? volStr) && double.TryParse(volStr, out double vol))
                    {
                        var target = Inputs.FirstOrDefault(x => x.Number == input);
                        if (target != null) target.Volume = vol;
                    }
                    break;
                case "audioon":
                case "audiooff":
                    if (input > 0)
                    {
                        var target = Inputs.FirstOrDefault(x => x.Number == input);
                        if (target != null) target.IsMuted = function.Equals("audiooff", StringComparison.OrdinalIgnoreCase);
                    }
                    break;
                case "setvirtualsetcamerapreset":
                case "virtualsetcamerapreset":
                    var vs = ProgramInput?.VirtualSet ?? PreviewInput?.VirtualSet;
                    var preset = vs?.CameraPresets.FirstOrDefault(p => p.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                    if (vs != null && preset != null) vs.CurrentPreset = preset;
                    break;
                case "ptzleft":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.PanLeftAsync(value);
                    break;
                case "ptzright":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.PanRightAsync(value);
                    break;
                case "ptzup":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.TiltUpAsync(value);
                    break;
                case "ptzdown":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.TiltDownAsync(value);
                    break;
                case "ptzstop":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.StopPanTiltAsync(value);
                    break;
                case "ptzzoomin":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.ZoomInAsync(value);
                    break;
                case "ptzzoomout":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.ZoomOutAsync(value);
                    break;
                case "ptzfocusauto":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.FocusAutoAsync(value);
                    break;
                case "ptzfocusmanual":
                    if (!string.IsNullOrWhiteSpace(value)) _ = PtzControllerService.FocusManualAsync(value);
                    break;
                case "ptzrecallpreset":
                    if (!string.IsNullOrWhiteSpace(value) && int.TryParse(args.GetValueOrDefault("Preset", "1"), out int prId))
                        _ = PtzControllerService.RecallPresetAsync(value, prId);
                    break;
            }
        }

        public InputItem? GetOverlay(int channel) => channel switch
        {
            1 => Overlay1,
            2 => Overlay2,
            3 => Overlay3,
            4 => Overlay4,
            5 => Overlay5,
            6 => Overlay6,
            7 => Overlay7,
            8 => Overlay8,
            _ => null
        };

        public void SetOverlay(int channel, InputItem? item)
        {
            switch (channel)
            {
                case 1: Overlay1 = item; break;
                case 2: Overlay2 = item; break;
                case 3: Overlay3 = item; break;
                case 4: Overlay4 = item; break;
                case 5: Overlay5 = item; break;
                case 6: Overlay6 = item; break;
                case 7: Overlay7 = item; break;
                case 8: Overlay8 = item; break;
            }
        }

        public void ToggleOverlay(int channel, int? inputNumber = null)
        {
            InputItem? current = GetOverlay(channel);

            if (inputNumber.HasValue)
            {
                var target = Inputs.FirstOrDefault(x => x.Number == inputNumber.Value);
                if (target == null) return;

                // In vMix: if this specific input is already on this channel, take it off.
                // If another input is on this channel, or channel is empty, switch to this input.
                SetOverlay(channel, current == target ? null : target);
            }
            else
            {
                // Toggle channel directly from Main Bar or Shortcut (e.g. F1-F8):
                // In vMix: if channel is active, take it off regardless of which input it was.
                // If channel is inactive, overlay PreviewInput (or last assigned).
                if (current != null)
                {
                    SetOverlay(channel, null);
                }
                else if (PreviewInput != null)
                {
                    SetOverlay(channel, PreviewInput);
                }
            }
        }

        public void OverlayIn(int channel, int? inputNumber = null)
        {
            var target = inputNumber.HasValue
                ? Inputs.FirstOrDefault(x => x.Number == inputNumber.Value)
                : PreviewInput;
            if (target != null) SetOverlay(channel, target);
        }

        public void OverlayOut(int channel)
        {
            SetOverlay(channel, null);
        }

        public void OverlayAllOff()
        {
            Overlay1 = null;
            Overlay2 = null;
            Overlay3 = null;
            Overlay4 = null;
            Overlay5 = null;
            Overlay6 = null;
            Overlay7 = null;
            Overlay8 = null;
        }

        public async Task PlayStingerAsync(int stingerNumber)
        {
            var stinger = Stingers.FirstOrDefault(s => s.StingerNumber == stingerNumber);
            if (stinger == null) return;

            var stingerInput = Inputs.FirstOrDefault(i => i.Number == stinger.AssignedInputNumber);
            if (stingerInput == null) return;

            // vMix standard: Stinger uses Overlay 4 (or configured channel)
            SetOverlay(4, stingerInput);
            stingerInput.IsPlaying = true;

            // Wait until Cut Point (when animation covers the screen)
            int cutPoint = Math.Max(50, stinger.CutPointMs);
            await Task.Delay(cutPoint);
            Cut();

            // Wait until Stinger animation finishes
            int remaining = Math.Max(50, stinger.DurationMs - cutPoint);
            await Task.Delay(remaining);

            if (Overlay4 == stingerInput)
            {
                SetOverlay(4, null);
            }
        }

        public void ToggleRecord()
        {
            if (Recording.IsRecording)
            {
                Recording.Stop();
                IsRecording = false;
                RecordDuration = TimeSpan.Zero;
            }
            else
            {
                bool started = Recording.Start(Configuration.RecordingFolder, Configuration.RecordingFormat, Configuration.RecordingBitrateKbps, ProgramFrames);
                IsRecording = started;
                if (!started) RecordDuration = TimeSpan.Zero;
                else Automation.FireTrigger("OnRecordingStart", ExecuteApiFunction);
            }
        }

        public void ToggleStream()
        {
            if (Streaming.IsLive || Streaming.Status == StreamStatus.Connecting)
            {
                Streaming.Stop();
                IsStreaming = false;
                StreamDuration = TimeSpan.Zero;
            }
            else
            {
                Streaming.Start(Configuration.StreamingUrl, Configuration.StreamingKey, Configuration.StreamingBitrateKbps, ProgramFrames);
                IsStreaming = true;
                if (Streaming.ReportedLive) Automation.FireTrigger("OnStreamStart", ExecuteApiFunction);
            }
        }

        public void ToggleFTB()
        {
            IsFTB = !IsFTB;
        }

        // Closes the capture graphs and meters this engine opened; call before the app exits so
        // no ffmpeg child is left holding a camera.
        public void Shutdown()
        {
            _clockTimer.Stop();
            _transitionTimer.Stop();
            _desktopCaptureTimer.Stop();
            foreach (var cam in _startedCameras) cam.Dispose();
            _startedCameras.Clear();
            foreach (var a in _startedAudioInputs) a.Dispose();
            _startedAudioInputs.Clear();
            AudioMeter.Dispose();
            Playout.Dispose();
            StreamDeckTally.Dispose();
            DataSources.Dispose();
            if (Api is IDisposable dApi) dApi.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
