@echo off
REM ============================================
REM MD Viewer - 建置安裝程式腳本
REM ============================================
REM 
REM 前置需求：
REM 1. 安裝 Inno Setup: https://jrsoftware.org/isdl.php
REM 2. 確保 ISCC.exe 在 PATH 中，或修改下方路徑
REM
REM ============================================

echo.
echo =============================================
echo   MD Viewer - Build Installer
echo =============================================
echo.

set VERSION=1.0.0
set INNO_SETUP="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

REM 檢查 Inno Setup 是否存在
if not exist %INNO_SETUP% (
    echo [錯誤] 找不到 Inno Setup！
    echo 請從 https://jrsoftware.org/isdl.php 下載並安裝
    echo 或修改此腳本中的 INNO_SETUP 路徑
    pause
    exit /b 1
)

REM 步驟 1: 建置發布版本
echo [1/3] 建置 Release 版本...
dotnet publish MD_Viewer/MD_Viewer.csproj -c Release -f net10.0-windows10.0.19041.0 -o ./publish
if errorlevel 1 (
    echo [錯誤] 建置失敗！
    pause
    exit /b 1
)

REM 步驟 2: 建立 installer 目錄
echo [2/3] 準備安裝程式目錄...
if not exist installer mkdir installer

REM 步驟 3: 執行 Inno Setup 編譯
echo [3/3] 編譯安裝程式...
%INNO_SETUP% setup.iss
if errorlevel 1 (
    echo [錯誤] Inno Setup 編譯失敗！
    pause
    exit /b 1
)

echo.
echo =============================================
echo   安裝程式建置完成！
echo =============================================
echo.
echo 輸出檔案:
echo   - installer\MD_Viewer_Setup_v%VERSION%_x64.exe
echo.

pause
