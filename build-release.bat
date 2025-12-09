@echo off
REM ============================================
REM MD Viewer - Windows 發行腳本
REM ============================================

echo.
echo =============================================
echo   MD Viewer - Build and Package for Windows
echo =============================================
echo.

set VERSION=1.0.0
set OUTPUT_DIR=publish
set PROJECT=MD_Viewer\MD_Viewer.csproj

REM 清理舊的發行檔案
echo [1/5] Cleaning previous builds...
if exist %OUTPUT_DIR% rmdir /s /q %OUTPUT_DIR%
mkdir %OUTPUT_DIR%

REM 還原套件
echo [2/5] Restoring packages...
dotnet restore %PROJECT%

REM 建置 Framework-dependent 版本
echo [3/5] Building framework-dependent version...
dotnet publish %PROJECT% ^
    -c Release ^
    -f net10.0-windows10.0.19041.0 ^
    -r win-x64 ^
    --self-contained false ^
    -o %OUTPUT_DIR%\framework-dependent

REM 建置 Self-contained 版本
echo [4/5] Building self-contained version...
dotnet publish %PROJECT% ^
    -c Release ^
    -f net10.0-windows10.0.19041.0 ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -o %OUTPUT_DIR%\self-contained

REM 建立 ZIP 檔案
echo [5/5] Creating ZIP archives...
powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\framework-dependent\*' -DestinationPath '%OUTPUT_DIR%\MD_Viewer-v%VERSION%-win-x64.zip' -Force"
powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\self-contained\*' -DestinationPath '%OUTPUT_DIR%\MD_Viewer-v%VERSION%-win-x64-selfcontained.zip' -Force"

echo.
echo =============================================
echo   Build completed successfully!
echo =============================================
echo.
echo Output files:
echo   - %OUTPUT_DIR%\MD_Viewer-v%VERSION%-win-x64.zip
echo   - %OUTPUT_DIR%\MD_Viewer-v%VERSION%-win-x64-selfcontained.zip
echo.

pause
