using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TaskScheduler = Microsoft.Win32.TaskScheduler;
using ModernTaskScheduler.Models;
using ModernTaskScheduler.ViewModels;

namespace ModernTaskScheduler.Services
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        public Task<List<TaskFolderItem>> GetFoldersFlatAsync()
        {
            return StaTask.Run(() =>
            {
                var list = new List<TaskFolderItem>();
                try
                {
                    using var ts = new TaskScheduler.TaskService();
                    if (!ts.Connected) return list;

                    void Traverse(TaskScheduler.TaskFolder folder, int depth)
                    {
                        string name = string.IsNullOrEmpty(folder.Name) ? @"\" : folder.Name;
                        string path = string.IsNullOrEmpty(folder.Path) ? @"\" : folder.Path;
                        int count = 0;
                        try { count = folder.Tasks.Count; } catch { }

                        string indent = depth > 0 ? new string(' ', (depth - 1) * 4) + "└─ " : string.Empty;
                        string display = depth == 0
                            ? @"📁 \ (Bibliothèque du Planificateur)"
                            : $"{indent}📁 {name} ({count})";

                        list.Add(new TaskFolderItem
                        {
                            Name = name,
                            Path = path,
                            DisplayName = display,
                            TaskCount = count
                        });

                        try
                        {
                            foreach (var sub in folder.SubFolders)
                            {
                                try
                                {
                                    Traverse(sub, depth + 1);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    Traverse(ts.RootFolder, 0);
                }
                catch { }

                return list;
            });
        }

        public Task<List<TaskItemViewModel>> GetTasksInFolderAsync(string folderPath)
        {
            return StaTask.Run(() =>
            {
                var list = new List<TaskItemViewModel>();
                try
                {
                    using var ts = new TaskScheduler.TaskService();
                    if (!ts.Connected) return list;

                    string normalizedPath = string.IsNullOrWhiteSpace(folderPath) ? @"\" : folderPath;
                    var folder = ts.GetFolder(normalizedPath) ?? ts.RootFolder;

                    foreach (TaskScheduler.Task t in folder.Tasks)
                    {
                        try
                        {
                            list.Add(new TaskItemViewModel(t));
                        }
                        catch { }
                    }
                }
                catch { }

                return list;
            });
        }

        public Task<List<TaskItemViewModel>> GetAllTasksAsync()
        {
            return StaTask.Run(() =>
            {
                var list = new List<TaskItemViewModel>();
                try
                {
                    using var ts = new TaskScheduler.TaskService();
                    if (!ts.Connected) return list;
                    CollectTasksRecursively(ts.RootFolder, list);
                }
                catch { }

                return list;
            });
        }

        private void CollectTasksRecursively(TaskScheduler.TaskFolder folder, List<TaskItemViewModel> list)
        {
            try
            {
                foreach (TaskScheduler.Task t in folder.Tasks)
                {
                    try
                    {
                        list.Add(new TaskItemViewModel(t));
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                foreach (var sub in folder.SubFolders)
                {
                    CollectTasksRecursively(sub, list);
                }
            }
            catch { }
        }

        public Task<TaskItemViewModel?> GetTaskByPathAsync(string taskPath)
        {
            return StaTask.Run(() =>
            {
                try
                {
                    using var ts = new TaskScheduler.TaskService();
                    if (!ts.Connected) return null;
                    TaskScheduler.Task? t = ts.GetTask(taskPath);
                    return t != null ? new TaskItemViewModel(t) : null;
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task RunTaskAsync(string taskPath)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");
                TaskScheduler.Task? task = ts.GetTask(taskPath) ?? throw new FileNotFoundException($"La tâche '{taskPath}' est introuvable.");
                task.Run();
            });
        }

        public Task StopTaskAsync(string taskPath)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");
                TaskScheduler.Task? task = ts.GetTask(taskPath) ?? throw new FileNotFoundException($"La tâche '{taskPath}' est introuvable.");
                task.Stop();
            });
        }

        public Task SetTaskEnabledAsync(string taskPath, bool enabled)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");
                TaskScheduler.Task? task = ts.GetTask(taskPath) ?? throw new FileNotFoundException($"La tâche '{taskPath}' est introuvable.");
                task.Enabled = enabled;
            });
        }

        public Task DeleteTaskAsync(string taskPath)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");

                int lastSlash = taskPath.LastIndexOf('\\');
                string folderPath = lastSlash > 0 ? taskPath.Substring(0, lastSlash) : @"\";
                string taskName = lastSlash >= 0 ? taskPath.Substring(lastSlash + 1) : taskPath;

                var folder = ts.GetFolder(folderPath) ?? ts.RootFolder;
                folder.DeleteTask(taskName, false);
            });
        }

        public Task CreateFolderAsync(string parentPath, string folderName)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");

                string normalizedParent = string.IsNullOrWhiteSpace(parentPath) ? @"\" : parentPath;
                var parent = ts.GetFolder(normalizedParent) ?? ts.RootFolder;
                parent.CreateFolder(folderName);
            });
        }

        public Task DeleteFolderAsync(string folderPath)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");

                if (string.IsNullOrWhiteSpace(folderPath) || folderPath == @"\")
                {
                    throw new InvalidOperationException("Impossible de supprimer le dossier racine.");
                }

                int lastSlash = folderPath.LastIndexOf('\\');
                string parentPath = lastSlash > 0 ? folderPath.Substring(0, lastSlash) : @"\";
                string folderName = lastSlash >= 0 ? folderPath.Substring(lastSlash + 1) : folderPath;

                var parent = ts.GetFolder(parentPath) ?? ts.RootFolder;
                parent.DeleteFolder(folderName, false);
            });
        }

        public Task CreateTaskAsync(TaskCreationModel model)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");

                string folderPath = string.IsNullOrWhiteSpace(model.FolderPath) ? @"\" : model.FolderPath;
                var folder = ts.GetFolder(folderPath);
                if (folder == null)
                {
                    folder = ts.RootFolder.CreateFolder(folderPath.TrimStart('\\'));
                }

                var td = ts.NewTask();

                ApplyModelToDefinition(td, model);

                folder.RegisterTaskDefinition(model.TaskName, td, TaskScheduler.TaskCreation.CreateOrUpdate, null, null, td.Principal.LogonType);
            });
        }

        public Task UpdateTaskAsync(string originalPath, TaskCreationModel model)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) throw new InvalidOperationException("Service de planification non connecté.");

                int lastSlash = originalPath.LastIndexOf('\\');
                string folderPath = lastSlash > 0 ? originalPath.Substring(0, lastSlash) : @"\";
                var folder = ts.GetFolder(folderPath) ?? ts.RootFolder;

                var existingTask = ts.GetTask(originalPath);
                var td = existingTask != null ? existingTask.Definition : ts.NewTask();

                ApplyModelToDefinition(td, model);

                folder.RegisterTaskDefinition(model.TaskName, td, TaskScheduler.TaskCreation.CreateOrUpdate, null, null, td.Principal.LogonType);
            });
        }

        private static void ApplyModelToDefinition(TaskScheduler.TaskDefinition td, TaskCreationModel model)
        {
            // Registration Info
            td.RegistrationInfo.Description = model.Description;
            td.RegistrationInfo.Author = string.IsNullOrWhiteSpace(model.Author) ? Environment.UserName : model.Author;

            // Principal & Security
            td.Principal.RunLevel = model.RunLevel;
            if (!string.IsNullOrWhiteSpace(model.UserAccount))
            {
                td.Principal.UserId = model.UserAccount;
            }

            if (model.RunWhetherUserLoggedOn)
            {
                td.Principal.LogonType = model.DoNotStorePassword
                    ? TaskScheduler.TaskLogonType.S4U
                    : TaskScheduler.TaskLogonType.Password;
            }
            else
            {
                td.Principal.LogonType = TaskScheduler.TaskLogonType.InteractiveToken;
            }

            // Action
            td.Actions.Clear();
            if (!string.IsNullOrWhiteSpace(model.ActionProgram))
            {
                var exec = new TaskScheduler.ExecAction(model.ActionProgram, model.ActionArguments, model.ActionWorkingDirectory);
                td.Actions.Add(exec);
            }

            // Trigger
            td.Triggers.Clear();
            ApplyTrigger(td, model);

            // Idle Conditions
            td.Settings.RunOnlyIfIdle = model.RunOnlyIfIdle;
            if (model.RunOnlyIfIdle)
            {
                td.Settings.IdleSettings.IdleDuration = TimeSpan.FromMinutes(model.IdleDurationMinutes);
                td.Settings.IdleSettings.WaitTimeout = TimeSpan.FromMinutes(model.IdleWaitTimeoutMinutes);
                td.Settings.IdleSettings.StopOnIdleEnd = model.StopOnIdleEnd;
                td.Settings.IdleSettings.RestartOnIdle = model.RestartOnIdle;
            }

            // Power Conditions
            td.Settings.DisallowStartIfOnBatteries = model.DisallowStartIfOnBatteries;
            td.Settings.StopIfGoingOnBatteries = model.StopIfGoingOnBatteries;
            td.Settings.WakeToRun = model.WakeToRun;

            // Network Conditions
            td.Settings.RunOnlyIfNetworkAvailable = model.RunOnlyIfNetworkAvailable;

            // Advanced Settings
            td.Settings.AllowDemandStart = model.AllowDemandStart;
            td.Settings.StartWhenAvailable = model.StartWhenAvailable;
            td.Settings.AllowHardTerminate = model.AllowHardTerminate;
            td.Settings.Hidden = model.Hidden;
            td.Settings.MultipleInstances = model.MultipleInstances;
            td.Settings.Compatibility = model.Compatibility;

            if (model.RestartOnFailure)
            {
                td.Settings.RestartCount = model.RestartCount;
                td.Settings.RestartInterval = TimeSpan.FromMinutes(model.RestartIntervalMinutes);
            }

            if (model.ExecutionTimeLimitHours > 0)
            {
                td.Settings.ExecutionTimeLimit = TimeSpan.FromHours(model.ExecutionTimeLimitHours);
            }
            else
            {
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            }

            if (model.DeleteExpiredTaskAfterDays > 0)
            {
                td.Settings.DeleteExpiredTaskAfter = TimeSpan.FromDays(model.DeleteExpiredTaskAfterDays);
            }
        }

        private static void ApplyTrigger(TaskScheduler.TaskDefinition td, TaskCreationModel model)
        {
            DateTime startBoundary = model.TriggerStartDate.Date + model.TriggerStartTime;
            TaskScheduler.Trigger trig;

            switch (model.TriggerType)
            {
                case "Once":
                case "Time":
                    trig = new TaskScheduler.TimeTrigger { StartBoundary = startBoundary };
                    break;
                case "Daily":
                    trig = new TaskScheduler.DailyTrigger { StartBoundary = startBoundary, DaysInterval = 1 };
                    break;
                case "Weekly":
                    trig = new TaskScheduler.WeeklyTrigger { StartBoundary = startBoundary, DaysOfWeek = TaskScheduler.DaysOfTheWeek.AllDays, WeeksInterval = 1 };
                    break;
                case "Monthly":
                    trig = new TaskScheduler.MonthlyTrigger { StartBoundary = startBoundary, MonthsOfYear = TaskScheduler.MonthsOfTheYear.AllMonths };
                    break;
                case "Logon":
                    var lt = new TaskScheduler.LogonTrigger();
                    if (model.TriggerDelaySeconds > 0) lt.Delay = TimeSpan.FromSeconds(model.TriggerDelaySeconds);
                    trig = lt;
                    break;
                case "Boot":
                    var bt = new TaskScheduler.BootTrigger();
                    if (model.TriggerDelaySeconds > 0) bt.Delay = TimeSpan.FromSeconds(model.TriggerDelaySeconds);
                    trig = bt;
                    break;
                case "Idle":
                    trig = new TaskScheduler.IdleTrigger();
                    break;
                case "SessionUnlock":
                    trig = new TaskScheduler.SessionStateChangeTrigger { StateChange = TaskScheduler.TaskSessionStateChangeType.SessionUnlock };
                    break;
                default:
                    trig = new TaskScheduler.TimeTrigger { StartBoundary = startBoundary };
                    break;
            }

            trig.Enabled = model.TriggerEnabled;

            if (model.HasEndDate)
            {
                trig.EndBoundary = model.TriggerEndDate.Date + model.TriggerEndTime;
            }

            if (model.TriggerRepeatMinutes > 0)
            {
                trig.Repetition.Interval = TimeSpan.FromMinutes(model.TriggerRepeatMinutes);
                trig.Repetition.Duration = model.TriggerRepeatDurationHours > 0 ? TimeSpan.FromHours(model.TriggerRepeatDurationHours) : TimeSpan.Zero;
                trig.Repetition.StopAtDurationEnd = model.TriggerStopAtDurationEnd;
            }

            if (model.TriggerExecutionTimeLimitHours > 0)
            {
                trig.ExecutionTimeLimit = TimeSpan.FromHours(model.TriggerExecutionTimeLimitHours);
            }

            td.Triggers.Add(trig);
        }

        public Task<string> ExportTaskXmlAsync(string taskPath)
        {
            return StaTask.Run(() =>
            {
                using var ts = new TaskScheduler.TaskService();
                if (!ts.Connected) return string.Empty;
                var task = ts.GetTask(taskPath);
                return task?.Definition?.XmlText ?? string.Empty;
            });
        }
    }
}
