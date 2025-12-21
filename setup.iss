; MD Viewer 安裝程式腳本
; 使用 Inno Setup 編譯
; 下載 Inno Setup: https://jrsoftware.org/isdl.php

#define MyAppName "MD Viewer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "sjvann"
#define MyAppURL "https://github.com/sjvann/MD_Viewer"
#define MyAppExeName "MD_Viewer.exe"

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

; 壓縮設定
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

; 權限設定（不需要管理員權限）
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; 其他設定
WizardStyle=modern
DisableProgramGroupPage=yes

; 支援的架構
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "tchinese"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "schinese"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; 主程式
Source: "publish\MD_Viewer.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; 註冊 .md 檔案關聯（可選）
Root: HKA; Subkey: "Software\Classes\.md\OpenWithProgids"; ValueType: string; ValueName: "MDViewer.md"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\MDViewer.md"; ValueType: string; ValueName: ""; ValueData: "Markdown 檔案"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\MDViewer.md\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\MDViewer.md\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

[Code]
// 安裝前檢查
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
