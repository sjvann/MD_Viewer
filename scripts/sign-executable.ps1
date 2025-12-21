# MD Viewer - 程式碼簽章腳本
# 使用此腳本對編譯後的執行檔進行程式碼簽章

param(
    [Parameter(Mandatory=$false)]
    [string]$CertificatePath = "",
    
    [Parameter(Mandatory=$false)]
    [string]$CertificatePassword = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ExecutablePath = "publish\self-contained\MD_Viewer.exe",
    
    [Parameter(Mandatory=$false)]
    [string]$TimestampServer = "http://timestamp.digicert.com"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MD Viewer - 程式碼簽章工具" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 signtool 是否存在
$signToolPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool.exe"
)

$signTool = $null
foreach ($path in $signToolPaths) {
    if (Test-Path $path) {
        $signTool = $path
        break
    }
}

if (-not $signTool) {
    Write-Host "錯誤：找不到 signtool.exe" -ForegroundColor Red
    Write-Host "請安裝 Windows SDK 或 Windows Kit" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "下載連結：" -ForegroundColor Yellow
    Write-Host "https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor Cyan
    exit 1
}

Write-Host "找到 signtool: $signTool" -ForegroundColor Green

# 檢查執行檔是否存在
if (-not (Test-Path $ExecutablePath)) {
    Write-Host "錯誤：找不到執行檔: $ExecutablePath" -ForegroundColor Red
    Write-Host "請先編譯專案或指定正確的路徑" -ForegroundColor Yellow
    exit 1
}

# 如果沒有提供憑證路徑，嘗試從憑證存放區取得
if ([string]::IsNullOrEmpty($CertificatePath)) {
    Write-Host "未提供憑證路徑，嘗試從憑證存放區取得..." -ForegroundColor Yellow
    
    # 列出可用的程式碼簽章憑證
    $certs = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object { $_.HasPrivateKey -and $_.EnhancedKeyUsageList -match "Code Signing" }
    
    if ($certs.Count -eq 0) {
        Write-Host "錯誤：找不到程式碼簽章憑證" -ForegroundColor Red
        Write-Host ""
        Write-Host "請執行以下命令建立測試憑證（僅供測試用）：" -ForegroundColor Yellow
        Write-Host 'New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=MD Viewer" -CertStoreLocation Cert:\CurrentUser\My' -ForegroundColor Cyan
        Write-Host ""
        Write-Host "或使用 -CertificatePath 參數指定 .pfx 憑證檔案" -ForegroundColor Yellow
        exit 1
    }
    
    if ($certs.Count -eq 1) {
        $cert = $certs[0]
        Write-Host "找到憑證: $($cert.Subject)" -ForegroundColor Green
        $thumbprint = $cert.Thumbprint
    } else {
        Write-Host "找到多個憑證，請選擇：" -ForegroundColor Yellow
        $index = 1
        foreach ($cert in $certs) {
            Write-Host "  [$index] $($cert.Subject) (到期日: $($cert.NotAfter))" -ForegroundColor Cyan
            $index++
        }
        $selection = Read-Host "請輸入編號"
        $cert = $certs[$selection - 1]
        $thumbprint = $cert.Thumbprint
    }
    
    # 使用憑證指紋簽章
    Write-Host "使用憑證指紋簽章..." -ForegroundColor Yellow
    $signArgs = @(
        "sign",
        "/sha1", $thumbprint,
        "/t", $TimestampServer,
        "/v",
        "`"$ExecutablePath`""
    )
} else {
    # 使用 .pfx 檔案簽章
    if (-not (Test-Path $CertificatePath)) {
        Write-Host "錯誤：找不到憑證檔案: $CertificatePath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "使用憑證檔案簽章..." -ForegroundColor Yellow
    $signArgs = @(
        "sign",
        "/f", "`"$CertificatePath`"",
        "/t", $TimestampServer,
        "/v"
    )
    
    if (-not [string]::IsNullOrEmpty($CertificatePassword)) {
        $signArgs += "/p"
        $signArgs += "`"$CertificatePassword`""
    }
    
    $signArgs += "`"$ExecutablePath`""
}

# 執行簽章
Write-Host ""
Write-Host "正在簽章: $ExecutablePath" -ForegroundColor Yellow
Write-Host ""

try {
    & $signTool $signArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "============================================" -ForegroundColor Green
        Write-Host "  簽章成功！" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Green
        Write-Host ""
        
        # 驗證簽章
        Write-Host "驗證簽章..." -ForegroundColor Yellow
        & $signTool verify /pa /v "`"$ExecutablePath`""
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "簽章驗證成功！" -ForegroundColor Green
        }
    } else {
        Write-Host ""
        Write-Host "簽章失敗，錯誤代碼: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "簽章時發生錯誤: $_" -ForegroundColor Red
    exit 1
}

