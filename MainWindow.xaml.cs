using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ModernTaskScheduler
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // 1. On déclare le TaskService au niveau de la classe
        private TaskService ts = new TaskService();

        public MainWindow()
        {
            this.InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            // 2. On utilise la connexion qui est maintenant persistante
            if (ts.Connected)
            {
                TaskListView.ItemsSource = ts.AllTasks;
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskListView.SelectedItem is Task selectedTask)
            {
                TaskNameText.Text = selectedTask.Name;
                TaskStateText.Text = $"État : {selectedTask.State}";
                TaskNextRunText.Text = "Prochaine exécution : " + (selectedTask.NextRunTime == DateTime.MinValue ? "Aucune" : selectedTask.NextRunTime.ToString());
            }
        }
    }
}
