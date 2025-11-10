@echo off
echo ============================================
echo Stock App - Windows Executable Build Script
echo ============================================
echo.

REM Frontend build
echo [1/4] Building frontend...
cd frontend
call npm install
if errorlevel 1 (
    echo Frontend npm install failed!
    pause
    exit /b 1
)

call npm run build
if errorlevel 1 (
    echo Frontend build failed!
    pause
    exit /b 1
)
echo Frontend build completed!
echo.

REM Frontend dosyalarını backend wwwroot'a kopyala
echo [2/4] Copying frontend files to backend wwwroot...
cd ..
REM wwwroot içindeki dist klasörünü temizle ve yeni build'i kopyala
if exist StockApp\wwwroot\dist (
    rmdir /s /q StockApp\wwwroot\dist
)
REM Frontend dist içindeki tüm dosyaları wwwroot'a kopyala
xcopy /E /I /Y frontend\dist\* StockApp\wwwroot
if errorlevel 1 (
    echo Failed to copy frontend files!
    pause
    exit /b 1
)
echo Frontend files copied successfully!
echo.

REM Backend publish (Windows self-contained executable)
echo [3/4] Publishing backend as Windows executable...
cd StockApp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ..\publish\win-x64
if errorlevel 1 (
    echo Backend publish failed!
    pause
    exit /b 1
)
echo Backend published successfully!
echo.

REM Veritabanı dosyasını kopyala (eğer varsa)
echo [4/4] Copying database file...
if exist stockapp.db (
    copy /Y stockapp.db ..\publish\win-x64\stockapp.db
    echo Database file copied!
)
if exist stockapp.db-shm (
    copy /Y stockapp.db-shm ..\publish\win-x64\stockapp.db-shm
)
if exist stockapp.db-wal (
    copy /Y stockapp.db-wal ..\publish\win-x64\stockapp.db-wal
)

echo.
echo ============================================
echo Build completed successfully!
echo ============================================
echo.
echo Executable location: publish\win-x64\StockApp.exe
echo.
echo To run the application:
echo   1. Navigate to publish\win-x64 folder
echo   2. Double-click StockApp.exe
echo   3. Open browser and go to: http://localhost:5134
echo.
pause

