using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Openmiks.Services
{
    // Gives any Visual a real 32-bit BGRA frame source (cached Fbo per visual) so one code path
    // covers camera, desktop capture, program output, recording and streaming.
    public static class ProgramSurface
    {
        private sealed class Fbo
        {
            // RenderTargetBitmap only accepts Pbgra32 (premultiplied). The program canvas is opaque
            // (alpha 255), where premultiplied equals straight BGRA, so ffmpeg gets -pixel_format bgra.
            public RenderTargetBitmap Rtb = null!;
            // Two buffers so an encoder thread can read the frame just published while the next
            // render writes the other one. ponytail: no queue; a consumer slower than two ticks
            // sees a repeated frame instead of a backlog, which is what live switching wants.
            public byte[][] Buffers = new byte[2][];
            public int Published;
        }

        private static readonly Dictionary<Visual, Fbo> Cache = new();

        // Last failure from Grab, so callers can report why instead of a silent null.
        public static string LastError { get; private set; } = string.Empty;

        public static (int Width, int Height) SizeOf(Visual? visual, int fallbackWidth = 1280, int fallbackHeight = 720)
        {
            double w = visual is FrameworkElement fe ? fe.ActualWidth : 0;
            double h = visual is FrameworkElement fe2 ? fe2.ActualHeight : 0;
            if (w < 2 || h < 2) return (fallbackWidth, fallbackHeight);
            // Even dimensions: encoders reject odd sizes.
            int iw = ((int)Math.Round(w) / 2) * 2;
            int ih = ((int)Math.Round(h) / 2) * 2;
            return (Math.Max(2, iw), Math.Max(2, ih));
        }

        public static byte[]? Grab(Visual? visual, out int width, out int height, int fallbackWidth = 1280, int fallbackHeight = 720)
        {
            var (w, h) = SizeOf(visual, fallbackWidth, fallbackHeight);
            width = w;
            height = h;
            LastError = string.Empty;
            if (visual == null)
            {
                LastError = "No visual to render.";
                return null;
            }

            try
            {
                if (!Cache.TryGetValue(visual, out var fbo) || fbo.Rtb.PixelWidth != w || fbo.Rtb.PixelHeight != h)
                {
                    fbo = new Fbo
                    {
                        Rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32),
                        Buffers = new[] { new byte[w * h * 4], new byte[w * h * 4] }
                    };
                    Cache[visual] = fbo;
                }

                int next = 1 - fbo.Published;
                fbo.Rtb.Render(visual);
                fbo.Rtb.CopyPixels(fbo.Buffers[next], w * 4, 0);
                fbo.Published = next;
                return fbo.Buffers[next];
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
        }

        // ffmpeg args that make a real framebuffer the input instead of gdigrab's desktop.
        public static string FfmpegInputArgs(int width, int height, int fps) =>
            $"-f rawvideo -pixel_format bgra -video_size {width}x{height} -framerate {fps} -i -";

        public static void Forget(Visual? visual)
        {
            if (visual != null) Cache.Remove(visual);
        }
    }
}
