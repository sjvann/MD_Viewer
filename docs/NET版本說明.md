# .NET 版本說明

## 當前版本配置

從建置輸出可以看到，專案使用了不同的 Target Framework Moniker (TFM)：

| 專案 | TFM | 說明 |
|------|-----|------|
| **MD_Viewer** | `net10.0-windows10.0.19041.0` | MAUI 主專案（平台特定） |
| **MD_Viewer.Shared** | `net10.0` | 共享專案（平台無關） |
| **MD_Viewer.Blazor** | `net10.0` | Blazor 專案（平台無關） |

## 為什麼版本不一致？

### ✅ 這是正確的設計！

#### 1. **MAUI 專案必須使用平台特定的 TFM**

```xml
<!-- MD_Viewer.csproj -->
<TargetFrameworks>net10.0-windows10.0.19041.0</TargetFrameworks>
```

**原因：**
- MAUI 是跨平台框架，需要指定目標平台
- `net10.0-windows10.0.19041.0` 表示：
  - 基礎框架：`.NET 10.0`
  - 目標平台：`Windows`
  - 最低版本：`10.0.19041.0` (Windows 10 版本 1903)

#### 2. **共享專案使用標準 TFM**

```xml
<!-- MD_Viewer.Shared.csproj -->
<TargetFramework>net10.0</TargetFramework>
```

**原因：**
- 共享專案不依賴特定平台
- 可以被多個平台專案引用（Windows、Android、iOS 等）
- 使用標準 `net10.0` 確保最大相容性

#### 3. **Blazor 專案使用標準 TFM**

```xml
<!-- MD_Viewer.Blazor.csproj -->
<TargetFramework>net10.0</TargetFramework>
```

**原因：**
- Blazor 組件是平台無關的
- 可以在任何 .NET 環境中運行（瀏覽器、伺服器、桌面等）

## 版本相容性

### ✅ 完全相容

雖然 TFM 看起來不同，但它們都基於 **.NET 10.0**，因此：

1. **所有專案使用相同的 .NET 10.0 基礎**
   - 共享相同的 API
   - 共享相同的執行時

2. **引用關係正常**
   ```
   MD_Viewer (net10.0-windows10.0.19041.0)
   ├── MD_Viewer.Shared (net10.0) ✅
   └── MD_Viewer.Blazor (net10.0) ✅
   ```

3. **建置系統自動處理**
   - .NET SDK 會自動解析相容的框架
   - 共享專案的 `net10.0` 可以被 `net10.0-windows10.0.19041.0` 引用

## 是否需要統一？

### ❌ 不建議統一

**如果將共享專案改為平台特定 TFM：**

```xml
<!-- ❌ 不建議 -->
<TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
```

**問題：**
1. 失去跨平台能力
2. 如果未來要支援 Android/iOS，需要修改多個專案
3. 違反共享專案的設計原則

**如果將 MAUI 專案改為標準 TFM：**

```xml
<!-- ❌ 不可能 -->
<TargetFramework>net10.0</TargetFramework>
```

**問題：**
1. MAUI 專案**必須**使用平台特定 TFM
2. 無法使用 MAUI 的平台特定功能
3. 建置會失敗

## 多平台支援時的配置

如果未來要支援多個平台，配置會是：

```xml
<!-- MD_Viewer.csproj -->
<TargetFrameworks>
  net10.0-android;
  net10.0-ios;
  net10.0-maccatalyst;
  net10.0-windows10.0.19041.0
</TargetFrameworks>
```

共享專案仍然保持 `net10.0`，可以被所有平台引用。

## 驗證版本一致性

### 檢查所有專案是否使用相同的 .NET 版本

```powershell
# 檢查所有 .csproj 檔案中的 TargetFramework
Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match 'TargetFramework[^>]*>([^<]+)<') {
        Write-Host "$($_.Name): $($matches[1])"
    }
}
```

### 檢查 global.json

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

這確保所有專案使用相同的 SDK 版本。

## 總結

| 問題 | 答案 |
|------|------|
| 版本不一致是問題嗎？ | ❌ 不是，這是正確的設計 |
| 需要修改嗎？ | ❌ 不需要 |
| 會影響功能嗎？ | ❌ 不會，所有專案都基於 .NET 10.0 |
| 建置會失敗嗎？ | ❌ 不會，.NET SDK 自動處理相容性 |

**結論：** 當前的配置是**最佳實踐**，符合 .NET MAUI 專案的標準架構。

