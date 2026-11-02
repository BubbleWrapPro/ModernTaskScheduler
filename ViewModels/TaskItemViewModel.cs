using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32.TaskScheduler;
using ModernTaskScheduler.Localization;
using WinTask = Microsoft.Win32.TaskScheduler.Task;

namespace ModernTaskScheduler.ViewModels
{
    public partial class TaskItemViewModel : ObservableObject
    {
        private static string L(string text) => LocalizationService.Instance.Translate(text);

        private static string F(string frenchTemplate, string englishTemplate, params object?[] values) =>
            LocalizationService.Instance.Format(frenchTemplate, englishTemplate, values);

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private string _folderPath = @"\";

        [ObservableProperty]
        private TaskState _state = TaskState.Unknown;

        [ObservableProperty]
        private string _stateDisplay = LocalizationService.Instance.Translate("Inconnu");

        [ObservableProperty]
        private string _stateBadgeColor = "#8A8886";

        [ObservableProperty]
        private string _stateBadgeBg = "#158A8886";

        [ObservableProperty]
        private string _stateIconGlyph = "\uE9CE";

        [ObservableProperty]
        private bool _enabled = true;

        [ObservableProperty]
        private DateTime _nextRunTime;

        [ObservableProperty]
        private string _nextRunTimeDisplay = LocalizationService.Instance.Translate("Aucune");

        [ObservableProperty]
        private DateTime _lastRunTime;

        [ObservableProperty]
        private string _lastRunTimeDisplay = LocalizationService.Instance.Translate("Jamais");

        [ObservableProperty]
        private int _lastTaskResult;

        [ObservableProperty]
        private string _lastTaskResultDisplay = LocalizationService.Instance.Translate("Non exécuté");

        [ObservableProperty]
        private string _author = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _registeredDate;

        [ObservableProperty]
        private string _principalUser = string.Empty;

        [ObservableProperty]
        private string _runLevelDisplay = "Standard (LUA)";

        [ObservableProperty]
        private bool _isHighestAvailable;

        [ObservableProperty]
        private string _logonTypeDisplay = LocalizationService.Instance.Translate("Interactif");

        [ObservableProperty]
        private string _xmlDefinition = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private bool _canRun;

        [ObservableProperty]
        private bool _canStop;

        public ObservableCollection<Models.TriggerDisplayItem> Triggers { get; } = new();
        public ObservableCollection<Models.ActionDisplayItem> Actions { get; } = new();
        public ObservableCollection<Models.ConditionDisplayItem> Conditions { get; } = new();
        public ObservableCollection<Models.SettingDisplayItem> Settings { get; } = new();

        public TaskItemViewModel(WinTask task)
        {
            UpdateFromTask(task);
        }

        public void UpdateFromTask(WinTask task)
        {
            try
            {
                Name = task.Name;
                Path = task.Path;
                FolderPath = task.Folder != null ? task.Folder.Path : @"\";
                State = task.State;
                Enabled = task.Enabled;

                // Running & Action states
                IsRunning = task.State == TaskState.Running;
                CanRun = task.Enabled && !IsRunning;
                CanStop = IsRunning;

                // State Badge & Glyph
                UpdateStateBadges(task.State);

                // Times
                NextRunTime = task.NextRunTime;
                NextRunTimeDisplay = task.NextRunTime == DateTime.MinValue || task.NextRunTime.Year < 1980
                    ? L("Aucune exécution planifiée")
                    : task.NextRunTime.ToString("g");

                LastRunTime = task.LastRunTime;
                LastRunTimeDisplay = task.LastRunTime == DateTime.MinValue || task.LastRunTime.Year < 1980
                    ? L("Jamais exécuté")
                    : task.LastRunTime.ToString("g");

                // Result Code
                LastTaskResult = task.LastTaskResult;
                LastTaskResultDisplay = FormatResultCode(task.LastTaskResult);
            }
            catch { }

            // Definition Properties with isolated safe blocks
            try
            {
                TaskDefinition? def = null;
                try
                {
                    def = task.Definition;
                }
                catch (Exception ex)
                {
                    Description = F("Impossible d'accéder à la définition : {0}", "Cannot access the definition: {0}", ex.Message);
                }

                if (def != null)
                {
                    // 1. Registration Info
                    try
                    {
                        if (def.RegistrationInfo != null)
                        {
                            Author = string.IsNullOrWhiteSpace(def.RegistrationInfo.Author) ? L("Système / Inconnu") : def.RegistrationInfo.Author;
                            Description = string.IsNullOrWhiteSpace(def.RegistrationInfo.Description) ? L("Aucune description fournie.") : def.RegistrationInfo.Description;
                            RegisteredDate = def.RegistrationInfo.Date;
                        }
                    }
                    catch { }

                    // 2. Principal
                    try
                    {
                        if (def.Principal != null)
                        {
                            IsHighestAvailable = def.Principal.RunLevel == TaskRunLevel.Highest;
                            PrincipalUser = string.IsNullOrWhiteSpace(def.Principal.UserId)
                                ? (string.IsNullOrWhiteSpace(def.Principal.GroupId) ? L("Utilisateur local") : def.Principal.GroupId)
                                : def.Principal.UserId;

                            RunLevelDisplay = def.Principal.RunLevel == TaskRunLevel.Highest
                                ? L("Privilèges les plus élevés (Administrateur)")
                                : L("Privilèges minimaux standard (LUA)");

                            LogonTypeDisplay = def.Principal.LogonType.ToString();
                        }
                    }
                    catch { }

                    // 3. XML Definition
                    try
                    {
                        XmlDefinition = def.XmlText ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        XmlDefinition = F("<!-- Erreur XML : {0} -->", "<!-- XML error: {0} -->", ex.Message);
                    }

                    // 4. Triggers, Actions, Conditions, Settings
                    PopulateTriggers(def);
                    PopulateActions(def);
                    PopulateConditions(def);
                    PopulateSettings(def);
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(Description))
                {
                    Description = F("Erreur de lecture : {0}", "Read error: {0}", ex.Message);
                }
            }
        }

        private void UpdateStateBadges(TaskState state)
        {
            switch (state)
            {
                case TaskState.Running:
                    StateDisplay = L("En cours d'exécution");
                    StateBadgeColor = "#0F7B0F"; // Fluent Green
                    StateBadgeBg = "#1F0F7B0F";
                    StateIconGlyph = "\uE768"; // Play
                    break;
                case TaskState.Ready:
                    StateDisplay = L("Prête");
                    StateBadgeColor = "#0078D4"; // Fluent Blue
                    StateBadgeBg = "#1F0078D4";
                    StateIconGlyph = "\uE73E"; // Check
                    break;
                case TaskState.Disabled:
                    StateDisplay = L("Désactivée");
                    StateBadgeColor = "#8A8886"; // Gray
                    StateBadgeBg = "#1F8A8886";
                    StateIconGlyph = "\uE711"; // Cancel
                    break;
                case TaskState.Queued:
                    StateDisplay = L("En attente");
                    StateBadgeColor = "#D83B01"; // Orange
                    StateBadgeBg = "#1FD83B01";
                    StateIconGlyph = "\uE823";
                    break;
                default:
                    StateDisplay = L("Inconnu");
                    StateBadgeColor = "#605E5C";
                    StateBadgeBg = "#1F605E5C";
                    StateIconGlyph = "\uE9CE";
                    break;
            }
        }

        private void PopulateTriggers(TaskDefinition def)
        {
            Triggers.Clear();
            if (def.Triggers == null) return;

            foreach (var tr in def.Triggers)
            {
                try
                {
                    string typeName = tr.TriggerType switch
                    {
                        TaskTriggerType.Time => L("À une heure programmée"),
                        TaskTriggerType.Daily => L("Quotidien"),
                        TaskTriggerType.Weekly => L("Hebdomadaire"),
                        TaskTriggerType.Monthly => L("Mensuel"),
                        TaskTriggerType.MonthlyDOW => L("Mensuel (Jour de la semaine)"),
                        TaskTriggerType.Idle => L("Sur inactivité du système"),
                        TaskTriggerType.Registration => L("À l'enregistrement de la tâche"),
                        TaskTriggerType.Boot => L("Au démarrage du système"),
                        TaskTriggerType.Logon => L("À la connexion d'un utilisateur"),
                        TaskTriggerType.SessionStateChange => L("Lors d'un changement de session"),
                        TaskTriggerType.Event => L("Sur événement Windows"),
                        _ => tr.TriggerType.ToString()
                    };

                    string icon = tr.TriggerType switch
                    {
                        TaskTriggerType.Boot => "\uE7E8", // Power button
                        TaskTriggerType.Logon => "\uE77B", // Contact
                        TaskTriggerType.Idle => "\uE708", // Moon / Sleep
                        TaskTriggerType.Event => "\uE7BA", // Warning / Alert
                        _ => "\uE823" // Calendar
                    };

                    string details = tr.ToString();
                    string start = tr.StartBoundary == DateTime.MinValue ? L("Non spécifié") : tr.StartBoundary.ToString("g");
                    string end = tr.EndBoundary == DateTime.MinValue ? L("Aucune date de fin") : tr.EndBoundary.ToString("g");
                    string rep = tr.Repetition != null && tr.Repetition.Interval != TimeSpan.Zero
                        ? F("Répéter toutes les {0}", "Repeat every {0}", tr.Repetition.Interval)
                        : L("Pas de répétition");

                    Triggers.Add(new Models.TriggerDisplayItem
                    {
                        TriggerType = typeName,
                        TriggerTypeId = tr.TriggerType.ToString(),
                        Details = details,
                        Enabled = tr.Enabled,
                        StatusDisplay = L(tr.Enabled ? "Activé" : "Désactivé"),
                        StartBoundary = start,
                        EndBoundary = end,
                        Repetition = rep,
                        IconGlyph = icon
                    });
                }
                catch { }
            }
        }

        private void PopulateActions(TaskDefinition def)
        {
            Actions.Clear();
            if (def.Actions == null) return;

            foreach (var act in def.Actions)
            {
                try
                {
                    if (act is ExecAction exec)
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = L("Démarrer un programme"),
                            Path = exec.Path ?? string.Empty,
                            Arguments = exec.Arguments ?? string.Empty,
                            WorkingDirectory = exec.WorkingDirectory ?? string.Empty,
                            Summary = $"{exec.Path} {exec.Arguments}".Trim(),
                            IconGlyph = "\uE756" // Executable icon
                        });
                    }
                    else if (act is EmailAction email)
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = L("Envoyer un e-mail"),
                            Path = email.Server ?? string.Empty,
                            Arguments = email.To ?? string.Empty,
                            WorkingDirectory = email.From ?? string.Empty,
                            Summary = F("À : {0} - Sujet : {1}", "To: {0} - Subject: {1}", email.To, email.Subject),
                            IconGlyph = "\uE715" // Mail icon
                        });
                    }
                    else if (act is ShowMessageAction msg)
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = L("Afficher un message"),
                            Path = msg.Title ?? string.Empty,
                            Arguments = msg.MessageBody ?? string.Empty,
                            Summary = $"{msg.Title}: {msg.MessageBody}",
                            IconGlyph = "\uE783" // Message icon
                        });
                    }
                    else if (act is ComHandlerAction com)
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = L("Gestionnaire COM personnalisé"),
                            Path = com.ClassId.ToString(),
                            Arguments = com.Data ?? string.Empty,
                            Summary = F("CLSID : {0}", "CLSID: {0}", com.ClassId),
                            IconGlyph = "\uE943" // Component icon
                        });
                    }
                    else
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = act.ActionType.ToString(),
                            Summary = act.ToString(),
                            IconGlyph = "\uE756"
                        });
                    }
                }
                catch { }
            }
        }

        private void PopulateConditions(TaskDefinition def)
        {
            Conditions.Clear();
            if (def.Settings == null) return;

            var set = def.Settings;
            try
            {
                // Idle Conditions
                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = L("Inactivité"),
                    Name = L("Démarrer uniquement si l'ordinateur est inactif"),
                    Value = set.IdleSettings != null && set.IdleSettings.IdleDuration != TimeSpan.Zero
                        ? F("Oui (inactif pendant {0})", "Yes (idle for {0})", set.IdleSettings.IdleDuration)
                        : L("Non"),
                    IsEnabled = set.IdleSettings != null && set.IdleSettings.IdleDuration != TimeSpan.Zero,
                    IconGlyph = "\uE708"
                });

                // Power Conditions
                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = L("Alimentation"),
                    Name = L("Démarrer la tâche seulement si l'ordinateur est sur secteur"),
                    Value = L(set.DisallowStartIfOnBatteries ? "Activé" : "Désactivé"),
                    IsEnabled = set.DisallowStartIfOnBatteries,
                    IconGlyph = "\uE7E8"
                });

                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = L("Alimentation"),
                    Name = L("Arrêter la tâche si l'ordinateur passe sur batterie"),
                    Value = L(set.StopIfGoingOnBatteries ? "Activé" : "Désactivé"),
                    IsEnabled = set.StopIfGoingOnBatteries,
                    IconGlyph = "\uEBB5"
                });

                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = L("Alimentation"),
                    Name = L("Sortir l'ordinateur du mode veille pour exécuter la tâche"),
                    Value = L(set.WakeToRun ? "Activé" : "Désactivé"),
                    IsEnabled = set.WakeToRun,
                    IconGlyph = "\uE706"
                });

                // Network Conditions
                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = L("Réseau"),
                    Name = L("Démarrer seulement si la connexion réseau est disponible"),
                    Value = L(set.RunOnlyIfNetworkAvailable ? "Activé" : "Désactivé"),
                    IsEnabled = set.RunOnlyIfNetworkAvailable,
                    IconGlyph = "\uE968"
                });
            }
            catch { }
        }

        private void PopulateSettings(TaskDefinition def)
        {
            Settings.Clear();
            if (def.Settings == null) return;

            var set = def.Settings;
            try
            {
                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Autoriser l'exécution à la demande"),
                    Value = L(set.AllowDemandStart ? "Oui" : "Non"),
                    Description = L("Permet de lancer la tâche manuellement à tout moment."),
                    IconGlyph = "\uE768"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Arrêter la tâche si elle dépasse le délai d'exécution"),
                    Value = set.ExecutionTimeLimit == TimeSpan.Zero
                        ? L("Pas de limite")
                        : F("{0} heure(s)", "{0} hour(s)", set.ExecutionTimeLimit.TotalHours),
                    Description = L("Évite qu'une tâche bloquée ne continue de s'exécuter indéfiniment."),
                    IconGlyph = "\uE916"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Politique en cas d'instances multiples"),
                    Value = FormatMultipleInstances(set.MultipleInstances),
                    Description = L("Comportement si la tâche est déclenchée alors qu'elle s'exécute déjà."),
                    IconGlyph = "\uE8D5"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Redémarrer en cas d'échec"),
                    Value = set.RestartCount > 0
                        ? F("{0} fois (intervalle : {1})", "{0} times (interval: {1})", set.RestartCount, set.RestartInterval)
                        : L("Désactivé"),
                    Description = L("Redémarre automatiquement le processus si un code d'erreur est renvoyé."),
                    IconGlyph = "\uE777"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Supprimer la tâche si elle n'est plus planifiée"),
                    Value = set.DeleteExpiredTaskAfter == TimeSpan.Zero
                        ? L("Jamais")
                        : F("Après {0} jour(s)", "After {0} day(s)", set.DeleteExpiredTaskAfter.TotalDays),
                    Description = L("Nettoie automatiquement les tâches à usage unique expirées."),
                    IconGlyph = "\uE74D"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = L("Visibilité de la tâche"),
                    Value = L(set.Hidden ? "Masquée" : "Visible"),
                    Description = L("Définit si la tâche est cachée dans les interfaces utilisateur standard."),
                    IconGlyph = "\uE890"
                });
            }
            catch { }
        }

        private static string FormatMultipleInstances(TaskInstancesPolicy policy)
        {
            return policy switch
            {
                TaskInstancesPolicy.IgnoreNew => L("Ne pas démarrer de nouvelle instance (Ignorer)"),
                TaskInstancesPolicy.Parallel => L("Exécuter une nouvelle instance en parallèle"),
                TaskInstancesPolicy.Queue => L("Mettre la nouvelle instance en file d'attente"),
                TaskInstancesPolicy.StopExisting => L("Arrêter l'instance existante et démarrer la nouvelle"),
                _ => policy.ToString()
            };
        }

        public static string FormatResultCode(int code)
        {
            uint uCode = (uint)code;
            return uCode switch
            {
                0x00000000 => L("Opération réussie (0x0)"),
                0x00041300 => L("La tâche est prête à s'exécuter (0x00041300)"),
                0x00041301 => L("Tâche actuellement en cours d'exécution (0x00041301)"),
                0x00041302 => L("La tâche est désactivée (0x00041302)"),
                0x00041303 => L("La tâche n'a pas encore été exécutée (0x00041303)"),
                0x00041304 => L("Aucune exécution ultérieure planifiée (0x00041304)"),
                0x00041305 => L("Un ou plusieurs déclencheurs non activés (0x00041305)"),
                0x00041306 => L("Dernière exécution interrompue par l'utilisateur (0x00041306)"),
                0x00041307 => L("Arrêté pour dépassement de délai imparti (0x00041307)"),
                0x80070002 => L("Le fichier spécifié est introuvable (0x80070002)"),
                0x80070005 => L("Accès refusé - Droits administrateur requis (0x80070005)"),
                0x8004130F => L("Informations d'identification invalides (0x8004130F)"),
                0x8004131F => L("Une instance est déjà en cours d'exécution (0x8004131F)"),
                _ => F("Code d'état : 0x{0:X8} ({1})", "Status code: 0x{0:X8} ({1})", uCode, code)
            };
        }
    }
}
