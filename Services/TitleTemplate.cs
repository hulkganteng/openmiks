using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;

namespace Openmiks.Services
{
    // Loads real vMix title templates (Titles\*.xaml) and drives their named fields.
    // vMix's templates reference clr-namespace:vMixTitleLibrary;assembly=vMixTitleLibrary, which
    // is a third-party assembly, so the markup is rewritten onto our own TextBlockDesign shim
    // before XamlReader parses it. Nothing about the template's own layout is changed.
    public sealed class TitleTemplate
    {
        public string Name { get; private set; } = "Title";
        public string TemplatePath { get; private set; } = string.Empty;
        public FrameworkElement? Root { get; private set; }
        public IReadOnlyList<string> Fields => _fields;
        // Fields the parser actually indexed by name, which is what SetField can reach.
        public IReadOnlyList<string> NamedFields => _named.Keys.ToList();
        public string LastError { get; private set; } = string.Empty;

        private readonly Dictionary<string, FrameworkElement> _named = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _fields = new();

        private const string VmixNamespace = "clr-namespace:vMixTitleLibrary;assembly=vMixTitleLibrary";
        // XamlReader.Parse resolves clr-namespace against a named assembly at runtime, so the shim
        // namespace must carry this assembly's own name.
        private const string LocalNamespace = "clr-namespace:Openmiks.TitleLibrary;assembly=Openmiks";
        // Renamed so the type can hold its own TemplatePath property without shadowing System.IO.Path.
        private static readonly string VmixTitlesDir = @"C:\Program Files (x86)\vMix\titles";

        public static TitleTemplate? Load(string xamlPath)
        {
            var t = new TitleTemplate();
            return t.LoadInternal(xamlPath) ? t : null;
        }

        // Reason the last Load call returned null; Load() cannot report it through the instance.
        public static string LastLoadError { get; private set; } = string.Empty;

        public static IReadOnlyList<string> AvailableTemplates()
        {
            var found = new List<string>();
            foreach (string dir in new[] { System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Titles"), VmixTitlesDir })
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (string f in Directory.GetFiles(dir, "*.xaml"))
                    {
                        if (!found.Contains(f)) found.Add(f);
                    }
                }
                catch { }
            }
            found.Sort(StringComparer.OrdinalIgnoreCase);
            return found;
        }

        private bool LoadInternal(string xamlPath)
        {
            try
            {
                if (!File.Exists(xamlPath))
                {
                    LastError = $"Template not found: {xamlPath}";
                    return false;
                }

                TemplatePath = xamlPath;
                Name = System.IO.Path.GetFileNameWithoutExtension(xamlPath);
                string markup = File.ReadAllText(xamlPath);
                // Fields first: the template's own names, in declaration order.
                var doc = XDocument.Parse(markup);
                foreach (var el in doc.Descendants())
                {
                    string? nm = el.Attribute("Name")?.Value;
                    string local = el.Name.LocalName;
                    if (string.IsNullOrEmpty(nm)) continue;
                    if (local is not ("TextBlockDesign" or "TextBlock" or "ImageDesign" or "Image")) continue;
                    if (!_fields.Contains(nm)) _fields.Add(nm);
                }

                markup = Rewrite(markup, System.IO.Path.GetDirectoryName(xamlPath) ?? string.Empty);
                Root = (FrameworkElement)XamlReader.Parse(markup);
                IndexNamed(Root);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                LastLoadError = $"{xamlPath}: {ex.Message}";
                Root = null;
                return false;
            }
        }

        private static string Rewrite(string markup, string templateDir)
        {
            markup = markup.Replace(VmixNamespace, LocalNamespace);

            // x:Class would demand a code-behind type this project does not have.
            markup = System.Text.RegularExpressions.Regex.Replace(markup, "\\sx:Class=\"[^\"]*\"", string.Empty);

            // Image sources are sibling .xaml.png files: resolve them to a real file URI so the
            // template renders its own artwork instead of failing to load a relative pack URI.
            return System.Text.RegularExpressions.Regex.Replace(markup, "Source=\"([^\"]+\\.png)\"", m =>
            {
                string rel = m.Groups[1].Value;
                foreach (string candidate in new[]
                         {
                             System.IO.Path.Combine(templateDir, rel),
                             System.IO.Path.Combine(VmixTitlesDir, rel)
                         })
                {
                    if (File.Exists(candidate)) return $"Source=\"{new Uri(candidate).AbsoluteUri}\"";
                }
                return m.Value;
            });
        }

        // Walks the logical tree: XamlReader.Parse does not build the visual tree until layout, so
        // VisualTreeHelper alone would see no children and index nothing.
        private void IndexNamed(DependencyObject root)
        {
            if (root is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
            {
                _named[fe.Name] = fe;
            }

            if (root is not DependencyObject node) return;

            foreach (object child in LogicalTreeHelper.GetChildren(node))
            {
                if (child is DependencyObject d) IndexNamed(d);
            }
        }

        // Fallback lookup for a field the logical walk could not index.
        private FrameworkElement? FindByName(string field) =>
            _named.TryGetValue(field, out var el) ? el
            : Root?.FindName(field) as FrameworkElement;

        public bool SetField(string field, string text)
        {
            var el = FindByName(field);
            if (el == null)
            {
                LastError = $"Template '{Name}' has no field '{field}'";
                return false;
            }

            switch (el)
            {
                case TextBlock tb:
                    tb.Text = text;
                    return true;
                case Image img:
                    try
                    {
                        img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(text));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LastError = ex.Message;
                        return false;
                    }
                default:
                    LastError = $"Field '{field}' is {el.GetType().Name}, not text or image";
                    return false;
            }
        }

        public string? GetField(string field) => FindByName(field) is TextBlock tb ? tb.Text : null;

        public bool HasField(string field) => FindByName(field) != null;

        public void ApplyHeadline(string headline, string subtitle)
        {
            // vMix's own templates name these variously; set every sensible alias.
            foreach (string f in new[] { "Heading", "Headline", "Title", "SubHeading", "News" })
            {
                if (HasField(f)) SetField(f, headline);
            }
            foreach (string f in new[] { "Description", "SubDescription", "SubHeading", "rDescription" })
            {
                if (HasField(f)) SetField(f, subtitle);
            }
        }
    }
}
