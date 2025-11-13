# MVVM 結構設計文件

## 1. MVVM 模式概述

本專案採用 **MVVM (Model-View-ViewModel)** 模式，使用 **CommunityToolkit.Mvvm** 簡化實作。

### 1.1 MVVM 三層關係

```
┌─────────────────────────────────────────────────┐
│                    View Layer                    │
│  (XAML + Code-Behind)                            │
│  - MainPage.xaml                                 │
│  - FileTreeView.xaml                             │
│  - PreviewView.xaml                              │
│  - 僅負責 UI 呈現，不包含業務邏輯                │
└──────────────────┬──────────────────────────────┘
                   │ Data Binding
                   │ Commands
┌──────────────────┴──────────────────────────────┐
│              ViewModel Layer                     │
│  - MainViewModel                                │
│  - FileTreeViewModel                            │
│  - PreviewViewModel                             │
│  - 包含 UI 狀態和命令邏輯                        │
└──────────────────┬──────────────────────────────┘
                   │ Uses
┌──────────────────┴──────────────────────────────┐
│              Model/Service Layer                │
│  - FileNode (Model)                             │
│  - IFileSystemService (Service)                 │
│  - IMarkdownService (Service)                   │
│  - 業務邏輯和資料模型                            │
└─────────────────────────────────────────────────┘
```

## 2. ViewModel 層級結構

### 2.1 ViewModel 繼承層級

```
ObservableObject (CommunityToolkit.Mvvm)
    │
    ├── MainViewModel (根 ViewModel)
    │       │
    │       ├── FileTreeViewModel (子 ViewModel)
    │       │
    │       └── PreviewViewModel (子 ViewModel)
    │
    └── (其他 ViewModel)
```

### 2.2 ViewModel 職責劃分

#### MainViewModel
- **職責**：應用程式主視窗的狀態管理
- **管理**：
  - 當前檢視模式（預覽/編輯）
  - 當前選中的檔案
  - 檔案內容
  - 工具列命令
- **協調**：協調子 ViewModel 之間的互動

#### FileTreeViewModel
- **職責**：檔案樹的狀態管理
- **管理**：
  - 磁碟列表
  - 目錄樹結構
  - 選中的節點
- **操作**：展開/收合節點、載入目錄

#### PreviewViewModel
- **職責**：預覽面板的狀態管理
- **管理**：
  - 渲染後的 HTML
  - 預覽樣式設定
- **操作**：更新預覽內容

## 3. 資料綁定設計

### 3.1 單向綁定 (One-Way Binding)

用於顯示資料，ViewModel → View：

```xml
<!-- 顯示檔案內容 -->
<Label Text="{Binding CurrentFileContent}" />

<!-- 顯示載入狀態 -->
<ActivityIndicator IsRunning="{Binding IsLoading}" />
```

### 3.2 雙向綁定 (Two-Way Binding)

用於可編輯的內容，ViewModel ↔ View：

```xml
<!-- 編輯模式下的內容編輯 -->
<Editor Text="{Binding CurrentFileContent, Mode=TwoWay}" />
```

### 3.3 命令綁定 (Command Binding)

用於使用者操作：

```xml
<!-- 編輯按鈕 -->
<Button Text="編輯" Command="{Binding ToggleEditModeCommand}" />

<!-- 匯出按鈕 -->
<Button Text="匯出" Command="{Binding ExportCommand}" 
        CommandParameter="{Binding SelectedFormat}" />
```

## 4. 命令模式實作

### 4.1 RelayCommand (無參數)

```csharp
[RelayCommand]
public void ToggleEditMode()
{
    CurrentMode = CurrentMode == ViewMode.Preview 
        ? ViewMode.Edit 
        : ViewMode.Preview;
}
```

對應 XAML：
```xml
<Button Command="{Binding ToggleEditModeCommand}" />
```

### 4.2 RelayCommand<T> (有參數)

```csharp
[RelayCommand]
public async Task ExportAsync(ExportFormat format)
{
    if (format == null) return;
    
    await _exportService.ExportAsync(format, CurrentFileContent, ...);
}
```

對應 XAML：
```xml
<Button Command="{Binding ExportCommand}" 
        CommandParameter="{Binding SelectedFormat}" />
```

### 4.3 AsyncRelayCommand (非同步命令)

```csharp
[RelayCommand]
public async Task LoadFileAsync(string filePath)
{
    try
    {
        IsLoading = true;
        CurrentFileContent = await _fileSystemService.ReadFileAsync(filePath);
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 4.4 命令啟用條件

```csharp
[RelayCommand(CanExecute = nameof(CanExport))]
public async Task ExportAsync(ExportFormat format)
{
    // 匯出邏輯
}

private bool CanExport()
{
    return !string.IsNullOrEmpty(CurrentFileContent) && !IsLoading;
}
```

## 5. 屬性變更通知

### 5.1 ObservableObject 自動通知

```csharp
public partial class MainViewModel : ObservableObject
{
    private ViewMode _currentMode = ViewMode.Preview;

    public ViewMode CurrentMode
    {
        get => _currentMode;
        set => SetProperty(ref _currentMode, value);
    }
}
```

### 5.2 自訂屬性變更邏輯

```csharp
public ViewMode CurrentMode
{
    get => _currentMode;
    set
    {
        if (SetProperty(ref _currentMode, value))
        {
            // 屬性變更後的額外邏輯
            OnPropertyChanged(nameof(IsEditMode));
            ToggleEditModeCommand.NotifyCanExecuteChanged();
        }
    }
}

public bool IsEditMode => CurrentMode == ViewMode.Edit;
```

### 5.3 集合變更通知

使用 `ObservableCollection<T>`：

```csharp
public ObservableCollection<FileNode> FileTree { get; } = new();

// 新增項目時自動通知 UI
FileTree.Add(newNode);

// 移除項目時自動通知 UI
FileTree.Remove(node);
```

## 6. ViewModel 生命週期

### 6.1 ViewModel 建立

```csharp
// 在 MauiProgram.cs 中註冊
services.AddTransient<MainViewModel>();

// 在 View 的 Code-Behind 或 XAML 中注入
public MainPage(MainViewModel viewModel)
{
    InitializeComponent();
    BindingContext = viewModel;
}
```

### 6.2 ViewModel 初始化

```csharp
public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        
        // 初始化邏輯
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        // 非同步初始化
        await LoadInitialDataAsync();
    }
}
```

### 6.3 ViewModel 清理

```csharp
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IMessenger _messenger;

    public MainViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<FileSelectedMessage>(this, OnFileSelected);
    }

    public void Dispose()
    {
        // 取消註冊訊息
        _messenger.UnregisterAll(this);
    }
}
```

## 7. ViewModel 間通訊

### 7.1 Messenger Pattern

使用 `IMessenger` 進行 ViewModel 間通訊：

```csharp
// 發送訊息
public partial class FileTreeViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public FileNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (SetProperty(ref _selectedNode, value) && value != null)
            {
                _messenger.Send(new FileSelectedMessage(value));
            }
        }
    }
}

// 接收訊息
public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Register<FileSelectedMessage>(this, OnFileSelected);
    }

    private void OnFileSelected(object recipient, FileSelectedMessage message)
    {
        CurrentFile = message.Value;
        LoadFileAsync(message.Value.Path);
    }
}
```

### 7.2 父 ViewModel 直接引用

當 ViewModel 有明確的父子關係時：

```csharp
public partial class MainViewModel : ObservableObject
{
    public FileTreeViewModel FileTreeViewModel { get; }
    public PreviewViewModel PreviewViewModel { get; }

    public MainViewModel(
        FileTreeViewModel fileTreeViewModel,
        PreviewViewModel previewViewModel)
    {
        FileTreeViewModel = fileTreeViewModel;
        PreviewViewModel = previewViewModel;
    }
}
```

## 8. 資料驗證

### 8.1 使用 ValidationAttribute

```csharp
public partial class ExportViewModel : ObservableValidator
{
    [Required]
    [MinLength(1)]
    public string OutputPath
    {
        get => _outputPath;
        set => SetProperty(ref _outputPath, value, true);
    }

    public bool HasErrors => GetErrors().Any();
}
```

### 8.2 自訂驗證邏輯

```csharp
public string OutputPath
{
    get => _outputPath;
    set
    {
        if (SetProperty(ref _outputPath, value))
        {
            ValidateOutputPath();
        }
    }
}

private void ValidateOutputPath()
{
    ClearErrors();
    
    if (string.IsNullOrEmpty(OutputPath))
    {
        AddError(nameof(OutputPath), "輸出路徑不能為空");
    }
    else if (!Path.IsPathRooted(OutputPath))
    {
        AddError(nameof(OutputPath), "輸出路徑必須是絕對路徑");
    }
}
```

## 9. 錯誤處理

### 9.1 ViewModel 中的錯誤處理

```csharp
[RelayCommand]
public async Task LoadFileAsync(string filePath)
{
    try
    {
        IsLoading = true;
        ErrorMessage = null;
        
        CurrentFileContent = await _fileSystemService.ReadFileAsync(filePath);
    }
    catch (FileNotFoundException ex)
    {
        ErrorMessage = $"檔案不存在：{ex.FileName}";
    }
    catch (UnauthorizedAccessException)
    {
        ErrorMessage = "沒有存取此檔案的權限";
    }
    catch (Exception ex)
    {
        ErrorMessage = $"載入檔案時發生錯誤：{ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 9.2 錯誤訊息綁定

```xml
<Label Text="{Binding ErrorMessage}" 
       IsVisible="{Binding HasError}"
       TextColor="Red" />
```

## 10. 範例：完整的 ViewModel

```csharp
namespace MD_Viewer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IMarkdownService _markdownService;
    private readonly IExportService _exportService;
    private readonly IMessenger _messenger;

    private ViewMode _currentMode = ViewMode.Preview;
    private FileNode? _currentFile;
    private string _currentFileContent = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;

    public MainViewModel(
        IFileSystemService fileSystemService,
        IMarkdownService markdownService,
        IExportService exportService,
        IMessenger messenger)
    {
        _fileSystemService = fileSystemService;
        _markdownService = markdownService;
        _exportService = exportService;
        _messenger = messenger;

        // 註冊訊息
        _messenger.Register<FileSelectedMessage>(this, OnFileSelected);
    }

    // 屬性
    public ViewMode CurrentMode
    {
        get => _currentMode;
        set => SetProperty(ref _currentMode, value);
    }

    public FileNode? CurrentFile
    {
        get => _currentFile;
        set => SetProperty(ref _currentFile, value);
    }

    public string CurrentFileContent
    {
        get => _currentFileContent;
        set => SetProperty(ref _currentFileContent, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEditMode => CurrentMode == ViewMode.Edit;
    public bool IsPreviewMode => CurrentMode == ViewMode.Preview;

    // 命令
    [RelayCommand]
    public void ToggleEditMode()
    {
        CurrentMode = CurrentMode == ViewMode.Preview 
            ? ViewMode.Edit 
            : ViewMode.Preview;
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    public async Task ExportAsync(ExportFormat format)
    {
        if (format == null || string.IsNullOrEmpty(CurrentFileContent))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // 選擇儲存位置
            var outputPath = await SelectOutputPathAsync(format.Extension);
            if (string.IsNullOrEmpty(outputPath))
                return;

            // 執行匯出
            await _exportService.ExportAsync(format, CurrentFileContent, outputPath);
            
            // 顯示成功訊息
            await ShowSuccessMessageAsync("匯出成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"匯出失敗：{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanExport()
    {
        return !string.IsNullOrEmpty(CurrentFileContent) && !IsLoading;
    }

    private void OnFileSelected(object recipient, FileSelectedMessage message)
    {
        if (message.Value?.Type != FileNodeType.File)
            return;

        LoadFileCommand.ExecuteAsync(message.Value.Path);
    }

    [RelayCommand]
    private async Task LoadFileAsync(string filePath)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            CurrentFileContent = await _fileSystemService.ReadFileAsync(filePath);
            CurrentFile = new FileNode { Path = filePath };
        }
        catch (Exception ex)
        {
            ErrorMessage = $"載入檔案失敗：{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<string?> SelectOutputPathAsync(string extension)
    {
        // 使用平台特定的檔案選擇對話框
        return await _fileSystemService.RequestFileAccessAsync();
    }

    private async Task ShowSuccessMessageAsync(string message)
    {
        // 顯示成功訊息（使用平台特定的通知）
        await Application.Current.MainPage.DisplayAlert("成功", message, "確定");
    }
}
```

