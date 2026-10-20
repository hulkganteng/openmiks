using System.Windows;
using System.Windows.Controls;
using VMixClone.Models;
using VMixClone.Services;

namespace VMixClone.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ProductionEngine _engine;
        private readonly VMixConfiguration _config;

        public SettingsWindow(ProductionEngine engine)
        {
            InitializeComponent();
            _engine = engine;
            _config = engine.Configuration;

            LoadSettingsIntoUI();
        }

        private void LoadSettingsIntoUI()
        {
            // Output Res
            foreach (ComboBoxItem item in OutputResCombo.Items)
            {
                if (item.Content.ToString()!.Contains(_config.OutputResolution))
                {
                    OutputResCombo.SelectedItem = item;
                    break;
                }
            }
            if (OutputResCombo.SelectedItem == null) OutputResCombo.SelectedIndex = 0;

            // Frame Rate
            foreach (ComboBoxItem item in FrameRateCombo.Items)
            {
                if (item.Content.ToString() == _config.FrameRate)
                {
                    FrameRateCombo.SelectedItem = item;
                    break;
                }
            }
            if (FrameRateCombo.SelectedItem == null) FrameRateCombo.SelectedIndex = 0;

            // Monitor
            MonitorCombo.SelectedIndex = Math.Clamp(_config.FullscreenMonitorIndex, 0, MonitorCombo.Items.Count - 1);

            // Recording
            RecFolderBox.Text = _config.RecordingFolder;
            foreach (ComboBoxItem item in RecFormatCombo.Items)
            {
                if (item.Content.ToString()!.StartsWith(_config.RecordingFormat))
                {
                    RecFormatCombo.SelectedItem = item;
                    break;
                }
            }
            if (RecFormatCombo.SelectedItem == null) RecFormatCombo.SelectedIndex = 0;

            // Streaming
            foreach (ComboBoxItem item in StreamServiceCombo.Items)
            {
                if (item.Content.ToString() == _config.StreamingService)
                {
                    StreamServiceCombo.SelectedItem = item;
                    break;
                }
            }
            if (StreamServiceCombo.SelectedItem == null) StreamServiceCombo.SelectedIndex = 0;

            StreamUrlBox.Text = _config.StreamingUrl;
            StreamKeyBox.Text = _config.StreamingKey;

            // Web
            WebEnabledCheck.IsChecked = _config.WebServerEnabled;
            WebPortBox.Text = _config.WebServerPort.ToString();
            WebPasswordBox.Text = _config.WebServerPassword;
            WebUrlDisplay.Text = $"http://localhost:{_config.WebServerPort}/controller";

            // Audio (OBS Style Hardware Mic/Aux Ingest + vMix Outputs)
            AutoAudioMixingCheck.IsChecked = _config.AutoAudioMixing;
            PopulateAudioHardwareDevices();
        }

        private void PopulateAudioHardwareDevices()
        {
            MicAux1Combo.Items.Clear();
            MicAux2Combo.Items.Clear();

            MicAux1Combo.Items.Add(new ComboBoxItem { Content = "Disabled", Tag = "" });
            MicAux2Combo.Items.Add(new ComboBoxItem { Content = "Disabled", Tag = "" });

            var audioDevs = CameraDeviceService.AudioDevices;
            foreach (var dev in audioDevs)
            {
                MicAux1Combo.Items.Add(new ComboBoxItem { Content = dev, Tag = dev });
                MicAux2Combo.Items.Add(new ComboBoxItem { Content = dev, Tag = dev });
            }

            SelectDeviceInCombo(MicAux1Combo, _config.MicAuxDevice1);
            SelectDeviceInCombo(MicAux2Combo, _config.MicAuxDevice2);
        }

        private static void SelectDeviceInCombo(ComboBox combo, string? target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                combo.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item && string.Equals(item.Tag?.ToString(), target, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.SelectedIndex = 0;
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                DisplayPanel.Visibility = Visibility.Collapsed;
                RecordingPanel.Visibility = Visibility.Collapsed;
                StreamingPanel.Visibility = Visibility.Collapsed;
                AudioPanel.Visibility = Visibility.Collapsed;
                WebPanel.Visibility = Visibility.Collapsed;
                ShortcutsPanel.Visibility = Visibility.Collapsed;
                AboutPanel.Visibility = Visibility.Collapsed;

                switch (tag)
                {
                    case "Display": DisplayPanel.Visibility = Visibility.Visible; break;
                    case "Recording": RecordingPanel.Visibility = Visibility.Visible; break;
                    case "Streaming": StreamingPanel.Visibility = Visibility.Visible; break;
                    case "Audio": AudioPanel.Visibility = Visibility.Visible; break;
                    case "Web": WebPanel.Visibility = Visibility.Visible; break;
                    case "Shortcuts": ShortcutsPanel.Visibility = Visibility.Visible; break;
                    case "About": AboutPanel.Visibility = Visibility.Visible; break;
                }
            }
        }

        private void StreamService_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (StreamServiceCombo.SelectedItem is ComboBoxItem item)
            {
                string s = item.Content.ToString()!;
                if (s.Contains("YouTube"))
                    StreamUrlBox.Text = "rtmp://a.rtmp.youtube.com/live2";
                else if (s.Contains("Facebook"))
                    StreamUrlBox.Text = "rtmps://live-api-s.facebook.com:443/rtmp/";
                else if (s.Contains("Twitch"))
                    StreamUrlBox.Text = "rtmp://live.twitch.tv/app/";
                else if (s.Contains("Restream"))
                    StreamUrlBox.Text = "rtmp://live.restream.io/live";
            }
        }

        private void BrowseRecFolder_Click(object sender, RoutedEventArgs e)
        {
            // Use Win32 Folder Browser Dialog or simple prompt
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                RecFolderBox.Text = dialog.FolderName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Save from UI to Configuration
            if (OutputResCombo.SelectedItem is ComboBoxItem resItem)
            {
                string r = resItem.Content.ToString()!;
                if (r.Contains("1920x1080")) _config.OutputResolution = "1920x1080";
                else if (r.Contains("1280x720")) _config.OutputResolution = "1280x720";
                else if (r.Contains("3840x2160")) _config.OutputResolution = "3840x2160";
                else if (r.Contains("1080x1920")) _config.OutputResolution = "1080x1920";
            }

            if (FrameRateCombo.SelectedItem is ComboBoxItem fpsItem)
            {
                _config.FrameRate = fpsItem.Content.ToString()!;
            }

            _config.FullscreenMonitorIndex = MonitorCombo.SelectedIndex;
            _config.RecordingFolder = RecFolderBox.Text.Trim();

            if (RecFormatCombo.SelectedItem is ComboBoxItem fmtItem)
            {
                string f = fmtItem.Content.ToString()!;
                if (f.StartsWith("AVI")) _config.RecordingFormat = "AVI";
                else if (f.StartsWith("MKV")) _config.RecordingFormat = "MKV";
                else if (f.StartsWith("WMV")) _config.RecordingFormat = "WMV";
                else _config.RecordingFormat = "MP4";
            }

            if (StreamServiceCombo.SelectedItem is ComboBoxItem srvItem)
            {
                _config.StreamingService = srvItem.Content.ToString()!;
            }
            _config.StreamingUrl = StreamUrlBox.Text.Trim();
            _config.StreamingKey = StreamKeyBox.Text.Trim();

            _config.WebServerEnabled = WebEnabledCheck.IsChecked == true;
            if (int.TryParse(WebPortBox.Text.Trim(), out int port) && port > 0)
            {
                _config.WebServerPort = port;
            }
            _config.WebServerPassword = WebPasswordBox.Text.Trim();

            _config.AutoAudioMixing = AutoAudioMixingCheck.IsChecked == true;

            // Audio Hardware Devices (OBS Style)
            if (MicAux1Combo.SelectedItem is ComboBoxItem mic1)
                _config.MicAuxDevice1 = mic1.Tag?.ToString() ?? "";
            if (MicAux2Combo.SelectedItem is ComboBoxItem mic2)
                _config.MicAuxDevice2 = mic2.Tag?.ToString() ?? "";

            // Save to disk
            _config.Save();

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
