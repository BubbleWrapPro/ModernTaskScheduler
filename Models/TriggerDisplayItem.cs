using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernTaskScheduler.Models
{
    public partial class TriggerDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _triggerType = string.Empty;

        [ObservableProperty]
        private string _details = string.Empty;

        [ObservableProperty]
        private bool _enabled = true;

        [ObservableProperty]
        private string _statusDisplay = "Activé";

        [ObservableProperty]
        private string _startBoundary = string.Empty;

        [ObservableProperty]
        private string _endBoundary = string.Empty;

        [ObservableProperty]
        private string _repetition = string.Empty;

        [ObservableProperty]
        private string _iconGlyph = "\uE823"; // Calendar icon

        partial void OnEnabledChanged(bool value)
        {
            StatusDisplay = value ? "Activé" : "Désactivé";
        }
    }
}
