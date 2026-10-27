using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernTaskScheduler.Models
{
    public partial class ActionDisplayItem : ObservableObject
    {
        [ObservableProperty]
        private string _actionType = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private string _arguments = string.Empty;

        [ObservableProperty]
        private string _workingDirectory = string.Empty;

        [ObservableProperty]
        private string _summary = string.Empty;

        [ObservableProperty]
        private string _iconGlyph = "\uE756"; // Command / Executable icon
    }
}
