# Local Deployment Script for FileCombiner
# This script runs tests, builds the release exe, and deploys it to C:\Bin

$ErrorActionPreference = "Stop"
$TargetPath = "C:\Bin\filecombiner.exe"
$BackupPath = "C:\Bin\filecombiner.exe.backup"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FileCombiner Local Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run tests
Write-Host "[1/5] Running tests..." -ForegroundColor Yellow
dotnet test --configuration Release --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed! Deployment aborted." -ForegroundColor Red
    exit 1
}
Write-Host "✅ All tests passed!" -ForegroundColor Green
Write-Host ""

# Step 2: Clean previous builds
Write-Host "[2/5] Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean --configuration Release
if (Test-Path "bin\Release") {
    Remove-Item -Recurse -Force "bin\Release"
}
Write-Host "✅ Clean complete!" -ForegroundColor Green
Write-Host ""

# Step 3: Build release
Write-Host "[3/5] Building release executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Deployment aborted." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build complete!" -ForegroundColor Green
Write-Host ""

# Step 4: Locate the built executable
$BuiltExe = Get-ChildItem -Path "bin\Release\net8.0\win-x64\publish\filecombiner.exe" -ErrorAction SilentlyContinue
if (-not $BuiltExe) {
    Write-Host "❌ Could not find built executable!" -ForegroundColor Red
    exit 1
}

Write-Host "Built executable: $($BuiltExe.FullName)" -ForegroundColor Gray
Write-Host "Size: $([math]::Round($BuiltExe.Length / 1MB, 2)) MB" -ForegroundColor Gray
Write-Host ""

# Step 5: Deploy to C:\Bin
Write-Host "[4/5] Deploying to $TargetPath..." -ForegroundColor Yellow

# Create backup if target exists
if (Test-Path $TargetPath) {
    Write-Host "Creating backup of existing executable..." -ForegroundColor Gray
    Copy-Item $TargetPath $BackupPath -Force
    Write-Host "Backup saved to: $BackupPath" -ForegroundColor Gray
}

# Ensure target directory exists
$TargetDir = Split-Path $TargetPath -Parent
if (-not (Test-Path $TargetDir)) {
    Write-Host "Creating directory: $TargetDir" -ForegroundColor Gray
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

# Copy new executable
try {
    Copy-Item $BuiltExe.FullName $TargetPath -Force
    Write-Host "✅ Deployment complete!" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to copy executable: $_" -ForegroundColor Red
    if (Test-Path $BackupPath) {
        Write-Host "Restoring backup..." -ForegroundColor Yellow
        Copy-Item $BackupPath $TargetPath -Force
        Write-Host "✅ Backup restored!" -ForegroundColor Green
    }
    exit 1
}
Write-Host ""

# Step 6: Verify deployment
Write-Host "[5/5] Verifying deployment..." -ForegroundColor Yellow
if (Test-Path $TargetPath) {
    $DeployedExe = Get-Item $TargetPath
    Write-Host "Deployed executable: $($DeployedExe.FullName)" -ForegroundColor Gray
    Write-Host "Size: $([math]::Round($DeployedExe.Length / 1MB, 2)) MB" -ForegroundColor Gray
    Write-Host "Modified: $($DeployedExe.LastWriteTime)" -ForegroundColor Gray
    Write-Host ""
    
    # Test the executable
    Write-Host "Testing executable..." -ForegroundColor Gray
    & $TargetPath --help | Out-Null
    if ($LASTEXITCODE -eq 1) {
        Write-Host "✅ Executable is working!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Executable ran but returned unexpected exit code: $LASTEXITCODE" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Deployment verification failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🎉 Deployment successful!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now use: filecombiner --help" -ForegroundColor White
