# SoundHop Build Script
# Builds the application and creates an installer

param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

$projectPath = "AudioSwitcher.UI\AudioSwitcher.UI.csproj"
$publishDir = "AudioSwitcher.UI\bin\$Platform\$Configuration\net10.0-windows10.0.19041.0\win-x64\publish"
$installerScript = "installer.iss"
$installerOutput = "installer-output"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SoundHop Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build and Publish
if (-not $SkipBuild) {
    Write-Host "[1/2] Building and publishing application..." -ForegroundColor Yellow
    
    # Clean previous publish
    if (Test-Path $publishDir) {
        Remove-Item -Path $publishDir -Recurse -Force
    }
    
    # Publish the application
    dotnet publish $projectPath `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:PublishReadyToRun=true `
        -o $publishDir
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "[1/2] Skipping build (using existing publish output)" -ForegroundColor Gray
}

# Step 2: Create Installer
if (-not $SkipInstaller) {
    Write-Host "[2/2] Creating installer..." -ForegroundColor Yellow
    
    # Find Inno Setup compiler
    $isccPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    
    $iscc = $null
    foreach ($path in $isccPaths) {
        if (Test-Path $path) {
            $iscc = $path
            break
        }
    }
    
    if (-not $iscc) {
        Write-Host "ERROR: Inno Setup compiler (ISCC.exe) not found!" -ForegroundColor Red
        Write-Host "Please install Inno Setup 6 from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "Using Inno Setup: $iscc" -ForegroundColor Gray
    
    # Create output directory
    if (-not (Test-Path $installerOutput)) {
        New-Item -ItemType Directory -Path $installerOutput | Out-Null
    }
    
    # Run Inno Setup compiler
    & $iscc $installerScript
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Installer creation failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Installer created successfully!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "[2/2] Skipping installer creation" -ForegroundColor Gray
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Build Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output files:" -ForegroundColor White
if (-not $SkipBuild) {
    Write-Host "  Application: $publishDir" -ForegroundColor Gray
}
if (-not $SkipInstaller) {
    $installerFile = Get-ChildItem -Path $installerOutput -Filter "*.exe" | Select-Object -First 1
    if ($installerFile) {
        Write-Host "  Installer:   $installerOutput\$($installerFile.Name)" -ForegroundColor Gray
    }
}
