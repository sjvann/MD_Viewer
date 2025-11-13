# Debug 輸出查看說明

## 方法 1：使用 Visual Studio（推薦）

### 步驟：
1. 在 Visual Studio 中開啟專案
2. 按 `F5` 或點擊「開始偵錯」執行應用程式
3. 打開「輸出」視窗：
   - 選單：`檢視 (View)` → `輸出 (Output)`
   - 或按快捷鍵：`Ctrl+Alt+O`
4. 在「輸出」視窗的右上角，選擇「顯示輸出來源」：
   - 選擇 **「偵錯 (Debug)」** 來查看 `System.Diagnostics.Debug.WriteLine` 的輸出
   - 選擇 **「偵錯主控台 (Debug Console)」** 來查看其他日誌
5. 在應用程式中點擊 PDF 或 HTML 格式
6. 回到「輸出」視窗查看以 `[DEBUG]` 或 `[ERROR]` 開頭的訊息

### 範例輸出：
```
[DEBUG] OnExportFormatTapped 被觸發
[DEBUG] sender 類型: Grid
[DEBUG] e.Parameter 類型: ExportFormat
[DEBUG] 從 CommandParameter 取得格式: PDF, IsEnabled: True, Extension: .pdf
[DEBUG] 準備呼叫 ExportAsync, 格式: PDF
[DEBUG] ExportAsync 開始執行
[DEBUG] format 是否為 null: False
[DEBUG] 格式資訊 - Name: PDF, Extension: .pdf, IsEnabled: True
...
```

## 方法 2：使用命令列（PowerShell）

### 步驟：
1. 開啟 PowerShell 或終端機
2. 切換到專案目錄：
   ```powershell
   cd C:\Projects\sjvann\MD_Viewer
   ```
3. 執行應用程式：
   ```powershell
   dotnet run --project .\MD_Viewer\ --framework net10.0-windows10.0.19041.0
   ```
4. Debug 訊息會直接顯示在命令列視窗中
5. 在應用程式中點擊 PDF 或 HTML 格式
6. 回到命令列視窗查看 debug 輸出

## 方法 3：使用 Visual Studio Code

### 步驟：
1. 在 VS Code 中開啟專案
2. 打開「終端機」面板（`Ctrl+` ` 或 `View` → `Terminal`）
3. 執行應用程式：
   ```powershell
   dotnet run --project .\MD_Viewer\ --framework net10.0-windows10.0.19041.0
   ```
4. Debug 訊息會顯示在終端機中

## 常見問題排查

### 如果看不到任何 debug 輸出：

1. **確認是在 Debug 模式執行**
   - Visual Studio：確認工具列顯示「Debug」而不是「Release」
   - 命令列：確認沒有使用 `-c Release` 參數

2. **確認輸出視窗選擇正確**
   - 在 Visual Studio 的「輸出」視窗中，確認選擇了「偵錯 (Debug)」而不是其他選項

3. **確認應用程式正在執行**
   - 如果應用程式沒有啟動，debug 訊息不會出現

4. **檢查是否有例外**
   - 如果有未處理的例外，應用程式可能會崩潰，導致看不到後續的 debug 訊息
   - 查看是否有 `[ERROR]` 開頭的訊息

## Debug 訊息說明

- `[DEBUG]` - 一般除錯訊息，顯示執行流程
- `[ERROR]` - 錯誤訊息，包含例外資訊

## 如果點擊沒有反應

請查看 debug 輸出，確認：
1. `OnExportFormatTapped 被觸發` - 確認點擊事件有被觸發
2. `從 CommandParameter 取得格式` 或 `從 BindingContext 取得格式` - 確認格式對象有被正確取得
3. `ExportAsync 開始執行` - 確認匯出方法有被呼叫
4. 如果某個步驟沒有出現，表示問題出在該步驟之前


