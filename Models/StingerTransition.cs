using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Openmiks.Models
{
    public class StingerTransition : INotifyPropertyChanged
    {
        private int _stingerNumber = 1;
        private string _name = "Stinger 1";
        private int _assignedInputNumber = 6;
        private int _cutPointMs = 500; // Time when underlying video cuts
        private int _durationMs = 1200;

        public int StingerNumber
        {
            get => _stingerNumber;
            set { _stingerNumber = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int AssignedInputNumber
        {
            get => _assignedInputNumber;
            set { _assignedInputNumber = value; OnPropertyChanged(); }
        }

        public int CutPointMs
        {
            get => _cutPointMs;
            set { _cutPointMs = Math.Max(50, value); OnPropertyChanged(); }
        }

        public int DurationMs
        {
            get => _durationMs;
            set { _durationMs = Math.Max(100, value); OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
