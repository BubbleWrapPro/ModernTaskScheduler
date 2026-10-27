using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernTaskScheduler.Models
{
    public partial class SettingDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _value = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _iconGlyph = "\uE713"; // Settings icon
    }
}
