# SoundHop Velopack Build Script
# Builds the application and creates installer + update packages

param(
    [switch]$SkipBuild,
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

$projectPath = "SoundHop.UI\SoundHop.UI.csproj"
$publishDir = "publish"
$releaseDir = "releases"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SoundHop Velopack Build Script" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build and Publish
if (-not $SkipBuild) {
    Write-Host "[1/2] Building and publishing application..." -ForegroundColor Yellow
    
    # Clean previous publish
    if (Test-Path $publishDir) {
        Remove-Item -Path $publishDir -Recurse -Force
    }
    
    # Publish the application (framework-dependent for smaller size)
    dotnet publish $projectPath `
        -c $Configuration `
        -r win-x64 `
        --self-contained false `
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

# Step 2: Create Velopack release
Write-Host "[2/2] Creating Velopack release..." -ForegroundColor Yellow

# Create releases directory
if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}

# Check if vpk is installed
$vpkInstalled = $null
try {
    $vpkInstalled = Get-Command vpk -ErrorAction SilentlyContinue
}
catch { }

if (-not $vpkInstalled) {
    Write-Host "Installing vpk (Velopack CLI)..." -ForegroundColor Yellow
    dotnet tool install -g vpk
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install vpk!" -ForegroundColor Red
        exit 1
    }
}

# Run Velopack pack command (no portable package)
vpk pack `
    --packId SoundHop `
    --packVersion $Version `
    --packDir $publishDir `
    --mainExe SoundHop.exe `
    --icon assets\app_icon.ico `
    --outputDir $releaseDir `
    --framework net10.0-x64-desktop `
    --noPortable

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Velopack packaging failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Build Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output files in '$releaseDir':" -ForegroundColor White
Get-ChildItem -Path $releaseDir | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Upload these files to your GitHub Release:" -ForegroundColor Yellow
Write-Host "  - SoundHop-$Version-win-x64-Setup.exe (installer)" -ForegroundColor White
Write-Host "  - releases.win.json (update manifest)" -ForegroundColor White
Write-Host "  - SoundHop-$Version-win-x64-full.nupkg (for delta updates)" -ForegroundColor White

