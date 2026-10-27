using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernTaskScheduler.Models
{
    public partial class ConditionDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private string _iconGlyph = "\uE946"; // Info/Settings icon
    }
}
