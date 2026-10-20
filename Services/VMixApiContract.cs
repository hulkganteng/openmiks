using System.ComponentModel;
using Openmiks.Models;

namespace Openmiks.Services
{
    public struct vMixTally
    {
        public int InputNumber;
        public Guid Key;
        public string COMPort;
        public int COMNumber;
    }

    public struct vMixTallys
    {
        public vMixTally[] Output;
        public vMixTally[] Preview;
    }

    public struct vMixState
    {
        public int Output;
        public int Preview;
        public int[] Overlays;
        public int[] OverlaysOutput;
        public int[] OverlaysPreview;
        public int[] OutputMix;
        public int[] PreviewMix;
        public int Inputs;
        public bool FTB;
        public string XML;
    }

    public delegate void StateChangedEventHandler(object sender, EventArgs e);
    public delegate void TallyChangedEventHandler(object sender, EventArgs e);

    public interface IvMixAPI
    {
        event StateChangedEventHandler? StateChanged;
        event TallyChangedEventHandler? TallyChanged;

        void Execute(string function);
        vMixState GetState(bool includeXml = true);
        vMixTallys GetTallys();
    }

    public class VMixApiService : IvMixAPI, IDisposable
    {
        private readonly ProductionEngine _engine;

        public event StateChangedEventHandler? StateChanged;
        public event TallyChangedEventHandler? TallyChanged;

        public VMixApiService(ProductionEngine engine)
        {
            _engine = engine;
            _engine.PropertyChanged += OnEnginePropertyChanged;
        }

        private void OnEnginePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ProductionEngine.ProgramInput) or nameof(ProductionEngine.PreviewInput))
            {
                TallyChanged?.Invoke(this, EventArgs.Empty);
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (e.PropertyName is nameof(ProductionEngine.IsFTB) or nameof(ProductionEngine.IsRecording) or nameof(ProductionEngine.IsStreaming))
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Execute(string function)
        {
            if (string.IsNullOrWhiteSpace(function)) return;
            string fn = function.Trim();
            string? param = null;
            int qIndex = fn.IndexOf('?');
            if (qIndex >= 0)
            {
                param = fn.Substring(qIndex + 1);
                fn = fn.Substring(0, qIndex);
            }
            else
            {
                int spaceIndex = fn.IndexOf(' ');
                if (spaceIndex >= 0)
                {
                    param = fn.Substring(spaceIndex + 1);
                    fn = fn.Substring(0, spaceIndex);
                }
            }

            _engine.ExecuteApiFunction(fn, param ?? string.Empty);
        }

        public vMixState GetState(bool includeXml = true)
        {
            var overlays = new int[]
            {
                _engine.Overlay1?.Number ?? 0,
                _engine.Overlay2?.Number ?? 0,
                _engine.Overlay3?.Number ?? 0,
                _engine.Overlay4?.Number ?? 0
            };

            return new vMixState
            {
                Output = _engine.ProgramInput?.Number ?? 0,
                Preview = _engine.PreviewInput?.Number ?? 0,
                Overlays = overlays,
                OverlaysOutput = (int[])overlays.Clone(),
                OverlaysPreview = new int[4],
                OutputMix = new int[] { _engine.ProgramInput?.Number ?? 0 },
                PreviewMix = new int[] { _engine.PreviewInput?.Number ?? 0 },
                Inputs = _engine.Inputs.Count,
                FTB = _engine.IsFTB,
                XML = includeXml ? BuildXmlSummary() : string.Empty
            };
        }

        public vMixTallys GetTallys()
        {
            var outputTallies = new List<vMixTally>();
            var previewTallies = new List<vMixTally>();

            if (_engine.ProgramInput != null)
            {
                outputTallies.Add(new vMixTally
                {
                    InputNumber = _engine.ProgramInput.Number,
                    Key = Guid.NewGuid(),
                    COMPort = string.Empty,
                    COMNumber = _engine.ProgramInput.Number
                });
            }

            if (_engine.PreviewInput != null)
            {
                previewTallies.Add(new vMixTally
                {
                    InputNumber = _engine.PreviewInput.Number,
                    Key = Guid.NewGuid(),
                    COMPort = string.Empty,
                    COMNumber = _engine.PreviewInput.Number
                });
            }

            return new vMixTallys
            {
                Output = outputTallies.ToArray(),
                Preview = previewTallies.ToArray()
            };
        }

        private string BuildXmlSummary()
        {
            return $"<vmix><version>29.0.0.49</version><active>{_engine.ProgramInput?.Number ?? 0}</active><preview>{_engine.PreviewInput?.Number ?? 0}</preview><recording>{_engine.IsRecording}</recording><streaming>{_engine.IsStreaming}</streaming></vmix>";
        }

        public void Dispose()
        {
            _engine.PropertyChanged -= OnEnginePropertyChanged;
        }
    }
}
