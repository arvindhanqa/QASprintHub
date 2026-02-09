# QA Sprint Hub - Build Script
# This script builds the application as a self-contained Windows executable

Write-Host "Building QA Sprint Hub..." -ForegroundColor Cyan

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build in Release mode
Write-Host "Building in Release mode..." -ForegroundColor Yellow
dotnet build -c Release

# Publish as self-contained executable
Write-Host "Publishing self-contained executable..." -ForegroundColor Yellow
dotnet publish src/QASprintHub/QASprintHub.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=false `
    -o ./publish

Write-Host "`nBuild complete!" -ForegroundColor Green
Write-Host "Output directory: ./publish" -ForegroundColor Green

# Generate hash for IT verification
Write-Host "`nGenerating SHA-256 hash for verification..." -ForegroundColor Yellow
Get-FileHash ./publish/QASprintHub.exe -Algorithm SHA256 | Format-List

Write-Host "`nDone! The application is ready in the ./publish folder." -ForegroundColor Cyan
