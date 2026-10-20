using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Openmiks.Effects;
using Openmiks.Models;

namespace Openmiks.Controls
{
    public partial class VirtualSetView : UserControl
    {
        private VirtualSet? _virtualSet;
        private IList<InputItem>? _availableInputs;
        private readonly DispatcherTimer _cameraPanTimer;
        private double _targetCamX = 0;
        private double _targetCamY = 0;
        private double _targetCamZoom = 1.0;
        private double _startCamX = 0;
        private double _startCamY = 0;
        private double _startCamZoom = 1.0;
        private DateTime _panStartTime;
        private double _panDurationMs = 1200;
        private bool _isPanning = false;

        public static readonly DependencyProperty VirtualSetProperty =
            DependencyProperty.Register(
                nameof(VirtualSet),
                typeof(VirtualSet),
                typeof(VirtualSetView),
                new PropertyMetadata(null, OnVirtualSetChanged));

        public VirtualSet? VirtualSet
        {
            get => (VirtualSet?)GetValue(VirtualSetProperty);
            set => SetValue(VirtualSetProperty, value);
        }

        public IList<InputItem>? AvailableInputs
        {
            get => _availableInputs;
            set
            {
                _availableInputs = value;
                RebuildLayers();
            }
        }

        public VirtualSetView()
        {
            InitializeComponent();

            _cameraPanTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _cameraPanTimer.Tick += OnCameraPanTick;
        }

        private static void OnVirtualSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VirtualSetView view)
            {
                if (e.OldValue is VirtualSet oldVs)
                {
                    oldVs.PropertyChanged -= view.OnVsPropertyChanged;
                }
                if (e.NewValue is VirtualSet newVs)
                {
                    newVs.PropertyChanged += view.OnVsPropertyChanged;
                }
                view._virtualSet = e.NewValue as VirtualSet;
                view.RebuildLayers();
                view.SyncCameraImmediate();
            }
        }

        private void OnVsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VirtualSet.CurrentPreset))
            {
                if (_virtualSet?.CurrentPreset != null)
                {
                    AnimateCameraTo(_virtualSet.CurrentPreset.X, _virtualSet.CurrentPreset.Y, _virtualSet.CurrentPreset.Zoom);
                }
            }
            else
            {
                Dispatcher.Invoke(RebuildLayers);
            }
        }

        public void AnimateCameraTo(double x, double y, double zoom, double durationMs = 1200)
        {
            _startCamX = CameraTranslate.X;
            _startCamY = CameraTranslate.Y;
            _startCamZoom = CameraScale.ScaleX;

            // vMix virtual set coordinates: (0,0) is center, x is normalized -1 to 1, y is -1 to 1
            _targetCamX = -x * 480.0;
            _targetCamY = y * 270.0;
            _targetCamZoom = Math.Max(0.5, zoom);
            _panDurationMs = durationMs;
            _panStartTime = DateTime.UtcNow;
            _isPanning = true;
            _cameraPanTimer.Start();
        }

        private void OnCameraPanTick(object? sender, EventArgs e)
        {
            if (!_isPanning) return;

            double elapsed = (DateTime.UtcNow - _panStartTime).TotalMilliseconds;
            double t = Math.Clamp(elapsed / _panDurationMs, 0.0, 1.0);

            // Smooth cubic ease out
            double ease = 1.0 - Math.Pow(1.0 - t, 3.0);

            CameraTranslate.X = _startCamX + (_targetCamX - _startCamX) * ease;
            CameraTranslate.Y = _startCamY + (_targetCamY - _startCamY) * ease;
            double curZoom = _startCamZoom + (_targetCamZoom - _startCamZoom) * ease;
            CameraScale.ScaleX = curZoom;
            CameraScale.ScaleY = curZoom;

            if (t >= 1.0)
            {
                _cameraPanTimer.Stop();
                _isPanning = false;
            }
        }

        private void SyncCameraImmediate()
        {
            if (_virtualSet?.CurrentPreset != null)
            {
                CameraTranslate.X = -_virtualSet.CurrentPreset.X * 480.0;
                CameraTranslate.Y = _virtualSet.CurrentPreset.Y * 270.0;
                double z = Math.Max(0.5, _virtualSet.CurrentPreset.Zoom);
                CameraScale.ScaleX = z;
                CameraScale.ScaleY = z;
            }
            else
            {
                CameraTranslate.X = 0;
                CameraTranslate.Y = 0;
                CameraScale.ScaleX = 1.0;
                CameraScale.ScaleY = 1.0;
            }
        }

        public void RebuildLayers()
        {
            LayersCanvas.Children.Clear();
            if (_virtualSet == null) return;

            foreach (var layer in _virtualSet.Layers)
            {
                FrameworkElement element;

                if (layer.PlaceholderType == "Talent" || layer.PlaceholderType == "Presenter")
                {
                    // Map live presenter input
                    InputItem? talentInput = null;
                    if (layer.AssignedInputNumber.HasValue)
                    {
                        talentInput = _availableInputs?.FirstOrDefault(x => x.Number == layer.AssignedInputNumber.Value);
                    }
                    if (talentInput == null)
                    {
                        // Default to camera input or first video input
                        talentInput = _availableInputs?.FirstOrDefault(x => x.Type == InputType.Camera) 
                                      ?? _availableInputs?.FirstOrDefault(x => x.Number == 4);
                    }

                    var display = new InputDisplay { Item = talentInput };
                    if (layer.EnableChromaKey)
                    {
                        display.Effect = new ChromaKeyEffect
                        {
                            KeyColor = layer.ChromaKeyColor,
                            Similarity = layer.ChromaSimilarity,
                            Smoothness = layer.ChromaSmoothness,
                            SpillSuppression = layer.ChromaSpill
                        };
                    }
                    element = display;
                }
                else if (layer.PlaceholderType == "Screen")
                {
                    // Map virtual set screen input (video / desktop / web / title)
                    InputItem? screenInput = null;
                    if (layer.AssignedInputNumber.HasValue)
                    {
                        screenInput = _availableInputs?.FirstOrDefault(x => x.Number == layer.AssignedInputNumber.Value);
                    }
                    if (screenInput == null)
                    {
                        screenInput = _availableInputs?.FirstOrDefault(x => x.Type == InputType.Video)
                                      ?? _availableInputs?.FirstOrDefault(x => x.Type == InputType.DesktopCapture)
                                      ?? _availableInputs?.FirstOrDefault(x => x.Number == 5);
                    }

                    element = new InputDisplay { Item = screenInput };
                }
                else
                {
                    // Static layer image from file
                    var img = new Image { Stretch = Stretch.Fill };
                    if (!string.IsNullOrEmpty(layer.ResolvedImagePath) && File.Exists(layer.ResolvedImagePath))
                    {
                        try
                        {
                            img.Source = new BitmapImage(new Uri(layer.ResolvedImagePath));
                        }
                        catch { }
                    }
                    element = img;
                }

                // Sizing and positioning based on layer properties
                // In vMix, virtual set canvas is 1920x1080 (scaled here to 960x540 viewport)
                double layerW = 960.0 * layer.Zoom;
                double layerH = 540.0 * layer.Zoom;
                element.Width = layerW;
                element.Height = layerH;
                element.Opacity = layer.Opacity;

                // Center position offset: x = 0 is center, y = 0 is center
                double posX = (480.0 - (layerW / 2.0)) + (layer.X * 480.0);
                double posY = (270.0 - (layerH / 2.0)) - (layer.Y * 270.0);

                Canvas.SetLeft(element, posX);
                Canvas.SetTop(element, posY);

                if (Math.Abs(layer.Rotation) > 0.01)
                {
                    element.RenderTransform = new RotateTransform(layer.Rotation, layerW / 2.0, layerH / 2.0);
                }

                LayersCanvas.Children.Add(element);
            }
        }
    }
}
