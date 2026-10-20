using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media;
using VMixClone.Models;

namespace VMixClone.Services
{
    public static class PresetService
    {
        public static void SavePreset(string filePath, ProductionEngine engine, VMixConfiguration config)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("vmix",
                    new XElement("version", "29.0.0.49"),
                    new XElement("edition", "Pro"),
                    new XElement("preset", Path.GetFileNameWithoutExtension(filePath)),
                    new XElement("outputResolution", config.OutputResolution),
                    new XElement("frameRate", config.FrameRate),
                    new XElement("active", engine.ProgramInput?.Number ?? 1),
                    new XElement("preview", engine.PreviewInput?.Number ?? 2),
                    new XElement("masterVolume", engine.MasterVolume),
                    new XElement("masterMuted", engine.MasterMuted),
                    new XElement("busAVolume", engine.BusAVolume),
                    new XElement("busAMuted", engine.BusAMuted),
                    new XElement("overlays",
                        new XElement("overlay", new XAttribute("number", 1), engine.Overlay1?.Number ?? 0),
                        new XElement("overlay", new XAttribute("number", 2), engine.Overlay2?.Number ?? 0),
                        new XElement("overlay", new XAttribute("number", 3), engine.Overlay3?.Number ?? 0),
                        new XElement("overlay", new XAttribute("number", 4), engine.Overlay4?.Number ?? 0)
                    ),
                    new XElement("inputs",
                        engine.Inputs.Select(inp => new XElement("input",
                            new XAttribute("key", inp.Key),
                            new XAttribute("number", inp.Number),
                            new XAttribute("type", inp.Type.ToString()),
                            new XAttribute("category", inp.Category.ToString()),
                            new XAttribute("title", inp.Title),
                            new XAttribute("volume", inp.Volume),
                            new XAttribute("muted", inp.IsMuted),
                            new XAttribute("solo", inp.IsSolo),
                            new XAttribute("color", inp.SolidColor.ToString()),
                            new XAttribute("headline", inp.Headline ?? ""),
                            new XAttribute("subtitle", inp.Subtitle ?? ""),
                            new XAttribute("vsFolder", inp.VirtualSet?.FolderPath ?? "")
                        ))
                    )
                )
            );

            doc.Save(filePath);
        }

        public static bool LoadPreset(string filePath, ProductionEngine engine, VMixConfiguration config)
        {
            if (!File.Exists(filePath)) return false;

            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Element("vmix");
                if (root == null) return false;

                if (root.Element("outputResolution") != null)
                {
                    config.OutputResolution = root.Element("outputResolution")!.Value;
                }
                if (root.Element("frameRate") != null)
                {
                    config.FrameRate = root.Element("frameRate")!.Value;
                }
                if (double.TryParse(root.Element("masterVolume")?.Value, out double mVol))
                {
                    engine.MasterVolume = mVol;
                }
                if (bool.TryParse(root.Element("masterMuted")?.Value, out bool mMute))
                {
                    engine.MasterMuted = mMute;
                }

                var inputsEl = root.Element("inputs");
                if (inputsEl != null)
                {
                    engine.Inputs.Clear();
                    foreach (var el in inputsEl.Elements("input"))
                    {
                        var item = new InputItem
                        {
                            Key = el.Attribute("key")?.Value ?? Guid.NewGuid().ToString(),
                            Number = int.TryParse(el.Attribute("number")?.Value, out int num) ? num : 1,
                            Title = el.Attribute("title")?.Value ?? "Input",
                            Headline = el.Attribute("headline")?.Value ?? "",
                            Subtitle = el.Attribute("subtitle")?.Value ?? "",
                            Volume = double.TryParse(el.Attribute("volume")?.Value, out double v) ? v : 100,
                            IsMuted = bool.TryParse(el.Attribute("muted")?.Value, out bool m) && m,
                            IsSolo = bool.TryParse(el.Attribute("solo")?.Value, out bool s) && s
                        };

                        if (Enum.TryParse<InputType>(el.Attribute("type")?.Value, out var t))
                        {
                            item.Type = t;
                        }
                        if (Enum.TryParse<InputCategory>(el.Attribute("category")?.Value, out var c))
                        {
                            item.Category = c;
                        }

                        string? colorStr = el.Attribute("color")?.Value;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            try
                            {
                                item.SolidColor = (Color)ColorConverter.ConvertFromString(colorStr);
                            }
                            catch { }
                        }

                        string? vsFolder = el.Attribute("vsFolder")?.Value;
                        if (!string.IsNullOrEmpty(vsFolder) && Directory.Exists(vsFolder))
                        {
                            item.VirtualSet = VirtualSet.LoadFromFolder(vsFolder);
                        }

                        engine.Inputs.Add(item);
                    }
                }

                if (int.TryParse(root.Element("active")?.Value, out int activeNum))
                {
                    engine.SetProgram(activeNum);
                }
                if (int.TryParse(root.Element("preview")?.Value, out int previewNum))
                {
                    engine.SetPreview(previewNum);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
