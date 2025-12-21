; MD Viewer 安裝程式腳本
; 使用 Inno Setup 編譯
; 下載 Inno Setup: https://jrsoftware.org/isdl.php

#define MyAppName "MD Viewer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "sjvann"
#define MyAppURL "https://github.com/sjvann/MD_Viewer"
#define MyAppExeName "MD_Viewer.exe"
#define PublishDir "publish"

[Setup]
; 應用程式識別碼（每個應用程式唯一）
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases

; 安裝目錄
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; 輸出設定
OutputDir=installer
OutputBaseFilename=MD_Viewer_Setup_v{#MyAppVersion}_x64

; 壓縮設定（使用最佳壓縮）
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=4

; 權限設定（不需要管理員權限）
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; 其他設定
WizardStyle=modern
DisableProgramGroupPage=yes
ShowLanguageDialog=auto
UninstallDisplayIcon={app}\{#MyAppExeName}

; 支援的架構
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; 版本資訊
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "tchinese"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "schinese"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "fileassoc"; Description: "將 .md 檔案與 MD Viewer 關聯"; GroupDescription: "檔案關聯:"

[Files]
; 包含發布目錄中的所有檔案
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
; 確保這些目錄存在
Name: "{app}\runtimes"; Flags: uninsalwaysuninstall

[Icons]
; 開始選單
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Markdown 文件檢視器"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; 桌面捷徑
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Markdown 文件檢視器"

[Run]
; 安裝完成後詢問是否啟動程式
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 解除安裝時刪除使用者資料（可選）
Type: filesandordirs; Name: "{app}"

[Registry]
; 註冊 .md 檔案關聯
Root: HKA; Subkey: "Software\Classes\.md\OpenWithProgids"; ValueType: string; ValueName: "MDViewer.md"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\MDViewer.md"; ValueType: string; ValueName: ""; ValueData: "Markdown 文件"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\MDViewer.md\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\MDViewer.md\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; 註冊 .markdown 檔案關聯
Root: HKA; Subkey: "Software\Classes\.markdown\OpenWithProgids"; ValueType: string; ValueName: "MDViewer.md"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc

[Code]
// 安裝前檢查
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

// 解除安裝前確認
function InitializeUninstall(): Boolean;
begin
  Result := MsgBox('確定要解除安裝 {#MyAppName} 嗎？', mbConfirmation, MB_YESNO) = IDYES;
end;
