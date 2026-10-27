using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.ViewModels;
using WinRT.Interop;

namespace ModernTaskScheduler.Views
{
    public sealed partial class TaskCreateEditWindow : Window
    {
        public TaskEditViewModel ViewModel { get; }
        private readonly IntPtr _hwnd;
        public event EventHandler<TaskCreationModel>? TaskSaved;

        public TaskCreateEditWindow(TaskEditViewModel viewModel, IEnumerable<TaskFolderItem> availableFolders)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            _hwnd = WindowNative.GetWindowHandle(this);

            ConfigureWindow();

            // Populate available folders
            ViewModel.AvailableFolders.Clear();
            foreach (var f in availableFolders)
            {
                ViewModel.AvailableFolders.Add(f);
            }

            // Ensure folder path selection exists
            if (string.IsNullOrEmpty(ViewModel.FolderPath) && ViewModel.AvailableFolders.Count > 0)
            {
                ViewModel.FolderPath = ViewModel.AvailableFolders[0].Path;
            }
        }

        private void ConfigureWindow()
        {
            Title = ViewModel.DialogTitle;

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            if (appWindow != null)
            {
                appWindow.Resize(new Windows.Graphics.SizeInt32(960, 820));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Validate())
            {
                return;
            }

            var model = ViewModel.ToCreationModel();
            TaskSaved?.Invoke(this, model);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                picker.FileTypeFilter.Add("*");
                picker.FileTypeFilter.Add(".exe");
                picker.FileTypeFilter.Add(".bat");
                picker.FileTypeFilter.Add(".cmd");
                picker.FileTypeFilter.Add(".ps1");
                picker.FileTypeFilter.Add(".vbs");

                if (_hwnd != IntPtr.Zero)
                {
                    InitializeWithWindow.Initialize(picker, _hwnd);
                }

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    ViewModel.ActionProgram = file.Path;
                    if (string.IsNullOrEmpty(ViewModel.ActionWorkingDirectory))
                    {
                        ViewModel.ActionWorkingDirectory = Path.GetDirectoryName(file.Path) ?? string.Empty;
                    }
                }
            }
            catch
            {
                // Fallback or user canceled
            }
        }
    }
}
