using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ModernTaskScheduler.Localization;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.Services;
using ModernTaskScheduler.ViewModels;
using ModernTaskScheduler.Views;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using WinRT.Interop;

namespace ModernTaskScheduler
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        public LocalizationService Strings => LocalizationService.Instance;
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
                root.DataContext = Strings;
                root.Loaded += async (s, e) =>
                {
                    LanguageSelector.SelectedValue = Strings.Language;
                    await ViewModel.InitializeAsync();
                };
            }
        }

        private async void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is not ComboBoxItem item || item.Tag is not string language || language == Strings.Language)
            {
                return;
            }

            try
            {
                Strings.SetLanguage(language);
                await ViewModel.ReloadFoldersAsync(ViewModel.SelectedFolder?.Path);
            }
            catch (Exception ex)
            {
                LanguageSelector.SelectedValue = Strings.Language;
                ViewModel.ShowNotification("Erreur de langue", ex.Message, InfoBarSeverity.Error);
            }
        }

        private void ConfigureWindowSizeAndTitle()
        {
            Title = Strings.WindowTitle;

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(1400, 800));
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
                Title = Strings.Translate("Confirmer la suppression"),
                Content = Strings.Format(
                    "Êtes-vous certain de vouloir supprimer définitivement la tâche planifiée suivante ?\n\nNom : {0}\nEmplacement : {1}\n\nCette action est irréversible.",
                    "Are you sure you want to permanently delete this scheduled task?\n\nName: {0}\nLocation: {1}\n\nThis action cannot be undone.",
                    task.Name,
                    task.Path),
                PrimaryButtonText = Strings.Translate("Supprimer définitivement"),
                CloseButtonText = Strings.Translate("Annuler"),
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
                PlaceholderText = Strings.Translate("Ex: MesTaches"),
                Width = 320
            };

            var stack = new StackPanel { Spacing = 10 };
            stack.Children.Add(new TextBlock
            {
                Text = Strings.Format("Dossier parent : {0}", "Parent folder: {0}", parentPath),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });
            stack.Children.Add(inputTextBox);

            var dialog = new ContentDialog
            {
                Title = Strings.Translate("Créer un nouveau dossier"),
                Content = stack,
                PrimaryButtonText = Strings.Translate("Créer le dossier"),
                CloseButtonText = Strings.Translate("Annuler"),
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
                Title = Strings.Translate("Confirmer la suppression du dossier"),
                Content = Strings.Format(
                    "Êtes-vous certain de vouloir supprimer le dossier suivant et tout son contenu éventuel ?\n\nChemin : {0}\n\nCette action est irréversible.",
                    "Are you sure you want to delete this folder and any contents?\n\nPath: {0}\n\nThis action cannot be undone.",
                    folderPath),
                PrimaryButtonText = Strings.Translate("Supprimer définitivement"),
                CloseButtonText = Strings.Translate("Annuler"),
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
            ViewModel.SetFilter("All");
        }

        private void FilterRunning_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Running");
        }

        private void FilterReady_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Ready");
        }

        private void FilterDisabled_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetFilter("Disabled");
        }

        private void ToggleEnabled_Click(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.ToggleEnabledSelectedTaskAsync();
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = new StackPanel
            {
                Spacing = 16,
                Padding = new Thickness(20)
            };

            var titleBlock = new TextBlock 
            { 
                Text = "Modern Task Scheduler", 
                FontSize = 20
            };
            stackPanel.Children.Add(titleBlock);

            var versionBlock = new TextBlock { Text = Strings.Translate("Version 1.1.0") };
            stackPanel.Children.Add(versionBlock);

            var descBlock = new TextBlock 
            { 
                Text = Strings.Translate("A modern, feature-rich Windows application for managing system tasks built with WinUI and .NET 8."),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(descBlock);

            var copyrightBlock = new TextBlock 
            { 
                Text = Strings.Translate("© 2024 BubbleWrapPro. All rights reserved."),
                FontSize = 12
            };
            stackPanel.Children.Add(copyrightBlock);

            var dialog = new ContentDialog
            {
                Title = Strings.Translate("About Modern Task Scheduler"),
                Content = stackPanel,
                CloseButtonText = Strings.Translate("Close"),
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
