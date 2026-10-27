using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32.TaskScheduler;
using WinTask = Microsoft.Win32.TaskScheduler.Task;

namespace ModernTaskScheduler.ViewModels
{
    public partial class TaskItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private string _folderPath = @"\";

        [ObservableProperty]
        private TaskState _state = TaskState.Unknown;

        [ObservableProperty]
        private string _stateDisplay = "Inconnu";

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
        private string _nextRunTimeDisplay = "Aucune";

        [ObservableProperty]
        private DateTime _lastRunTime;

        [ObservableProperty]
        private string _lastRunTimeDisplay = "Jamais";

        [ObservableProperty]
        private int _lastTaskResult;

        [ObservableProperty]
        private string _lastTaskResultDisplay = "Non exécuté";

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
        private string _logonTypeDisplay = "Interactif";

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
                    ? "Aucune exécution planifiée"
                    : task.NextRunTime.ToString("g");

                LastRunTime = task.LastRunTime;
                LastRunTimeDisplay = task.LastRunTime == DateTime.MinValue || task.LastRunTime.Year < 1980
                    ? "Jamais exécuté"
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
                    Description = $"Impossible d'accéder à la définition : {ex.Message}";
                }

                if (def != null)
                {
                    // 1. Registration Info
                    try
                    {
                        if (def.RegistrationInfo != null)
                        {
                            Author = string.IsNullOrWhiteSpace(def.RegistrationInfo.Author) ? "Système / Inconnu" : def.RegistrationInfo.Author;
                            Description = string.IsNullOrWhiteSpace(def.RegistrationInfo.Description) ? "Aucune description fournie." : def.RegistrationInfo.Description;
                            RegisteredDate = def.RegistrationInfo.Date;
                        }
                    }
                    catch { }

                    // 2. Principal
                    try
                    {
                        if (def.Principal != null)
                        {
                            PrincipalUser = string.IsNullOrWhiteSpace(def.Principal.UserId)
                                ? (string.IsNullOrWhiteSpace(def.Principal.GroupId) ? "Utilisateur local" : def.Principal.GroupId)
                                : def.Principal.UserId;

                            RunLevelDisplay = def.Principal.RunLevel == TaskRunLevel.Highest
                                ? "Privilèges les plus élevés (Administrateur)"
                                : "Privilèges minimaux standard (LUA)";

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
                        XmlDefinition = $"<!-- Erreur XML: {ex.Message} -->";
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
                    Description = $"Erreur de lecture: {ex.Message}";
                }
            }
        }

        private void UpdateStateBadges(TaskState state)
        {
            switch (state)
            {
                case TaskState.Running:
                    StateDisplay = "En cours d'exécution";
                    StateBadgeColor = "#0F7B0F"; // Fluent Green
                    StateBadgeBg = "#1F0F7B0F";
                    StateIconGlyph = "\uE768"; // Play
                    break;
                case TaskState.Ready:
                    StateDisplay = "Prête";
                    StateBadgeColor = "#0078D4"; // Fluent Blue
                    StateBadgeBg = "#1F0078D4";
                    StateIconGlyph = "\uE73E"; // Check
                    break;
                case TaskState.Disabled:
                    StateDisplay = "Désactivée";
                    StateBadgeColor = "#8A8886"; // Gray
                    StateBadgeBg = "#1F8A8886";
                    StateIconGlyph = "\uE711"; // Cancel
                    break;
                case TaskState.Queued:
                    StateDisplay = "En attente";
                    StateBadgeColor = "#D83B01"; // Orange
                    StateBadgeBg = "#1FD83B01";
                    StateIconGlyph = "\uE823";
                    break;
                default:
                    StateDisplay = "Inconnu";
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
                        TaskTriggerType.Time => "À une heure programmée",
                        TaskTriggerType.Daily => "Quotidien",
                        TaskTriggerType.Weekly => "Hebdomadaire",
                        TaskTriggerType.Monthly => "Mensuel",
                        TaskTriggerType.MonthlyDOW => "Mensuel (Jour de la semaine)",
                        TaskTriggerType.Idle => "Sur inactivité du système",
                        TaskTriggerType.Registration => "À l'enregistrement de la tâche",
                        TaskTriggerType.Boot => "Au démarrage du système",
                        TaskTriggerType.Logon => "À la connexion d'un utilisateur",
                        TaskTriggerType.SessionStateChange => "Lors d'un changement de session",
                        TaskTriggerType.Event => "Sur événement Windows",
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
                    string start = tr.StartBoundary == DateTime.MinValue ? "Non spécifié" : tr.StartBoundary.ToString("g");
                    string end = tr.EndBoundary == DateTime.MinValue ? "Aucune date de fin" : tr.EndBoundary.ToString("g");
                    string rep = tr.Repetition != null && tr.Repetition.Interval != TimeSpan.Zero
                        ? $"Répéter toutes les {tr.Repetition.Interval}"
                        : "Pas de répétition";

                    Triggers.Add(new Models.TriggerDisplayItem
                    {
                        TriggerType = typeName,
                        Details = details,
                        Enabled = tr.Enabled,
                        StatusDisplay = tr.Enabled ? "Activé" : "Désactivé",
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
                            ActionType = "Démarrer un programme",
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
                            ActionType = "Envoyer un e-mail",
                            Path = email.Server ?? string.Empty,
                            Arguments = email.To ?? string.Empty,
                            WorkingDirectory = email.From ?? string.Empty,
                            Summary = $"À: {email.To} - Sujet: {email.Subject}",
                            IconGlyph = "\uE715" // Mail icon
                        });
                    }
                    else if (act is ShowMessageAction msg)
                    {
                        Actions.Add(new Models.ActionDisplayItem
                        {
                            ActionType = "Afficher un message",
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
                            ActionType = "Gestionnaire COM personnalisé",
                            Path = com.ClassId.ToString(),
                            Arguments = com.Data ?? string.Empty,
                            Summary = $"CLSID: {com.ClassId}",
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
                    Category = "Inactivité",
                    Name = "Démarrer uniquement si l'ordinateur est inactif",
                    Value = set.IdleSettings != null && set.IdleSettings.IdleDuration != TimeSpan.Zero
                        ? $"Oui (inactif pendant {set.IdleSettings.IdleDuration})"
                        : "Non",
                    IsEnabled = set.IdleSettings != null && set.IdleSettings.IdleDuration != TimeSpan.Zero,
                    IconGlyph = "\uE708"
                });

                // Power Conditions
                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = "Alimentation",
                    Name = "Démarrer la tâche seulement si l'ordinateur est sur secteur",
                    Value = set.DisallowStartIfOnBatteries ? "Activé" : "Désactivé",
                    IsEnabled = set.DisallowStartIfOnBatteries,
                    IconGlyph = "\uE7E8"
                });

                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = "Alimentation",
                    Name = "Arrêter la tâche si l'ordinateur passe sur batterie",
                    Value = set.StopIfGoingOnBatteries ? "Activé" : "Désactivé",
                    IsEnabled = set.StopIfGoingOnBatteries,
                    IconGlyph = "\uEBB5"
                });

                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = "Alimentation",
                    Name = "Sortir l'ordinateur du mode veille pour exécuter la tâche",
                    Value = set.WakeToRun ? "Activé" : "Désactivé",
                    IsEnabled = set.WakeToRun,
                    IconGlyph = "\uE706"
                });

                // Network Conditions
                Conditions.Add(new Models.ConditionDisplayItem
                {
                    Category = "Réseau",
                    Name = "Démarrer seulement si la connexion réseau est disponible",
                    Value = set.RunOnlyIfNetworkAvailable ? "Activé" : "Désactivé",
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
                    Name = "Autoriser l'exécution à la demande",
                    Value = set.AllowDemandStart ? "Oui" : "Non",
                    Description = "Permet de lancer la tâche manuellement à tout moment.",
                    IconGlyph = "\uE768"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = "Arrêter la tâche si elle dépasse le délai d'exécution",
                    Value = set.ExecutionTimeLimit == TimeSpan.Zero ? "Pas de limite" : $"{set.ExecutionTimeLimit.TotalHours} heure(s)",
                    Description = "Évite qu'une tâche bloquée ne continue de s'exécuter indéfiniment.",
                    IconGlyph = "\uE916"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = "Politique en cas d'instances multiples",
                    Value = FormatMultipleInstances(set.MultipleInstances),
                    Description = "Comportement si la tâche est déclenchée alors qu'elle s'exécute déjà.",
                    IconGlyph = "\uE8D5"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = "Redémarrer en cas d'échec",
                    Value = set.RestartCount > 0 ? $"{set.RestartCount} fois (intervalle: {set.RestartInterval})" : "Désactivé",
                    Description = "Redémarre automatiquement le processus si un code d'erreur est renvoyé.",
                    IconGlyph = "\uE777"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = "Supprimer la tâche si elle n'est plus planifiée",
                    Value = set.DeleteExpiredTaskAfter == TimeSpan.Zero ? "Jamais" : $"Après {set.DeleteExpiredTaskAfter.TotalDays} jour(s)",
                    Description = "Nettoie automatiquement les tâches à usage unique expirées.",
                    IconGlyph = "\uE74D"
                });

                Settings.Add(new Models.SettingDisplayItem
                {
                    Name = "Visibilité de la tâche",
                    Value = set.Hidden ? "Masquée" : "Visible",
                    Description = "Définit si la tâche est cachée dans les interfaces utilisateur standard.",
                    IconGlyph = "\uE890"
                });
            }
            catch { }
        }

        private static string FormatMultipleInstances(TaskInstancesPolicy policy)
        {
            return policy switch
            {
                TaskInstancesPolicy.IgnoreNew => "Ne pas démarrer de nouvelle instance (Ignorer)",
                TaskInstancesPolicy.Parallel => "Exécuter une nouvelle instance en parallèle",
                TaskInstancesPolicy.Queue => "Mettre la nouvelle instance en file d'attente",
                TaskInstancesPolicy.StopExisting => "Arrêter l'instance existante et démarrer la nouvelle",
                _ => policy.ToString()
            };
        }

        public static string FormatResultCode(int code)
        {
            uint uCode = (uint)code;
            return uCode switch
            {
                0x00000000 => "Opération réussie (0x0)",
                0x00041300 => "La tâche est prête à s'exécuter (0x00041300)",
                0x00041301 => "Tâche actuellement en cours d'exécution (0x00041301)",
                0x00041302 => "La tâche est désactivée (0x00041302)",
                0x00041303 => "La tâche n'a pas encore été exécutée (0x00041303)",
                0x00041304 => "Aucune exécution ultérieure planifiée (0x00041304)",
                0x00041305 => "Un ou plusieurs déclencheurs non activés (0x00041305)",
                0x00041306 => "Dernière exécution interrompue par l'utilisateur (0x00041306)",
                0x00041307 => "Arrêté pour dépassement de délai imparti (0x00041307)",
                0x80070002 => "Le fichier spécifié est introuvable (0x80070002)",
                0x80070005 => "Accès refusé - Droits administrateur requis (0x80070005)",
                0x8004130F => "Informations d'identification invalides (0x8004130F)",
                0x8004131F => "Une instance est déjà en cours d'exécution (0x8004131F)",
                _ => $"Code d'état : 0x{uCode:X8} ({code})"
            };
        }
    }
}
