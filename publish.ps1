# MD_Viewer 發布腳本
# 發布 Windows 版本

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\publish\windows"
)

Write-Host "正在發布 Windows 版本..." -ForegroundColor Green
Write-Host "配置: $Configuration" -ForegroundColor Yellow
Write-Host "輸出路徑: $OutputPath" -ForegroundColor Yellow

dotnet publish .\MD_Viewer\ `
    --framework net10.0-windows10.0.19041.0 `
    --configuration $Configuration `
    --output $OutputPath `
    -p:RuntimeIdentifier=win-x64

Write-Host "發布完成！" -ForegroundColor Green
Write-Host "輸出位置: $OutputPath" -ForegroundColor Cyan

