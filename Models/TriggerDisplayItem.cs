using CommunityToolkit.Mvvm.ComponentModel;
using ModernTaskScheduler.Localization;

namespace ModernTaskScheduler.Models
{
    public partial class TriggerDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _triggerType = string.Empty;

        public string TriggerTypeId { get; set; } = string.Empty;

        [ObservableProperty]
        private string _details = string.Empty;

        [ObservableProperty]
        private bool _enabled = true;

        [ObservableProperty]
        private string _statusDisplay = "Enabled";

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
            StatusDisplay = LocalizationService.Instance.Translate(value ? "Activé" : "Désactivé");
        }
    }
}
