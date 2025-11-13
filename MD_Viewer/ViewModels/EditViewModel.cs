using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;

namespace MD_Viewer.ViewModels;

/// <summary>
/// 編輯 ViewModel
/// </summary>
public partial class EditViewModel : ObservableObject, IDisposable
{
	private readonly IMessenger _messenger;
	private readonly ILogger<EditViewModel> _logger;
	private PreviewViewModel? _previewViewModel;

	private string _markdownContent = string.Empty;
	private System.Threading.Timer? _debounceTimer;
	private const int DebounceDelayMs = 500; // 防抖延遲 500ms

	public EditViewModel(
		IMessenger messenger,
		ILogger<EditViewModel> logger)
	{
		_messenger = messenger;
		_logger = logger;
	}

	/// <summary>
	/// 設定預覽 ViewModel（用於即時同步）
	/// </summary>
	public PreviewViewModel? PreviewViewModel
	{
		get => _previewViewModel;
		set => SetProperty(ref _previewViewModel, value);
	}

	/// <summary>
	/// Markdown 內容
	/// </summary>
	public string MarkdownContent
	{
		get => _markdownContent;
		set
		{
			if (SetProperty(ref _markdownContent, value))
			{
				// 使用防抖機制更新預覽
				DebounceUpdatePreview();
				// 通知 UI 狀態變更
				OnPropertyChanged(nameof(ShowEditor));
				OnPropertyChanged(nameof(ShowEmptyState));
			}
		}
	}

	/// <summary>
	/// 是否顯示編輯器（有內容）
	/// </summary>
	public bool ShowEditor => !string.IsNullOrEmpty(MarkdownContent);

	/// <summary>
	/// 是否顯示空狀態（無內容）
	/// </summary>
	public bool ShowEmptyState => string.IsNullOrEmpty(MarkdownContent);

	/// <summary>
	/// 防抖更新預覽
	/// </summary>
	private void DebounceUpdatePreview()
	{
		// 清除之前的計時器
		_debounceTimer?.Dispose();

		// 建立新的計時器
		_debounceTimer = new System.Threading.Timer(_ =>
		{
			// 在主執行緒上更新預覽
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UpdatePreview();
			});
		}, null, DebounceDelayMs, Timeout.Infinite);
	}

	/// <summary>
	/// 更新預覽
	/// </summary>
	private void UpdatePreview()
	{
		try
		{
			PreviewViewModel?.UpdatePreviewCommand?.Execute(MarkdownContent);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "更新預覽失敗");
		}
	}

	/// <summary>
	/// 載入內容到編輯器
	/// </summary>
	public void LoadContent(string content)
	{
		MarkdownContent = content ?? string.Empty;
	}

	/// <summary>
	/// 清除內容
	/// </summary>
	public void ClearContent()
	{
		MarkdownContent = string.Empty;
	}

	/// <summary>
	/// 釋放資源
	/// </summary>
	public void Dispose()
	{
		_debounceTimer?.Dispose();
		_debounceTimer = null;
	}
}

