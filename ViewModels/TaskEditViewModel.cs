using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32.TaskScheduler;
using ModernTaskScheduler.Models;

namespace ModernTaskScheduler.ViewModels
{
    public partial class TaskEditViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _originalPath = string.Empty;

        [ObservableProperty]
        private string _dialogTitle = "Créer une nouvelle tâche planifiée";

        // General
        [ObservableProperty]
        private string _taskName = "NouvelleTache";

        [ObservableProperty]
        private string _folderPath = @"\";

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _author = Environment.UserName;

        [ObservableProperty]
        private string _userAccount = Environment.UserName;

        [ObservableProperty]
        private bool _isHighestAvailable = false;

        [ObservableProperty]
        private bool _runWhetherUserLoggedOn = false;

        [ObservableProperty]
        private bool _doNotStorePassword = true;

        [ObservableProperty]
        private bool _hidden = false;

        [ObservableProperty]
        private int _compatibilityIndex = 0; // 0=Win10/11 (V2_1), 1=Win7/2008R2 (V2), 2=WinVista (V2)

        // Trigger settings
        [ObservableProperty]
        private int _triggerTypeIndex = 0; // 0=Daily, 1=Weekly, 2=Monthly, 3=Once, 4=Logon, 5=Boot, 6=Idle, 7=SessionUnlock

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now;

        [ObservableProperty]
        private TimeSpan _startTime = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private bool _hasEndDate = false;

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now.AddYears(1);

        [ObservableProperty]
        private TimeSpan _endTime = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private int _delayIndex = 0; // 0=None, 1=30s, 2=1 min, 3=5 min, 4=15 min, 5=30 min, 6=1h

        [ObservableProperty]
        private int _repeatIntervalIndex = 0; // 0=None, 1=5 min, 2=10 min, 3=15 min, 4=30 min, 5=1 heure

        [ObservableProperty]
        private int _repeatDurationIndex = 3; // 0=1h, 1=4h, 2=12h, 3=1 jour, 4=3 jours, 5=Indéfini

        [ObservableProperty]
        private bool _stopAtDurationEnd = false;

        [ObservableProperty]
        private int _executionLimitIndex = 0; // 0=None, 1=1h, 2=2h, 3=4h, 4=8h, 5=1 jour, 6=3 jours

        [ObservableProperty]
        private bool _triggerEnabled = true;

        // Action settings
        [ObservableProperty]
        private string _actionProgram = "notepad.exe";

        [ObservableProperty]
        private string _actionArguments = string.Empty;

        [ObservableProperty]
        private string _actionWorkingDirectory = string.Empty;

        // Conditions: Idle
        [ObservableProperty]
        private bool _runOnlyIfIdle = false;

        [ObservableProperty]
        private int _idleDurationMinutes = 10;

        [ObservableProperty]
        private int _idleWaitTimeoutMinutes = 60;

        [ObservableProperty]
        private bool _stopOnIdleEnd = true;

        [ObservableProperty]
        private bool _restartOnIdle = false;

        // Conditions: Power
        [ObservableProperty]
        private bool _disallowStartIfOnBatteries = false;

        [ObservableProperty]
        private bool _stopIfGoingOnBatteries = false;

        [ObservableProperty]
        private bool _wakeToRun = false;

        // Conditions: Network
        [ObservableProperty]
        private bool _runOnlyIfNetworkAvailable = false;

        // Settings
        [ObservableProperty]
        private bool _allowDemandStart = true;

        [ObservableProperty]
        private bool _startWhenAvailable = false;

        [ObservableProperty]
        private bool _restartOnFailure = false;

        [ObservableProperty]
        private int _restartCount = 3;

        [ObservableProperty]
        private int _restartIntervalMinutes = 1;

        [ObservableProperty]
        private int _executionTimeLimitHoursIndex = 6; // 0=None, 1=1h, 2=2h, 3=4h, 4=8h, 5=1 jour, 6=3 jours (72h)

        [ObservableProperty]
        private bool _allowHardTerminate = true;

        [ObservableProperty]
        private int _deleteExpiredIndex = 0; // 0=Jamais, 1=Immédiatement, 2=30 jours, 3=90 jours, 4=180 jours, 5=365 jours

        [ObservableProperty]
        private int _multipleInstancesIndex = 0; // 0=Ignore, 1=Parallel, 2=Queue, 3=StopExisting

        // Folders list for destination ComboBox
        public ObservableCollection<TaskFolderItem> AvailableFolders { get; } = new();

        // Validation
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError = false;

        public TaskEditViewModel()
        {
            DialogTitle = "Créer une nouvelle tâche planifiée";
            IsEditMode = false;
        }

        public void LoadFromTask(TaskItemViewModel task)
        {
            IsEditMode = true;
            OriginalPath = task.Path;
            DialogTitle = $"Modifier la tâche : {task.Name}";

            TaskName = task.Name;
            FolderPath = task.FolderPath;
            Description = task.Description;
            Author = task.Author;
            UserAccount = task.PrincipalUser;
            IsHighestAvailable = task.RunLevelDisplay.Contains("Administrateur");

            if (task.Actions.Count > 0)
            {
                var act = task.Actions[0];
                ActionProgram = act.Path;
                ActionArguments = act.Arguments;
                ActionWorkingDirectory = act.WorkingDirectory;
            }

            if (task.Triggers.Count > 0)
            {
                var trig = task.Triggers[0];
                TriggerEnabled = trig.Enabled;
                if (trig.TriggerType.Contains("Quotidien")) TriggerTypeIndex = 0;
                else if (trig.TriggerType.Contains("Hebdo")) TriggerTypeIndex = 1;
                else if (trig.TriggerType.Contains("Mensuel")) TriggerTypeIndex = 2;
                else if (trig.TriggerType.Contains("Connexion")) TriggerTypeIndex = 4;
                else if (trig.TriggerType.Contains("Démarrage")) TriggerTypeIndex = 5;
                else if (trig.TriggerType.Contains("inactivité")) TriggerTypeIndex = 6;
                else if (trig.TriggerType.Contains("session") || trig.TriggerType.Contains("verrouillage")) TriggerTypeIndex = 7;
                else TriggerTypeIndex = 3; // Once
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                ErrorMessage = "Le nom de la tâche ne peut pas être vide.";
                HasError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(ActionProgram))
            {
                ErrorMessage = "Le chemin du programme ou script à exécuter est obligatoire.";
                HasError = true;
                return false;
            }

            ErrorMessage = string.Empty;
            HasError = false;
            return true;
        }

        public TaskCreationModel ToCreationModel()
        {
            string triggerType = TriggerTypeIndex switch
            {
                0 => "Daily",
                1 => "Weekly",
                2 => "Monthly",
                3 => "Once",
                4 => "Logon",
                5 => "Boot",
                6 => "Idle",
                7 => "SessionUnlock",
                _ => "Daily"
            };

            int delaySeconds = DelayIndex switch
            {
                1 => 30,
                2 => 60,
                3 => 300,
                4 => 900,
                5 => 1800,
                6 => 3600,
                _ => 0
            };

            int repeatMinutes = RepeatIntervalIndex switch
            {
                1 => 5,
                2 => 10,
                3 => 15,
                4 => 30,
                5 => 60,
                _ => 0
            };

            int repeatDurationHours = RepeatDurationIndex switch
            {
                0 => 1,
                1 => 4,
                2 => 12,
                3 => 24,
                4 => 72,
                _ => 0 // Indefinite
            };

            int triggerExecLimitHours = ExecutionLimitIndex switch
            {
                1 => 1,
                2 => 2,
                3 => 4,
                4 => 8,
                5 => 24,
                6 => 72,
                _ => 0
            };

            double execTimeLimitHours = ExecutionTimeLimitHoursIndex switch
            {
                1 => 1,
                2 => 2,
                3 => 4,
                4 => 8,
                5 => 24,
                6 => 72,
                _ => 0
            };

            int deleteExpiredDays = DeleteExpiredIndex switch
            {
                1 => 0,
                2 => 30,
                3 => 90,
                4 => 180,
                5 => 365,
                _ => -1
            };

            TaskInstancesPolicy policy = MultipleInstancesIndex switch
            {
                1 => TaskInstancesPolicy.Parallel,
                2 => TaskInstancesPolicy.Queue,
                3 => TaskInstancesPolicy.StopExisting,
                _ => TaskInstancesPolicy.IgnoreNew
            };

            TaskCompatibility compat = CompatibilityIndex switch
            {
                1 => TaskCompatibility.V2,
                2 => TaskCompatibility.V1,
                _ => TaskCompatibility.V2_1
            };

            return new TaskCreationModel
            {
                TaskName = TaskName.Trim(),
                FolderPath = string.IsNullOrWhiteSpace(FolderPath) ? @"\" : FolderPath.Trim(),
                Description = Description.Trim(),
                Author = Author.Trim(),
                UserAccount = string.IsNullOrWhiteSpace(UserAccount) ? Environment.UserName : UserAccount.Trim(),
                RunLevel = IsHighestAvailable ? TaskRunLevel.Highest : TaskRunLevel.LUA,
                RunWhetherUserLoggedOn = RunWhetherUserLoggedOn,
                DoNotStorePassword = DoNotStorePassword,
                Hidden = Hidden,
                Compatibility = compat,

                TriggerType = triggerType,
                TriggerStartDate = StartDate,
                TriggerStartTime = StartTime,
                HasEndDate = HasEndDate,
                TriggerEndDate = EndDate,
                TriggerEndTime = EndTime,
                TriggerDelaySeconds = delaySeconds,
                TriggerRepeatMinutes = repeatMinutes,
                TriggerRepeatDurationHours = repeatDurationHours,
                TriggerStopAtDurationEnd = StopAtDurationEnd,
                TriggerExecutionTimeLimitHours = triggerExecLimitHours,
                TriggerEnabled = TriggerEnabled,

                ActionProgram = ActionProgram.Trim(),
                ActionArguments = ActionArguments.Trim(),
                ActionWorkingDirectory = ActionWorkingDirectory.Trim(),

                RunOnlyIfIdle = RunOnlyIfIdle,
                IdleDurationMinutes = IdleDurationMinutes,
                IdleWaitTimeoutMinutes = IdleWaitTimeoutMinutes,
                StopOnIdleEnd = StopOnIdleEnd,
                RestartOnIdle = RestartOnIdle,

                DisallowStartIfOnBatteries = DisallowStartIfOnBatteries,
                StopIfGoingOnBatteries = StopIfGoingOnBatteries,
                WakeToRun = WakeToRun,
                RunOnlyIfNetworkAvailable = RunOnlyIfNetworkAvailable,

                AllowDemandStart = AllowDemandStart,
                StartWhenAvailable = StartWhenAvailable,
                RestartOnFailure = RestartOnFailure,
                RestartCount = RestartCount,
                RestartIntervalMinutes = RestartIntervalMinutes,
                ExecutionTimeLimitHours = execTimeLimitHours,
                AllowHardTerminate = AllowHardTerminate,
                DeleteExpiredTaskAfterDays = deleteExpiredDays,
                MultipleInstances = policy
            };
        }
    }
}
