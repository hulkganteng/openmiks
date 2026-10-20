using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Openmiks.Services;

namespace Openmiks.Models
{
    public class InputItem : INotifyPropertyChanged
    {
        private int _number;
        private string _key = Guid.NewGuid().ToString();
        private string _title = "Input";
        private InputType _type = InputType.ColorBars;
        private InputCategory _category = InputCategory.All;
        private string _categoryHex = "#3e3e42";

        private double _volume = 100.0;
        private bool _isMuted = false;
        private bool _isSolo = false;
        private double _audioLevelL = 0.65;
        private double _audioLevelR = 0.65;
        private string _audioBusses = "M";

        private string _filePath = string.Empty;
        private Color _solidColor = Colors.DarkSlateGray;
        private string _headline = "LIVE BROADCAST";
        private string _subtitle = "Breaking News & Analysis";
        private string _titleTemplatePath = string.Empty;
        private string _ptzIpAddress = string.Empty;
        private string _playoutFolder = string.Empty;
        private int _slideshowIntervalSeconds = 5;
        private bool _shufflePlayout;

        private string _cameraDeviceName = string.Empty;
        private string _cameraResolution = "1920x1080";
        private string _cameraFps = "59.94p";
        private string _webUrl = "https://www.google.com";

        private bool _isProgram = false;
        private bool _isPreview = false;
        private bool _isOverlay1 = false;
        private bool _isOverlay2 = false;
        private bool _isOverlay3 = false;
        private bool _isOverlay4 = false;
        private bool _isOverlay5 = false;
        private bool _isOverlay6 = false;
        private bool _isOverlay7 = false;
        private bool _isOverlay8 = false;

        private bool _isPlaying = true;
        private bool _isLoop = true;
        private TimeSpan _mediaPosition = TimeSpan.Zero;
        private TimeSpan _mediaDuration = TimeSpan.FromMinutes(1);

        private ImageSource? _desktopCaptureSource;
        private ImageSource? _imageSource;
        private VirtualSet? _virtualSet;
        private CameraCaptureService? _capture;
        private AudioMeterService? _audioCapture;
        private string _audioDeviceName = string.Empty;

        private bool _enableChromaKey = false;
        private Color _chromaKeyColor = Color.FromRgb(0, 255, 0);
        private double _chromaSimilarity = 0.38;
        private double _chromaSmoothness = 0.10;
        private double _chromaSpill = 0.80;

        // Object Edit / Transform (vMix + OBS Mode)
        private double _panX = 0.0;
        private double _panY = 0.0;
        private double _zoomX = 1.0;
        private double _zoomY = 1.0;
        private double _rotation = 0.0;
        private double _cropLeft = 0.0;
        private double _cropRight = 0.0;
        private double _cropTop = 0.0;
        private double _cropBottom = 0.0;

        public VirtualSet? VirtualSet
        {
            get => _virtualSet;
            set { _virtualSet = value; OnPropertyChanged(); }
        }

        public bool EnableChromaKey
        {
            get => _enableChromaKey;
            set { _enableChromaKey = value; OnPropertyChanged(); }
        }

        public Color ChromaKeyColor
        {
            get => _chromaKeyColor;
            set { _chromaKeyColor = value; OnPropertyChanged(); }
        }

        public double ChromaSimilarity
        {
            get => _chromaSimilarity;
            set { _chromaSimilarity = value; OnPropertyChanged(); }
        }

        public double ChromaSmoothness
        {
            get => _chromaSmoothness;
            set { _chromaSmoothness = value; OnPropertyChanged(); }
        }

        public double ChromaSpill
        {
            get => _chromaSpill;
            set { _chromaSpill = value; OnPropertyChanged(); }
        }

        // --- Object Edit / Transform Properties (OBS + vMix) ---
        public double PanX
        {
            get => _panX;
            set { _panX = value; OnPropertyChanged(); }
        }

        public double PanY
        {
            get => _panY;
            set { _panY = value; OnPropertyChanged(); }
        }

        public double ZoomX
        {
            get => _zoomX;
            set { _zoomX = Math.Max(0.01, value); OnPropertyChanged(); }
        }

        public double ZoomY
        {
            get => _zoomY;
            set { _zoomY = Math.Max(0.01, value); OnPropertyChanged(); }
        }

        public double Rotation
        {
            get => _rotation;
            set { _rotation = value; OnPropertyChanged(); }
        }

        public double CropLeft
        {
            get => _cropLeft;
            set { _cropLeft = Math.Clamp(value, 0.0, 0.95); OnPropertyChanged(); }
        }

        public double CropRight
        {
            get => _cropRight;
            set { _cropRight = Math.Clamp(value, 0.0, 0.95); OnPropertyChanged(); }
        }

        public double CropTop
        {
            get => _cropTop;
            set { _cropTop = Math.Clamp(value, 0.0, 0.95); OnPropertyChanged(); }
        }

        public double CropBottom
        {
            get => _cropBottom;
            set { _cropBottom = Math.Clamp(value, 0.0, 0.95); OnPropertyChanged(); }
        }

        public void ResetTransform()
        {
            PanX = 0.0;
            PanY = 0.0;
            ZoomX = 1.0;
            ZoomY = 1.0;
            Rotation = 0.0;
            CropLeft = 0.0;
            CropRight = 0.0;
            CropTop = 0.0;
            CropBottom = 0.0;
        }

        public void ApplyPreset(string preset)
        {
            switch (preset.ToLowerInvariant())
            {
                case "center":
                    PanX = 0.0;
                    PanY = 0.0;
                    break;
                case "100":
                    ZoomX = 1.0;
                    ZoomY = 1.0;
                    break;
                case "fit":
                case "reset":
                    ResetTransform();
                    break;
                case "fill":
                    PanX = 0.0;
                    PanY = 0.0;
                    ZoomX = 1.25;
                    ZoomY = 1.25;
                    Rotation = 0.0;
                    break;
                case "pipbottomright":
                case "pip_br":
                    ZoomX = 0.35;
                    ZoomY = 0.35;
                    PanX = 0.60;
                    PanY = 0.60;
                    Rotation = 0.0;
                    break;
                case "pipbottomleft":
                case "pip_bl":
                    ZoomX = 0.35;
                    ZoomY = 0.35;
                    PanX = -0.60;
                    PanY = 0.60;
                    Rotation = 0.0;
                    break;
                case "piptopright":
                case "pip_tr":
                    ZoomX = 0.35;
                    ZoomY = 0.35;
                    PanX = 0.60;
                    PanY = -0.60;
                    Rotation = 0.0;
                    break;
                case "piptopleft":
                case "pip_tl":
                    ZoomX = 0.35;
                    ZoomY = 0.35;
                    PanX = -0.60;
                    PanY = -0.60;
                    Rotation = 0.0;
                    break;
            }
        }

        public int Number
        {
            get => _number;
            set { _number = value; OnPropertyChanged(); }
        }

        public string Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public InputType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public InputCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                CategoryHex = GetCategoryColor(value);
                OnPropertyChanged();
            }
        }

        public string CategoryHex
        {
            get => _categoryHex;
            set { _categoryHex = value; OnPropertyChanged(); }
        }

        public double Volume
        {
            get => _volume;
            set { _volume = Math.Clamp(value, 0.0, 100.0); OnPropertyChanged(); }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
        }

        public bool IsSolo
        {
            get => _isSolo;
            set { _isSolo = value; OnPropertyChanged(); }
        }

        public double AudioLevelL
        {
            get => _audioLevelL;
            set { _audioLevelL = value; OnPropertyChanged(); }
        }

        public double AudioLevelR
        {
            get => _audioLevelR;
            set { _audioLevelR = value; OnPropertyChanged(); }
        }

        public string AudioBusses
        {
            get => _audioBusses;
            set { _audioBusses = value; OnPropertyChanged(); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public Color SolidColor
        {
            get => _solidColor;
            set { _solidColor = value; OnPropertyChanged(); }
        }

        public string Headline
        {
            get => _headline;
            set { _headline = value; OnPropertyChanged(); }
        }

        public string Subtitle
        {
            get => _subtitle;
            set { _subtitle = value; OnPropertyChanged(); }
        }

        // Path to a real vMix title template (Titles\*.xaml). Empty keeps the built-in lower third.
        public string TitleTemplatePath
        {
            get => _titleTemplatePath;
            set { _titleTemplatePath = value; OnPropertyChanged(); }
        }

        // Camera IP for VISCA over IP PTZ; empty means no PTZ control is offered.
        public string PtzIpAddress
        {
            get => _ptzIpAddress;
            set { _ptzIpAddress = value; OnPropertyChanged(); }
        }

        // Playlist / photo slideshow source folder and options.
        public string PlayoutFolder
        {
            get => _playoutFolder;
            set { _playoutFolder = value; OnPropertyChanged(); }
        }

        public int SlideshowIntervalSeconds
        {
            get => _slideshowIntervalSeconds;
            set { _slideshowIntervalSeconds = Math.Max(1, value); OnPropertyChanged(); }
        }

        public bool ShufflePlayout
        {
            get => _shufflePlayout;
            set { _shufflePlayout = value; OnPropertyChanged(); }
        }

        public string CameraDeviceName
        {
            get => _cameraDeviceName;
            set { _cameraDeviceName = value; OnPropertyChanged(); }
        }

        public string CameraResolution
        {
            get => _cameraResolution;
            set { _cameraResolution = value; OnPropertyChanged(); }
        }

        public string CameraFps
        {
            get => _cameraFps;
            set { _cameraFps = value; OnPropertyChanged(); }
        }

        public string WebUrl
        {
            get => _webUrl;
            set { _webUrl = value; OnPropertyChanged(); }
        }

        public bool IsProgram
        {
            get => _isProgram;
            set { _isProgram = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsPreview
        {
            get => _isPreview;
            set { _isPreview = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay1
        {
            get => _isOverlay1;
            set { _isOverlay1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay2
        {
            get => _isOverlay2;
            set { _isOverlay2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay3
        {
            get => _isOverlay3;
            set { _isOverlay3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay4
        {
            get => _isOverlay4;
            set { _isOverlay4 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay5
        {
            get => _isOverlay5;
            set { _isOverlay5 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay6
        {
            get => _isOverlay6;
            set { _isOverlay6 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay7
        {
            get => _isOverlay7;
            set { _isOverlay7 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsOverlay8
        {
            get => _isOverlay8;
            set { _isOverlay8 = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        public bool IsLoop
        {
            get => _isLoop;
            set { _isLoop = value; OnPropertyChanged(); }
        }

        public TimeSpan MediaPosition
        {
            get => _mediaPosition;
            set { _mediaPosition = value; OnPropertyChanged(); }
        }

        public TimeSpan MediaDuration
        {
            get => _mediaDuration;
            set { _mediaDuration = value; OnPropertyChanged(); }
        }

        public ImageSource? DesktopCaptureSource
        {
            get => _desktopCaptureSource;
            set { _desktopCaptureSource = value; OnPropertyChanged(); }
        }

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(); }
        }

        public CameraCaptureService? Capture
        {
            get => _capture;
            set { _capture = value; OnPropertyChanged(); }
        }

        public AudioMeterService? AudioCapture
        {
            get => _audioCapture;
            set { _audioCapture = value; OnPropertyChanged(); }
        }

        public string AudioDeviceName
        {
            get => _audioDeviceName;
            set { _audioDeviceName = value; OnPropertyChanged(); }
        }

        // True when this input is on any output surface: program, preview or overlays 1-8.
        // Capture graphs and timers key off this so nothing keeps running off-screen.
        public bool IsVisible => IsProgram || IsPreview || IsOverlay1 || IsOverlay2 || IsOverlay3
                              || IsOverlay4 || IsOverlay5 || IsOverlay6 || IsOverlay7 || IsOverlay8;

        private static string GetCategoryColor(InputCategory cat) => cat switch
        {
            InputCategory.Red => "#e53e3e",
            InputCategory.Green => "#38a169",
            InputCategory.Orange => "#dd6b20",
            InputCategory.Purple => "#805ad5",
            InputCategory.Aqua => "#319795",
            InputCategory.Blue => "#3182ce",
            _ => "#3e3e42"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
