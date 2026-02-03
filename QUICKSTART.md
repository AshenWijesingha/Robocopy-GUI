# Quick Start Guide for Developers

Get up and running with Robocopy GUI development in 5 minutes.

## Prerequisites Check

Before starting, verify you have:

```bash
# Check .NET SDK version (need 6.0 or higher)
dotnet --version

# Check Visual Studio (optional but recommended)
# Should show Visual Studio 2022 or later
```

If you see errors, install from:
- .NET SDK: https://dotnet.microsoft.com/download/dotnet/6.0
- Visual Studio: https://visualstudio.microsoft.com/downloads/

## 5-Minute Setup

### Step 1: Get the Code (1 minute)

```bash
# Clone the repository
git clone https://github.com/yourusername/robocopy-gui.git
cd robocopy-gui
```

### Step 2: Build the Project (2 minutes)

```bash
# Restore dependencies and build
dotnet restore
dotnet build

# Or if using Visual Studio:
# Open RobocopyGUI.sln and press Ctrl+Shift+B
```

### Step 3: Run the Application (1 minute)

```bash
# Run directly
dotnet run --project RobocopyGUI/RobocopyGUI.csproj

# Or press F5 in Visual Studio
```

### Step 4: Test Basic Functionality (1 minute)

1. Click "Browse" for Source Folder → Select any folder
2. Click "Browse" for Destination → Select different folder
3. Select "Backup" preset
4. Click "Start Copy"
5. Verify files are copied

## Project Structure Overview

```
RobocopyGUI/
├── MainWindow.xaml          # UI layout
├── MainWindow.xaml.cs       # UI logic
├── App.xaml                 # Application entry
├── Models/                  # Data models
│   ├── CopyOptions.cs
│   ├── CopyProfile.cs
│   └── CopyResult.cs
├── Services/                # Business logic
│   ├── RobocopyService.cs
│   └── LoggingService.cs
└── RobocopyGUI.csproj      # Project configuration
```

## Common Development Tasks

### Building for Release

```bash
# Create standalone executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Output: bin/Release/net6.0-windows/win-x64/publish/RobocopyGUI.exe
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "TestName~RobocopyService"
```

### Debugging

**In Visual Studio:**
1. Set breakpoint (F9)
2. Start debugging (F5)
3. Step through code (F10/F11)

**Command line:**
```bash
# Run with debugger attached
dotnet run --project RobocopyGUI/RobocopyGUI.csproj --configuration Debug
```

### Making Changes

1. **Edit UI:** Modify `MainWindow.xaml`
2. **Edit Logic:** Modify `MainWindow.xaml.cs` or service classes
3. **Rebuild:** Press `Ctrl+Shift+B`
4. **Test:** Press `F5`

### Adding New Features

Example: Adding a new copy option

1. **Add property to CopyOptions.cs:**
```csharp
public bool MyNewOption { get; set; } = false;
```

2. **Add UI element in MainWindow.xaml:**
```xml
<CheckBox x:Name="MyNewOptionCheck" 
         Content="My New Option"
         Margin="0,0,0,10"/>
```

3. **Wire up in MainWindow.xaml.cs:**
```csharp
private CopyOptions BuildCopyOptions()
{
    return new CopyOptions
    {
        // ... existing options ...
        MyNewOption = MyNewOptionCheck.IsChecked ?? false
    };
}
```

4. **Add to command builder in RobocopyService.cs:**
```csharp
if (options.MyNewOption)
{
    args.Add("/MY_PARAM");
}
```

## Troubleshooting Quick Fixes

### Build Error: "SDK not found"
```bash
# Install .NET 6.0 SDK
winget install Microsoft.DotNet.SDK.6
```

### Build Error: "Package restore failed"
```bash
# Clear cache and restore
dotnet nuget locals all --clear
dotnet restore
```

### Runtime Error: "Application won't start"
```bash
# Check runtime is installed
dotnet --list-runtimes

# Install if missing
winget install Microsoft.DotNet.Runtime.6
```

### UI Not Updating After Code Changes
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

## Development Workflow

### Daily Workflow
```bash
# 1. Pull latest changes
git pull origin main

# 2. Create feature branch
git checkout -b feature/my-feature

# 3. Make changes and test
dotnet build
dotnet run

# 4. Commit changes
git add .
git commit -m "Add my feature"

# 5. Push and create PR
git push origin feature/my-feature
```

### Before Committing
```bash
# Run tests
dotnet test

# Check code formatting (if configured)
dotnet format

# Build release to verify
dotnet build -c Release
```

## Useful Commands Cheat Sheet

```bash
# Build commands
dotnet build                    # Debug build
dotnet build -c Release         # Release build
dotnet clean                    # Clean build artifacts

# Run commands
dotnet run                      # Run application
dotnet run --no-build           # Run without building

# Test commands
dotnet test                     # Run all tests
dotnet test --logger "console"  # Verbose test output

# Publish commands
dotnet publish -c Release       # Framework-dependent
dotnet publish -c Release --self-contained  # Self-contained

# Package commands
dotnet add package PackageName  # Add NuGet package
dotnet list package             # List packages
dotnet restore                  # Restore packages
```

## Next Steps

1. **Read the full README:** See `README.md` for complete documentation
2. **Check build guide:** See `BUILD_GUIDE.md` for detailed build instructions
3. **Review architecture:** Understand the MVVM pattern used
4. **Join discussions:** Check GitHub Discussions for questions
5. **Report issues:** Use GitHub Issues for bugs

## Getting Help

- **Documentation:** All .md files in the repository
- **Code examples:** Check existing services and models
- **GitHub Issues:** For bugs and feature requests
- **Discussions:** For questions and ideas

## Pro Tips

1. **Use Hot Reload:** Make UI changes without restarting (Visual Studio 2022)
2. **Learn XAML:** Understanding WPF/XAML will make UI development easier
3. **Use Debugger:** Don't rely only on console output
4. **Test Incrementally:** Test small changes before making large ones
5. **Read Robocopy Docs:** Understanding Robocopy helps with features

## Example: Complete Feature Addition

Let's add a "Verify Copy" option:

**1. Model (CopyOptions.cs):**
```csharp
public bool VerifyCopy { get; set; } = false;
```

**2. UI (MainWindow.xaml):**
```xml
<CheckBox x:Name="VerifyCopyCheck" 
         Content="Verify copied files"
         Margin="0,0,0,10"/>
```

**3. Logic (MainWindow.xaml.cs):**
```csharp
VerifyCopy = VerifyCopyCheck.IsChecked ?? false
```

**4. Service (RobocopyService.cs):**
```csharp
if (options.VerifyCopy)
{
    args.Add("/V");  // Verify flag
}
```

**5. Test:**
```bash
dotnet build
dotnet run
# Test the new checkbox
```

**6. Commit:**
```bash
git add .
git commit -m "Add verify copy option"
```

Done! 🎉

---

**Happy Coding!** 🚀
