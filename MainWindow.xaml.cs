using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.Services;
using ModernTaskScheduler.ViewModels;
using ModernTaskScheduler.Views;
using Windows.ApplicationModel.DataTransfer;
using WinRT.Interop;

namespace ModernTaskScheduler
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        private readonly ITaskSchedulerService _taskService;
        private readonly IntPtr _hwnd;

        public MainWindow()
        {
            this.InitializeComponent();

            _hwnd = WindowNative.GetWindowHandle(this);
            ConfigureWindowSizeAndTitle();

            _taskService = new TaskSchedulerService();
            ViewModel = new MainViewModel(_taskService, this.DispatcherQueue);

            // Configure UI dialog delegates
            ViewModel.ShowCreateDialogAsync = ShowCreateDialogImplAsync;
            ViewModel.ShowEditDialogAsync = ShowEditDialogImplAsync;
            ViewModel.ConfirmDeleteAsync = ConfirmDeleteImplAsync;
            ViewModel.ShowCreateFolderDialogAsync = ShowCreateFolderDialogImplAsync;
            ViewModel.ConfirmDeleteFolderAsync = ConfirmDeleteFolderImplAsync;
            ViewModel.CopyToClipboardAction = CopyToClipboardImpl;

            // Load data when loaded
            if (this.Content is FrameworkElement root)
            {
                root.Loaded += async (s, e) =>
                {
                    await ViewModel.InitializeAsync();
                };
            }
        }

        private void ConfigureWindowSizeAndTitle()
        {
            Title = "Planificateur de tâches Windows (Fluent)";

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(1280, 820));
            }
        }

        private async Task<bool> ShowCreateDialogImplAsync(TaskCreationModel initialModel)
        {
            var tcs = new TaskCompletionSource<bool>();

            var editVm = new TaskEditViewModel
            {
                FolderPath = initialModel.FolderPath
            };

            var win = new TaskCreateEditWindow(editVm, ViewModel.Folders);
            win.TaskSaved += async (s, model) =>
            {
                try
                {
                    await _taskService.CreateTaskAsync(model);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    ViewModel.ShowNotification("Erreur lors de la création", ex.Message, InfoBarSeverity.Error);
                    tcs.TrySetResult(false);
                }
            };

            win.Closed += (s, e) =>
            {
                tcs.TrySetResult(false);
            };

            win.Activate();
            return await tcs.Task;
        }

        private async Task<bool> ShowEditDialogImplAsync(TaskItemViewModel task)
        {
            var tcs = new TaskCompletionSource<bool>();

            var editVm = new TaskEditViewModel();
            editVm.LoadFromTask(task);

            var win = new TaskCreateEditWindow(editVm, ViewModel.Folders);
            win.TaskSaved += async (s, model) =>
            {
                try
                {
                    await _taskService.UpdateTaskAsync(task.Path, model);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    ViewModel.ShowNotification("Erreur lors de la modification", ex.Message, InfoBarSeverity.Error);
                    tcs.TrySetResult(false);
                }
            };

            win.Closed += (s, e) =>
            {
                tcs.TrySetResult(false);
            };

            win.Activate();
            return await tcs.Task;
        }

        private async Task<bool> ConfirmDeleteImplAsync(TaskItemViewModel task)
        {
            if (this.Content?.XamlRoot == null) return false;

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmer la suppression",
                Content = $"Êtes-vous certain de vouloir supprimer définitivement la tâche planifiée suivante ?\n\nNom : {task.Name}\nEmplacement : {task.Path}\n\nCette action est irréversible.",
                PrimaryButtonText = "Supprimer définitivement",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        private async Task<string?> ShowCreateFolderDialogImplAsync(string parentPath)
        {
            if (this.Content?.XamlRoot == null) return null;

            var inputTextBox = new TextBox
            {
                PlaceholderText = "Ex: MesTaches",
                Width = 320
            };

            var stack = new StackPanel { Spacing = 10 };
            stack.Children.Add(new TextBlock
            {
                Text = $"Dossier parent : {parentPath}",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            stack.Children.Add(inputTextBox);

            var dialog = new ContentDialog
            {
                Title = "Créer un nouveau dossier",
                Content = stack,
                PrimaryButtonText = "Créer le dossier",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary ? inputTextBox.Text.Trim() : null;
        }

        private async Task<bool> ConfirmDeleteFolderImplAsync(string folderPath)
        {
            if (this.Content?.XamlRoot == null) return false;

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmer la suppression du dossier",
                Content = $"Êtes-vous certain de vouloir supprimer le dossier suivant et tout son contenu éventuel ?\n\nChemin : {folderPath}\n\nCette action est irréversible.",
                PrimaryButtonText = "Supprimer définitivement",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        private void CopyToClipboardImpl(string text)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(text);
                Clipboard.SetContent(package);
            }
            catch { }
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Toutes");
        }

        private void FilterRunning_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("En cours");
        }

        private void FilterReady_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Prêtes");
        }

        private void FilterDisabled_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Désactivées");
        }

        private void ToggleEnabled_Click(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.ToggleEnabledSelectedTaskAsync();
        }
    }
}
