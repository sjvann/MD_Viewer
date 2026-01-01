# MD Viewer

一個使用 .NET MAUI 與 Blazor WebView 打造的跨平台 Markdown 檢視 / 編輯工具，目前優先支援 Windows 桌面環境。

> 專注於「好讀、好寫、好匯出」的 Markdown 體驗。

## 功能特色

- **舒適的 Markdown 閱讀體驗**：支援多數常用語法（標題、表格、程式碼區塊、待辦清單等），使用 Markdig 解析並搭配 Blazor WebView 呈現。
- **編輯 + 即時預覽**：在同一個視窗中切換「預覽模式」與「編輯模式」，編輯時可立即看到更新後的渲染結果。
- **多種主題**：內建多款亮色 / 暗色主題，包含類似 One Dark Pro、Dracula、Nord、Solarized、GitHub Dark/Light 等常見風格。
- **檔案總管與磁碟瀏覽**：左側提供檔案樹與磁碟清單，可快速在本機資料夾中瀏覽與開啟 Markdown 檔案。
- **匯出功能**：支援將 Markdown 匯出為 **PDF**（QuestPDF、可選擇 A4 / Letter / Legal 與直向 / 橫向、頁碼）與 **HTML**（含樣式）。
- **未儲存變更保護**：偵測編輯中的變更，關閉視窗或按下離開前會詢問是否先儲存，避免誤關造成內容遺失。
- **MVVM 架構**：以 CommunityToolkit.MVVM 建構 ViewModel，維持良好可讀性與可維護性。
- **擴充性設計**：匯出模組預留 DOCX / ODF 等格式的擴充點，未來可依需求擴充。

## 截圖

> 下列路徑為範例，實際專案中請將對應圖片放在 `docs/screenshots` 資料夾。

- 預覽模式：![Preview](docs/screenshots/preview-mode.png)
- 編輯模式：![Edit](docs/screenshots/edit-mode.png)
- 主題切換：![Themes](docs/screenshots/themes.png)

## 安裝與使用（使用發行版本）

1. 前往 GitHub 專案頁面的 **Releases**，下載最新的 Windows 安裝檔或免安裝壓縮檔。
2. 若使用安裝程式（例如 `MD_Viewer_Setup_v1.0.0_x64.exe`），依照安裝精靈完成安裝後，在開始功能表搜尋「MD Viewer」啟動。
3. 若使用免安裝版壓縮檔（例如 `MD_Viewer-v1.0.0-win-x64.zip`）：
   - 解壓縮到任意資料夾（建議非系統保護路徑，例如 `D:\Apps\MD_Viewer\`）。
   - 執行資料夾中的 `MD_Viewer.vbs`（推薦）或 `MD_Viewer.bat` 啟動程式。

### 系統需求（執行已發行版本）

- Windows 10 版本 19041（或更新版本）
- x64 處理器

## 從原始碼建置

### 先決條件

- Visual Studio 2022（建議最新版），安裝：
  - **.NET Multiplatform App UI development** 工作負載（MAUI）
  - 對應版本的 **.NET SDK**（目前專案使用 `net10.0` / `net10.0-windows10.0.19041.0`）

### 建置步驟

```bash
git clone https://github.com/<your-account>/MD_Viewer.git
cd MD_Viewer
dotnet build MD_Viewer.sln
```

或是透過 Visual Studio 開啟 `MD_Viewer.sln`：

1. 選擇啟動專案為 **MD_Viewer**。
2. 目標平台選擇 **Windows Machine**。
3. 按下 F5（偵錯執行）或 Ctrl+F5（直接執行）。

## 專案結構概觀

```text
MD_Viewer/                # MAUI 主專案（Windows App）
MD_Viewer.Shared/         # 共用 ViewModel / Models / 服務
MD_Viewer.Blazor/         # Markdown 預覽的 Blazor 元件
docs/                     # 架構設計、技術分析、測試規劃等文件
docs/screenshots/         # 截圖（供 README 與說明文件使用）
scripts/                  # 簽章、雜湊計算等 PowerShell 腳本
release/                  # 封裝後的執行檔結構與說明
publish/                  # dotnet publish / 釋出版輸出
```

## 架構與技術

- **UI / 平台**：.NET MAUI（目前目標 Windows 10/11），使用 Shell + XAML 介面。
- **預覽引擎**：Blazor WebView + marked.js + highlight.js + KaTeX 等前端套件（透過內嵌網頁呈現）。
- **Markdown 解析**：Markdig（C# 端），啟用多種進階擴充。
- **匯出 PDF**：QuestPDF，使用 Markdown AST 直接產生版面。
- **架構風格**：MVVM（CommunityToolkit.MVVM），以 DI 注入服務與 ViewModel。
- **平台抽象**：IPlatformFileSystem / IPlatformFilePicker 等介面，對應 Windows / 後續其他平台實作。

## 相關文件

- `CHANGELOG.md`：版本變更紀錄。
- `docs/NET版本說明.md`：各專案 Target Framework / .NET 版本說明。
- `docs/技術分析報告.md`：技術選型與實作細節。
- `docs/避免防毒軟體誤判指南.md`、`防毒軟體誤判-快速解決.md`：與防毒軟體誤判相關的說明。
- `scripts/README.md`：簽章與雜湊計算腳本的使用方式。

## 授權條款

本專案採用 [MIT License](LICENSE) 授權，你可以自由使用、修改與散佈本程式碼，但請保留著作權與授權宣告。

---

如果你在使用或建置過程中遇到任何問題，歡迎透過 Issue 回報或提交 PR 協助改進。
