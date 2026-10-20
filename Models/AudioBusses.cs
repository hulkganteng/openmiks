using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VMixClone.Models
{
    public class AudioBus : INotifyPropertyChanged
    {
        private string _name = "Master";
        private string _tag = "M";
        private double _volume = 85.0; // 0 - 100
        private bool _isMuted = false;
        private double _meterLevelL = 0.7;
        private double _meterLevelR = 0.7;
        private string _assignedOutputDevice = "Default Windows Playback";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Tag
        {
            get => _tag;
            set { _tag = value; OnPropertyChanged(); }
        }

        public double Volume
        {
            get => _volume;
            set { _volume = Math.Clamp(value, 0.0, 100.0); OnPropertyChanged(); }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
        }

        public double MeterLevelL
        {
            get => _meterLevelL;
            set { _meterLevelL = value; OnPropertyChanged(); }
        }

        public double MeterLevelR
        {
            get => _meterLevelR;
            set { _meterLevelR = value; OnPropertyChanged(); }
        }

        public string AssignedOutputDevice
        {
            get => _assignedOutputDevice;
            set { _assignedOutputDevice = value; OnPropertyChanged(); }
        }

        public double DbLevel => _isMuted ? -99.0 : (_volume - 100.0) * 0.4;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Vst3EffectSlot : INotifyPropertyChanged
    {
        private string _pluginName = "vMix 3-Band Parametric EQ";
        private bool _isBypassed = false;
        private double _wetDryMix = 1.0; // 0 to 1
        private double _param1 = 0.0; // Gain / Threshold
        private double _param2 = 1000.0; // Freq / Ratio
        private double _param3 = 1.0; // Q / Release

        public string PluginName
        {
            get => _pluginName;
            set { _pluginName = value; OnPropertyChanged(); }
        }

        public bool IsBypassed
        {
            get => _isBypassed;
            set { _isBypassed = value; OnPropertyChanged(); }
        }

        public double WetDryMix
        {
            get => _wetDryMix;
            set { _wetDryMix = Math.Clamp(value, 0.0, 1.0); OnPropertyChanged(); }
        }

        public double Param1
        {
            get => _param1;
            set { _param1 = value; OnPropertyChanged(); }
        }

        public double Param2
        {
            get => _param2;
            set { _param2 = value; OnPropertyChanged(); }
        }

        public double Param3
        {
            get => _param3;
            set { _param3 = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
