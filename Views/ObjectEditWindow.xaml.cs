using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VMixClone.Models;

namespace VMixClone.Views
{
    public partial class ObjectEditWindow : Window
    {
        private readonly InputItem _item;
        private bool _isUpdatingUi = false;

        public ObjectEditWindow(InputItem item)
        {
            InitializeComponent();
            _item = item;

            TitleText.Text = $"EDIT OBJEK: #{item.Number} {item.Title.ToUpper()}";

            ViewportDisplay.Item = item;
            ViewportGizmo.Item = item;

            _item.PropertyChanged += Item_PropertyChanged;
            Closed += (s, e) => _item.PropertyChanged -= Item_PropertyChanged;

            LoadValuesFromItem();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isUpdatingUi) return;

            if (e.PropertyName is nameof(InputItem.PanX) or nameof(InputItem.PanY)
                or nameof(InputItem.ZoomX) or nameof(InputItem.ZoomY)
                or nameof(InputItem.Rotation)
                or nameof(InputItem.CropLeft) or nameof(InputItem.CropRight)
                or nameof(InputItem.CropTop) or nameof(InputItem.CropBottom))
            {
                Dispatcher.Invoke(LoadValuesFromItem);
            }
        }

        private void LoadValuesFromItem()
        {
            _isUpdatingUi = true;
            try
            {
                PanXSlider.Value = _item.PanX;
                PanYSlider.Value = _item.PanY;
                PanXText.Text = _item.PanX.ToString("F2", CultureInfo.InvariantCulture);
                PanYText.Text = _item.PanY.ToString("F2", CultureInfo.InvariantCulture);

                ZoomXSlider.Value = _item.ZoomX;
                ZoomYSlider.Value = _item.ZoomY;
                ZoomXText.Text = _item.ZoomX.ToString("F2", CultureInfo.InvariantCulture);
                ZoomYText.Text = _item.ZoomY.ToString("F2", CultureInfo.InvariantCulture);

                RotationSlider.Value = _item.Rotation;
                RotationText.Text = $"{Math.Round(_item.Rotation, 1)}°";

                CropLSlider.Value = _item.CropLeft;
                CropRSlider.Value = _item.CropRight;
                CropTSlider.Value = _item.CropTop;
                CropBSlider.Value = _item.CropBottom;

                CropLText.Text = $"{(int)(_item.CropLeft * 100)}%";
                CropRText.Text = $"{(int)(_item.CropRight * 100)}%";
                CropTText.Text = $"{(int)(_item.CropTop * 100)}%";
                CropBText.Text = $"{(int)(_item.CropBottom * 100)}%";

                CoordsStatusText.Text = $"Pan: ({_item.PanX:+0.00;-0.00;0.00}, {_item.PanY:+0.00;-0.00;0.00}) | " +
                                       $"Zoom: {(int)(_item.ZoomX * 100)}% x {(int)(_item.ZoomY * 100)}% | " +
                                       $"Rot: {(int)_item.Rotation}° | " +
                                       $"Crop: L:{(int)(_item.CropLeft * 100)}% R:{(int)(_item.CropRight * 100)}% T:{(int)(_item.CropTop * 100)}% B:{(int)(_item.CropBottom * 100)}%";
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdatingUi || _item == null) return;

            if (sender == PanXSlider)
            {
                _item.PanX = Math.Round(PanXSlider.Value, 2);
                PanXText.Text = _item.PanX.ToString("F2", CultureInfo.InvariantCulture);
            }
            else if (sender == PanYSlider)
            {
                _item.PanY = Math.Round(PanYSlider.Value, 2);
                PanYText.Text = _item.PanY.ToString("F2", CultureInfo.InvariantCulture);
            }
            else if (sender == ZoomXSlider)
            {
                _item.ZoomX = Math.Round(ZoomXSlider.Value, 2);
                ZoomXText.Text = _item.ZoomX.ToString("F2", CultureInfo.InvariantCulture);
                if (LockAspectCheck.IsChecked == true && Math.Abs(_item.ZoomY - _item.ZoomX) > 0.001)
                {
                    _item.ZoomY = _item.ZoomX;
                    ZoomYSlider.Value = _item.ZoomY;
                    ZoomYText.Text = ZoomXText.Text;
                }
            }
            else if (sender == ZoomYSlider)
            {
                _item.ZoomY = Math.Round(ZoomYSlider.Value, 2);
                ZoomYText.Text = _item.ZoomY.ToString("F2", CultureInfo.InvariantCulture);
                if (LockAspectCheck.IsChecked == true && Math.Abs(_item.ZoomX - _item.ZoomY) > 0.001)
                {
                    _item.ZoomX = _item.ZoomY;
                    ZoomXSlider.Value = _item.ZoomX;
                    ZoomXText.Text = ZoomYText.Text;
                }
            }
            else if (sender == RotationSlider)
            {
                _item.Rotation = Math.Round(RotationSlider.Value, 1);
                RotationText.Text = $"{_item.Rotation}°";
            }
            else if (sender == CropLSlider)
            {
                _item.CropLeft = CropLSlider.Value;
                CropLText.Text = $"{(int)(_item.CropLeft * 100)}%";
            }
            else if (sender == CropRSlider)
            {
                _item.CropRight = CropRSlider.Value;
                CropRText.Text = $"{(int)(_item.CropRight * 100)}%";
            }
            else if (sender == CropTSlider)
            {
                _item.CropTop = CropTSlider.Value;
                CropTText.Text = $"{(int)(_item.CropTop * 100)}%";
            }
            else if (sender == CropBSlider)
            {
                _item.CropBottom = CropBSlider.Value;
                CropBText.Text = $"{(int)(_item.CropBottom * 100)}%";
            }

            CoordsStatusText.Text = $"Pan: ({_item.PanX:+0.00;-0.00;0.00}, {_item.PanY:+0.00;-0.00;0.00}) | " +
                                   $"Zoom: {(int)(_item.ZoomX * 100)}% x {(int)(_item.ZoomY * 100)}% | " +
                                   $"Rot: {(int)_item.Rotation}° | " +
                                   $"Crop: L:{(int)(_item.CropLeft * 100)}% R:{(int)(_item.CropRight * 100)}% T:{(int)(_item.CropTop * 100)}% B:{(int)(_item.CropBottom * 100)}%";
        }

        private void Text_LostFocus(object sender, RoutedEventArgs e) => CommitTextBox(sender);

        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Return)
            {
                CommitTextBox(sender);
                Keyboard.ClearFocus();
            }
        }

        private void CommitTextBox(object sender)
        {
            if (_isUpdatingUi || _item == null) return;

            if (sender == PanXText && double.TryParse(PanXText.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double px))
            {
                _item.PanX = px;
                PanXSlider.Value = px;
            }
            else if (sender == PanYText && double.TryParse(PanYText.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double py))
            {
                _item.PanY = py;
                PanYSlider.Value = py;
            }
            else if (sender == ZoomXText && double.TryParse(ZoomXText.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double zx))
            {
                _item.ZoomX = Math.Max(0.05, zx);
                ZoomXSlider.Value = _item.ZoomX;
                if (LockAspectCheck.IsChecked == true)
                {
                    _item.ZoomY = _item.ZoomX;
                    ZoomYSlider.Value = _item.ZoomX;
                }
            }
            else if (sender == ZoomYText && double.TryParse(ZoomYText.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double zy))
            {
                _item.ZoomY = Math.Max(0.05, zy);
                ZoomYSlider.Value = _item.ZoomY;
                if (LockAspectCheck.IsChecked == true)
                {
                    _item.ZoomX = _item.ZoomY;
                    ZoomXSlider.Value = _item.ZoomY;
                }
            }
            else if (sender == RotationText)
            {
                string clean = RotationText.Text.Replace("°", "").Trim();
                if (double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out double rot))
                {
                    _item.Rotation = rot;
                    RotationSlider.Value = rot;
                }
            }
        }

        private void ModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (CanvasCol == null || SlidersCol == null) return;

            if (ModeDualRadio.IsChecked == true)
            {
                CanvasCol.Width = new GridLength(6, GridUnitType.Star);
                SlidersCol.Width = new GridLength(4, GridUnitType.Star);
            }
            else if (ModeObsRadio.IsChecked == true)
            {
                CanvasCol.Width = new GridLength(1, GridUnitType.Star);
                SlidersCol.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else if (ModeVmixRadio.IsChecked == true)
            {
                CanvasCol.Width = new GridLength(4, GridUnitType.Star);
                SlidersCol.Width = new GridLength(6, GridUnitType.Star);
            }
        }

        private void CropToggle_Click(object sender, RoutedEventArgs e)
        {
            ViewportGizmo.IsCropMode = CropToggleBtn.IsChecked == true;
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string preset)
            {
                _item.ApplyPreset(preset);
                LoadValuesFromItem();
            }
        }

        private void ResetPanX_Click(object sender, RoutedEventArgs e)
        {
            _item.PanX = 0;
            LoadValuesFromItem();
        }

        private void ResetPanY_Click(object sender, RoutedEventArgs e)
        {
            _item.PanY = 0;
            LoadValuesFromItem();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            _item.ZoomX = 1.0;
            _item.ZoomY = 1.0;
            LoadValuesFromItem();
        }

        private void ResetRotation_Click(object sender, RoutedEventArgs e)
        {
            _item.Rotation = 0;
            LoadValuesFromItem();
        }

        private void ResetCrop_Click(object sender, RoutedEventArgs e)
        {
            _item.CropLeft = 0;
            _item.CropRight = 0;
            _item.CropTop = 0;
            _item.CropBottom = 0;
            LoadValuesFromItem();
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            _item.ResetTransform();
            LoadValuesFromItem();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
