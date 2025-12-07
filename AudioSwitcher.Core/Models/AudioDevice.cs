using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioSwitcher.Core.Models
{
    public class AudioDevice : INotifyPropertyChanged
    {
        private bool _isDefault;
        private bool _isFavorite;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        
        public bool IsDefault 
        { 
            get => _isDefault; 
            set { if (_isDefault != value) { _isDefault = value; OnPropertyChanged(); } } 
        }
        
        public string IconPath { get; set; } = string.Empty; 
        
        public bool IsFavorite 
        { 
            get => _isFavorite; 
            set { if (_isFavorite != value) { _isFavorite = value; OnPropertyChanged(); } } 
        } 
          
        public bool IsDefaultComms { get; set; }
        
        public string DisplayName 
        { 
            get 
            {
                var match = System.Text.RegularExpressions.Regex.Match(Name, @"^(.*)\s\((.*)\)$");
                if (match.Success) return match.Groups[1].Value;
                return Name;
            } 
        }

        public string DisplaySubName 
        { 
            get 
            {
                var match = System.Text.RegularExpressions.Regex.Match(Name, @"^(.*)\s\((.*)\)$");
                if (match.Success) return match.Groups[2].Value;
                return string.Empty;
            } 
        }

        public bool HasSubName => !string.IsNullOrEmpty(DisplaySubName);
        
        public override string ToString() => Name;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
