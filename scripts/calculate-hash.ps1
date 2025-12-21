# MD Viewer - 計算檔案雜湊值腳本
# 用於產生發布檔案的雜湊值，供使用者驗證檔案完整性

param(
    [Parameter(Mandatory=$false)]
    [string]$FilePath = "publish\self-contained\MD_Viewer.exe"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MD Viewer - 檔案雜湊值計算工具" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $FilePath)) {
    Write-Host "錯誤：找不到檔案: $FilePath" -ForegroundColor Red
    exit 1
}

$fileInfo = Get-Item $FilePath
Write-Host "檔案: $($fileInfo.FullName)" -ForegroundColor Yellow
Write-Host "大小: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Yellow
Write-Host ""

Write-Host "計算雜湊值..." -ForegroundColor Yellow
Write-Host ""

# 計算 SHA256
$sha256 = Get-FileHash -Path $FilePath -Algorithm SHA256
Write-Host "SHA256:" -ForegroundColor Cyan
Write-Host "  $($sha256.Hash)" -ForegroundColor White
Write-Host ""

# 計算 SHA1
$sha1 = Get-FileHash -Path $FilePath -Algorithm SHA1
Write-Host "SHA1:" -ForegroundColor Cyan
Write-Host "  $($sha1.Hash)" -ForegroundColor White
Write-Host ""

# 計算 MD5
$md5 = Get-FileHash -Path $FilePath -Algorithm MD5
Write-Host "MD5:" -ForegroundColor Cyan
Write-Host "  $($md5.Hash)" -ForegroundColor White
Write-Host ""

# 產生 Markdown 格式的輸出
$output = @"
## 檔案資訊

- **檔案名稱**: $($fileInfo.Name)
- **檔案大小**: $([math]::Round($fileInfo.Length / 1MB, 2)) MB
- **修改時間**: $($fileInfo.LastWriteTime)

## 雜湊值

### SHA256
\`\`\`
$($sha256.Hash)
\`\`\`

### SHA1
\`\`\`
$($sha1.Hash)
\`\`\`

### MD5
\`\`\`
$($md5.Hash)
\`\`\`

## 驗證指令

### PowerShell
\`\`\`powershell
# 驗證 SHA256
Get-FileHash -Path "MD_Viewer.exe" -Algorithm SHA256

# 驗證 SHA1
Get-FileHash -Path "MD_Viewer.exe" -Algorithm SHA1

# 驗證 MD5
Get-FileHash -Path "MD_Viewer.exe" -Algorithm MD5
\`\`\`

### Linux/Mac
\`\`\`bash
# 驗證 SHA256
sha256sum MD_Viewer.exe

# 驗證 SHA1
sha1sum MD_Viewer.exe

# 驗證 MD5
md5sum MD_Viewer.exe
\`\`\`
"@

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Markdown 格式輸出" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host $output -ForegroundColor White
Write-Host ""

# 儲存到檔案
$outputFile = "hash-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"
$output | Out-File -FilePath $outputFile -Encoding UTF8
Write-Host "雜湊值已儲存至: $outputFile" -ForegroundColor Green

