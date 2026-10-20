using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Openmiks.Models;

namespace Openmiks.Services
{
    public class AudioMixerEngine : INotifyPropertyChanged
    {
        public ObservableCollection<AudioBus> Busses { get; } = new();
        public AudioBus MasterBus { get; }
        public AudioBus BusA { get; }
        public AudioBus BusB { get; }
        public AudioBus BusC { get; }
        public AudioBus BusD { get; }
        public AudioBus BusE { get; }
        public AudioBus BusF { get; }
        public AudioBus BusG { get; }

        private double _headphoneVolume = 80.0;
        private bool _isHeadphoneSolo = false;

        public double HeadphoneVolume
        {
            get => _headphoneVolume;
            set { _headphoneVolume = Math.Clamp(value, 0.0, 100.0); OnPropertyChanged(); }
        }

        public bool IsHeadphoneSolo
        {
            get => _isHeadphoneSolo;
            set { _isHeadphoneSolo = value; OnPropertyChanged(); }
        }

        public AudioMixerEngine()
        {
            MasterBus = new AudioBus { Name = "Master", Tag = "M", Volume = 85 };
            BusA = new AudioBus { Name = "Bus A", Tag = "A", Volume = 80 };
            BusB = new AudioBus { Name = "Bus B", Tag = "B", Volume = 80 };
            BusC = new AudioBus { Name = "Bus C", Tag = "C", Volume = 80 };
            BusD = new AudioBus { Name = "Bus D", Tag = "D", Volume = 80 };
            BusE = new AudioBus { Name = "Bus E", Tag = "E", Volume = 80 };
            BusF = new AudioBus { Name = "Bus F", Tag = "F", Volume = 80 };
            BusG = new AudioBus { Name = "Bus G", Tag = "G", Volume = 80 };

            Busses.Add(MasterBus);
            Busses.Add(BusA);
            Busses.Add(BusB);
            Busses.Add(BusC);
            Busses.Add(BusD);
            Busses.Add(BusE);
            Busses.Add(BusF);
            Busses.Add(BusG);
        }

        public AudioBus? GetBus(string tag)
        {
            return Busses.FirstOrDefault(b => b.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase) ||
                                              b.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public void SetBusVolume(string tag, double volume)
        {
            var bus = GetBus(tag);
            if (bus != null) bus.Volume = volume;
        }

        public void ToggleBusMute(string tag)
        {
            var bus = GetBus(tag);
            if (bus != null) bus.IsMuted = !bus.IsMuted;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
