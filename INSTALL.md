# Modern Task Scheduler - Installation Guide

## Version 1.0.0 - Professional Release

Welcome to Modern Task Scheduler! This guide will help you install and run the application.

## Prerequisites

- **Operating System**: Windows 10 (Build 19041) or later
- **Architecture**: x86, x64, or ARM64
- **Requirements**: No additional runtime required (.NET 8 is bundled)
- **Privileges**: Administrator rights needed for task management operations

## Installation Methods

### Method 1: Direct Download & Run (Recommended for End Users)

1. Download the release package for your architecture:
   - `ModernTaskScheduler-x64.zip` (64-bit - **Recommended for most users**)
   - `ModernTaskScheduler-x86.zip` (32-bit)
   - `ModernTaskScheduler-arm64.zip` (ARM64 devices)

2. Extract the ZIP file to your preferred location:
   ```
   C:\Program Files\ModernTaskScheduler\
   ```

3. Run `ModernTaskScheduler.exe` from the extracted folder

4. If prompted by Windows, click **"Yes"** to grant administrator privileges

### Method 2: Build from Source (For Developers)

#### Prerequisites for Building
- Windows 10 (Build 19041) or later
- Visual Studio 2024 (Community, Professional, or Enterprise)
  - Include ".NET desktop development" workload
- .NET 8 SDK (installed with Visual Studio)

#### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/BubbleWrapPro/ModernTaskScheduler.git
   cd ModernTaskScheduler
   ```

2. Build the project:
   ```bash
   dotnet build -c Release
   ```

3. Run from source:
   ```bash
   dotnet run --configuration Release
   ```

4. Or create a self-contained executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained
   ```

5. The executable is located at:
   ```
   bin/x64/Release/net8.0-windows10.0.19041.0/win-x64/ModernTaskScheduler.exe
   ```

### Method 3: Create Shortcut (Optional)

To make accessing the application easier:

1. Right-click on `ModernTaskScheduler.exe`
2. Select **"Create shortcut"**
3. Move the shortcut to your Desktop or Start Menu folder

**Alternative**: Pin to Start Menu:
1. Right-click `ModernTaskScheduler.exe`
2. Select **"Pin to Start"**

## Application Startup

### First Launch

1. Double-click `ModernTaskScheduler.exe`
2. Windows may display a security prompt - click **"Run"** to proceed
3. The application will request administrator privileges
4. Accept the UAC prompt to continue

### Application Features Available After Launch

✓ Browse all Windows scheduled tasks
✓ Create new scheduled tasks
✓ Edit existing tasks
✓ Delete tasks
✓ Run tasks on demand
✓ Stop running tasks
✓ Enable/disable tasks
✓ Organize tasks in folders
✓ Search for specific tasks
✓ Export task information to XML
✓ View real-time task status

## System Integration

### Windows Scheduled Tasks
The application directly accesses Windows Task Scheduler through COM interop:
- All modifications are saved immediately to the system
- No separate database or configuration files needed
- Works seamlessly with existing Windows task scheduler

### File Locations
- **Configuration**: Integrated with Windows Task Scheduler (no separate config needed)
- **Logs**: Standard Windows Event Viewer
- **Cache**: Temporary memory-based caching (no disk storage)

## Troubleshooting

### Issue: Application won't start

**Solution**: 
1. Ensure you're running Windows 10 Build 19041 or later
2. Check that you have administrator privileges
3. Try running in compatibility mode:
   - Right-click `ModernTaskScheduler.exe`
   - Select **"Properties"** → **"Compatibility"**
   - Select **"Run this program as an administrator"**
   - Click **"Apply"** and **"OK"**

### Issue: Cannot modify tasks

**Solution**:
1. The application requires administrator privileges
2. Windows will show a UAC prompt on startup
3. Click **"Yes"** to grant permissions
4. If still having issues, run in administrator mode (see above)

### Issue: Tasks not displaying

**Solution**:
1. Refresh the task list: Click **"Refresh"** button
2. Wait for tasks to load (first load may take a few seconds)
3. If still not showing, check Windows Task Scheduler directly:
   - Press `Win + R`
   - Type `taskschd.msc`
   - Press **Enter**

### Issue: About window won't open

**Solution**: This is non-critical. The application still functions normally.

## Uninstallation

### Option 1: Simple Removal
1. Delete the folder containing `ModernTaskScheduler.exe`
2. That's it! No registry entries are created.

### Option 2: Remove from Start Menu
1. Right-click shortcut in Start Menu
2. Select **"Uninstall"** or **"Remove from this list"**

## Updates

### Checking for Updates
1. Click the **"About"** button in the application
2. Check the version number (currently v1.0.0)
3. Visit [GitHub Releases](https://github.com/BubbleWrapPro/ModernTaskScheduler/releases) for updates

### Updating
1. Download the latest version
2. Extract to the same folder
3. Click **"Yes"** to replace existing files
4. Restart the application

## System Requirements by Architecture

### 64-bit (x64) - **Recommended**
- **File**: `ModernTaskScheduler-x64.zip`
- **Executable Size**: ~130 MB (includes runtime)
- **Memory**: 100-200 MB during operation
- **Disk Space**: 200 MB minimum
- **Recommended for**: Most modern computers

### 32-bit (x86)
- **File**: `ModernTaskScheduler-x86.zip`
- **Executable Size**: ~120 MB (includes runtime)
- **Memory**: 80-150 MB during operation
- **Disk Space**: 180 MB minimum
- **Recommended for**: Older hardware

### ARM64
- **File**: `ModernTaskScheduler-arm64.zip`
- **Executable Size**: ~125 MB (includes runtime)
- **Memory**: 100-200 MB during operation
- **Disk Space**: 200 MB minimum
- **Recommended for**: Surface Pro X, ARM-based Windows devices

## Performance

- **Startup Time**: Typically 2-4 seconds on SSD
- **Memory Usage**: 80-200 MB depending on task count
- **Refresh Rate**: Tasks update automatically every 5 seconds
- **Supported Tasks**: Handles 1000+ tasks efficiently

## Security Notes

⚠️ **Important**:
- This application requires administrative privileges to modify tasks
- Only administrators can run or delete tasks
- All operations are logged by Windows Task Scheduler
- No sensitive data is stored locally
- Network access is not required for local task management

## Getting Help

### Documentation
- [README.md](README.md) - Project overview
- [GitHub Wiki](https://github.com/BubbleWrapPro/ModernTaskScheduler/wiki) - Detailed guides

### Support Channels
1. **GitHub Issues**: [Report a bug](https://github.com/BubbleWrapPro/ModernTaskScheduler/issues)
2. **GitHub Discussions**: [Ask questions](https://github.com/BubbleWrapPro/ModernTaskScheduler/discussions)

## Technical Support

### System Information to Provide
When reporting issues, please include:
- Windows version (Settings → System → About)
- System architecture (32-bit or 64-bit)
- Application version (About dialog)
- Error messages or symptoms
- Steps to reproduce the issue

## Quick Start After Installation

1. **Launch**: Run `ModernTaskScheduler.exe`
2. **Grant Permission**: Accept administrator UAC prompt
3. **View Tasks**: Wait for tasks to load (5-10 seconds)
4. **Search**: Use the search box to find specific tasks
5. **Run Task**: Right-click a task and select "Run"
6. **Learn More**: Click the "About" button for feature details

---

**Version**: 1.0.0  
**Last Updated**: January 2024  
**Status**: ✅ Production Ready  
**Support**: https://github.com/BubbleWrapPro/ModernTaskScheduler
