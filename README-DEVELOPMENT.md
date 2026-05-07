# EventEase Development Guide

## 🚨 File Locking Issue Solution

The "file is being used by another process" error occurs because the EventEase application continues running in the background and locks the executable file during development.

## 🛠️ Quick Solutions

### Option 1: Use the Development Scripts (Recommended)

```powershell
# Stop the application before building
.\STOP-APPLICATION.ps1

# Or use the comprehensive workflow script
.\DEV-WORKFLOW.ps1 stop

# Build after stopping
.\DEV-WORKFLOW.ps1 build

# Start again
.\DEV-WORKFLOW.ps1 start
```

### Option 2: Manual Process Management

```powershell
# Kill all EventEase processes
taskkill /F /IM EventEase.exe /T

# Wait 2 seconds, then build
dotnet build

# Start the application
dotnet run --urls="http://localhost:5000"
```

### Option 3: Visual Studio Solution

If using Visual Studio:
1. **Stop Debugging**: Press `Shift + F5` or click the stop button
2. **Close Browser**: Close the browser tab/window
3. **Wait**: Give it 2-3 seconds for processes to terminate
4. **Build**: Build the project normally

## 📋 Development Workflow Scripts

### STOP-APPLICATION.ps1
- Completely stops all EventEase processes
- Removes locked files
- Use before building or making changes

### DEV-WORKFLOW.ps1
- Comprehensive development workflow
- Actions: `start`, `stop`, `restart`, `build`, `clean`

```powershell
# Usage examples
.\DEV-WORKFLOW.ps1 start      # Start application
.\DEV-WORKFLOW.ps1 stop       # Stop application
.\DEV-WORKFLOW.ps1 restart    # Restart application
.\DEV-WORKFLOW.ps1 build      # Stop, clean, and build
.\DEV-WORKFLOW.ps1 clean      # Full clean of build artifacts
```

## 🔧 Best Practices

### Before Making Code Changes
1. **Always stop the application** first
2. **Use the scripts** to ensure complete cleanup
3. **Wait 2-3 seconds** after stopping before building

### During Development
1. **Use `DEV-WORKFLOW.ps1 build`** instead of regular build
2. **Check for running processes** if you get errors
3. **Restart with `DEV-WORKFLOW.ps1 restart`** after successful build

### Common Error Patterns
```
ERROR: Could not copy file... being used by another process
SOLUTION: Run .\STOP-APPLICATION.ps1 before building
```

```
ERROR: The process cannot access the file
SOLUTION: Use taskkill /F /IM EventEase.exe /T
```

## 🚀 Quick Start Commands

```powershell
# Fresh start (clean build and run)
.\DEV-WORKFLOW.ps1 clean
.\DEV-WORKFLOW.ps1 build
.\DEV-WORKFLOW.ps1 start

# Quick restart after code changes
.\DEV-WORKFLOW.ps1 restart

# Just stop and build
.\STOP-APPLICATION.ps1
dotnet build
```

## 📝 Why This Happens

1. **ASP.NET Core** keeps the executable locked while running
2. **Background processes** may continue after closing browser
3. **Child processes** spawn and lock additional files
4. **Build process** needs exclusive access to overwrite files

The scripts handle all these scenarios by:
- Force-killing the entire process tree
- Waiting for proper termination
- Cleaning up locked files
- Providing a clean build environment

## 🎯 Recommended Workflow

1. **Make code changes**
2. **Run**: `.\DEV-WORKFLOW.ps1 restart`
3. **Test** your changes
4. **Repeat**

This eliminates all file locking issues and provides a smooth development experience!
