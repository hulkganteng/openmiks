using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Linq;

namespace Openmiks.Models
{
    public class VirtualSetLayer : INotifyPropertyChanged
    {
        private string _name = "Layer";
        private string _fileName = string.Empty;
        private double _x = 0;
        private double _y = 0;
        private double _zoom = 1.0;
        private double _rotation = 0;
        private double _opacity = 1.0;
        private bool _isDynamic = false;
        private string _placeholderType = "Background";
        private int? _assignedInputNumber = null;
        private bool _enableChromaKey = false;
        private Color _chromaKeyColor = Color.FromRgb(0, 255, 0);
        private double _chromaSimilarity = 0.38;
        private double _chromaSmoothness = 0.10;
        private double _chromaSpill = 0.80;
        private string _resolvedImagePath = string.Empty;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public double Zoom
        {
            get => _zoom;
            set { _zoom = value; OnPropertyChanged(); }
        }

        public double Rotation
        {
            get => _rotation;
            set { _rotation = value; OnPropertyChanged(); }
        }

        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(); }
        }

        public bool IsDynamic
        {
            get => _isDynamic;
            set { _isDynamic = value; OnPropertyChanged(); }
        }

        public string PlaceholderType
        {
            get => _placeholderType;
            set { _placeholderType = value; OnPropertyChanged(); }
        }

        public int? AssignedInputNumber
        {
            get => _assignedInputNumber;
            set { _assignedInputNumber = value; OnPropertyChanged(); }
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

        public string ResolvedImagePath
        {
            get => _resolvedImagePath;
            set { _resolvedImagePath = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class VirtualSetCameraPreset : INotifyPropertyChanged
    {
        private string _name = "Wide";
        private double _x = 0;
        private double _y = 0;
        private double _zoom = 1.0;
        private double _rotation = 0;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        public double Zoom
        {
            get => _zoom;
            set { _zoom = value; OnPropertyChanged(); }
        }

        public double Rotation
        {
            get => _rotation;
            set { _rotation = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class VirtualSet : INotifyPropertyChanged
    {
        private string _name = "Virtual Set";
        private string _folderPath = string.Empty;
        private VirtualSetCameraPreset? _currentPreset;
        private double _activeCameraX = 0;
        private double _activeCameraY = 0;
        private double _activeCameraZoom = 1.0;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string FolderPath
        {
            get => _folderPath;
            set { _folderPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<VirtualSetLayer> Layers { get; } = new();
        public ObservableCollection<VirtualSetCameraPreset> CameraPresets { get; } = new();

        public VirtualSetCameraPreset? CurrentPreset
        {
            get => _currentPreset;
            set
            {
                _currentPreset = value;
                if (value != null)
                {
                    ActiveCameraX = value.X;
                    ActiveCameraY = value.Y;
                    ActiveCameraZoom = value.Zoom;
                }
                OnPropertyChanged();
            }
        }

        public double ActiveCameraX
        {
            get => _activeCameraX;
            set { _activeCameraX = value; OnPropertyChanged(); }
        }

        public double ActiveCameraY
        {
            get => _activeCameraY;
            set { _activeCameraY = value; OnPropertyChanged(); }
        }

        public double ActiveCameraZoom
        {
            get => _activeCameraZoom;
            set { _activeCameraZoom = value; OnPropertyChanged(); }
        }

        public static VirtualSet LoadFromFolder(string folderPath)
        {
            var vs = new VirtualSet
            {
                Name = Path.GetFileName(folderPath),
                FolderPath = folderPath
            };

            string configFile = Path.Combine(folderPath, "config.xml");
            if (File.Exists(configFile))
            {
                try
                {
                    var doc = XDocument.Load(configFile);
                    var root = doc.Element("virtualSet");
                    if (root != null)
                    {
                        foreach (var el in root.Elements("input"))
                        {
                            string layerName = el.Attribute("name")?.Value ?? "Layer";
                            string fileName = el.Value?.Trim() ?? string.Empty;
                            double.TryParse(el.Attribute("x")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double x);
                            double.TryParse(el.Attribute("y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double y);
                            double.TryParse(el.Attribute("zoom")?.Value ?? el.Attribute("scale")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double zoom);
                            if (zoom <= 0) zoom = 1.0;
                            double.TryParse(el.Attribute("rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double rot);
                            double.TryParse(el.Attribute("opacity")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double op);
                            if (op <= 0) op = 1.0;
                            bool isDynamic = bool.TryParse(el.Attribute("dynamic")?.Value, out bool dyn) && dyn;

                            string placeholderType = "Background";
                            bool chroma = false;
                            if (layerName.Contains("Talent", StringComparison.OrdinalIgnoreCase) || layerName.Contains("Presenter", StringComparison.OrdinalIgnoreCase))
                            {
                                placeholderType = "Talent";
                                isDynamic = true;
                                chroma = true;
                            }
                            else if (layerName.Contains("Screen", StringComparison.OrdinalIgnoreCase))
                            {
                                placeholderType = "Screen";
                                isDynamic = true;
                            }
                            else if (layerName.Contains("Foreground", StringComparison.OrdinalIgnoreCase))
                            {
                                placeholderType = "Foreground";
                            }

                            string resolvedPath = Path.Combine(folderPath, fileName);
                            var layer = new VirtualSetLayer
                            {
                                Name = layerName,
                                FileName = fileName,
                                ResolvedImagePath = File.Exists(resolvedPath) ? resolvedPath : string.Empty,
                                X = x,
                                Y = y,
                                Zoom = zoom,
                                Rotation = rot,
                                Opacity = op,
                                IsDynamic = isDynamic,
                                PlaceholderType = placeholderType,
                                EnableChromaKey = chroma
                            };
                            vs.Layers.Add(layer);
                        }

                        foreach (var el in root.Elements("zoom"))
                        {
                            string presetName = el.Attribute("name")?.Value ?? "Preset";
                            double.TryParse(el.Attribute("x")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double x);
                            double.TryParse(el.Attribute("y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double y);
                            double.TryParse(el.Attribute("zoom")?.Value ?? el.Attribute("scale")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double zoom);
                            if (zoom <= 0) zoom = 1.0;
                            double.TryParse(el.Attribute("rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double rot);

                            vs.CameraPresets.Add(new VirtualSetCameraPreset
                            {
                                Name = presetName,
                                X = x,
                                Y = y,
                                Zoom = zoom,
                                Rotation = rot
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading virtual set: {ex.Message}");
                }
            }

            if (vs.CameraPresets.Count == 0)
            {
                vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Full", X = 0, Y = 0, Zoom = 1.0 });
                vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Medium Shot", X = 0, Y = -0.15, Zoom = 1.8 });
                vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Close Up", X = 0, Y = -0.35, Zoom = 2.6 });
            }

            vs.CurrentPreset = vs.CameraPresets.FirstOrDefault(p => p.Name.Contains("Full") || p.Name.Contains("Wide")) ?? vs.CameraPresets[0];

            return vs;
        }

        public void SaveToFolder(string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var root = new XElement("virtualSet");
            foreach (var layer in Layers)
            {
                var el = new XElement("input",
                    new XAttribute("name", layer.Name),
                    new XAttribute("x", layer.X.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("y", layer.Y.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("zoom", layer.Zoom.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("rotation", layer.Rotation.ToString("F2", CultureInfo.InvariantCulture)),
                    new XAttribute("opacity", layer.Opacity.ToString("F2", CultureInfo.InvariantCulture)),
                    new XAttribute("dynamic", layer.IsDynamic.ToString().ToLowerInvariant()),
                    layer.FileName
                );
                root.Add(el);
            }

            foreach (var preset in CameraPresets)
            {
                var el = new XElement("zoom",
                    new XAttribute("name", preset.Name),
                    new XAttribute("x", preset.X.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("y", preset.Y.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("zoom", preset.Zoom.ToString("F6", CultureInfo.InvariantCulture)),
                    new XAttribute("rotation", preset.Rotation.ToString("F2", CultureInfo.InvariantCulture))
                );
                root.Add(el);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save(Path.Combine(targetFolder, "config.xml"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
