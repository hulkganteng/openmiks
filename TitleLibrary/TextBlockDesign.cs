using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Openmiks.TitleLibrary
{
    // Stand-in for vMix's own vMixTitleLibrary.TextBlockDesign so shipped .xaml title templates
    // can be loaded by XamlReader without the third-party assembly. Fill maps onto Foreground.
    public class TextBlockDesign : TextBlock
    {
        public Brush Fill
        {
            get => Foreground;
            set => Foreground = value;
        }
    }
}
