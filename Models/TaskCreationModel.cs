using System;
using Microsoft.Win32.TaskScheduler;

namespace ModernTaskScheduler.Models
{
    public class TaskCreationModel
    {
        // General
        public string TaskName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = @"\";
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = Environment.UserName;
        public TaskRunLevel RunLevel { get; set; } = TaskRunLevel.LUA;
        public bool RunWhetherUserLoggedOn { get; set; } = false;
        public bool DoNotStorePassword { get; set; } = true;
        public string UserAccount { get; set; } = Environment.UserName;
        public bool Hidden { get; set; } = false;
        public TaskCompatibility Compatibility { get; set; } = TaskCompatibility.V2_1;

        // Trigger
        public string TriggerType { get; set; } = "Daily"; // Once, Daily, Weekly, Monthly, Boot, Logon, Idle, SessionUnlock
        public DateTimeOffset TriggerStartDate { get; set; } = DateTimeOffset.Now;
        public TimeSpan TriggerStartTime { get; set; } = DateTime.Now.TimeOfDay;
        public bool HasEndDate { get; set; } = false;
        public DateTimeOffset TriggerEndDate { get; set; } = DateTimeOffset.Now.AddYears(1);
        public TimeSpan TriggerEndTime { get; set; } = DateTime.Now.TimeOfDay;
        public int TriggerDelaySeconds { get; set; } = 0;
        public int TriggerRepeatMinutes { get; set; } = 0;
        public int TriggerRepeatDurationHours { get; set; } = 24;
        public bool TriggerStopAtDurationEnd { get; set; } = false;
        public int TriggerExecutionTimeLimitHours { get; set; } = 0;
        public bool TriggerEnabled { get; set; } = true;

        // Action
        public string ActionProgram { get; set; } = string.Empty;
        public string ActionArguments { get; set; } = string.Empty;
        public string ActionWorkingDirectory { get; set; } = string.Empty;

        // Conditions: Idle
        public bool RunOnlyIfIdle { get; set; } = false;
        public int IdleDurationMinutes { get; set; } = 10;
        public int IdleWaitTimeoutMinutes { get; set; } = 60;
        public bool StopOnIdleEnd { get; set; } = true;
        public bool RestartOnIdle { get; set; } = false;

        // Conditions: Power
        public bool DisallowStartIfOnBatteries { get; set; } = false;
        public bool StopIfGoingOnBatteries { get; set; } = false;
        public bool WakeToRun { get; set; } = false;

        // Conditions: Network
        public bool RunOnlyIfNetworkAvailable { get; set; } = false;

        // Settings
        public bool AllowDemandStart { get; set; } = true;
        public bool StartWhenAvailable { get; set; } = false;
        public bool RestartOnFailure { get; set; } = false;
        public int RestartCount { get; set; } = 3;
        public int RestartIntervalMinutes { get; set; } = 1;
        public double ExecutionTimeLimitHours { get; set; } = 72;
        public bool AllowHardTerminate { get; set; } = true;
        public int DeleteExpiredTaskAfterDays { get; set; } = 0;
        public TaskInstancesPolicy MultipleInstances { get; set; } = TaskInstancesPolicy.IgnoreNew;
    }
}
