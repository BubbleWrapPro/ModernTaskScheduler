using System.Collections.Generic;
using System.Threading.Tasks;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.ViewModels;

namespace ModernTaskScheduler.Services
{
    public interface ITaskSchedulerService
    {
        Task<List<TaskFolderItem>> GetFoldersFlatAsync();
        Task<List<TaskItemViewModel>> GetTasksInFolderAsync(string folderPath);
        Task<List<TaskItemViewModel>> GetAllTasksAsync();
        Task<TaskItemViewModel?> GetTaskByPathAsync(string taskPath);
        Task RunTaskAsync(string taskPath);
        Task StopTaskAsync(string taskPath);
        Task SetTaskEnabledAsync(string taskPath, bool enabled);
        Task DeleteTaskAsync(string taskPath);
        Task CreateTaskAsync(TaskCreationModel model);
        Task UpdateTaskAsync(string originalPath, TaskCreationModel model);
        Task<string> ExportTaskXmlAsync(string taskPath);
        Task CreateFolderAsync(string parentPath, string folderName);
        Task DeleteFolderAsync(string folderPath);
    }
}
