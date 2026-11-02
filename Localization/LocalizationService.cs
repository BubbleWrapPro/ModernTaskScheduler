using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
namespace ModernTaskScheduler.Localization
{
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        private static readonly Lazy<LocalizationService> LazyInstance = new(() => new LocalizationService());
        private readonly Dictionary<string, string> _english = new(StringComparer.Ordinal)
        {
            ["Toutes"] = "All", ["En cours"] = "Running", ["Prêtes"] = "Ready", ["Désactivées"] = "Disabled",
            ["Actualiser"] = "Refresh", ["Nouvelle tâche"] = "New Task", ["Créer une nouvelle tâche planifiée"] = "Create a new scheduled task",
            ["Rechercher une tâche (nom, description, auteur)..."] = "Search tasks (name, description, author)...",
            ["DOSSIERS DE LA BIBLIOTHÈQUE"] = "TASK LIBRARY FOLDERS", ["Nouveau"] = "New", ["Tout explorer"] = "Explore all",
            ["Aucune tâche sélectionnée"] = "No task selected", ["Sélectionnez une tâche dans la colonne de gauche pour afficher et gérer ses déclencheurs, actions, conditions et paramètres avancés, ou créez une nouvelle tâche dès maintenant."] = "Select a task in the left pane to view and manage its triggers, actions, conditions, and advanced settings, or create a new task now.",
            ["Créer une nouvelle tâche"] = "Create a new task", ["tâches"] = "tasks", ["Général"] = "General",
            ["Description"] = "Description", ["État d'exécution & Diagnostics"] = "Run status & diagnostics",
            ["Dernière exécution"] = "Last run", ["Prochaine exécution planifiée"] = "Next scheduled run",
            ["Résultat de la dernière exécution"] = "Last run result", ["Compte et privilèges de sécurité"] = "Account and security privileges",
            ["Compte utilisateur"] = "User account", ["Niveau d'élévation"] = "Elevation level",
            ["Actions"] = "Actions", ["Conditions"] = "Conditions", ["Paramètres"] = "Settings", ["Définition XML"] = "XML definition",
            ["Copier le XML dans le presse-papiers"] = "Copy XML to clipboard", ["Exécuter immédiatement cette tâche"] = "Run this task now",
            ["Arrêter l'exécution de la tâche en cours"] = "Stop the running task", ["Activer ou désactiver cette tâche"] = "Enable or disable this task",
            ["Modifier les propriétés de la tâche"] = "Edit task properties", ["Supprimer définitivement cette tâche"] = "Permanently delete this task",
            ["Copier la définition XML de la tâche"] = "Copy the task XML definition", ["Recharger les propriétés de cette tâche"] = "Reload this task's properties",
            ["Propriétés de la tâche"] = "Task properties", ["Configuration avancée selon les spécifications Windows Task Scheduler (100% options)"] = "Advanced configuration based on Windows Task Scheduler specifications (all options)",
            ["Erreur de validation"] = "Validation error", ["Nom de la tâche *"] = "Task name *", ["Ex: NettoyageAutomatique"] = "E.g. AutomaticCleanup",
            ["Dossier d'enregistrement"] = "Save in folder", ["Description de la tâche"] = "Task description",
            ["Décrivez l'objectif et les détails de cette tâche planifiée..."] = "Describe the purpose and details of this scheduled task...",
            ["Auteur"] = "Author", ["Compte utilisateur exécutant"] = "Run as user account", ["Ex: SYSTEM ou NomUtilisateur"] = "E.g. SYSTEM or UserName",
            ["Options de sécurité et droits d'exécution"] = "Security and run options", ["Exécuter même si l'utilisateur n'est pas connecté"] = "Run whether user is logged on or not",
            ["Ne pas stocker le mot de passe (la tâche n'accédera qu'aux ressources locales - S4U)"] = "Do not store password (task can only access local resources - S4U)",
            ["Exécuter avec les autorisations maximales (Élévation Administrateur)"] = "Run with highest privileges (administrator)",
            ["Masquer la tâche (Invisible dans taskschd standard)"] = "Hide the task (not visible in standard Task Scheduler)",
            ["Configurer pour :"] = "Configure for:", ["Déclencheurs"] = "Triggers", ["Activer ce déclencheur"] = "Enable this trigger",
            ["Activé"] = "Enabled", ["Désactivé"] = "Disabled", ["Lancer la tâche :"] = "Start the task:",
            ["Quotidien (Chaque jour)"] = "Daily", ["Hebdomadaire (Chaque semaine)"] = "Weekly", ["Mensuel (Chaque mois)"] = "Monthly",
            ["À une heure programmée (Une seule fois)"] = "At a scheduled time (once)", ["À la connexion d'un utilisateur"] = "At user logon",
            ["Au démarrage de l'ordinateur"] = "At computer startup", ["Sur inactivité de l'ordinateur"] = "When the computer is idle",
            ["Au déverrouillage de la session"] = "At session unlock", ["Date de début"] = "Start date", ["Heure de début"] = "Start time",
            ["Paramètres avancés du déclencheur"] = "Advanced trigger settings", ["Reporter la tâche jusqu'à :"] = "Delay task by:",
            ["Aucun délai"] = "No delay", ["Répéter la tâche toutes les :"] = "Repeat task every:", ["Aucune répétition"] = "No repetition",
            ["pendant une durée de :"] = "for a duration of:", ["Indéfiniment"] = "Indefinitely",
            ["Arrêter toutes les tâches en cours à la fin de la durée de répétition"] = "Stop all running tasks at the end of the repetition duration",
            ["Arrêter la tâche si elle s'exécute plus de :"] = "Stop task if it runs longer than:",
            ["Pas de limite spécifique au déclencheur"] = "No trigger-specific limit", ["Date d'expiration de la planification"] = "Schedule expiration date",
            ["Date de fin"] = "End date", ["Heure de fin"] = "End time", ["Action : Démarrer un programme"] = "Action: Start a program",
            ["Programme / Script *"] = "Program / script *", ["Parcourir l'ordinateur..."] = "Browse computer...", ["Parcourir..."] = "Browse...",
            ["Arguments facultatifs (paramètres de la commande)"] = "Optional arguments (command parameters)", ["Dossier de démarrage (facultatif)"] = "Start-in folder (optional)",
            ["Conditions & Paramètres"] = "Conditions & settings", ["Conditions d'inactivité de l'ordinateur"] = "Computer idle conditions",
            ["Gestion de l'alimentation"] = "Power management", ["Conditions réseau"] = "Network conditions",
            ["Paramètres avancés d'exécution de la tâche"] = "Advanced task settings", ["Ne jamais supprimer"] = "Never delete",
            ["Immédiatement après expiration"] = "Immediately after expiration", ["Annuler"] = "Cancel", ["Enregistrer la tâche"] = "Save task",
            ["Programme / Script :"] = "Program / script:", ["Arguments :"] = "Arguments:", ["Dossier de travail :"] = "Working folder:",
            ["Toutes les tâches affichées"] = "All tasks displayed", ["Tâche créée"] = "Task created",
            ["Tâche modifiée"] = "Task updated", ["Erreur"] = "Error", ["Erreur de connexion"] = "Connection error",
            ["Erreur lors de l'actualisation"] = "Refresh error", ["Actualisation réussie"] = "Refresh successful",
            ["Erreur de lecture du dossier"] = "Folder read error", ["Dossier créé"] = "Folder created", ["Dossier supprimé"] = "Folder deleted",
            ["Erreur lors de la création du dossier"] = "Folder creation error", ["Erreur lors de la suppression du dossier"] = "Folder deletion error",
            ["Tâche exécutée"] = "Task started", ["Erreur lors de l'exécution"] = "Task run error", ["Tâche arrêtée"] = "Task stopped",
            ["Erreur lors de l'arrêt"] = "Task stop error", ["État modifié"] = "Status changed", ["Erreur de modification"] = "Update error",
            ["Tâche supprimée"] = "Task deleted", ["Erreur de suppression"] = "Delete error", ["Erreur lors de la création"] = "Creation error",
            ["Erreur lors de la modification"] = "Update error", ["XML copié"] = "XML copied", ["Erreur d'exportation"] = "Export error",
            ["Confirmer la suppression"] = "Confirm deletion", ["Supprimer définitivement"] = "Delete permanently", ["Confirmer la suppression du dossier"] = "Confirm folder deletion",
            ["Créer un nouveau dossier"] = "Create a new folder", ["Créer le dossier"] = "Create folder",
            ["Version 1.1.0"] = "Version 1.1.0", ["Aucune"] = "None", ["Jamais"] = "Never",
            ["Prêt"] = "Ready", ["Chargement des tâches..."] = "Loading tasks...", ["Connexion au service du Planificateur de tâches..."] = "Connecting to Task Scheduler...",
            ["Actualisation des tâches et dossiers..."] = "Refreshing tasks and folders...", ["Chargement de toutes les tâches de la bibliothèque..."] = "Loading all library tasks...",
            ["v1.1.0 - Gestion professionnelle des tâches"] = "v1.1.0 - Professional task management",
            ["Afficher récursivement toutes les tâches de toutes les branches"] = "Recursively show tasks in all folders",
            ["Créer un nouveau sous-dossier"] = "Create a new subfolder", ["Supprimer ce dossier"] = "Delete this folder",
            ["À propos de Modern Task Scheduler"] = "About Modern Task Scheduler", ["Fermer"] = "Close", ["Ex: MesTaches"] = "E.g. MyTasks",
            ["Êtes-vous certain de vouloir supprimer définitivement la tâche planifiée suivante ?\n\nNom : {0}\nEmplacement : {1}\n\nCette action est irréversible."] = "Are you sure you want to permanently delete this scheduled task?\n\nName: {0}\nLocation: {1}\n\nThis action cannot be undone.",
            ["Dossier parent : {0}"] = "Parent folder: {0}",
            ["Êtes-vous certain de vouloir supprimer le dossier suivant et tout son contenu éventuel ?\n\nChemin : {0}\n\nCette action est irréversible."] = "Are you sure you want to delete this folder and any contents?\n\nPath: {0}\n\nThis action cannot be undone.",
            ["Une application Windows moderne et complète pour gérer les tâches système, créée avec WinUI et .NET 8."] = "A modern, feature-rich Windows application for managing system tasks built with WinUI and .NET 8.",
            ["© 2024 BubbleWrapPro. Tous droits réservés."] = "© 2024 BubbleWrapPro. All rights reserved.",
            ["Windows 10 / Windows 11 / Windows Server 2016 (V2.1)"] = "Windows 10 / Windows 11 / Windows Server 2016 (V2.1)",
            ["Windows 7 / Windows Server 2008 R2 (V2.0)"] = "Windows 7 / Windows Server 2008 R2 (V2.0)",
            ["Windows Vista / Windows Server 2008 (V1.0)"] = "Windows Vista / Windows Server 2008 (V1.0)",
            ["1 minute"] = "1 minute", ["5 minutes"] = "5 minutes", ["10 minutes"] = "10 minutes", ["15 minutes"] = "15 minutes", ["30 minutes"] = "30 minutes",
            ["1 heure"] = "1 hour", ["4 heures"] = "4 hours", ["8 heures"] = "8 hours", ["12 heures"] = "12 hours", ["1 jour"] = "1 day", ["3 jours"] = "3 days",
            ["30 secondes"] = "30 seconds", ["2 heures"] = "2 hours", ["Pas de limite (indéfini)"] = "No limit (unlimited)",
            ["1 jour (24h)"] = "1 day (24h)", ["3 jours (72h - recommandé)"] = "3 days (72h - recommended)",
            ["Après 30 jours"] = "After 30 days", ["Après 90 jours"] = "After 90 days", ["Après 180 jours"] = "After 180 days", ["Après 365 jours"] = "After 365 days",
            ["Oui"] = "Yes", ["Non"] = "No",
            ["Démarrer la tâche seulement si l'ordinateur est inactif pendant 10 minutes"] = "Start the task only if the computer is idle for 10 minutes",
            ["Arrêter la tâche si l'ordinateur quitte l'état inactif"] = "Stop the task if the computer is no longer idle",
            ["Redémarrer la tâche si l'état inactif reprend"] = "Restart the task when the idle state resumes",
            ["Ne démarrer la tâche que si l'ordinateur est sur secteur"] = "Start the task only if the computer is on AC power",
            ["Arrêter si l'ordinateur bascule sur batterie"] = "Stop if the computer switches to battery power",
            ["Sortir l'ordinateur du mode veille pour exécuter la tâche"] = "Wake the computer to run this task",
            ["Démarrer seulement si une connexion réseau est disponible"] = "Start only if a network connection is available",
            ["Autoriser l'exécution de la tâche à la demande"] = "Allow the task to be run on demand",
            ["Exécuter la tâche dès que possible après un démarrage manqué"] = "Run the task as soon as possible after a missed start",
            ["Si la tâche échoue, la redémarrer toutes les minutes (jusqu'à 3 tentatives)"] = "If the task fails, restart it every minute (up to 3 attempts)",
            ["Forcer l'arrêt de la tâche si elle ne s'arrête pas sur demande"] = "Force the task to stop if it does not stop when requested",
            ["Supprimer la tâche si non planifiée après :"] = "Delete the task if it is not scheduled again after:",
            ["Si la tâche est déjà en cours d'exécution, la règle suivante s'applique :"] = "If the task is already running, the following rule applies:",
            ["Ne pas démarrer de nouvelle instance (Ignorer)"] = "Do not start a new instance (ignore)",
            ["Exécuter des instances en parallèle"] = "Run instances in parallel",
            ["Mettre la nouvelle instance en file d'attente"] = "Queue a new instance",
            ["Arrêter l'instance existante"] = "Stop the existing instance",
            ["Erreur de langue"] = "Language error",
            ["Exécuter"] = "Run", ["Arrêter"] = "Stop", ["Activer / Désactiver"] = "Enable / Disable",
            ["Modifier"] = "Edit", ["Supprimer"] = "Delete", ["Exporter XML"] = "Export XML",
            ["Ex: C:\\Scripts"] = "E.g. C:\\Scripts",
            ["Inconnu"] = "Unknown", ["Aucune exécution planifiée"] = "No scheduled run", ["Jamais exécuté"] = "Never run",
            ["Système / Inconnu"] = "System / Unknown", ["Aucune description fournie."] = "No description provided.",
            ["Utilisateur local"] = "Local user", ["Privilèges les plus élevés (Administrateur)"] = "Highest privileges (Administrator)",
            ["Privilèges minimaux standard (LUA)"] = "Standard least privileges (LUA)",
            ["En cours d'exécution"] = "Running", ["Prête"] = "Ready", ["Désactivée"] = "Disabled", ["En attente"] = "Queued",
            ["À une heure programmée"] = "At a scheduled time", ["Quotidien"] = "Daily", ["Hebdomadaire"] = "Weekly",
            ["Mensuel"] = "Monthly", ["Mensuel (Jour de la semaine)"] = "Monthly (day of week)",
            ["Sur inactivité du système"] = "On system idle", ["À l'enregistrement de la tâche"] = "At task registration",
            ["Au démarrage du système"] = "At system startup", ["Lors d'un changement de session"] = "On session change",
            ["Sur événement Windows"] = "On Windows event", ["Non spécifié"] = "Not specified", ["Aucune date de fin"] = "No end date",
            ["Répéter toutes les {0}"] = "Repeat every {0}", ["Pas de répétition"] = "No repetition",
            ["Démarrer un programme"] = "Start a program", ["Envoyer un e-mail"] = "Send an email",
            ["Afficher un message"] = "Display a message", ["Gestionnaire COM personnalisé"] = "Custom COM handler",
            ["À : {0} - Sujet : {1}"] = "To: {0} - Subject: {1}", ["CLSID : {0}"] = "CLSID: {0}",
            ["Inactivité"] = "Idle", ["Alimentation"] = "Power", ["Réseau"] = "Network",
            ["Démarrer uniquement si l'ordinateur est inactif"] = "Start only if the computer is idle",
            ["Oui (inactif pendant {0})"] = "Yes (idle for {0})",
            ["Démarrer la tâche seulement si l'ordinateur est sur secteur"] = "Start the task only if the computer is on AC power",
            ["Arrêter la tâche si l'ordinateur passe sur batterie"] = "Stop the task if the computer switches to battery power",
            ["Permet de lancer la tâche manuellement à tout moment."] = "Allows the task to be started manually at any time.",
            ["Autoriser l'exécution à la demande"] = "Allow the task to be run on demand",
            ["Arrêter la tâche si elle dépasse le délai d'exécution"] = "Stop the task if it exceeds the execution time limit",
            ["Pas de limite"] = "No limit", ["{0} heure(s)"] = "{0} hour(s)",
            ["Évite qu'une tâche bloquée ne continue de s'exécuter indéfiniment."] = "Prevents a stuck task from continuing to run indefinitely.",
            ["Politique en cas d'instances multiples"] = "Multiple instance policy",
            ["Comportement si la tâche est déclenchée alors qu'elle s'exécute déjà."] = "Behavior when the task is triggered while it is already running.",
            ["Redémarrer en cas d'échec"] = "Restart on failure", ["{0} fois (intervalle : {1})"] = "{0} times (interval: {1})",
            ["Redémarre automatiquement le processus si un code d'erreur est renvoyé."] = "Automatically restarts the process if an error code is returned.",
            ["Supprimer la tâche si elle n'est plus planifiée"] = "Delete the task if it is no longer scheduled",
            ["Après {0} jour(s)"] = "After {0} day(s)",
            ["Nettoie automatiquement les tâches à usage unique expirées."] = "Automatically cleans up expired one-time tasks.",
            ["Visibilité de la tâche"] = "Task visibility", ["Masquée"] = "Hidden", ["Visible"] = "Visible",
            ["Définit si la tâche est cachée dans les interfaces utilisateur standard."] = "Controls whether the task is hidden in standard user interfaces.",
            ["Exécuter une nouvelle instance en parallèle"] = "Run a new instance in parallel",
            ["Arrêter l'instance existante et démarrer la nouvelle"] = "Stop the existing instance and start the new one",
            ["Opération réussie (0x0)"] = "The operation completed successfully (0x0)",
            ["La tâche est prête à s'exécuter (0x00041300)"] = "The task is ready to run (0x00041300)",
            ["Tâche actuellement en cours d'exécution (0x00041301)"] = "The task is currently running (0x00041301)",
            ["La tâche est désactivée (0x00041302)"] = "The task is disabled (0x00041302)",
            ["La tâche n'a pas encore été exécutée (0x00041303)"] = "The task has not yet run (0x00041303)",
            ["Aucune exécution ultérieure planifiée (0x00041304)"] = "No further runs are scheduled (0x00041304)",
            ["Un ou plusieurs déclencheurs non activés (0x00041305)"] = "One or more triggers are not enabled (0x00041305)",
            ["Dernière exécution interrompue par l'utilisateur (0x00041306)"] = "The last run was terminated by the user (0x00041306)",
            ["Arrêté pour dépassement de délai imparti (0x00041307)"] = "Stopped because the time limit was exceeded (0x00041307)",
            ["Le fichier spécifié est introuvable (0x80070002)"] = "The specified file was not found (0x80070002)",
            ["Accès refusé - Droits administrateur requis (0x80070005)"] = "Access denied - administrator privileges required (0x80070005)",
            ["Informations d'identification invalides (0x8004130F)"] = "Invalid credentials (0x8004130F)",
            ["Une instance est déjà en cours d'exécution (0x8004131F)"] = "An instance is already running (0x8004131F)",
            ["Code d'état : 0x{0:X8} ({1})"] = "Status code: 0x{0:X8} ({1})",
            ["Impossible d'accéder à la définition : {0}"] = "Cannot access the definition: {0}",
            ["Erreur XML : {0}"] = "XML error: {0}", ["Erreur de lecture : {0}"] = "Read error: {0}",
            ["NouvelleTache"] = "NewTask",
            ["Ex: /c start script.bat ou -ExecutionPolicy Bypass -File C:\\script.ps1"] = "E.g. /c start script.bat or -ExecutionPolicy Bypass -File C:\\script.ps1",
            ["Ex: C:\\Windows\\System32\\cmd.exe ou C:\\Scripts\\backup.bat"] = "E.g. C:\\Windows\\System32\\cmd.exe or C:\\Scripts\\backup.bat",
            ["Interactif"] = "Interactive", ["Service de planification non connecté."] = "Task Scheduler service is not connected.",
            ["La tâche '{0}' est introuvable."] = "Task '{0}' was not found.",
            ["Impossible de supprimer le dossier racine."] = "The root folder cannot be deleted.",
            ["📁 \\ (Bibliothèque du Planificateur)"] = "📁 \\ (Task Scheduler Library)",
            ["activée"] = "enabled", ["désactivée"] = "disabled", ["Non exécuté"] = "Not run",
            ["La définition XML a été copiée dans le presse-papiers."] = "The XML definition was copied to the clipboard.",
            ["Le XML a été copié dans le presse-papiers."] = "The XML was copied to the clipboard.",
            ["Les dossiers et tâches ont été synchronisés."] = "Folders and tasks were synchronized.",
            ["Démarrer seulement si la connexion réseau est disponible"] = "Start only if a network connection is available",
            ["Le nom de la tâche ne peut pas être vide."] = "The task name cannot be empty.",
            ["Le chemin du programme ou script à exécuter est obligatoire."] = "The program or script path is required."
        };

        public static LocalizationService Instance => LazyInstance.Value;
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Language { get; private set; } = "fr";
        public string LanguageDisplayName => Language == "en" ? "English" : "Français";
        public string WindowTitle => "Modern Task Scheduler v1.1.0";
        public string Subtitle => Translate("v1.1.0 - Gestion professionnelle des tâches");
        public string SearchPlaceholder => Translate("Rechercher une tâche (nom, description, auteur)...");
        public string RefreshText => Translate("Actualiser");
        public string NewTaskText => Translate("Nouvelle tâche");
        public string AboutTooltip => Translate("À propos de Modern Task Scheduler");
        public string FolderHeader => Translate("DOSSIERS DE LA BIBLIOTHÈQUE");
        public string CreateFolderTooltip => Translate("Créer un nouveau sous-dossier");
        public string NewText => Translate("Nouveau");
        public string DeleteFolderTooltip => Translate("Supprimer ce dossier");
        public string FilterAll => Translate("Toutes");
        public string FilterRunning => Translate("En cours");
        public string FilterReady => Translate("Prêtes");
        public string FilterDisabled => Translate("Désactivées");
        public string ExploreAllTooltip => Translate("Afficher récursivement toutes les tâches de toutes les branches");
        public string ExploreAll => Translate("Tout explorer");
        public string TasksWord => Translate("tâches");
        public string EmptyTitle => Translate("Aucune tâche sélectionnée");
        public string EmptyDescription => Translate("Sélectionnez une tâche dans la colonne de gauche pour afficher et gérer ses déclencheurs, actions, conditions et paramètres avancés, ou créez une nouvelle tâche dès maintenant.");
        public string CreateNewTask => Translate("Créer une nouvelle tâche");
        public string GeneralTab => Translate("Général");
        public string TriggersTab => Translate("Déclencheurs");
        public string ActionsTab => Translate("Actions");
        public string ConditionsTab => Translate("Conditions");
        public string SettingsTab => Translate("Paramètres");
        public string XmlTab => Translate("Définition XML");
        public string CopyXml => Translate("Copier le XML dans le presse-papiers");
        public string EditorSubtitle => Translate("Configuration avancée selon les spécifications Windows Task Scheduler (100% options)");
        public string Cancel => Translate("Annuler");
        public string SaveTask => Translate("Enregistrer la tâche");

        private LocalizationService()
        {
            try
            {
                string settingsFile = GetSettingsFilePath();
                if (File.Exists(settingsFile))
                {
                    string? savedLanguage = JsonSerializer.Deserialize<string>(File.ReadAllText(settingsFile));
                    if (savedLanguage is "en" or "fr") Language = savedLanguage;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Language = "fr";
            }
        }

        public void SetLanguage(string language)
        {
            if (language is not ("en" or "fr")) throw new ArgumentOutOfRangeException(nameof(language));
            if (Language == language) return;

            string settingsFile = GetSettingsFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)!);
            File.WriteAllText(settingsFile, JsonSerializer.Serialize(language));
            Language = language;
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(LanguageDisplayName));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(SearchPlaceholder));
            OnPropertyChanged(nameof(RefreshText));
            OnPropertyChanged(nameof(NewTaskText));
            OnPropertyChanged(nameof(AboutTooltip));
            OnPropertyChanged(nameof(FolderHeader));
            OnPropertyChanged(nameof(CreateFolderTooltip));
            OnPropertyChanged(nameof(NewText));
            OnPropertyChanged(nameof(DeleteFolderTooltip));
            OnPropertyChanged(nameof(FilterAll));
            OnPropertyChanged(nameof(FilterRunning));
            OnPropertyChanged(nameof(FilterReady));
            OnPropertyChanged(nameof(FilterDisabled));
            OnPropertyChanged(nameof(ExploreAllTooltip));
            OnPropertyChanged(nameof(ExploreAll));
            OnPropertyChanged(nameof(TasksWord));
            OnPropertyChanged(nameof(EmptyTitle));
            OnPropertyChanged(nameof(EmptyDescription));
            OnPropertyChanged(nameof(CreateNewTask));
            OnPropertyChanged(nameof(GeneralTab));
            OnPropertyChanged(nameof(TriggersTab));
            OnPropertyChanged(nameof(ActionsTab));
            OnPropertyChanged(nameof(ConditionsTab));
            OnPropertyChanged(nameof(SettingsTab));
            OnPropertyChanged(nameof(XmlTab));
            OnPropertyChanged(nameof(CopyXml));
            OnPropertyChanged(nameof(EditorSubtitle));
            OnPropertyChanged(nameof(Cancel));
            OnPropertyChanged(nameof(SaveTask));
        }

        public string Translate(string text) => Translate(text, Language);

        public string Translate(string text, string language)
        {
            if (language == "en")
            {
                return _english.TryGetValue(text, out string? englishText) ? englishText : text;
            }

            foreach (KeyValuePair<string, string> translation in _english)
            {
                if (translation.Value == text) return translation.Key;
            }

            return text;
        }

        public string Format(string frenchTemplate, string englishTemplate, params object?[] values) =>
            string.Format(System.Globalization.CultureInfo.CurrentCulture, Language == "en" ? englishTemplate : frenchTemplate, values);

        public string LanguageFor(string frenchFilter) => Translate(frenchFilter);

        private static string GetSettingsFilePath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ModernTaskScheduler",
            "language.json");

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
