# Script to install the compiled plugin into the PowerToys Run plugin directory
$ErrorActionPreference = "Stop";

function Test-Admin {
    $currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    $currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Admin)) {
    Write-Output "Script is not running as administrator. Retry with elevated privileges. Exiting...";
    Start-Sleep 1;
    exit
}

if (Test-Path "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run") {
    Write-Output "Testing if PowerToys is running...";
    $pt = Get-Process "PowerToys" -ea SilentlyContinue;
    if ($pt) {
        Write-Output "PowerToys is running, killing it";
        Stop-Process -Name PowerToys -Force
        Start-Sleep 2;
    }

    Write-Output "Installing plugin...";
    if (Test-Path "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\PasswordGenerator") {
        Write-Output "Deleting existing files";
        Remove-Item -Recurse -Force "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\PasswordGenerator";
    }
    mkdir "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\PasswordGenerator";
    
    Copy-Item -Recurse ".\bin\x64\Release\*" "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\PasswordGenerator";
    
    Write-Output "Install Complete, launching PowerToys";
    $powerToysPath = "$env:LOCALAPPDATA\PowerToys\PowerToys.exe"
    if (Test-Path $powerToysPath) {
        Invoke-Item $powerToysPath;
        Write-Output "PowerToys running";
    }
    else {
        Write-Output "PowerToys.exe not found at $powerToysPath";
    }
}
else {
    Write-Output "Unable to find PowerToys installation - you will need to modify this script";
}