# Stock App - Windows Executable Build Script (PowerShell)
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Stock App - Windows Executable Build Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Frontend build
Write-Host "[1/4] Building frontend..." -ForegroundColor Yellow
Set-Location frontend
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "Frontend npm install failed!" -ForegroundColor Red
    pause
    exit 1
}

npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Frontend build failed!" -ForegroundColor Red
    pause
    exit 1
}
Write-Host "Frontend build completed!" -ForegroundColor Green
Write-Host ""

# Frontend dosyalarını backend wwwroot'a kopyala
Write-Host "[2/4] Copying frontend files to backend wwwroot..." -ForegroundColor Yellow
Set-Location ..
if (Test-Path "StockApp\wwwroot\dist") {
    Remove-Item -Recurse -Force "StockApp\wwwroot\dist"
}
# Frontend dist içindeki tüm dosyaları wwwroot'a kopyala
Copy-Item -Path "frontend\dist\*" -Destination "StockApp\wwwroot\" -Recurse -Force
Write-Host "Frontend files copied successfully!" -ForegroundColor Green
Write-Host ""

# Backend publish (Windows self-contained executable)
Write-Host "[3/4] Publishing backend as Windows executable..." -ForegroundColor Yellow
Set-Location StockApp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ..\publish\win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend publish failed!" -ForegroundColor Red
    pause
    exit 1
}
Write-Host "Backend published successfully!" -ForegroundColor Green
Write-Host ""

# Veritabanı dosyasını kopyala (eğer varsa)
Write-Host "[4/4] Copying database file..." -ForegroundColor Yellow
if (Test-Path "stockapp.db") {
    Copy-Item -Force "stockapp.db" "..\publish\win-x64\stockapp.db"
    Write-Host "Database file copied!" -ForegroundColor Green
}
if (Test-Path "stockapp.db-shm") {
    Copy-Item -Force "stockapp.db-shm" "..\publish\win-x64\stockapp.db-shm"
}
if (Test-Path "stockapp.db-wal") {
    Copy-Item -Force "stockapp.db-wal" "..\publish\win-x64\stockapp.db-wal"
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Executable location: publish\win-x64\StockApp.exe" -ForegroundColor Yellow
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  1. Navigate to publish\win-x64 folder" -ForegroundColor White
Write-Host "  2. Double-click StockApp.exe" -ForegroundColor White
Write-Host "  3. Open browser and go to: http://localhost:5134" -ForegroundColor White
Write-Host ""
pause

