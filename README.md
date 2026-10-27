# Modern Task Scheduler

**Version: 1.0.0** | **Status: Stable & Production-Ready** ✅

A modern, feature-rich Windows application for managing system tasks with an intuitive graphical interface built with WinUI and .NET 8. Professional-grade task scheduling management with a clean, modern UI.

## 🌟 Key Features

- **Task Management**: Browse, view, and manage Windows scheduled tasks with ease
- **Task Creation/Editing**: Create and modify tasks with full support for triggers, actions, and conditions
- **Folder Organization**: Navigate tasks through an organized folder hierarchy
- **Search & Filtering**: Quick search and advanced filtering by name and status
- **Task Control**: Run, stop, enable/disable tasks directly from the application
- **Task Export**: Export task configurations to XML for backup and portability
- **Real-time Monitoring**: Real-time task status updates and activity monitoring
- **Professional UI**: Modern Fluent Design with responsive layout
- **Multi-platform Support**: Build for x86, x64, and ARM64 architectures

## 💻 Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: WinUI 3 / Windows App SDK 2.5.1
- **Architecture**: MVVM (Model-View-ViewModel) Pattern
- **Toolkit**: MVVM Community Toolkit 8.4.2
- **Task Integration**: Windows Task Scheduler COM Interop
- **Localization**: English UI with professional styling

## 📋 Project Structure

```
ModernTaskScheduler/
├── Services/              # Business logic and Task Scheduler integration
│   ├── ITaskSchedulerService.cs
│   ├── TaskSchedulerService.cs
│   └── StaTask.cs
├── ViewModels/            # MVVM ViewModels for UI logic
│   ├── MainViewModel.cs
│   ├── TaskItemViewModel.cs
│   └── TaskEditViewModel.cs
├── Views/                 # XAML Windows and dialogs
│   ├── MainWindow.xaml
│   ├── TaskCreateEditWindow.xaml
│   └── AboutWindow.xaml
├── Models/                # Data models and structures
│   ├── TaskCreationModel.cs
│   ├── TaskFolderItem.cs
│   ├── TriggerDisplayItem.cs
│   ├── ActionDisplayItem.cs
│   ├── ConditionDisplayItem.cs
│   └── SettingDisplayItem.cs
├── Converters/            # XAML value converters
│   └── Converters.cs
├── Assets/                # Application icons and images
├── Properties/            # Build and publish configurations
└── Package.appxmanifest   # Windows App Package manifest
```

## 🚀 Getting Started

### Prerequisites

- **OS**: Windows 10 (Build 19041) or later
- **Runtime**: .NET 8 Runtime
- **Dev Tool** (for development): Visual Studio 2024 with .NET desktop development workload

### Installation

#### From Source
```bash
git clone https://github.com/BubbleWrapPro/ModernTaskScheduler.git
cd ModernTaskScheduler
dotnet build
dotnet run
```

#### Building Release Executable
```bash
dotnet publish -c Release -r win-x64
```

## 🔧 Core Components

### Services
| Class | Purpose |
|-------|---------|
| `ITaskSchedulerService` | Interface for task operations abstraction |
| `TaskSchedulerService` | Full implementation of task management operations |
| `StaTask` | Helper for Single-Threaded Apartment COM operations |

### ViewModels
| Class | Purpose |
|-------|---------|
| `MainViewModel` | Central application state and coordination |
| `TaskItemViewModel` | Individual task representation and operations |
| `TaskEditViewModel` | Task creation/editing form logic |

### Data Models
| Class | Purpose |
|-------|---------|
| `TaskCreationModel` | Data transfer object for new tasks |
| `TaskFolderItem` | Folder hierarchy representation |
| `TriggerDisplayItem` | Task trigger information display |
| `ActionDisplayItem` | Task action information display |
| `ConditionDisplayItem` | Task condition information display |
| `SettingDisplayItem` | Task settings information display |

### Converters
- `BoolToVisibilityConverter`: Convert boolean values to UI visibility
- `NullToVisibilityConverter`: Convert null checks to UI visibility
- `StateToColorConverter`: Convert task states to color representation
- `BoolToStringConverter`: Convert boolean values to string labels

## 📊 Architecture Highlights

### MVVM Pattern
The application follows strict MVVM architecture with:
- **View Layer**: XAML (MainWindow, TaskCreateEditWindow, AboutWindow)
- **ViewModel Layer**: Observable objects using MVVM Community Toolkit
- **Model Layer**: Data structures and business logic
- **Service Layer**: Task Scheduler integration abstraction

### Async/Await Pattern
- All service operations are fully async
- Responsive UI with proper dispatcher usage
- Cancellation token support for long operations

### Dependency Injection
- Services are injected into ViewModels
- Clean separation of concerns
- Testable architecture

## 🔐 Permissions & Requirements

**Note**: This application requires administrative privileges to:
- Create scheduled tasks
- Modify existing tasks
- Delete scheduled tasks
- Run tasks on demand

The application will request elevation when needed.

## 🎨 Branding & Identity

- **Company**: BubbleWrapPro
- **Product Name**: Modern Task Scheduler
- **Version**: 1.0.0
- **Brand Color**: #2E5090 (Primary), #0078D4 (Accent)
- **Design Language**: Microsoft Fluent Design System

## 📦 Packaging & Distribution

The project is configured for:
- **Self-contained executable** publishing
- **Multi-platform builds** (x86, x64, ARM64)
- **MSIX packaging** for Windows App Store distribution
- **Ready-to-Run (R2R)** compilation for optimized startup

Publish profiles available:
- `win-x64.pubxml` - 64-bit Windows
- `win-x86.pubxml` - 32-bit Windows
- `win-arm64.pubxml` - ARM64 Windows

## 🧪 Testing

The codebase is designed for testability:
- Service interfaces enable mocking
- ViewModel logic is isolated from UI
- MVVM Community Toolkit enables testing patterns

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is open source and available under the MIT License.
See LICENSE file for details.

## 👨‍💻 Author

**BubbleWrapPro**
- GitHub: [@BubbleWrapPro](https://github.com/BubbleWrapPro)
- Email: Contact via GitHub

## 📞 Support

For issues, feature requests, or questions:
1. Check existing [GitHub Issues](https://github.com/BubbleWrapPro/ModernTaskScheduler/issues)
2. Create a new issue with detailed information
3. Include screenshots or error logs when applicable

## 🗂️ Project Status

- ✅ **v1.0.0** - Initial stable release
  - Core task management features
  - Professional UI with Fluent Design
  - Full CRUD operations on tasks
  - Search and filtering
  - About dialog with app information

## 📚 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet)
- [Windows Task Scheduler](https://learn.microsoft.com/en-us/windows/win32/taskschd/about-the-task-scheduler)

---

**Last Updated**: January 2024  
**Maintainer**: BubbleWrapPro  
**Status**: 🟢 Active & Maintained
