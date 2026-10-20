using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Openmiks.Services
{
    public class ScriptItem : INotifyPropertyChanged
    {
        private string _name = "AutoSwitchScript";
        private string _code = "// vMix C# Script Example\r\nAPI.Function(\"Cut\");\r\nAPI.Sleep(2000);\r\nAPI.Function(\"Fade\", \"Duration=1000\");";
        private bool _isRunning = false;
        private DateTime _lastRun = DateTime.MinValue;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        public DateTime LastRun
        {
            get => _lastRun;
            set { _lastRun = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TriggerRule : INotifyPropertyChanged
    {
        private string _trigger = "OnTransitionComplete";
        private string _function = "OverlayInput1";
        private string _input = "3";
        private string _value = "";
        private int _delayMs = 0;

        public string Trigger
        {
            get => _trigger;
            set { _trigger = value; OnPropertyChanged(); }
        }

        public string Function
        {
            get => _function;
            set { _function = value; OnPropertyChanged(); }
        }

        public string Input
        {
            get => _input;
            set { _input = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public int DelayMs
        {
            get => _delayMs;
            set { _delayMs = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ScriptingAndTriggersEngine : INotifyPropertyChanged
    {
        public ObservableCollection<ScriptItem> Scripts { get; } = new();
        public ObservableCollection<TriggerRule> Triggers { get; } = new();

        public ScriptingAndTriggersEngine()
        {
            Scripts.Add(new ScriptItem
            {
                Name = "IntroSequence",
                Code = "API.Function(\"ActiveInput\", \"Input=1\");\r\nAPI.Sleep(1500);\r\nAPI.Function(\"Fade\", \"Duration=800\");\r\nAPI.Function(\"OverlayInput1\", \"Input=3\");"
            });

            Triggers.Add(new TriggerRule
            {
                Trigger = "OnRecordingStart",
                Function = "OverlayInput2",
                Input = "7", // Logo Bug
                DelayMs = 0
            });
        }

        public async Task ExecuteScriptAsync(ScriptItem script, Action<string, string> apiExecutor)
        {
            script.IsRunning = true;
            script.LastRun = DateTime.UtcNow;

            try
            {
                // Line by line parser for rapid embedded execution
                var lines = script.Code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//")) continue;

                    if (trimmed.StartsWith("API.Sleep(", StringComparison.OrdinalIgnoreCase))
                    {
                        int start = trimmed.IndexOf('(') + 1;
                        int end = trimmed.IndexOf(')');
                        if (int.TryParse(trimmed.Substring(start, end - start), out int sleepMs))
                        {
                            await Task.Delay(sleepMs);
                        }
                    }
                    else if (trimmed.StartsWith("API.Function(", StringComparison.OrdinalIgnoreCase))
                    {
                        int start = trimmed.IndexOf('(') + 1;
                        int end = trimmed.LastIndexOf(')');
                        string inner = trimmed.Substring(start, end - start);
                        string[] parts = inner.Split(',', 2, StringSplitOptions.TrimEntries);
                        string func = parts[0].Trim('"');
                        string param = parts.Length > 1 ? parts[1].Trim('"') : "";
                        apiExecutor(func, param);
                    }
                }
            }
            finally
            {
                script.IsRunning = false;
            }
        }

        public void FireTrigger(string triggerName, Action<string, string> apiExecutor)
        {
            var matched = Triggers.Where(t => t.Trigger.Equals(triggerName, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var rule in matched)
            {
                if (rule.DelayMs > 0)
                {
                    Task.Delay(rule.DelayMs).ContinueWith(_ =>
                    {
                        apiExecutor(rule.Function, $"Input={rule.Input}&Value={rule.Value}");
                    });
                }
                else
                {
                    apiExecutor(rule.Function, $"Input={rule.Input}&Value={rule.Value}");
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
