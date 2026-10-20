using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VMixClone.Services
{
    public static class ScreenCaptureService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] byte[] lpvBits, ref BITMAPINFO lpbi, uint uUsage);

        private const int SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] bmiColors;
        }

        private static WriteableBitmap? _cachedBitmap;
        private static byte[]? _pixelBuffer;

        public static WriteableBitmap? CapturePrimaryScreen(int targetWidth = 1280, int targetHeight = 720)
        {
            try
            {
                int screenW = (int)SystemParameters.PrimaryScreenWidth;
                int screenH = (int)SystemParameters.PrimaryScreenHeight;
                if (screenW <= 0 || screenH <= 0) return null;

                if (_cachedBitmap == null || _cachedBitmap.PixelWidth != screenW || _cachedBitmap.PixelHeight != screenH)
                {
                    _cachedBitmap = new WriteableBitmap(screenW, screenH, 96, 96, PixelFormats.Bgr32, null);
                    _pixelBuffer = new byte[screenW * screenH * 4];
                }

                IntPtr hdcScreen = GetDC(IntPtr.Zero);
                IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, screenW, screenH);
                IntPtr hOld = SelectObject(hdcMem, hBitmap);

                BitBlt(hdcMem, 0, 0, screenW, screenH, hdcScreen, 0, 0, SRCCOPY);

                var bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmi.bmiHeader.biWidth = screenW;
                bmi.bmiHeader.biHeight = -screenH; // Top-down DIB
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = 0; // BI_RGB

                if (_pixelBuffer != null)
                {
                    GetDIBits(hdcMem, hBitmap, 0, (uint)screenH, _pixelBuffer, ref bmi, 0);

                    _cachedBitmap.Lock();
                    Marshal.Copy(_pixelBuffer, 0, _cachedBitmap.BackBuffer, _pixelBuffer.Length);
                    _cachedBitmap.AddDirtyRect(new Int32Rect(0, 0, screenW, screenH));
                    _cachedBitmap.Unlock();
                }

                SelectObject(hdcMem, hOld);
                DeleteObject(hBitmap);
                DeleteDC(hdcMem);
                ReleaseDC(IntPtr.Zero, hdcScreen);

                return _cachedBitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
