# FLUENCY - Windows 11 Management Center

## Overview
**Fluency** is a comprehensive Windows 11 management and optimization application built with WinUI 3 and .NET 8. It provides a modern, Fluent Design System interface for managing various aspects of your Windows 11 system.

## Features

### 1. **Dashboard**
- **System Information Display**: Real-time OS version, computer name, processor, and RAM information
- **Quick Actions**: Fast access to PowerShell, Settings, and Windows Update
- **Quick Links**: Direct shortcuts to System Settings, Apps & Features, and Startup apps

### 2. **Interactive Terminal**
- **Multi-Shell Support**: 
  - PowerShell 7
  - PowerShell 5
  - Command Prompt
- **Real-time Output**: Stream live command results
- **Command Input**: Type and execute commands directly
- **Session Management**: Start/stop shell sessions with visual feedback

### 3. **WSL Manager**
- **WSL Status Check**: Verify WSL installation and status
- **Install WSL**: Easy WSL installation
- **Distro Management**: 
  - List available online distros
  - View installed distributions
  - Remove distros
  - Refresh distro list

### 4. **Windows Debloater** ⭐ (NEW)
Powerful debloating and optimization tools using PowerShell:

#### Debloat Operations:
- **Remove Bloatware Apps**: Removes common bloatware like Candy Crush, Twitter, Slack, TikTok, Disney+, Spotify, Netflix
- **Disable Telemetry**: Disable Windows data collection and diagnostic data
- **Disable Cortana**: Remove Cortana voice assistant
- **Disable OneDrive**: Disable OneDrive cloud sync
- **Remove Store Apps**: Clean up unnecessary Microsoft Store applications
- **Clean Temp Files**: Remove temporary system files

#### Features:
- Real-time operation logging
- Progress tracking
- Error handling and reporting
- One-click debloat operations

### 5. **Startup Apps Manager** ⭐ (NEW)
Comprehensive startup application management:

#### Capabilities:
- **Registry-based Startup**: Manage HKCU\Run registry entries
- **Startup Folder**: Control startup folder applications
- **Application Details**: View app name, path, and source
- **Quick Toggle**: Enable/disable startup apps
- **Remove Apps**: Remove apps from startup
- **Refresh List**: Update the application list

### 6. **System Tweaks** ⭐ (NEW)
Advanced Windows 11 optimization tweaks:

#### Performance Tweaks:
- Disable Visual Effects
- Disable Animations
- Disable Transparency Effects

#### Privacy & Security:
- Disable Activity History
- Disable Game Bar
- Disable Cloud Sync

#### UI Enhancements:
- Restore Classic Context Menu (Windows 10-style)

#### Features:
- Confirmation dialogs before applying changes
- Admin elevation support
- Success/error notifications
- Easy rollback documentation

## Design & UX

### Fluent Design System
- **Mica Backdrop**: Modern frosted glass effect for window backgrounds
- **Layered Surfaces**: Proper visual hierarchy with LayerFillColor
- **Rounded Corners**: Modern corner radius (8px default)
- **Spacing**: Consistent 16-20px spacing between elements
- **Color Scheme**: Uses theme-aware colors for light/dark modes
- **Typography**: Modern font hierarchy with semantic text styles

### Navigation
- **Sidebar NavigationView**: Easy navigation between sections
- **Icon-based Navigation**: Clear Segoe MDL2 icons for each section
- **Organized Structure**:
  - Dashboard
  - Terminal
  - WSL Manager
  - Debloater (separated with visual divider)
  - Startup Apps
  - System Tweaks

### Updated Styling
- **Removed Green Terminal Text**: Modern colored text matching system theme
- **Professional Cards**: Border and fill colors that match Fluent Design
- **Modern Input Controls**: Updated text boxes with proper spacing
- **Consistent Buttons**: Accent colors for primary actions

## Technical Details

### Technology Stack
- **Framework**: .NET 8.0
- **UI Framework**: WinUI 3 (Windows App SDK 1.8)
- **Minimum Windows**: Windows 10 Build 19041
- **Platforms**: x86, x64, ARM64

### Architecture
- **Modular Design**: Each feature in separate XAML pages
- **Code-behind Pattern**: Proper separation of concerns
- **Async Operations**: Non-blocking UI operations
- **Process Management**: Safe PowerShell script execution

### Key Classes
- `DashboardPage`: System information and quick actions
- `TerminalPage`: Interactive shell execution
- `WslPage`: WSL management interface
- `DebloaterPage`: Debloating operations
- `StartupAppsPage`: Startup application management
- `SystemTweaksPage`: System optimization tweaks
- `StartupApp`: Data model for startup applications

## Usage Instructions

### Running the Application
1. Launch Fluency from the Start menu or application launcher
2. Grant administrator privileges if prompted (required for some features)
3. Use the sidebar to navigate between different management sections

### Debloater Usage
1. Navigate to the **Debloater** section
2. Click any debloat operation button
3. View operation progress in the operation log
4. Use "Clear Log" to reset the log display

### Startup Manager Usage
1. Go to **Startup Apps**
2. Click "Refresh" to load current startup apps
3. Toggle switches to enable/disable apps
4. Click "Remove" to permanently remove from startup

### System Tweaks Usage
1. Open **System Tweaks**
2. Select a tweak to apply
3. Confirm the action in the dialog
4. Wait for the operation to complete
5. Restart your computer if prompted

## Requirements & Notes

### Administrator Privileges
- Some operations (debloating, tweaks, WSL) require administrator rights
- Some features work with limited privileges (Terminal, System Info)

### Safety Considerations
- Always create a system restore point before using debloater
- Some tweaks may require a system restart
- Backup important data before using cleanup utilities

### Compatibility
- Windows 11 optimized (works on Windows 10.0.19041+)
- English language UI
- Dark/Light theme support

## Future Enhancements
- System backup and restore functionality
- Network optimization tools
- Disk cleanup and defragmentation UI
- Performance monitoring dashboard
- Batch operations
- Settings persistence
- Custom debloat profiles

## License & Attribution
- Uses Microsoft.Windows.SDK.BuildTools
- Uses Microsoft.WindowsAppSDK 1.8
- Follows Fluent Design System guidelines

---

**Fluency** - Making Windows 11 management simple, beautiful, and powerful.
