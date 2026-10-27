using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernTaskScheduler.Models
{
    public partial class TaskFolderItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = @"\";

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private int _taskCount;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        public ObservableCollection<TaskFolderItem> SubFolders { get; set; } = new();

        public override string ToString() => DisplayName;
    }
}
