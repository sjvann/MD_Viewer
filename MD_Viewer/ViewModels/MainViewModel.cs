using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MD_Viewer.Messages;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.Services.Platform;
using MD_Viewer.ViewModels;

namespace MD_Viewer.ViewModels;

/// <summary>
/// 主 ViewModel
/// </summary>
public partial class MainViewModel : ObservableObject, IMainViewModel
{
	private readonly IFileSystemService _fileSystemService;
	private readonly IMarkdownService _markdownService;
	private readonly IExportService _exportService;
	private readonly IPlatformFilePicker _filePicker;
	private readonly IMessenger _messenger;

	private ViewMode _currentMode = ViewMode.Preview;
	private FileNode? _currentFile;
	private string _currentFileContent = string.Empty;
	private bool _isLoading;
	private string? _errorMessage;
	private bool _isSaving;
	private string? _saveMessage;
	private bool _isExporting;
	private string? _exportMessage;
	private bool _showExportMenu;

	// PDF 匯出選項
	private PdfPageSize _exportPageSize = PdfPageSize.A4;
	private PdfPageOrientation _exportOrientation = PdfPageOrientation.Portrait;
	private bool _includePageNumbers = true;

	public MainViewModel(
		IFileSystemService fileSystemService,
		IMarkdownService markdownService,
		IExportService exportService,
		IPlatformFilePicker filePicker,
		IMessenger messenger,
		FileTreeViewModel fileTreeViewModel,
		PreviewViewModel previewViewModel,
		EditViewModel editViewModel)
	{
		_fileSystemService = fileSystemService;
		_markdownService = markdownService;
		_exportService = exportService;
		_filePicker = filePicker;
		_messenger = messenger;
		FileTreeViewModel = fileTreeViewModel;
		PreviewViewModel = previewViewModel;
		EditViewModel = editViewModel;

		// 設定 EditViewModel 的 PreviewViewModel 引用（用於即時同步）
		EditViewModel.PreviewViewModel = PreviewViewModel;

		// 註冊訊息接收
		_messenger.Register<FileSelectedMessage>(this, OnFileSelected);

		// 初始化支援的匯出格式（從 ExportService 取得）
		InitializeExportFormats();
	}

	/// <summary>
	/// 初始化支援的匯出格式
	/// </summary>
	private void InitializeExportFormats()
	{
		var formats = _exportService.GetSupportedFormats();
		_supportedExportFormats.Clear();
		foreach (var format in formats)
		{
			_supportedExportFormats.Add(format);
		}
	}

	/// <summary>
	/// 檔案樹 ViewModel
	/// </summary>
	public FileTreeViewModel FileTreeViewModel { get; }

	/// <summary>
	/// 預覽 ViewModel
	/// </summary>
	public IPreviewViewModel? PreviewViewModel { get; }

	/// <summary>
	/// 編輯 ViewModel
	/// </summary>
	public IEditViewModel? EditViewModel { get; }

	/// <summary>
	/// 當前檢視模式
	/// </summary>
	public ViewMode CurrentMode
	{
		get => _currentMode;
		set
		{
			if (SetProperty(ref _currentMode, value))
			{
				// 當模式變更時，通知相關屬性更新
				OnPropertyChanged(nameof(IsEditMode));
				OnPropertyChanged(nameof(EditButtonText));
				// 當切換到編輯模式時，載入內容到編輯器
				if (value == ViewMode.Edit && !string.IsNullOrEmpty(CurrentFileContent))
				{
					EditViewModel?.LoadContent(CurrentFileContent);
				}
			}
		}
	}

	/// <summary>
	/// 當前選中的檔案
	/// </summary>
	public FileNode? CurrentFile
	{
		get => _currentFile;
		set
		{
			if (SetProperty(ref _currentFile, value))
			{
				// 當檔案變更時，通知 CanSave 更新
				OnPropertyChanged(nameof(CanSave));
				SaveFileCommand?.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// 當前檔案內容
	/// </summary>
	public string CurrentFileContent
	{
		get => _currentFileContent;
		set
		{
			if (SetProperty(ref _currentFileContent, value))
			{
				// 當檔案內容變更時，更新預覽和編輯器
				PreviewViewModel?.UpdatePreview(value);
				EditViewModel?.LoadContent(value);
				// 通知 CanExport 更新
				OnPropertyChanged(nameof(CanExport));
				ToggleExportMenuCommand?.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// 是否載入中
	/// </summary>
	public bool IsLoading
	{
		get => _isLoading;
		set => SetProperty(ref _isLoading, value);
	}

	/// <summary>
	/// 錯誤訊息
	/// </summary>
	public string? ErrorMessage
	{
		get => _errorMessage;
		set => SetProperty(ref _errorMessage, value);
	}

	/// <summary>
	/// 是否為編輯模式
	/// </summary>
	public bool IsEditMode => CurrentMode == ViewMode.Edit;

	/// <summary>
	/// 編輯按鈕文字（根據當前模式切換）
	/// </summary>
	public string EditButtonText => IsEditMode ? "預覽" : "編輯";

	/// <summary>
	/// 是否可以匯出（有當前檔案內容且非匯出中）
	/// </summary>
	public bool CanExport => !string.IsNullOrEmpty(CurrentFileContent) && !IsExporting;

	/// <summary>
	/// 是否正在匯出
	/// </summary>
	public bool IsExporting
	{
		get => _isExporting;
		set
		{
			if (SetProperty(ref _isExporting, value))
			{
				OnPropertyChanged(nameof(CanExport));
			}
		}
	}

	/// <summary>
	/// 匯出訊息（成功或錯誤）
	/// </summary>
	public string? ExportMessage
	{
		get => _exportMessage;
		set => SetProperty(ref _exportMessage, value);
	}

	/// <summary>
	/// 是否顯示匯出選單
	/// </summary>
	public bool ShowExportMenu
	{
		get => _showExportMenu;
		set => SetProperty(ref _showExportMenu, value);
	}

	/// <summary>
	/// 匯出頁面大小
	/// </summary>
	public PdfPageSize ExportPageSize
	{
		get => _exportPageSize;
		set => SetProperty(ref _exportPageSize, value);
	}

	/// <summary>
	/// 匯出頁面方向
	/// </summary>
	public PdfPageOrientation ExportOrientation
	{
		get => _exportOrientation;
		set => SetProperty(ref _exportOrientation, value);
	}

	/// <summary>
	/// 是否包含頁碼
	/// </summary>
	public bool IncludePageNumbers
	{
		get => _includePageNumbers;
		set => SetProperty(ref _includePageNumbers, value);
	}

	/// <summary>
	/// 是否正在儲存
	/// </summary>
	public bool IsSaving
	{
		get => _isSaving;
		set
		{
			if (SetProperty(ref _isSaving, value))
			{
				// 當儲存狀態變更時，通知 CanSave 更新
				OnPropertyChanged(nameof(CanSave));
				SaveFileCommand?.NotifyCanExecuteChanged();
			}
		}
	}

	/// <summary>
	/// 儲存訊息（成功或錯誤）
	/// </summary>
	public string? SaveMessage
	{
		get => _saveMessage;
		set => SetProperty(ref _saveMessage, value);
	}

	/// <summary>
	/// 是否可以儲存（有當前檔案且非儲存中）
	/// </summary>
	public bool CanSave => CurrentFile != null && !IsSaving;

	/// <summary>
	/// 支援的匯出格式列表
	/// </summary>
	public IEnumerable<ExportFormat> SupportedExportFormats => _supportedExportFormats;
	private readonly ObservableCollection<ExportFormat> _supportedExportFormats = new();

	/// <summary>
	/// 切換編輯模式命令
	/// </summary>
	[RelayCommand]
	public void ToggleEditMode()
	{
		CurrentMode = CurrentMode == ViewMode.Preview ? ViewMode.Edit : ViewMode.Preview;
	}

	/// <summary>
	/// 切換匯出選單顯示狀態
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanExport))]
	public void ToggleExportMenu()
	{
		if (string.IsNullOrEmpty(CurrentFileContent))
		{
			ExportMessage = "沒有可匯出的內容";
			return;
		}

		ShowExportMenu = !ShowExportMenu;
	}

	/// <summary>
	/// 關閉匯出選單
	/// </summary>
	[RelayCommand]
	public void CloseExportMenu()
	{
		ShowExportMenu = false;
	}

	// 設定 PDF 選項命令
	[RelayCommand] public void SetPageSizeA4() => ExportPageSize = PdfPageSize.A4;
	[RelayCommand] public void SetPageSizeLetter() => ExportPageSize = PdfPageSize.Letter;
	[RelayCommand] public void SetPageSizeLegal() => ExportPageSize = PdfPageSize.Legal;
	[RelayCommand] public void SetOrientationPortrait() => ExportOrientation = PdfPageOrientation.Portrait;
	[RelayCommand] public void SetOrientationLandscape() => ExportOrientation = PdfPageOrientation.Landscape;

	/// <summary>
	/// 匯出命令
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanExport))]
	public async Task ExportAsync(ExportFormat? format)
	{
		if (format == null)
		{
			ExportMessage = "請選擇匯出格式";
			return;
		}

		if (!format.IsEnabled)
		{
			ExportMessage = $"格式 {format.Name} 尚未實作";
			return;
		}

		if (string.IsNullOrEmpty(CurrentFileContent))
		{
			ExportMessage = "沒有可匯出的內容";
			return;
		}

		try
		{
			IsExporting = true;
			ExportMessage = null;
			ErrorMessage = null;
			ShowExportMenu = false;

			// 取得要匯出的內容（優先使用編輯器內容）
			var contentToExport = EditViewModel?.MarkdownContent ?? CurrentFileContent;

			// 產生預設檔名（基於當前檔案名稱）
			var defaultFileName = GetDefaultExportFileName(format.Extension);

			// 顯示檔案儲存對話框
			var fileTypeChoices = new Dictionary<string, string[]>
			{
				{ format.Name, new[] { format.Extension } }
			};

			var outputPath = await _filePicker.PickSaveFileAsync(defaultFileName, fileTypeChoices);

			if (string.IsNullOrEmpty(outputPath))
			{
				// 使用者取消操作
				return;
			}

			// 執行匯出（依格式分支）
			var exportOptions = new ExportOptions
			{
				IncludeStyles = true,
				Title = CurrentFile?.Name,
				Author = null,
				PageSize = ExportPageSize,
				Orientation = ExportOrientation,
				IncludePageNumbers = IncludePageNumbers
			};

			if (string.Equals(format.Extension, ".pdf", StringComparison.OrdinalIgnoreCase))
			{
				await _exportService.ExportToPdfAsync(contentToExport, outputPath, exportOptions);
			}
			else if (string.Equals(format.Extension, ".html", StringComparison.OrdinalIgnoreCase))
			{
				await _exportService.ExportToHtmlAsync(contentToExport, outputPath, exportOptions);
			}
			else
			{
				throw new NotSupportedException($"尚未支援的匯出格式：{format.Name}");
			}

			ExportMessage = "匯出成功";
		}
		catch (Exception ex)
		{
			ExportMessage = $"匯出失敗: {ex.Message}";
			ErrorMessage = ExportMessage;
		}
		finally
		{
			IsExporting = false;
			// 清除匯出訊息（3 秒後）
			_ = Task.Delay(3000).ContinueWith(_ =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ExportMessage = null;
				});
			});
		}
	}

	/// <summary>
	/// 取得預設匯出檔名
	/// </summary>
	private string GetDefaultExportFileName(string extension)
	{
		if (CurrentFile != null && !string.IsNullOrEmpty(CurrentFile.Name))
		{
			// 移除原始副檔名，加上新的副檔名
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(CurrentFile.Name);
			return $"{fileNameWithoutExtension}{extension}";
		}

		return $"document{extension}";
	}

	/// <summary>
	/// 儲存檔案命令
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanSave))]
	public async Task SaveFileAsync()
	{
		if (CurrentFile == null || string.IsNullOrEmpty(CurrentFile.Path))
		{
			SaveMessage = "沒有選中的檔案";
			return;
		}

		try
		{
			IsSaving = true;
			SaveMessage = null;
			ErrorMessage = null;

			// 從 EditViewModel 取得編輯後的內容
			var contentToSave = EditViewModel?.MarkdownContent ?? CurrentFileContent;

			// 寫入檔案
			await _fileSystemService.WriteFileAsync(CurrentFile.Path, contentToSave);

			// 更新 CurrentFileContent
			CurrentFileContent = contentToSave;

			SaveMessage = "儲存成功";
		}
		catch (Exception ex)
		{
			SaveMessage = $"儲存失敗: {ex.Message}";
			ErrorMessage = SaveMessage;
		}
		finally
		{
			IsSaving = false;
			SaveFileCommand?.NotifyCanExecuteChanged();
			// 清除儲存訊息（3 秒後）
			_ = Task.Delay(3000).ContinueWith(_ =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					SaveMessage = null;
				});
			});
		}
	}

	/// <summary>
	/// 處理檔案選擇訊息
	/// </summary>
	private void OnFileSelected(object recipient, FileSelectedMessage message)
	{
		if (message.Value?.Type != FileNodeType.File)
			return;

		CurrentFile = message.Value;
		// 切換回預覽模式（如果正在編輯模式）
		if (CurrentMode == ViewMode.Edit)
		{
			CurrentMode = ViewMode.Preview;
		}
		_ = LoadFileAsync(message.Value.Path);
	}

	/// <summary>
	/// 載入檔案內容
	/// </summary>
	private async Task LoadFileAsync(string filePath)
	{
		try
		{
			IsLoading = true;
			ErrorMessage = null;
			SaveMessage = null;

			var content = await _fileSystemService.ReadFileAsync(filePath);
			CurrentFileContent = content;
		}
		catch (Exception ex)
		{
			ErrorMessage = $"無法讀取檔案: {ex.Message}";
			CurrentFileContent = string.Empty;
			EditViewModel?.ClearContent();
		}
		finally
		{
			IsLoading = false;
		}
	}
}

