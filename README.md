# Modern Task Scheduler

A modern, feature-rich Windows application for managing system tasks with an intuitive graphical interface built with WinUI and .NET 8.

## Features

- **Task Management**: Browse, view, and manage Windows scheduled tasks
- **Task Creation/Editing**: Create and modify tasks with full support for triggers, actions, and conditions
- **Folder Organization**: Navigate tasks through an organized folder hierarchy
- **Search & Filtering**: Quick search and filter tasks by name and status
- **Task Control**: Run, stop, enable/disable tasks directly from the application
- **Task Export**: Export task configurations to XML for backup
- **Real-time Monitoring**: Real-time task status updates and activity monitoring

## Technology Stack

- **Framework**: .NET 8
- **UI**: WinUI 3 / Windows App SDK
- **Architecture**: MVVM (Model-View-ViewModel)
- **Toolkit**: MVVM Community Toolkit
- **Task Interop**: Windows Task Scheduler COM interop
- **Localization**: French language support

## Project Structure

```
ModernTaskScheduler/
├── Services/              # Business logic and Task Scheduler integration
├── ViewModels/            # MVVM ViewModels
├── Views/                 # XAML windows and dialogs
├── Models/                # Data models and structures
├── Converters/            # XAML value converters
├── Assets/                # Application icons and images
└── Properties/            # Build and publish configurations
```

## Getting Started

### Prerequisites

- Windows 10 (Build 19041) or later
- .NET 8 Runtime
- Visual Studio 2024 or later (for development)

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

## Key Classes

### Services
- `ITaskSchedulerService`: Interface for Task Scheduler operations
- `TaskSchedulerService`: Implementation of task management operations
- `StaTask`: Helper for STA thread context

### ViewModels
- `MainViewModel`: Central application state management
- `TaskItemViewModel`: Individual task representation
- `TaskEditViewModel`: Task creation/editing forms

### Models
- `TaskCreationModel`: Data transfer object for task creation
- `TaskFolderItem`: Folder hierarchy representation
- `TriggerDisplayItem`, `ActionDisplayItem`, `ConditionDisplayItem`: Task properties

### Converters
- `BoolToVisibilityConverter`: Boolean to visibility conversion
- `NullToVisibilityConverter`: Null to visibility conversion
- `StateToColorConverter`: Task state to color conversion
- `BoolToStringConverter`: Boolean to string conversion

## Development

The project uses:
- **MVVM Community Toolkit** for reactive property binding
- **Windows Task Scheduler COM API** for system integration
- **WinUI 3** for modern Windows UI

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and features.

## License

This project is open source and available under the MIT License.

## Author

BubbleWrapPro

---

**Note**: This application requires administrative privileges to modify or create scheduled tasks.
