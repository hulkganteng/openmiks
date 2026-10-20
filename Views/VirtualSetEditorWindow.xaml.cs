using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Openmiks.Models;

namespace Openmiks.Views
{
    public partial class VirtualSetEditorWindow : Window
    {
        private VirtualSet _virtualSet;
        private VirtualSetLayer? _selectedLayer;
        private bool _isUpdatingUi = false;

        public VirtualSet VirtualSetResult => _virtualSet;

        public VirtualSetEditorWindow(VirtualSet? initialSet = null, IList<InputItem>? availableInputs = null)
        {
            InitializeComponent();

            _virtualSet = initialSet ?? CreateDefaultTemplate();
            SetPreviewView.AvailableInputs = availableInputs;
            SetPreviewView.VirtualSet = _virtualSet;

            LayersList.ItemsSource = _virtualSet.Layers;
            SetNameBox.Text = _virtualSet.Name;

            UpdatePresetButtons();
            if (_virtualSet.Layers.Count > 0)
            {
                LayersList.SelectedIndex = 0;
            }
        }

        private static VirtualSet CreateDefaultTemplate()
        {
            var vs = new VirtualSet { Name = "Studio_Production" };
            vs.Layers.Add(new VirtualSetLayer
            {
                Name = "Background",
                FileName = "background.png",
                PlaceholderType = "Background",
                X = 0,
                Y = 0,
                Zoom = 1.0,
                Opacity = 1.0
            });
            vs.Layers.Add(new VirtualSetLayer
            {
                Name = "Screen",
                FileName = "screen.png",
                PlaceholderType = "Screen",
                IsDynamic = true,
                X = -0.45,
                Y = 0.1,
                Zoom = 0.35,
                Opacity = 1.0
            });
            vs.Layers.Add(new VirtualSetLayer
            {
                Name = "Talent",
                FileName = "talent.png",
                PlaceholderType = "Talent",
                IsDynamic = true,
                EnableChromaKey = true,
                X = 0.15,
                Y = -0.1,
                Zoom = 0.65,
                Opacity = 1.0
            });
            vs.Layers.Add(new VirtualSetLayer
            {
                Name = "Foreground Desk",
                FileName = "foreground.png",
                PlaceholderType = "Foreground",
                X = 0,
                Y = -0.35,
                Zoom = 0.8,
                Opacity = 1.0
            });

            vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Wide", X = 0, Y = 0, Zoom = 1.0 });
            vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Close Up", X = 0.15, Y = -0.15, Zoom = 1.8 });
            vs.CameraPresets.Add(new VirtualSetCameraPreset { Name = "Screen Zoom", X = -0.4, Y = 0.05, Zoom = 2.0 });

            vs.CurrentPreset = vs.CameraPresets[0];
            return vs;
        }

        private void UpdatePresetButtons()
        {
            CameraPresetBar.Children.Clear();
            var lbl = new TextBlock
            {
                Text = "CAMERA PRESETS: ",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            CameraPresetBar.Children.Add(lbl);

            foreach (var preset in _virtualSet.CameraPresets)
            {
                var btn = new Button
                {
                    Content = preset.Name,
                    Tag = preset,
                    Margin = new Thickness(0, 0, 6, 0),
                    Padding = new Thickness(8, 3, 8, 3),
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 46)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 68)),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                btn.Click += (s, e) =>
                {
                    _virtualSet.CurrentPreset = preset;
                };
                CameraPresetBar.Children.Add(btn);
            }
        }

        private void LayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedLayer = LayersList.SelectedItem as VirtualSetLayer;
            if (_selectedLayer == null) return;

            _isUpdatingUi = true;
            PropLayerName.Text = _selectedLayer.Name;
            PropTypeCombo.SelectedIndex = _selectedLayer.PlaceholderType switch
            {
                "Talent" => 1,
                "Screen" => 2,
                "Foreground" => 3,
                _ => 0
            };
            PropXSlider.Value = _selectedLayer.X;
            PropYSlider.Value = _selectedLayer.Y;
            PropZoomSlider.Value = _selectedLayer.Zoom;
            PropOpacitySlider.Value = _selectedLayer.Opacity;
            PropChromaCheck.IsChecked = _selectedLayer.EnableChromaKey;
            PropSimilaritySlider.Value = _selectedLayer.ChromaSimilarity;
            PropSmoothnessSlider.Value = _selectedLayer.ChromaSmoothness;
            PropSpillSlider.Value = _selectedLayer.ChromaSpill;
            _isUpdatingUi = false;
        }

        private void Prop_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUi || _selectedLayer == null) return;

            _selectedLayer.Name = PropLayerName.Text.Trim();
            _selectedLayer.X = PropXSlider.Value;
            _selectedLayer.Y = PropYSlider.Value;
            _selectedLayer.Zoom = PropZoomSlider.Value;
            _selectedLayer.Opacity = PropOpacitySlider.Value;
            _selectedLayer.EnableChromaKey = PropChromaCheck.IsChecked == true;
            _selectedLayer.ChromaSimilarity = PropSimilaritySlider.Value;
            _selectedLayer.ChromaSmoothness = PropSmoothnessSlider.Value;
            _selectedLayer.ChromaSpill = PropSpillSlider.Value;

            SetPreviewView.RebuildLayers();
        }

        private void PropType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingUi || _selectedLayer == null) return;

            _selectedLayer.PlaceholderType = PropTypeCombo.SelectedIndex switch
            {
                1 => "Talent",
                2 => "Screen",
                3 => "Foreground",
                _ => "Background"
            };
            _selectedLayer.IsDynamic = (_selectedLayer.PlaceholderType == "Talent" || _selectedLayer.PlaceholderType == "Screen");
            _selectedLayer.EnableChromaKey = (_selectedLayer.PlaceholderType == "Talent");
            PropChromaCheck.IsChecked = _selectedLayer.EnableChromaKey;

            SetPreviewView.RebuildLayers();
        }

        private void AddLayerPng_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|All Image Files (*.*)|*.*",
                Title = "Import Virtual Set Layer Image"
            };
            if (dlg.ShowDialog() == true)
            {
                var layer = new VirtualSetLayer
                {
                    Name = Path.GetFileNameWithoutExtension(dlg.FileName),
                    FileName = Path.GetFileName(dlg.FileName),
                    ResolvedImagePath = dlg.FileName,
                    PlaceholderType = "Background",
                    Zoom = 1.0,
                    Opacity = 1.0
                };
                _virtualSet.Layers.Add(layer);
                LayersList.SelectedItem = layer;
                SetPreviewView.RebuildLayers();
            }
        }

        private void AddTalentLayer_Click(object sender, RoutedEventArgs e)
        {
            var layer = new VirtualSetLayer
            {
                Name = "Presenter (Chroma Key)",
                FileName = "talent.png",
                PlaceholderType = "Talent",
                IsDynamic = true,
                EnableChromaKey = true,
                Zoom = 0.65,
                Opacity = 1.0
            };
            _virtualSet.Layers.Add(layer);
            LayersList.SelectedItem = layer;
            SetPreviewView.RebuildLayers();
        }

        private void MoveLayerUp_Click(object sender, RoutedEventArgs e)
        {
            int idx = LayersList.SelectedIndex;
            if (idx > 0)
            {
                var item = _virtualSet.Layers[idx];
                _virtualSet.Layers.RemoveAt(idx);
                _virtualSet.Layers.Insert(idx - 1, item);
                LayersList.SelectedItem = item;
                SetPreviewView.RebuildLayers();
            }
        }

        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLayer != null)
            {
                _virtualSet.Layers.Remove(_selectedLayer);
                SetPreviewView.RebuildLayers();
            }
        }

        private void AddPreset_Click(object sender, RoutedEventArgs e)
        {
            string pName = $"Shot {_virtualSet.CameraPresets.Count + 1}";
            _virtualSet.CameraPresets.Add(new VirtualSetCameraPreset
            {
                Name = pName,
                X = 0,
                Y = 0,
                Zoom = 1.0
            });
            UpdatePresetButtons();
        }

        private void NewSet_Click(object sender, RoutedEventArgs e)
        {
            _virtualSet = CreateDefaultTemplate();
            SetPreviewView.VirtualSet = _virtualSet;
            LayersList.ItemsSource = _virtualSet.Layers;
            SetNameBox.Text = _virtualSet.Name;
            UpdatePresetButtons();
            if (_virtualSet.Layers.Count > 0) LayersList.SelectedIndex = 0;
        }

        private void OpenSet_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Virtual Set Directory"
            };
            if (dlg.ShowDialog() == true)
            {
                _virtualSet = VirtualSet.LoadFromFolder(dlg.FolderName);
                SetPreviewView.VirtualSet = _virtualSet;
                LayersList.ItemsSource = _virtualSet.Layers;
                SetNameBox.Text = _virtualSet.Name;
                UpdatePresetButtons();
                if (_virtualSet.Layers.Count > 0) LayersList.SelectedIndex = 0;
            }
        }

        private void SaveSet_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Destination Folder to Save Virtual Set"
            };
            if (dlg.ShowDialog() == true)
            {
                _virtualSet.Name = SetNameBox.Text.Trim();
                _virtualSet.SaveToFolder(dlg.FolderName);
                StatusText.Text = $"Virtual Set saved to {dlg.FolderName}";
                MessageBox.Show("Virtual Set successfully saved with config.xml!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
