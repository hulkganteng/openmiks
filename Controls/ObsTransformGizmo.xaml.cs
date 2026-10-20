using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Openmiks.Models;

namespace Openmiks.Controls
{
    public partial class ObsTransformGizmo : UserControl
    {
        private InputItem? _item;
        private bool _isDraggingBox = false;
        private bool _isDraggingHandle = false;
        private bool _isDraggingRot = false;
        private string _activeHandle = string.Empty;
        private Point _dragStartPos;
        private double _startPanX;
        private double _startPanY;
        private double _startZoomX;
        private double _startZoomY;
        private double _startCropL;
        private double _startCropR;
        private double _startCropT;
        private double _startCropB;
        private double _startRotation;
        private Point _boxCenter;

        private bool _forceCropMode = false;
        public bool IsCropMode
        {
            get => _forceCropMode;
            set
            {
                _forceCropMode = value;
                UpdateGizmoColors();
            }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                nameof(Item),
                typeof(InputItem),
                typeof(ObsTransformGizmo),
                new PropertyMetadata(null, OnItemChanged));

        public InputItem? Item
        {
            get => (InputItem?)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public ObsTransformGizmo()
        {
            InitializeComponent();
            SizeChanged += (s, e) => UpdateGizmoLayout();
            Loaded += (s, e) => UpdateGizmoLayout();
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ObsTransformGizmo gizmo)
            {
                if (e.OldValue is InputItem oldItem)
                {
                    oldItem.PropertyChanged -= gizmo.OnItemPropertyChanged;
                }
                if (e.NewValue is InputItem newItem)
                {
                    newItem.PropertyChanged += gizmo.OnItemPropertyChanged;
                }
                gizmo._item = e.NewValue as InputItem;
                gizmo.UpdateGizmoLayout();
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(InputItem.PanX) or nameof(InputItem.PanY)
                or nameof(InputItem.ZoomX) or nameof(InputItem.ZoomY)
                or nameof(InputItem.Rotation)
                or nameof(InputItem.CropLeft) or nameof(InputItem.CropRight)
                or nameof(InputItem.CropTop) or nameof(InputItem.CropBottom))
            {
                UpdateGizmoLayout();
            }
        }

        public void UpdateGizmoColors()
        {
            bool isCrop = _forceCropMode || (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            var stroke = isCrop ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            var fill = isCrop ? new SolidColorBrush(Color.FromArgb(0x18, 0x22, 0xC5, 0x5E)) : new SolidColorBrush(Color.FromArgb(0x10, 0xEF, 0x44, 0x44));

            BoxBorder.Stroke = stroke;
            BoxBorder.Fill = fill;
            CenterDot.Fill = stroke;
            RotLine.Stroke = stroke;
            RotHandle.BorderBrush = stroke;
            HudBadge.BorderBrush = stroke;

            foreach (var handle in new[] { HandleNW, HandleN, HandleNE, HandleE, HandleSE, HandleS, HandleSW, HandleW })
            {
                handle.BorderBrush = stroke;
            }
        }

        public void UpdateGizmoLayout()
        {
            if (_item == null || ActualWidth <= 0 || ActualHeight <= 0)
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            Visibility = Visibility.Visible;
            UpdateGizmoColors();

            double w = ActualWidth;
            double h = ActualHeight;

            double cropW = Math.Max(10, w * (1.0 - _item.CropLeft - _item.CropRight));
            double cropH = Math.Max(10, h * (1.0 - _item.CropTop - _item.CropBottom));

            double boxW = Math.Max(20, cropW * _item.ZoomX);
            double boxH = Math.Max(20, cropH * _item.ZoomY);

            double cx = (w / 2.0) + (_item.PanX * (w / 2.0));
            double cy = (h / 2.0) + (_item.PanY * (h / 2.0));
            _boxCenter = new Point(cx, cy);

            double bx = cx - (boxW / 2.0);
            double by = cy - (boxH / 2.0);

            Canvas.SetLeft(BoxBorder, bx);
            Canvas.SetTop(BoxBorder, by);
            BoxBorder.Width = boxW;
            BoxBorder.Height = boxH;

            // Center dot
            Canvas.SetLeft(CenterDot, cx - 3);
            Canvas.SetTop(CenterDot, cy - 3);

            // Rotation Arm and Handle
            double rotArmLen = 24;
            RotLine.X1 = cx;
            RotLine.Y1 = by;
            RotLine.X2 = cx;
            RotLine.Y2 = by - rotArmLen;

            Canvas.SetLeft(RotHandle, cx - 7);
            Canvas.SetTop(RotHandle, by - rotArmLen - 7);

            // Position 8 handles
            PositionHandle(HandleNW, bx, by);
            PositionHandle(HandleN, cx, by);
            PositionHandle(HandleNE, bx + boxW, by);
            PositionHandle(HandleE, bx + boxW, cy);
            PositionHandle(HandleSE, bx + boxW, by + boxH);
            PositionHandle(HandleS, cx, by + boxH);
            PositionHandle(HandleSW, bx, by + boxH);
            PositionHandle(HandleW, bx, cy);

            // HUD badge positioning (bottom-left of box)
            Canvas.SetLeft(HudBadge, Math.Max(4, bx));
            Canvas.SetTop(HudBadge, Math.Min(h - 26, by + boxH + 6));
            HudText.Text = $"X: {_item.PanX:+0.00;-0.00;0.00} | Y: {_item.PanY:+0.00;-0.00;0.00} | Zoom: {(int)(_item.ZoomX * 100)}% | Rot: {(int)_item.Rotation}°";

            // Rotate entire BoxCanvas around (cx, cy)
            var rot = new RotateTransform(_item.Rotation, cx, cy);
            BoxCanvas.RenderTransform = rot;
        }

        private static void PositionHandle(Border b, double x, double y)
        {
            Canvas.SetLeft(b, x - (b.Width / 2.0));
            Canvas.SetTop(b, y - (b.Height / 2.0));
        }

        // --- Drag Move ---
        private void Box_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_item == null) return;
            _isDraggingBox = true;
            _dragStartPos = e.GetPosition(GizmoCanvas);
            _startPanX = _item.PanX;
            _startPanY = _item.PanY;
            BoxBorder.CaptureMouse();
            e.Handled = true;
        }

        private void Box_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingBox || _item == null) return;
            Point curr = e.GetPosition(GizmoCanvas);
            double dx = curr.X - _dragStartPos.X;
            double dy = curr.Y - _dragStartPos.Y;

            double hw = ActualWidth / 2.0;
            double hh = ActualHeight / 2.0;

            if (hw > 0 && hh > 0)
            {
                _item.PanX = Math.Round(_startPanX + (dx / hw), 3);
                _item.PanY = Math.Round(_startPanY + (dy / hh), 3);
            }
        }

        private void Box_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingBox)
            {
                _isDraggingBox = false;
                BoxBorder.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        // --- Drag Handles (Scale or Crop) ---
        private void Handle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_item == null || sender is not Border b) return;
            _isDraggingHandle = true;
            _activeHandle = b.Tag?.ToString() ?? "SE";
            _dragStartPos = e.GetPosition(GizmoCanvas);
            _startZoomX = _item.ZoomX;
            _startZoomY = _item.ZoomY;
            _startCropL = _item.CropLeft;
            _startCropR = _item.CropRight;
            _startCropT = _item.CropTop;
            _startCropB = _item.CropBottom;

            UpdateGizmoColors();
            b.CaptureMouse();
            e.Handled = true;
        }

        private void Handle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingHandle || _item == null) return;
            Point curr = e.GetPosition(GizmoCanvas);
            double dx = curr.X - _dragStartPos.X;
            double dy = curr.Y - _dragStartPos.Y;

            bool isCrop = _forceCropMode || (Keyboard.Modifiers & ModifierKeys.Alt) != 0;

            if (isCrop)
            {
                // Crop mode
                double w = ActualWidth > 0 ? ActualWidth : 1920;
                double h = ActualHeight > 0 ? ActualHeight : 1080;

                switch (_activeHandle)
                {
                    case "W":
                        _item.CropLeft = Math.Clamp(_startCropL + (dx / w), 0.0, 0.9);
                        break;
                    case "E":
                        _item.CropRight = Math.Clamp(_startCropR - (dx / w), 0.0, 0.9);
                        break;
                    case "N":
                        _item.CropTop = Math.Clamp(_startCropT + (dy / h), 0.0, 0.9);
                        break;
                    case "S":
                        _item.CropBottom = Math.Clamp(_startCropB - (dy / h), 0.0, 0.9);
                        break;
                    case "NW":
                        _item.CropLeft = Math.Clamp(_startCropL + (dx / w), 0.0, 0.9);
                        _item.CropTop = Math.Clamp(_startCropT + (dy / h), 0.0, 0.9);
                        break;
                    case "NE":
                        _item.CropRight = Math.Clamp(_startCropR - (dx / w), 0.0, 0.9);
                        _item.CropTop = Math.Clamp(_startCropT + (dy / h), 0.0, 0.9);
                        break;
                    case "SW":
                        _item.CropLeft = Math.Clamp(_startCropL + (dx / w), 0.0, 0.9);
                        _item.CropBottom = Math.Clamp(_startCropB - (dy / h), 0.0, 0.9);
                        break;
                    case "SE":
                        _item.CropRight = Math.Clamp(_startCropR - (dx / w), 0.0, 0.9);
                        _item.CropBottom = Math.Clamp(_startCropB - (dy / h), 0.0, 0.9);
                        break;
                }
            }
            else
            {
                // Scale / Zoom mode
                double factorX = 1.0;
                double factorY = 1.0;
                double baseDim = (ActualWidth + ActualHeight) / 4.0;

                switch (_activeHandle)
                {
                    case "SE":
                        factorX = 1.0 + ((dx + dy) / baseDim);
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorX, 3), 0.05, 5.0);
                        break;
                    case "NW":
                        factorX = 1.0 - ((dx + dy) / baseDim);
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorX, 3), 0.05, 5.0);
                        break;
                    case "NE":
                        factorX = 1.0 + ((dx - dy) / baseDim);
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorX, 3), 0.05, 5.0);
                        break;
                    case "SW":
                        factorX = 1.0 + ((-dx + dy) / baseDim);
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorX, 3), 0.05, 5.0);
                        break;
                    case "E":
                        factorX = 1.0 + (dx / (baseDim / 2.0));
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        break;
                    case "W":
                        factorX = 1.0 - (dx / (baseDim / 2.0));
                        _item.ZoomX = Math.Clamp(Math.Round(_startZoomX * factorX, 3), 0.05, 5.0);
                        break;
                    case "S":
                        factorY = 1.0 + (dy / (baseDim / 2.0));
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorY, 3), 0.05, 5.0);
                        break;
                    case "N":
                        factorY = 1.0 - (dy / (baseDim / 2.0));
                        _item.ZoomY = Math.Clamp(Math.Round(_startZoomY * factorY, 3), 0.05, 5.0);
                        break;
                }
            }
        }

        private void Handle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingHandle)
            {
                _isDraggingHandle = false;
                if (sender is Border b) b.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        // --- Drag Rotation ---
        private void RotHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_item == null) return;
            _isDraggingRot = true;
            _dragStartPos = e.GetPosition(GizmoCanvas);
            _startRotation = _item.Rotation;
            RotHandle.CaptureMouse();
            e.Handled = true;
        }

        private void RotHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingRot || _item == null) return;
            Point curr = e.GetPosition(GizmoCanvas);
            double angleStart = Math.Atan2(_dragStartPos.Y - _boxCenter.Y, _dragStartPos.X - _boxCenter.X) * (180.0 / Math.PI);
            double angleCurr = Math.Atan2(curr.Y - _boxCenter.Y, curr.X - _boxCenter.X) * (180.0 / Math.PI);
            double deltaAngle = angleCurr - angleStart;

            double newRot = (_startRotation + deltaAngle) % 360.0;
            if (newRot > 180.0) newRot -= 360.0;
            if (newRot < -180.0) newRot += 360.0;

            _item.Rotation = Math.Round(newRot, 1);
        }

        private void RotHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingRot)
            {
                _isDraggingRot = false;
                RotHandle.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
    }
}
