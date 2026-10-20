using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Openmiks.Controls;
using Openmiks.Models;
using Openmiks.Services;

namespace Openmiks.Views
{
    public partial class MultiViewWindow : Window
    {
        private readonly ProductionEngine _engine;
        private string _currentLayout = "Studio";
        private readonly DispatcherTimer _tallyTimer;
        private bool _isFullscreen = false;
        private WindowState _prevWindowState;
        private WindowStyle _prevWindowStyle;

        public MultiViewWindow(ProductionEngine engine)
        {
            InitializeComponent();
            _engine = engine;

            BuildLayout();

            _tallyTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _tallyTimer.Tick += (s, e) => UpdateTallyBorders();
            _tallyTimer.Start();

            Closing += (s, e) => _tallyTimer.Stop();
        }

        private void Layout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string layout)
            {
                _currentLayout = layout;
                BuildLayout();
            }
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullscreen)
            {
                _prevWindowState = WindowState;
                _prevWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _isFullscreen = true;
            }
            else
            {
                WindowStyle = _prevWindowStyle;
                WindowState = _prevWindowState;
                _isFullscreen = false;
            }
        }

        private void BuildLayout()
        {
            MultiViewRoot.Children.Clear();
            MultiViewRoot.RowDefinitions.Clear();
            MultiViewRoot.ColumnDefinitions.Clear();

            switch (_currentLayout)
            {
                case "Quad":
                    BuildGrid(2, 2);
                    break;
                case "Nine":
                    BuildGrid(3, 3);
                    break;
                case "Sixteen":
                    BuildGrid(4, 4);
                    break;
                case "Studio":
                default:
                    BuildStudioLayout();
                    break;
            }
        }

        private void BuildStudioLayout()
        {
            MultiViewRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.3, GridUnitType.Star) });
            MultiViewRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) });

            MultiViewRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            MultiViewRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Top Left: Large Preview
            var pvBox = CreateBox(null, "PREVIEW", true, false);
            Grid.SetRow(pvBox, 0);
            Grid.SetColumn(pvBox, 0);
            MultiViewRoot.Children.Add(pvBox);

            // Top Right: Large Program
            var pgmBox = CreateBox(null, "PROGRAM (LIVE)", false, true);
            Grid.SetRow(pgmBox, 0);
            Grid.SetColumn(pgmBox, 1);
            MultiViewRoot.Children.Add(pgmBox);

            // Bottom: 8-Input Grid
            var bottomGrid = new Grid();
            for (int r = 0; r < 2; r++) bottomGrid.RowDefinitions.Add(new RowDefinition());
            for (int c = 0; c < 4; c++) bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());

            int idx = 0;
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    var inp = _engine.Inputs.Count > idx ? _engine.Inputs[idx] : null;
                    var box = CreateBox(inp, inp != null ? $"{inp.Number}. {inp.Title}" : $"Input {idx + 1}");
                    Grid.SetRow(box, r);
                    Grid.SetColumn(box, c);
                    bottomGrid.Children.Add(box);
                    idx++;
                }
            }

            Grid.SetRow(bottomGrid, 1);
            Grid.SetColumnSpan(bottomGrid, 2);
            MultiViewRoot.Children.Add(bottomGrid);
        }

        private void BuildGrid(int rows, int cols)
        {
            for (int r = 0; r < rows; r++) MultiViewRoot.RowDefinitions.Add(new RowDefinition());
            for (int c = 0; c < cols; c++) MultiViewRoot.ColumnDefinitions.Add(new ColumnDefinition());

            int idx = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var inp = _engine.Inputs.Count > idx ? _engine.Inputs[idx] : null;
                    var box = CreateBox(inp, inp != null ? $"{inp.Number}. {inp.Title}" : $"Input {idx + 1}");
                    Grid.SetRow(box, r);
                    Grid.SetColumn(box, c);
                    MultiViewRoot.Children.Add(box);
                    idx++;
                }
            }
        }

        private Border CreateBox(InputItem? item, string label, bool isPreviewMonitor = false, bool isProgramMonitor = false)
        {
            var border = new Border
            {
                Margin = new Thickness(3),
                Background = new SolidColorBrush(Color.FromRgb(15, 15, 18)),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(40, 40, 46)),
                CornerRadius = new CornerRadius(3),
                Tag = isPreviewMonitor ? "PV_MONITOR" : (isProgramMonitor ? "PGM_MONITOR" : item)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header Bar
            var hdr = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(25, 25, 30)),
                Padding = new Thickness(6, 2, 6, 2)
            };
            var txt = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            hdr.Child = txt;
            Grid.SetRow(hdr, 0);
            grid.Children.Add(hdr);

            // Display
            var display = new InputDisplay { Item = item };
            Grid.SetRow(display, 1);
            grid.Children.Add(display);

            border.Child = grid;
            return border;
        }

        private void UpdateTallyBorders()
        {
            foreach (var child in MultiViewRoot.Children)
            {
                if (child is Border b)
                {
                    UpdateBoxBorder(b);
                }
                else if (child is Grid subGrid)
                {
                    foreach (var subChild in subGrid.Children)
                    {
                        if (subChild is Border subB) UpdateBoxBorder(subB);
                    }
                }
            }
        }

        private void UpdateBoxBorder(Border b)
        {
            if (b.Tag is string s)
            {
                if (s == "PV_MONITOR")
                {
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 230, 118)); // Green
                    if (b.Child is Grid g && g.Children.Count > 1 && g.Children[1] is InputDisplay id)
                    {
                        if (id.Item != _engine.PreviewInput) id.Item = _engine.PreviewInput;
                    }
                }
                else if (s == "PGM_MONITOR")
                {
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 23, 68)); // Red
                    if (b.Child is Grid g && g.Children.Count > 1 && g.Children[1] is InputDisplay id)
                    {
                        if (id.Item != _engine.ProgramInput) id.Item = _engine.ProgramInput;
                    }
                }
            }
            else if (b.Tag is InputItem item)
            {
                if (item == _engine.ProgramInput)
                {
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 23, 68)); // Red
                }
                else if (item == _engine.PreviewInput)
                {
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 230, 118)); // Green
                }
                else
                {
                    b.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 40, 46));
                }
            }
        }
    }
}
