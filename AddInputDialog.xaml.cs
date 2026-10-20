using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMixClone.Models;
using VMixClone.Services;
using VMixClone.Views;

namespace VMixClone
{
    public partial class AddInputDialog : Window
    {
        public InputItem? ResultItem { get; private set; }
        private string _activeCategory = "Video";
        private readonly int _nextNumber;
        private readonly IList<InputItem>? _existingInputs;

        public AddInputDialog(int nextNumber, IList<InputItem>? existingInputs = null)
        {
            InitializeComponent();
            _nextNumber = nextNumber;
            _existingInputs = existingInputs;

            PopulateCameraDevices();
            PopulateAudioDevices();
            PopulateVirtualSets();
        }

        private void PopulateCameraDevices()
        {
            CameraDeviceCombo.Items.Clear();

            // Real DirectShow enumeration via ffmpeg; the host list is never invented.
            var devices = CameraDeviceService.VideoDevices;
            foreach (var name in devices)
            {
                CameraDeviceCombo.Items.Add(new ComboBoxItem { Content = name, IsSelected = CameraDeviceCombo.Items.Count == 0 });
            }

            if (CameraDeviceCombo.Items.Count == 0)
            {
                CameraDeviceCombo.Items.Add(new ComboBoxItem
                {
                    Content = "No DirectShow device found (check camera driver)",
                    IsSelected = true
                });
            }

            CameraResCombo.SelectedIndex = 0;
            CameraFpsCombo.SelectedIndex = 0;
            CameraFormatCombo.SelectedIndex = 0;
            DesktopScreenCombo.SelectedIndex = 0;
            TitlePresetCombo.SelectedIndex = 0;
            ColourPresetCombo.SelectedIndex = 0;
            ClockFormatCombo.SelectedIndex = 0;
        }

        private void PopulateAudioDevices()
        {
            AudioDeviceCombo.Items.Clear();

            var audioDevs = CameraDeviceService.AudioDevices;
            foreach (var name in audioDevs)
            {
                AudioDeviceCombo.Items.Add(new ComboBoxItem
                {
                    Content = name,
                    IsSelected = AudioDeviceCombo.Items.Count == 0
                });
            }

            if (AudioDeviceCombo.Items.Count == 0)
            {
                AudioDeviceCombo.Items.Add(new ComboBoxItem
                {
                    Content = "No Audio Device found (check soundcard driver)",
                    IsSelected = true
                });
            }

            AudioChannelCombo.SelectedIndex = 0;
        }

        private void PopulateVirtualSets()
        {
            VirtualSetCombo.Items.Clear();

            // Scan vMix default VirtualSets folder
            string vmixVsPath = @"C:\Program Files (x86)\vMix\VirtualSets";
            if (Directory.Exists(vmixVsPath))
            {
                foreach (var dir in Directory.GetDirectories(vmixVsPath))
                {
                    if (File.Exists(Path.Combine(dir, "config.xml")))
                    {
                        VirtualSetCombo.Items.Add(new ComboBoxItem
                        {
                            Content = Path.GetFileName(dir),
                            Tag = dir
                        });
                    }
                }
            }

            if (VirtualSetCombo.Items.Count > 0)
            {
                // Select NewsStudio or first item
                int foundIdx = -1;
                for (int i = 0; i < VirtualSetCombo.Items.Count; i++)
                {
                    if (((ComboBoxItem)VirtualSetCombo.Items[i]).Content.ToString()!.Contains("NewsStudio"))
                    {
                        foundIdx = i;
                        break;
                    }
                }
                VirtualSetCombo.SelectedIndex = foundIdx >= 0 ? foundIdx : 0;
            }

            // Populate Talent & Screen inputs from existingInputs
            VsTalentCombo.Items.Clear();
            VsScreenCombo.Items.Clear();

            VsTalentCombo.Items.Add(new ComboBoxItem { Content = "Default (Camera Input)", Tag = 0, IsSelected = true });
            VsScreenCombo.Items.Add(new ComboBoxItem { Content = "Default (Video / Desktop)", Tag = 0, IsSelected = true });

            if (_existingInputs != null)
            {
                foreach (var inp in _existingInputs)
                {
                    VsTalentCombo.Items.Add(new ComboBoxItem
                    {
                        Content = $"Input {inp.Number}: {inp.Title} ({inp.Type})",
                        Tag = inp.Number
                    });
                    VsScreenCombo.Items.Add(new ComboBoxItem
                    {
                        Content = $"Input {inp.Number}: {inp.Title} ({inp.Type})",
                        Tag = inp.Number
                    });
                }
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                _activeCategory = tag;

                VideoPanel.Visibility = Visibility.Collapsed;
                CameraPanel.Visibility = Visibility.Collapsed;
                DesktopPanel.Visibility = Visibility.Collapsed;
                ImagePanel.Visibility = Visibility.Collapsed;
                AudioPanel.Visibility = Visibility.Collapsed;
                AudioDevicePanel.Visibility = Visibility.Collapsed;
                TitlePanel.Visibility = Visibility.Collapsed;
                ColourPanel.Visibility = Visibility.Collapsed;
                ClockPanel.Visibility = Visibility.Collapsed;
                WebPanel.Visibility = Visibility.Collapsed;
                VirtualSetPanel.Visibility = Visibility.Collapsed;
                PlayoutPanel.Visibility = Visibility.Collapsed;

                switch (tag)
                {
                    case "Video": VideoPanel.Visibility = Visibility.Visible; break;
                    case "Camera": CameraPanel.Visibility = Visibility.Visible; break;
                    case "Desktop": DesktopPanel.Visibility = Visibility.Visible; break;
                    case "Image": ImagePanel.Visibility = Visibility.Visible; break;
                    case "Audio": AudioPanel.Visibility = Visibility.Visible; break;
                    case "AudioDevice": AudioDevicePanel.Visibility = Visibility.Visible; break;
                    case "Title": TitlePanel.Visibility = Visibility.Visible; break;
                    case "Colour": ColourPanel.Visibility = Visibility.Visible; break;
                    case "Clock": ClockPanel.Visibility = Visibility.Visible; break;
                    case "Web": WebPanel.Visibility = Visibility.Visible; break;
                    case "VirtualSet": VirtualSetPanel.Visibility = Visibility.Visible; break;
                    case "Playout": PlayoutPanel.Visibility = Visibility.Visible; break;
                }
            }
        }

        private void BrowseVideo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files (*.mp4;*.avi;*.mkv;*.wmv;*.mov)|*.mp4;*.avi;*.mkv;*.wmv;*.mov|All Files (*.*)|*.*",
                Title = "Select Video File"
            };
            if (dlg.ShowDialog() == true)
            {
                VideoPathBox.Text = dlg.FileName;
            }
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.bmp;*.webp|All Files (*.*)|*.*",
                Title = "Select Image File"
            };
            if (dlg.ShowDialog() == true)
            {
                ImagePathBox.Text = dlg.FileName;
            }
        }

        private void BrowseAudio_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.wav;*.m4a;*.aac;*.wma)|*.mp3;*.wav;*.m4a;*.aac;*.wma|All Files (*.*)|*.*",
                Title = "Select Audio File"
            };
            if (dlg.ShowDialog() == true)
            {
                AudioPathBox.Text = dlg.FileName;
            }
        }

        private void BrowseVsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Virtual Set Directory with config.xml"
            };
            if (dlg.ShowDialog() == true)
            {
                var item = new ComboBoxItem
                {
                    Content = Path.GetFileName(dlg.FolderName),
                    Tag = dlg.FolderName,
                    IsSelected = true
                };
                VirtualSetCombo.Items.Insert(0, item);
                VirtualSetCombo.SelectedIndex = 0;
            }
        }

        private void BrowsePlayout_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select folder with video and/or image files"
            };
            if (dlg.ShowDialog() != true) return;

            PlayoutFolderBox.Text = dlg.FolderName;

            // Counts real media only, so the summary never overstates what will play.
            int videos = 0, images = 0;
            try
            {
                foreach (string file in Directory.EnumerateFiles(dlg.FolderName))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (PlayoutEngine.VideoExtensions.Contains(ext)) videos++;
                    else if (PlayoutEngine.ImageExtensions.Contains(ext)) images++;
                }
            }
            catch (Exception ex)
            {
                PlayoutSummaryText.Text = $"Could not read folder: {ex.Message}";
                return;
            }

            PlayoutSummaryText.Text = videos == 0 && images == 0
                ? "No playable media found in this folder."
                : $"Found {videos} video file(s) and {images} image(s).";
        }

        private void OpenVseEditor_Click(object sender, RoutedEventArgs e)
        {
            var vse = new VirtualSetEditorWindow(null, _existingInputs) { Owner = this };
            if (vse.ShowDialog() == true && vse.VirtualSetResult != null)
            {
                var item = new ComboBoxItem
                {
                    Content = vse.VirtualSetResult.Name,
                    Tag = vse.VirtualSetResult.FolderPath,
                    IsSelected = true
                };
                VirtualSetCombo.Items.Insert(0, item);
                VirtualSetCombo.SelectedIndex = 0;
            }
        }

        private void TitlePreset_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (HeadlineBox == null || SubtitleBox == null) return;

            switch (TitlePresetCombo.SelectedIndex)
            {
                case 0:
                    HeadlineBox.Text = "BREAKING NEWS";
                    SubtitleBox.Text = "Live broadcast coverage and continuous updates";
                    break;
                case 1:
                    HeadlineBox.Text = "SPECIAL GUEST";
                    SubtitleBox.Text = "Dr. Alexander Miller - Senior Broadcast Analyst";
                    break;
                case 2:
                    HeadlineBox.Text = "LIVE HD";
                    SubtitleBox.Text = "vMix Broadcast Network";
                    break;
                case 3:
                    HeadlineBox.Text = "CHAMPIONSHIP CUP";
                    SubtitleBox.Text = "TEAM A  2 - 1  TEAM B";
                    break;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var item = new InputItem
            {
                Number = _nextNumber
            };

            switch (_activeCategory)
            {
                case "Video":
                    item.Type = InputType.Video;
                    item.FilePath = VideoPathBox.Text.Trim();
                    item.Title = string.IsNullOrEmpty(item.FilePath) ? $"Video {_nextNumber}" : Path.GetFileNameWithoutExtension(item.FilePath);
                    item.IsLoop = VideoLoopCheck.IsChecked == true;
                    item.Category = InputCategory.Blue;
                    item.AudioLevelL = 0.75;
                    item.AudioLevelR = 0.75;
                    break;

                case "Camera":
                    item.Type = InputType.Camera;
                    if (CameraDeviceCombo.SelectedItem is ComboBoxItem devItem)
                    {
                        item.CameraDeviceName = devItem.Content.ToString()!;
                    }
                    if (CameraResCombo.SelectedItem is ComboBoxItem resItem)
                    {
                        // Resolution combo carries "1280x720 (HD)"; the engine needs the plain size.
                        item.CameraResolution = resItem.Content.ToString()!.Split(' ')[0];
                    }
                    if (CameraFpsCombo.SelectedItem is ComboBoxItem fpsItem)
                    {
                        item.CameraFps = fpsItem.Content.ToString()!;
                    }
                    item.Title = $"{item.CameraDeviceName}";
                    item.Category = InputCategory.Red;
                    item.AudioLevelL = 0.8;
                    item.AudioLevelR = 0.85;
                    break;

                case "Desktop":
                    item.Type = InputType.DesktopCapture;
                    item.Title = "Desktop Display";
                    item.Category = InputCategory.Green;
                    item.AudioLevelL = 0.5;
                    item.AudioLevelR = 0.5;
                    break;

                case "Image":
                    item.Type = InputType.Image;
                    item.FilePath = ImagePathBox.Text.Trim();
                    item.Title = string.IsNullOrEmpty(item.FilePath) ? $"Image {_nextNumber}" : Path.GetFileNameWithoutExtension(item.FilePath);
                    item.Category = InputCategory.Purple;
                    item.IsMuted = true;
                    item.AudioLevelL = 0.0;
                    item.AudioLevelR = 0.0;
                    break;

                case "Audio":
                    item.Type = InputType.AudioFile;
                    item.FilePath = AudioPathBox.Text.Trim();
                    item.Title = string.IsNullOrEmpty(item.FilePath) ? $"Audio {_nextNumber}" : Path.GetFileNameWithoutExtension(item.FilePath);
                    item.IsLoop = AudioLoopCheck.IsChecked == true;
                    item.Category = InputCategory.Orange;
                    item.AudioLevelL = 0.8;
                    item.AudioLevelR = 0.8;
                    break;

                case "AudioDevice":
                    item.Type = InputType.AudioDevice;
                    if (AudioDeviceCombo.SelectedItem is ComboBoxItem audioDevItem)
                    {
                        item.AudioDeviceName = audioDevItem.Content.ToString()!;
                    }
                    item.Title = string.IsNullOrEmpty(item.AudioDeviceName) ? $"Audio Device {_nextNumber}" : item.AudioDeviceName;
                    item.Category = InputCategory.Orange;
                    item.AudioLevelL = 0.0;
                    item.AudioLevelR = 0.0;
                    break;

                case "Title":
                    item.Type = InputType.Title;
                    item.Headline = HeadlineBox.Text.Trim();
                    item.Subtitle = SubtitleBox.Text.Trim();
                    item.Title = string.IsNullOrEmpty(item.Headline) ? "Lower Third" : item.Headline;
                    item.Category = InputCategory.Orange;
                    item.IsMuted = true;
                    item.AudioLevelL = 0.0;
                    item.AudioLevelR = 0.0;
                    break;

                case "Colour":
                    if (ColourPresetCombo.SelectedIndex == 0)
                    {
                        item.Type = InputType.ColorBars;
                        item.Title = "Color Bars";
                        item.Category = InputCategory.All;
                        item.AudioLevelL = 0.65;
                        item.AudioLevelR = 0.65;
                    }
                    else
                    {
                        item.Type = InputType.Colour;
                        item.Title = "Colour Backdrop";
                        item.Category = InputCategory.Aqua;
                        switch (ColourPresetCombo.SelectedIndex)
                        {
                            case 1: item.SolidColor = Color.FromRgb(26, 54, 93); break;
                            case 2: item.SolidColor = Color.FromRgb(116, 42, 42); break;
                            case 3: item.SolidColor = Color.FromRgb(34, 84, 61); break;
                            case 4: item.SolidColor = Color.FromRgb(68, 51, 122); break;
                        }
                        item.AudioLevelL = 0.0;
                        item.AudioLevelR = 0.0;
                        item.IsMuted = true;
                    }
                    break;

                case "Clock":
                    item.Type = InputType.Clock;
                    item.Title = "Studio Clock";
                    item.Category = InputCategory.Orange;
                    item.AudioLevelL = 0.4;
                    item.AudioLevelR = 0.4;
                    break;

                case "Web":
                    item.Type = InputType.WebBrowser;
                    item.WebUrl = WebUrlBox.Text.Trim();
                    item.Title = "Web Browser";
                    item.Category = InputCategory.Aqua;
                    item.IsMuted = true;
                    item.AudioLevelL = 0.0;
                    item.AudioLevelR = 0.0;
                    break;

                case "VirtualSet":
                    item.Type = InputType.VirtualSet;
                    string vsFolder = string.Empty;
                    if (VirtualSetCombo.SelectedItem is ComboBoxItem vsItem && vsItem.Tag is string p)
                    {
                        vsFolder = p;
                    }
                    if (string.IsNullOrEmpty(vsFolder))
                    {
                        string defaultVs = @"C:\Program Files (x86)\vMix\VirtualSets\NewsStudio";
                        vsFolder = Directory.Exists(defaultVs) ? defaultVs : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VirtualSets", "Default");
                    }

                    var vsObj = VirtualSet.LoadFromFolder(vsFolder);

                    if (VsTalentCombo.SelectedItem is ComboBoxItem tItem && tItem.Tag is int tNum && tNum > 0)
                    {
                        var talentLayer = vsObj.Layers.FirstOrDefault(l => l.PlaceholderType == "Talent");
                        if (talentLayer != null) talentLayer.AssignedInputNumber = tNum;
                    }
                    if (VsScreenCombo.SelectedItem is ComboBoxItem sItem && sItem.Tag is int sNum && sNum > 0)
                    {
                        var screenLayer = vsObj.Layers.FirstOrDefault(l => l.PlaceholderType == "Screen");
                        if (screenLayer != null) screenLayer.AssignedInputNumber = sNum;
                    }

                    item.VirtualSet = vsObj;
                    item.Title = $"VS - {vsObj.Name}";
                    item.Category = InputCategory.Blue;
                    break;
                case "Playout":
                    item.Type = PlayoutModeCombo.SelectedIndex == 1 ? InputType.PhotoSlideshow : InputType.Playlist;
                    item.FilePath = PlayoutFolderBox.Text.Trim();
                    item.IsLoop = PlayoutLoopCheck.IsChecked == true;
                    item.ShufflePlayout = PlayoutShuffleCheck.IsChecked == true;
                    item.PlayoutFolder = item.FilePath;
                    if (PlayoutIntervalCombo.SelectedItem is ComboBoxItem intItem && int.TryParse(intItem.Content.ToString(), out int secs))
                    {
                        item.SlideshowIntervalSeconds = secs;
                    }
                    item.Title = string.IsNullOrEmpty(item.FilePath)
                        ? (item.Type == InputType.PhotoSlideshow ? "Photo Slideshow" : "Video Playlist")
                        : $"{Path.GetFileName(item.FilePath)} ({item.Type})";
                    item.Category = InputCategory.Blue;
                    item.IsMuted = true;
                    item.AudioLevelL = 0.7;
                    item.AudioLevelR = 0.7;
                    break;
            }

            ResultItem = item;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
