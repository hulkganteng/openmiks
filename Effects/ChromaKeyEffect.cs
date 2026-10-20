using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace VMixClone.Effects
{
    public class ChromaKeyEffect : ShaderEffect
    {
        private static readonly PixelShader _pixelShader = new();

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ChromaKeyEffect), 0);

        public static readonly DependencyProperty KeyColorProperty =
            DependencyProperty.Register(
                nameof(KeyColor),
                typeof(Color),
                typeof(ChromaKeyEffect),
                new UIPropertyMetadata(Color.FromRgb(0, 255, 0), PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty SimilarityProperty =
            DependencyProperty.Register(
                nameof(Similarity),
                typeof(double),
                typeof(ChromaKeyEffect),
                new UIPropertyMetadata(0.38, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty SmoothnessProperty =
            DependencyProperty.Register(
                nameof(Smoothness),
                typeof(double),
                typeof(ChromaKeyEffect),
                new UIPropertyMetadata(0.10, PixelShaderConstantCallback(2)));

        public static readonly DependencyProperty SpillSuppressionProperty =
            DependencyProperty.Register(
                nameof(SpillSuppression),
                typeof(double),
                typeof(ChromaKeyEffect),
                new UIPropertyMetadata(0.80, PixelShaderConstantCallback(3)));

        static ChromaKeyEffect()
        {
            try
            {
                string psPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Effects", "ChromaKey.ps");
                if (File.Exists(psPath))
                {
                    _pixelShader.UriSource = new Uri(psPath, UriKind.Absolute);
                }
                else
                {
                    // Fallback to project relative location if running in dev
                    string devPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Effects", "ChromaKey.ps");
                    if (File.Exists(devPath))
                    {
                        _pixelShader.UriSource = new Uri(Path.GetFullPath(devPath), UriKind.Absolute);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load ChromaKey.ps: {ex.Message}");
            }
        }

        public ChromaKeyEffect()
        {
            PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(KeyColorProperty);
            UpdateShaderValue(SimilarityProperty);
            UpdateShaderValue(SmoothnessProperty);
            UpdateShaderValue(SpillSuppressionProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public Color KeyColor
        {
            get => (Color)GetValue(KeyColorProperty);
            set => SetValue(KeyColorProperty, value);
        }

        public double Similarity
        {
            get => (double)GetValue(SimilarityProperty);
            set => SetValue(SimilarityProperty, value);
        }

        public double Smoothness
        {
            get => (double)GetValue(SmoothnessProperty);
            set => SetValue(SmoothnessProperty, value);
        }

        public double SpillSuppression
        {
            get => (double)GetValue(SpillSuppressionProperty);
            set => SetValue(SpillSuppressionProperty, value);
        }
    }
}
