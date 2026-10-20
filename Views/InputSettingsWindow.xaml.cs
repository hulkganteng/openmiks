using System.Windows;
using Openmiks.Models;
using Openmiks.Services;

namespace Openmiks.Views
{
    public partial class InputSettingsWindow : Window
    {
        private readonly InputItem _item;
        public bool IsDeleted { get; private set; } = false;

        public InputSettingsWindow(InputItem item)
        {
            InitializeComponent();
            _item = item;

            HeaderTitle.Text = $"INPUT {item.Number} SETTINGS: {item.Title.ToUpper()}";
            TitleBox.Text = item.Title;

            CategoryCombo.SelectedIndex = item.Category switch
            {
                InputCategory.Red => 1,
                InputCategory.Green => 2,
                InputCategory.Orange => 3,
                InputCategory.Purple => 4,
                InputCategory.Aqua => 5,
                InputCategory.Blue => 6,
                _ => 0
            };

            VolumeSlider.Value = item.Volume;
            VolumeValText.Text = $"{(int)item.Volume}%";
            MuteCheck.IsChecked = item.IsMuted;
            SoloCheck.IsChecked = item.IsSolo;

            ChromaKeyCheck.IsChecked = item.EnableChromaKey;
            SimilaritySlider.Value = item.ChromaSimilarity;
            SmoothnessSlider.Value = item.ChromaSmoothness;
            SpillSlider.Value = item.ChromaSpill;

            UpdateTransformSummary();

            if (item.Type == InputType.Title)
            {
                TitleFieldsPanel.Visibility = Visibility.Visible;
                HeadlineBox.Text = item.Headline;
                SubtitleBox.Text = item.Subtitle;

                // Real vMix templates, plus the built-in lower third as the empty choice.
                TitleTemplateCombo.Items.Add("(built-in lower third)");
                foreach (string path in TitleTemplate.AvailableTemplates())
                {
                    TitleTemplateCombo.Items.Add(path);
                }

                int idx = 0;
                for (int i = 1; i < TitleTemplateCombo.Items.Count; i++)
                {
                    if (string.Equals((string)TitleTemplateCombo.Items[i], item.TitleTemplatePath, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }
                TitleTemplateCombo.SelectedIndex = idx;
            }

            // VISCA PTZ only applies to cameras and capture devices.
            if (item.Type is InputType.Camera or InputType.Video or InputType.DesktopCapture)
            {
                PtzPanel.Visibility = Visibility.Visible;
                if (!string.IsNullOrWhiteSpace(item.PtzIpAddress)) PtzIpBox.Text = item.PtzIpAddress;
            }
        }

        private async void PtzLeft_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.PanLeftAsync(ip), "PAN LEFT");
        private async void PtzRight_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.PanRightAsync(ip), "PAN RIGHT");
        private async void PtzUp_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.TiltUpAsync(ip), "TILT UP");
        private async void PtzDown_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.TiltDownAsync(ip), "TILT DOWN");
        private async void PtzStop_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.StopPanTiltAsync(ip), "STOP");
        private async void PtzRecall1_Click(object sender, RoutedEventArgs e) => await PtzAsync(ip => PtzControllerService.RecallPresetAsync(ip, 1), "RECALL PRESET 1");

        private async Task PtzAsync(Func<string, Task> action, string label)
        {
            string ip = PtzIpBox.Text.Trim();
            _item.PtzIpAddress = ip;

            if (string.IsNullOrWhiteSpace(ip))
            {
                PtzStatusText.Text = "Enter the camera IP first.";
                return;
            }

            try
            {
                await action(ip);
                PtzStatusText.Text = $"{label} -> sent to {ip}:52381 (UDP, no reply expected; cameras do not acknowledge VISCA-over-IP by default).";
            }
            catch (Exception ex)
            {
                PtzStatusText.Text = $"{label} failed: {ex.Message}";
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeValText != null)
            {
                VolumeValText.Text = $"{(int)e.NewValue}%";
            }
        }

        private void UpdateTransformSummary()
        {
            if (TransformSummaryText != null)
            {
                TransformSummaryText.Text = $"Pan: ({_item.PanX:+0.00;-0.00;0.00}, {_item.PanY:+0.00;-0.00;0.00}) • " +
                                           $"Zoom: {(int)(_item.ZoomX * 100)}% x {(int)(_item.ZoomY * 100)}% • " +
                                           $"Rot: {(int)_item.Rotation}° • " +
                                           $"Crop: {(int)((_item.CropLeft + _item.CropRight + _item.CropTop + _item.CropBottom) * 100)}%";
            }
        }

        private void OpenObjectEditor_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ObjectEditWindow(_item) { Owner = this };
            editor.ShowDialog();
            UpdateTransformSummary();
        }

        private void ResetTransform_Click(object sender, RoutedEventArgs e)
        {
            _item.ResetTransform();
            UpdateTransformSummary();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _item.Title = string.IsNullOrWhiteSpace(TitleBox.Text) ? _item.Title : TitleBox.Text.Trim();
            _item.Category = CategoryCombo.SelectedIndex switch
            {
                1 => InputCategory.Red,
                2 => InputCategory.Green,
                3 => InputCategory.Orange,
                4 => InputCategory.Purple,
                5 => InputCategory.Aqua,
                6 => InputCategory.Blue,
                _ => InputCategory.All
            };

            _item.Volume = VolumeSlider.Value;
            _item.IsMuted = MuteCheck.IsChecked == true;
            _item.IsSolo = SoloCheck.IsChecked == true;

            _item.EnableChromaKey = ChromaKeyCheck.IsChecked == true;
            _item.ChromaSimilarity = SimilaritySlider.Value;
            _item.ChromaSmoothness = SmoothnessSlider.Value;
            _item.ChromaSpill = SpillSlider.Value;

            if (_item.Type == InputType.Title)
            {
                _item.Headline = HeadlineBox.Text.Trim();
                _item.Subtitle = SubtitleBox.Text.Trim();
                _item.TitleTemplatePath = TitleTemplateCombo.SelectedIndex <= 0
                    ? string.Empty
                    : TitleTemplateCombo.SelectedItem as string ?? string.Empty;
            }

            if (_item.Type is InputType.Camera or InputType.Video or InputType.DesktopCapture)
            {
                _item.PtzIpAddress = PtzIpBox.Text.Trim();
            }

            DialogResult = true;
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to remove input '{_item.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
