using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using TaskState = Microsoft.Win32.TaskScheduler.TaskState;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.Services;

namespace ModernTaskScheduler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ITaskSchedulerService _taskService;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly Microsoft.UI.Xaml.DispatcherTimer _refreshTimer;
        private readonly List<TaskItemViewModel> _allLoadedTasks = new();

        [ObservableProperty]
        private ObservableCollection<TaskFolderItem> _folders = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanDeleteSelectedFolder))]
        private TaskFolderItem? _selectedFolder;

        public bool CanDeleteSelectedFolder => SelectedFolder != null && SelectedFolder.Path != @"\" && !string.IsNullOrWhiteSpace(SelectedFolder.Path);

        [ObservableProperty]
        private ObservableCollection<TaskItemViewModel> _filteredTasks = new();

        [ObservableProperty]
        private TaskItemViewModel? _selectedTask;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "Toutes";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LoadingVisibility))]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = "Chargement des tâches...";

        [ObservableProperty]
        private string _statusMessage = "Prêt";

        [ObservableProperty]
        private int _totalTasksCount;

        [ObservableProperty]
        private int _runningTasksCount;

        [ObservableProperty]
        private int _readyTasksCount;

        [ObservableProperty]
        private int _disabledTasksCount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TaskDetailsVisibility))]
        [NotifyPropertyChangedFor(nameof(EmptyStateVisibility))]
        private bool _hasSelectedTask;

        public Microsoft.UI.Xaml.Visibility LoadingVisibility => IsLoading ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        public Microsoft.UI.Xaml.Visibility TaskDetailsVisibility => HasSelectedTask ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        public Microsoft.UI.Xaml.Visibility EmptyStateVisibility => HasSelectedTask ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

        // InfoBar properties
        [ObservableProperty]
        private bool _isInfoBarOpen;

        [ObservableProperty]
        private string _infoBarTitle = string.Empty;

        [ObservableProperty]
        private string _infoBarMessage = string.Empty;

        [ObservableProperty]
        private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;

        // Delegates for UI dialog coordination
        public Func<TaskCreationModel, Task<bool>>? ShowCreateDialogAsync { get; set; }
        public Func<TaskItemViewModel, Task<bool>>? ShowEditDialogAsync { get; set; }
        public Func<TaskItemViewModel, Task<bool>>? ConfirmDeleteAsync { get; set; }
        public Func<string, Task<string?>>? ShowCreateFolderDialogAsync { get; set; }
        public Func<string, Task<bool>>? ConfirmDeleteFolderAsync { get; set; }
        public Action<string>? CopyToClipboardAction { get; set; }

        public MainViewModel(ITaskSchedulerService taskService, DispatcherQueue dispatcherQueue)
        {
            _taskService = taskService;
            _dispatcherQueue = dispatcherQueue;

            // Real-time live status timer (polls every 3 seconds)
            _refreshTimer = new Microsoft.UI.Xaml.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += OnRealTimeTick;
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            LoadingMessage = "Connexion au service du Planificateur de tâches...";

            try
            {
                await ReloadFoldersAsync(@"\");
                _refreshTimer.Start();
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur de connexion", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ReloadFoldersAsync(string? pathToSelect = null)
        {
            var folderList = await _taskService.GetFoldersFlatAsync();
            Folders.Clear();
            foreach (var f in folderList)
            {
                Folders.Add(f);
            }

            if (Folders.Count > 0)
            {
                var target = !string.IsNullOrEmpty(pathToSelect)
                    ? Folders.FirstOrDefault(f => f.Path.Equals(pathToSelect, StringComparison.OrdinalIgnoreCase)) ?? Folders[0]
                    : Folders[0];

                SelectedFolder = target;
                await LoadTasksForFolderAsync(target.Path);
            }
        }

        partial void OnSelectedFolderChanged(TaskFolderItem? value)
        {
            if (value != null)
            {
                _ = LoadTasksForFolderAsync(value.Path);
            }
        }

        partial void OnSelectedTaskChanged(TaskItemViewModel? value)
        {
            HasSelectedTask = value != null;
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnSelectedFilterChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            IsLoading = true;
            LoadingMessage = "Actualisation des tâches et dossiers...";

            try
            {
                string currentPath = SelectedFolder?.Path ?? @"\";
                await ReloadFoldersAsync(currentPath);
                ShowNotification("Actualisation réussie", "Les dossiers et tâches ont été synchronisés.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur lors de l'actualisation", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadAllTasksAsync()
        {
            IsLoading = true;
            LoadingMessage = "Chargement de toutes les tâches de la bibliothèque...";

            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                UpdateTaskList(tasks);
                StatusMessage = $"Toutes les tâches affichées ({tasks.Count} tâches chargées)";
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadTasksForFolderAsync(string folderPath)
        {
            IsLoading = true;
            LoadingMessage = $"Chargement du dossier '{folderPath}'...";

            try
            {
                var tasks = await _taskService.GetTasksInFolderAsync(folderPath);
                UpdateTaskList(tasks);
                StatusMessage = $"Dossier : {folderPath} ({tasks.Count} tâches)";
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur de lecture du dossier", ex.Message, InfoBarSeverity.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateTaskList(List<TaskItemViewModel> tasks)
        {
            _allLoadedTasks.Clear();
            _allLoadedTasks.AddRange(tasks);

            string? previousSelectedPath = SelectedTask?.Path;

            ApplyFilter();

            if (!string.IsNullOrEmpty(previousSelectedPath))
            {
                SelectedTask = FilteredTasks.FirstOrDefault(t => t.Path == previousSelectedPath);
            }

            UpdateCounts();
        }

        private void ApplyFilter()
        {
            string query = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;
            string filter = SelectedFilter;

            var result = _allLoadedTasks.AsEnumerable();

            if (filter == "En cours")
            {
                result = result.Where(t => t.State == TaskState.Running);
            }
            else if (filter == "Prêtes")
            {
                result = result.Where(t => t.State == TaskState.Ready);
            }
            else if (filter == "Désactivées")
            {
                result = result.Where(t => t.State == TaskState.Disabled);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                result = result.Where(t =>
                    t.Name.ToLowerInvariant().Contains(query) ||
                    t.Description.ToLowerInvariant().Contains(query) ||
                    t.Author.ToLowerInvariant().Contains(query) ||
                    t.Path.ToLowerInvariant().Contains(query));
            }

            FilteredTasks.Clear();
            foreach (var item in result)
            {
                FilteredTasks.Add(item);
            }
        }

        private void UpdateCounts()
        {
            TotalTasksCount = _allLoadedTasks.Count;
            RunningTasksCount = _allLoadedTasks.Count(t => t.State == TaskState.Running);
            ReadyTasksCount = _allLoadedTasks.Count(t => t.State == TaskState.Ready);
            DisabledTasksCount = _allLoadedTasks.Count(t => t.State == TaskState.Disabled);
        }

        [RelayCommand]
        public void SetFilter(string filter)
        {
            SelectedFilter = filter;
        }

        // ================= DOSSIERS CRUD =================

        [RelayCommand]
        public async Task CreateFolderAsync()
        {
            if (ShowCreateFolderDialogAsync == null) return;

            string parentPath = SelectedFolder?.Path ?? @"\";
            string? folderName = await ShowCreateFolderDialogAsync(parentPath);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    await _taskService.CreateFolderAsync(parentPath, folderName.Trim());
                    string newPath = parentPath == @"\" ? $@"\{folderName.Trim()}" : $@"{parentPath}\{folderName.Trim()}";
                    await ReloadFoldersAsync(newPath);
                    ShowNotification("Dossier créé", $"Le dossier '{folderName}' a été créé avec succès.", InfoBarSeverity.Success);
                }
                catch (Exception ex)
                {
                    ShowNotification("Erreur lors de la création du dossier", ex.Message, InfoBarSeverity.Error);
                }
            }
        }

        [RelayCommand]
        public async Task DeleteFolderAsync()
        {
            if (SelectedFolder == null || !CanDeleteSelectedFolder || ConfirmDeleteFolderAsync == null) return;

            string targetPath = SelectedFolder.Path;
            bool confirmed = await ConfirmDeleteFolderAsync(targetPath);
            if (confirmed)
            {
                try
                {
                    await _taskService.DeleteFolderAsync(targetPath);
                    await ReloadFoldersAsync(@"\");
                    ShowNotification("Dossier supprimé", $"Le dossier '{targetPath}' a été supprimé.", InfoBarSeverity.Success);
                }
                catch (Exception ex)
                {
                    ShowNotification("Erreur lors de la suppression du dossier", ex.Message, InfoBarSeverity.Error);
                }
            }
        }

        // ================= TÂCHES CRUD & API =================

        [RelayCommand]
        public async Task RunSelectedTaskAsync()
        {
            if (SelectedTask == null) return;

            var task = SelectedTask;
            task.IsBusy = true;

            try
            {
                await _taskService.RunTaskAsync(task.Path);
                ShowNotification("Tâche exécutée", $"L'ordre d'exécution pour '{task.Name}' a été envoyé avec succès.", InfoBarSeverity.Success);

                await Task.Delay(500);
                await RefreshSingleTaskAsync(task);
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur lors de l'exécution", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                task.IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task StopSelectedTaskAsync()
        {
            if (SelectedTask == null) return;

            var task = SelectedTask;
            task.IsBusy = true;

            try
            {
                await _taskService.StopTaskAsync(task.Path);
                ShowNotification("Tâche arrêtée", $"L'ordre d'arrêt pour '{task.Name}' a été envoyé avec succès.", InfoBarSeverity.Informational);

                await Task.Delay(500);
                await RefreshSingleTaskAsync(task);
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur lors de l'arrêt", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                task.IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ToggleEnabledSelectedTaskAsync()
        {
            if (SelectedTask == null) return;

            var task = SelectedTask;
            task.IsBusy = true;
            bool newEnabled = !task.Enabled;

            try
            {
                await _taskService.SetTaskEnabledAsync(task.Path, newEnabled);
                task.Enabled = newEnabled;

                string actionLabel = newEnabled ? "activée" : "désactivée";
                ShowNotification("État modifié", $"La tâche '{task.Name}' est maintenant {actionLabel}.", InfoBarSeverity.Success);

                await RefreshSingleTaskAsync(task);
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur de modification", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                task.IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteSelectedTaskAsync()
        {
            if (SelectedTask == null) return;

            var task = SelectedTask;

            if (ConfirmDeleteAsync != null)
            {
                bool confirmed = await ConfirmDeleteAsync(task);
                if (!confirmed) return;
            }

            task.IsBusy = true;

            try
            {
                await _taskService.DeleteTaskAsync(task.Path);
                _allLoadedTasks.Remove(task);
                FilteredTasks.Remove(task);
                SelectedTask = null;

                UpdateCounts();
                ShowNotification("Tâche supprimée", $"La tâche '{task.Name}' a été définitivement supprimée.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur de suppression", ex.Message, InfoBarSeverity.Error);
            }
            finally
            {
                task.IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task CreateTaskAsync()
        {
            if (ShowCreateDialogAsync == null) return;

            var model = new TaskCreationModel
            {
                FolderPath = SelectedFolder?.Path ?? @"\"
            };

            bool success = await ShowCreateDialogAsync(model);
            if (success)
            {
                ShowNotification("Tâche créée", $"La tâche '{model.TaskName}' a été créée et enregistrée avec succès.", InfoBarSeverity.Success);
                await LoadTasksForFolderAsync(SelectedFolder?.Path ?? @"\");
            }
        }

        [RelayCommand]
        public async Task EditSelectedTaskAsync()
        {
            if (SelectedTask == null || ShowEditDialogAsync == null) return;

            bool success = await ShowEditDialogAsync(SelectedTask);
            if (success)
            {
                ShowNotification("Tâche modifiée", $"La tâche '{SelectedTask.Name}' a été mise à jour avec succès.", InfoBarSeverity.Success);
                await LoadTasksForFolderAsync(SelectedFolder?.Path ?? @"\");
            }
        }

        [RelayCommand]
        public async Task ExportXmlAsync()
        {
            if (SelectedTask == null) return;

            try
            {
                string xml = await _taskService.ExportTaskXmlAsync(SelectedTask.Path);
                if (!string.IsNullOrEmpty(xml) && CopyToClipboardAction != null)
                {
                    CopyToClipboardAction(xml);
                    ShowNotification("XML copié", "La définition XML a été copiée dans le presse-papiers.", InfoBarSeverity.Success);
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Erreur d'exportation", ex.Message, InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        public void CopySelectedXml()
        {
            if (SelectedTask == null || string.IsNullOrEmpty(SelectedTask.XmlDefinition)) return;
            CopyToClipboardAction?.Invoke(SelectedTask.XmlDefinition);
            ShowNotification("XML copié", "Le XML a été copié dans le presse-papiers.", InfoBarSeverity.Success);
        }

        private async Task RefreshSingleTaskAsync(TaskItemViewModel task)
        {
            var updated = await _taskService.GetTaskByPathAsync(task.Path);
            if (updated != null)
            {
                task.State = updated.State;
                task.Enabled = updated.Enabled;
                task.LastRunTime = updated.LastRunTime;
                task.LastRunTimeDisplay = updated.LastRunTimeDisplay;
                task.NextRunTime = updated.NextRunTime;
                task.NextRunTimeDisplay = updated.NextRunTimeDisplay;
                task.LastTaskResult = updated.LastTaskResult;
                task.LastTaskResultDisplay = updated.LastTaskResultDisplay;
                task.IsRunning = updated.IsRunning;
                task.CanRun = updated.CanRun;
                task.CanStop = updated.CanStop;
                task.StateDisplay = updated.StateDisplay;
                task.StateBadgeColor = updated.StateBadgeColor;
                task.StateBadgeBg = updated.StateBadgeBg;
                task.StateIconGlyph = updated.StateIconGlyph;
            }
            UpdateCounts();
        }

        private async void OnRealTimeTick(object? sender, object e)
        {
            if (_allLoadedTasks.Any(t => t.State == TaskState.Running) || (SelectedTask != null && SelectedTask.State == TaskState.Running))
            {
                var runningTasks = _allLoadedTasks.Where(t => t.State == TaskState.Running).ToList();
                foreach (var taskVm in runningTasks)
                {
                    try
                    {
                        var updated = await _taskService.GetTaskByPathAsync(taskVm.Path);
                        if (updated != null)
                        {
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                taskVm.State = updated.State;
                                taskVm.IsRunning = updated.IsRunning;
                                taskVm.CanRun = updated.CanRun;
                                taskVm.CanStop = updated.CanStop;
                                taskVm.LastRunTimeDisplay = updated.LastRunTimeDisplay;
                                taskVm.NextRunTimeDisplay = updated.NextRunTimeDisplay;
                                taskVm.LastTaskResultDisplay = updated.LastTaskResultDisplay;
                                taskVm.StateDisplay = updated.StateDisplay;
                                taskVm.StateBadgeColor = updated.StateBadgeColor;
                                taskVm.StateBadgeBg = updated.StateBadgeBg;
                                taskVm.StateIconGlyph = updated.StateIconGlyph;
                            });
                        }
                    }
                    catch { }
                }

                _dispatcherQueue.TryEnqueue(UpdateCounts);
            }
        }

        public void ShowNotification(string title, string message, InfoBarSeverity severity)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                InfoBarTitle = title;
                InfoBarMessage = message;
                InfoBarSeverity = severity;
                IsInfoBarOpen = true;
            });
        }
    }
}
