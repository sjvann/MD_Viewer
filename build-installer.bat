@echo off
chcp 65001 >nul
REM ============================================
REM MD Viewer - 建置安裝程式腳本
REM ============================================
REM 
REM 前置需求：
REM 1. 安裝 Inno Setup 6: https://jrsoftware.org/isdl.php
REM 2. 確保已安裝 .NET 10 SDK
REM
REM ============================================

echo.
echo =============================================
echo   MD Viewer - Build Installer
echo =============================================
echo.

set VERSION=1.0.0
set PUBLISH_DIR=publish
set INSTALLER_DIR=installer

REM 尋找 Inno Setup
set INNO_SETUP=
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set INNO_SETUP="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set INNO_SETUP="C:\Program Files\Inno Setup 6\ISCC.exe"
) else (
    where ISCC.exe >nul 2>&1
    if not errorlevel 1 (
        set INNO_SETUP=ISCC.exe
    )
)

REM 檢查 Inno Setup 是否存在
if "%INNO_SETUP%"=="" (
    echo [錯誤] 找不到 Inno Setup！
    echo.
    echo 請從以下網址下載並安裝 Inno Setup 6:
    echo https://jrsoftware.org/isdl.php
    echo.
    pause
    exit /b 1
)

echo [資訊] 使用 Inno Setup: %INNO_SETUP%
echo.

REM 步驟 1: 清理舊的發布目錄
echo [1/4] 清理舊的發布目錄...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%INSTALLER_DIR%" rmdir /s /q "%INSTALLER_DIR%"
mkdir "%INSTALLER_DIR%"

REM 步驟 2: 建置發布版本
echo [2/4] 建置 Release 版本...
echo.
dotnet publish MD_Viewer/MD_Viewer.csproj -c Release -f net10.0-windows10.0.19041.0 -o ./%PUBLISH_DIR% --no-restore
if errorlevel 1 (
    echo.
    echo [錯誤] 建置失敗！
    pause
    exit /b 1
)

REM 步驟 3: 計算發布檔案大小
echo.
echo [3/4] 統計發布檔案...
set /a FILE_COUNT=0
for /r "%PUBLISH_DIR%" %%f in (*) do set /a FILE_COUNT+=1
echo      檔案數量: %FILE_COUNT%

REM 步驟 4: 執行 Inno Setup 編譯
echo.
echo [4/4] 編譯安裝程式...
echo.
%INNO_SETUP% setup.iss
if errorlevel 1 (
    echo.
    echo [錯誤] Inno Setup 編譯失敗！
    pause
    exit /b 1
)

REM 顯示結果
echo.
echo =============================================
echo   建置完成！
echo =============================================
echo.

REM 顯示安裝程式大小
for %%f in ("%INSTALLER_DIR%\MD_Viewer_Setup_v%VERSION%_x64.exe") do (
    set SIZE=%%~zf
    set /a SIZE_MB=%%~zf / 1048576
)
echo 輸出檔案:
echo   %INSTALLER_DIR%\MD_Viewer_Setup_v%VERSION%_x64.exe
echo   大小: 約 %SIZE_MB% MB
echo.
echo 安裝程式功能:
echo   - 自動安裝到 Program Files
echo   - 建立桌面捷徑（可選）
echo   - 關聯 .md 檔案（可選）
echo   - 支援靜默安裝: /SILENT 或 /VERYSILENT
echo.

pause
