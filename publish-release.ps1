# MD Viewer 發布腳本
# 建立乾淨的發布目錄結構：根目錄只有啟動器，其他檔案放在 app 子目錄

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\release"
)

$ErrorActionPreference = "Stop"

Write-Host "=== MD Viewer Release Build ===" -ForegroundColor Cyan
Write-Host ""

# 1. 清理舊的發布目錄
$publishDir = ".\publish"
$releaseDir = $OutputDir

if (Test-Path $publishDir) {
    Write-Host "清理舊的發布目錄..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $publishDir
}

if (Test-Path $releaseDir) {
    Write-Host "清理舊的輸出目錄..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $releaseDir
}

# 2. 執行發布
Write-Host "`n正在發布專案..." -ForegroundColor Cyan
dotnet publish MD_Viewer/MD_Viewer.csproj -c $Configuration -f net10.0-windows10.0.19041.0 -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "發布失敗！" -ForegroundColor Red
    exit 1
}

# 3. 建立整理後的目錄結構
Write-Host "`n正在整理檔案結構..." -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null
New-Item -ItemType Directory -Force -Path "$releaseDir\app" | Out-Null

# 4. 將所有發布檔案移到 app 子目錄
Write-Host "將程式檔案移到 app 資料夾..." -ForegroundColor Gray
Copy-Item -Recurse "$publishDir\*" "$releaseDir\app\"

# 5. 建立根目錄的啟動批次檔
$launcherBat = @'
@echo off
cd /d "%~dp0app"
start "" "MD_Viewer.exe" %*
'@
Set-Content -Path "$releaseDir\MD_Viewer.bat" -Value $launcherBat -Encoding ASCII

# 6. 建立 VBS 啟動器（隱藏命令提示字元視窗）
$launcherVbs = @'
Set WshShell = CreateObject("WScript.Shell")
WshShell.CurrentDirectory = CreateObject("Scripting.FileSystemObject").GetParentFolderName(WScript.ScriptFullName) & "\app"
WshShell.Run """" & WshShell.CurrentDirectory & "\MD_Viewer.exe""", 1, False
'@
Set-Content -Path "$releaseDir\MD_Viewer.vbs" -Value $launcherVbs -Encoding ASCII

# 7. 建立 README
$readmeContent = @"
# MD Viewer v1.0.0

## 目錄結構

```
MD_Viewer/
├── MD_Viewer.vbs      <- 雙擊這個啟動程式（推薦）
├── MD_Viewer.bat      <- 或使用這個啟動
├── README.txt         <- 說明文件
└── app/               <- 程式檔案（請勿修改）
    ├── MD_Viewer.exe
    ├── *.dll
    └── ...
```

## 使用方式

雙擊 `MD_Viewer.vbs` 或 `MD_Viewer.bat` 啟動程式。

也可以直接執行 `app\MD_Viewer.exe`。

## 系統需求

- Windows 10 版本 1903 (Build 19041) 或更新版本
- x64 處理器

## 注意事項

- 請勿移動或刪除 `app` 資料夾中的檔案
- 若要建立桌面捷徑，請對 `MD_Viewer.vbs` 右鍵 -> 傳送到 -> 桌面
"@
Set-Content -Path "$releaseDir\README.txt" -Value $readmeContent -Encoding UTF8

# 8. 統計結果
Write-Host "`n=== 發布完成 ===" -ForegroundColor Green

$rootFileCount = (Get-ChildItem -Path $releaseDir -File).Count
$appFileCount = (Get-ChildItem -Path "$releaseDir\app" -File -Recurse).Count
$totalSize = (Get-ChildItem -Path $releaseDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "`n目錄結構：" -ForegroundColor Cyan
Write-Host "  根目錄檔案數：$rootFileCount"
Write-Host "  app 資料夾檔案數：$appFileCount"
Write-Host "  總大小：$([math]::Round($totalSize, 2)) MB"

Write-Host "`n根目錄內容：" -ForegroundColor Cyan
Get-ChildItem -Path $releaseDir | ForEach-Object {
    if ($_.PSIsContainer) {
        Write-Host "  [資料夾] $($_.Name)/" -ForegroundColor Yellow
    } else {
        Write-Host "  [檔案]   $($_.Name)" -ForegroundColor White
    }
}

Write-Host "`n輸出目錄：$((Resolve-Path $releaseDir).Path)" -ForegroundColor Green

# 9. 建立 ZIP 檔案
$zipName = "MD_Viewer-v1.0.0-win-x64.zip"
Write-Host "`n正在建立 ZIP 檔案..." -ForegroundColor Cyan
if (Test-Path $zipName) { Remove-Item $zipName }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory((Resolve-Path $releaseDir).Path, $zipName)

$zipSize = (Get-Item $zipName).Length / 1MB
Write-Host "已建立：$zipName ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green

Write-Host "`n完成！" -ForegroundColor Green
