# 腳本說明

此資料夾包含用於建置和發布 MD Viewer 的輔助腳本。

## 腳本列表

### 1. `sign-executable.ps1` - 程式碼簽章腳本

用於對編譯後的執行檔進行程式碼簽章，這是避免防毒軟體誤判最有效的方法。

#### 使用方法

**基本用法（使用憑證存放區中的憑證）：**
```powershell
.\scripts\sign-executable.ps1 -ExecutablePath "publish\self-contained\MD_Viewer.exe"
```

**使用 .pfx 憑證檔案：**
```powershell
.\scripts\sign-executable.ps1 `
    -CertificatePath "path\to\certificate.pfx" `
    -CertificatePassword "your_password" `
    -ExecutablePath "publish\self-contained\MD_Viewer.exe"
```

**參數說明：**
- `-CertificatePath`: 憑證檔案路徑（.pfx 格式），如果未提供則從憑證存放區選擇
- `-CertificatePassword`: 憑證密碼（僅在使用 .pfx 檔案時需要）
- `-ExecutablePath`: 要簽章的執行檔路徑（預設：`publish\self-contained\MD_Viewer.exe`）
- `-TimestampServer`: 時間戳記伺服器（預設：`http://timestamp.digicert.com`）

#### 建立測試憑證

如果還沒有程式碼簽章憑證，可以建立一個測試用的自我簽章憑證：

```powershell
New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=MD Viewer" `
    -CertStoreLocation Cert:\CurrentUser\My `
    -KeyUsage DigitalSignature `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(1)
```

**注意：** 自我簽章憑證僅供測試使用，正式發布時應使用商業憑證。

### 2. `calculate-hash.ps1` - 計算檔案雜湊值

計算執行檔的雜湊值（SHA256、SHA1、MD5），用於發布時提供給使用者驗證檔案完整性。

#### 使用方法

```powershell
.\scripts\calculate-hash.ps1 -FilePath "publish\self-contained\MD_Viewer.exe"
```

**參數說明：**
- `-FilePath`: 要計算雜湊值的檔案路徑（預設：`publish\self-contained\MD_Viewer.exe`）

#### 輸出

腳本會：
1. 在終端顯示所有雜湊值
2. 產生一個 Markdown 格式的檔案（`hash-YYYYMMDD-HHMMSS.md`），包含：
   - 檔案資訊
   - 所有雜湊值
   - 驗證指令範例

## 完整發布流程建議

1. **編譯專案**
   ```powershell
   .\build-release.bat
   ```

2. **簽章執行檔**（如果有憑證）
   ```powershell
   .\scripts\sign-executable.ps1 -ExecutablePath "publish\self-contained\MD_Viewer.exe"
   ```

3. **計算雜湊值**
   ```powershell
   .\scripts\calculate-hash.ps1 -FilePath "publish\self-contained\MD_Viewer.exe"
   ```

4. **建立安裝程式**（使用 Inno Setup）
   ```powershell
   # 執行 setup.iss 建立安裝程式
   ```

5. **簽章安裝程式**（如果有憑證）
   ```powershell
   .\scripts\sign-executable.ps1 -ExecutablePath "installer\MD_Viewer_Setup_v1.0.0_x64.exe"
   ```

6. **計算安裝程式雜湊值**
   ```powershell
   .\scripts\calculate-hash.ps1 -FilePath "installer\MD_Viewer_Setup_v1.0.0_x64.exe"
   ```

## 取得程式碼簽章憑證

### 商業憑證（推薦）

以下是一些提供程式碼簽章憑證的廠商：

- **DigiCert**: https://www.digicert.com/code-signing/
- **Sectigo (原 Comodo)**: https://sectigo.com/ssl-certificates-tls/code-signing
- **GlobalSign**: https://www.globalsign.com/en/code-signing-certificate
- **SSL.com**: https://www.ssl.com/certificates/code-signing/

費用通常為每年 $200-500 USD。

### 免費選項

- **Open Source Code Signing**: 某些組織為開源專案提供免費簽章服務
- **Azure Key Vault**: 可以儲存和管理憑證（仍需購買憑證）

## 相關文件

- [避免防毒軟體誤判指南](../docs/避免防毒軟體誤判指南.md)
- [Microsoft 程式碼簽章文件](https://docs.microsoft.com/en-us/windows/win32/seccrypto/cryptography-tools)

