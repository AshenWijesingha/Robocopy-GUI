# Build and Deployment Guide

This guide provides detailed instructions for building and deploying the Robocopy GUI application.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Building the Application](#building-the-application)
3. [Creating a Standalone Executable](#creating-a-standalone-executable)
4. [Creating an Installer](#creating-an-installer)
5. [Testing](#testing)
6. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

1. **Visual Studio 2022** (or later)
   - Download: https://visualstudio.microsoft.com/downloads/
   - Edition: Community, Professional, or Enterprise
   - Required Workloads:
     * .NET desktop development
     * Windows Presentation Foundation (WPF)

2. **.NET 8.0 SDK** (or later)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```bash
     dotnet --version
     ```

3. **Git** (optional, for version control)
   - Download: https://git-scm.com/downloads

### Optional Software

1. **Inno Setup** (for creating installers)
   - Download: https://jrsoftware.org/isdl.php
   
2. **WiX Toolset** (alternative installer creator)
   - Download: https://wixtoolset.org/releases/

## Building the Application

### Method 1: Using Visual Studio

#### Step 1: Open the Solution
1. Launch Visual Studio 2022
2. Click "Open a project or solution"
3. Navigate to `RobocopyGUI.sln`
4. Click "Open"

#### Step 2: Restore NuGet Packages
Visual Studio should automatically restore packages. If not:
1. Right-click on the solution in Solution Explorer
2. Select "Restore NuGet Packages"
3. Wait for completion

#### Step 3: Build the Solution
1. Select build configuration:
   - **Debug**: For development (includes debug symbols)
   - **Release**: For production (optimized)

2. Build the solution:
   - Press `Ctrl+Shift+B`, or
   - Menu: Build → Build Solution, or
   - Right-click solution → Build Solution

3. Check for errors in the Error List window

#### Step 4: Run the Application
1. Press `F5` to run with debugging, or
2. Press `Ctrl+F5` to run without debugging
3. The application should launch

### Method 2: Using Command Line

#### Step 1: Navigate to Project Directory
```bash
cd path\to\robocopy-gui
```

#### Step 2: Restore Dependencies
```bash
dotnet restore
```

#### Step 3: Build the Project

**Debug Build:**
```bash
dotnet build --configuration Debug
```

**Release Build:**
```bash
dotnet build --configuration Release
```

#### Step 4: Run the Application
```bash
dotnet run --project RobocopyGUI\RobocopyGUI.csproj
```

### Build Output Locations

After building, the executable will be in:
- **Debug**: `RobocopyGUI\bin\Debug\net8.0-windows\RobocopyGUI.exe`
- **Release**: `RobocopyGUI\bin\Release\net8.0-windows\RobocopyGUI.exe`

## Creating a Standalone Executable

> **Important:** Before publishing, ensure no previous instances of the application are running 
> and the publish output folder is not locked by any process (antivirus, Windows Explorer, etc.).

### Prerequisites for Single-File Publishing

The project is pre-configured with single-file publishing settings in `RobocopyGUI.csproj`:
- `PublishSingleFile=true`
- `SelfContained=true`
- `IncludeNativeLibrariesForSelfExtract=true`
- `EnableCompressionInSingleFile=true`

### Method 1: Simple Publish (Recommended)

Since the project is pre-configured with single-file publishing settings, you can simply run:

```bash
# Clean any previous build artifacts first
dotnet clean -c Release

# Publish with default settings (single file, self-contained, win-x64)
dotnet publish -c Release -r win-x64
```

Output: `RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish\RobocopyGUI.exe`

### Method 2: Framework-Dependent (Smaller Size)

This requires .NET 8.0 Runtime to be installed on target machine.

```bash
dotnet clean -c Release
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false
```

Output: `RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish\`

**Pros:**
- Smaller file size (~1-2 MB)
- Faster build time

**Cons:**
- Requires .NET 8.0 Runtime on target machine

### Method 3: Self-Contained Single File (Larger Size)

Includes .NET Runtime - no installation required on target machine.

```bash
# Clean first to avoid "Access Denied" errors
dotnet clean -c Release

# Publish self-contained single file
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish\RobocopyGUI.exe`

**Pros:**
- No dependencies required
- Works on any Windows 10+ machine
- Single executable file

**Cons:**
- Larger file size (~70-80 MB)
- Longer build time

### Method 4: Trimmed Self-Contained (Optimized)

Removes unused code for smaller size.

```bash
dotnet clean -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true
```

**Pros:**
- Smaller than regular self-contained (~40-50 MB)
- No dependencies required

**Cons:**
- May have compatibility issues with reflection-heavy code
- Requires thorough testing

### Publish for Different Architectures

**64-bit Windows (default):**
```bash
dotnet clean -c Release
dotnet publish -c Release -r win-x64
```

**32-bit Windows:**
```bash
dotnet clean -c Release
dotnet publish -c Release -r win-x86
```

**ARM64 Windows:**
```bash
dotnet clean -c Release
dotnet publish -c Release -r win-arm64
```

### Troubleshooting Publish Errors

#### "Access to the path is denied" Error

This error occurs when the output executable is locked. Solutions:

1. **Close all instances of the application** before publishing
2. **Run `dotnet clean` before publishing:**
   ```bash
   dotnet clean -c Release
   dotnet publish -c Release
   ```
3. **Delete the publish folder manually:**
   ```bash
   # Windows CMD
   rmdir /s /q RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish
   
   # PowerShell
   Remove-Item -Recurse -Force RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish
   ```
4. **Temporarily disable antivirus** software that may be scanning the output
5. **Close Windows Explorer** if it has the publish folder open
6. **Restart Visual Studio** if it has locks on files

#### "GenerateBundle task failed" Error

This is typically caused by the same file locking issues. Follow the steps above.

#### "Unexpected error occurred" Error

1. Check the detailed log files in:
   - `RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish\`
   - `%TEMP%\` folder for MSBuild logs
2. Run with verbose logging:
   ```bash
   dotnet publish -c Release -v detailed > build.log 2>&1
   ```
3. Ensure you have the latest .NET SDK installed:
   ```bash
   dotnet --version
   ```

## Creating an Installer

### Option 1: Using Inno Setup

#### Step 1: Install Inno Setup
Download and install from https://jrsoftware.org/isdl.php

#### Step 2: Create Installer Script

Create `setup.iss` file:

```ini
[Setup]
AppName=Robocopy GUI
AppVersion=1.0.0
AppPublisher=Your Name
AppPublisherURL=https://github.com/yourusername/robocopy-gui
DefaultDirName={autopf}\RobocopyGUI
DefaultGroupName=Robocopy GUI
OutputDir=installer
OutputBaseFilename=RobocopyGUI-Setup-v1.0.0
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\RobocopyGUI.exe

[Files]
Source: "RobocopyGUI\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Robocopy GUI"; Filename: "{app}\RobocopyGUI.exe"
Name: "{group}\Uninstall Robocopy GUI"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Robocopy GUI"; Filename: "{app}\RobocopyGUI.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\RobocopyGUI.exe"; Description: "Launch Robocopy GUI"; Flags: postinstall nowait skipifsilent
```

#### Step 3: Compile Installer
1. Open `setup.iss` in Inno Setup
2. Click "Build" → "Compile"
3. Installer will be created in `installer\` directory

### Option 2: Using WiX Toolset

#### Step 1: Install WiX
```bash
dotnet tool install --global wix
```

#### Step 2: Create Product.wxs

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" 
           Name="Robocopy GUI" 
           Language="1033" 
           Version="1.0.0" 
           Manufacturer="Your Name" 
           UpgradeCode="YOUR-GUID-HERE">
    
    <Package InstallerVersion="200" 
             Compressed="yes" 
             InstallScope="perMachine" />

    <MajorUpgrade DowngradeErrorMessage="A newer version of [ProductName] is already installed." />
    
    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="Robocopy GUI" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
  </Product>

  <Fragment>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="RobocopyGUI" />
      </Directory>
    </Directory>
  </Fragment>

  <Fragment>
    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <!-- Add your files here -->
    </ComponentGroup>
  </Fragment>
</Wix>
```

#### Step 3: Build Installer
```bash
wix build Product.wxs -o RobocopyGUI.msi
```

## Testing

### Unit Testing

Run all tests:
```bash
dotnet test
```

Run with code coverage:
```bash
dotnet test /p:CollectCoverage=true
```

### Manual Testing Checklist

- [ ] Application launches without errors
- [ ] Browse buttons work for source/destination
- [ ] All presets apply correct settings
- [ ] Command preview updates correctly
- [ ] Copy operation executes successfully
- [ ] Progress bar shows during copy
- [ ] Log displays output correctly
- [ ] Stop button terminates operation
- [ ] Profile save/load works
- [ ] Settings persist across sessions
- [ ] Application handles errors gracefully

### Test Scenarios

1. **Small Files Test**
   - Create 1000 small files (1 KB each)
   - Test with different thread counts
   - Verify completion

2. **Large Files Test**
   - Create 10 large files (100 MB each)
   - Test with thread count = 1
   - Verify completion

3. **Network Copy Test**
   - Copy to/from network location
   - Test with retry settings
   - Verify resilience

4. **Mirror Mode Test**
   - Create test folders with extras
   - Use Mirror mode
   - Verify deletion of extras

5. **Error Handling Test**
   - Invalid paths
   - Locked files
   - Insufficient permissions
   - Network interruption

## Troubleshooting

### Common Build Errors

#### Error: SDK not found
**Solution:**
```bash
# Install .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8

# Verify installation
dotnet --version
```

#### Error: NuGet restore failed
**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

#### Error: Missing Windows SDK
**Solution:**
- Install Windows 10 SDK via Visual Studio Installer
- Workloads → Windows application development

### Runtime Errors

#### Error: Application won't start
**Check:**
1. .NET 8.0 Runtime installed (if using framework-dependent build)?
2. Windows 10 version 1809 or later?
3. All dependencies included in publish?

#### Error: Robocopy not found
**Solution:**
- Robocopy is built into Windows 10+
- Check Windows version
- Try running `robocopy /?` in CMD

#### Error: Access denied
**Solution:**
- Run as Administrator
- Check folder permissions
- Disable antivirus temporarily

### Performance Issues

#### Slow copy speed
**Try:**
- Increase thread count (8-16)
- Use SSD for destination
- Disable antivirus scanning
- Close other applications

#### High CPU usage
**Try:**
- Decrease thread count
- Enable `/NP` flag (already enabled)
- Check for malware

## Deployment Checklist

Before releasing:

- [ ] Update version number in all files
- [ ] Build in Release configuration
- [ ] Run all tests
- [ ] Create installer
- [ ] Test installer on clean machine
- [ ] Create release notes
- [ ] Tag release in Git
- [ ] Upload to distribution platform
- [ ] Update documentation

## Distribution

### GitHub Releases
```bash
# Tag release
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# Upload installer and exe to GitHub Releases
```

### Microsoft Store
Follow Microsoft Store submission guidelines:
https://docs.microsoft.com/windows/uwp/publish/

### Chocolatey Package
Create package definition and submit to Chocolatey:
https://chocolatey.org/docs/create-packages

## Maintenance

### Updating Dependencies
```bash
# Check for updates
dotnet list package --outdated

# Update specific package
dotnet add package System.Text.Json --version 8.0.1

# Update all packages
dotnet outdated --upgrade
```

### Security Scanning
```bash
# Check for vulnerabilities
dotnet list package --vulnerable
```

## Support

For issues or questions:
- GitHub Issues: https://github.com/yourusername/robocopy-gui/issues
- Email: support@robocopy-gui.com
- Discord: https://discord.gg/robocopy-gui

---

**Last Updated:** February 2026
