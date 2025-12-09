#if WINDOWS
using Microsoft.Extensions.Logging;
using MD_Viewer.Services.Platform;
using WinRT.Interop;

namespace MD_Viewer.Platforms.Windows;

/// <summary>
/// Windows 平台檔案選擇器實作
/// </summary>
public class WindowsFilePicker : IPlatformFilePicker
{
	private readonly ILogger<WindowsFilePicker> _logger;

	public WindowsFilePicker(ILogger<WindowsFilePicker> logger)
	{
		_logger = logger;
	}

	/// <summary>
	/// 顯示檔案儲存對話框
	/// </summary>
	public async Task<string?> PickSaveFileAsync(string? defaultFileName = null, Dictionary<string, string[]>? fileTypeChoices = null)
	{
		try
		{
			var picker = new global::Windows.Storage.Pickers.FileSavePicker();
			
			// 取得視窗句柄並初始化 picker（MAUI 必需）
			var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
			if (mauiWindow?.Handler?.PlatformView is MauiWinUIWindow winUIWindow)
			{
				var hwnd = WindowNative.GetWindowHandle(winUIWindow);
				InitializeWithWindow.Initialize(picker, hwnd);
			}
			
			// 設定初始位置
			picker.SuggestedStartLocation = global::Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

			// 設定檔案類型過濾器
			if (fileTypeChoices != null && fileTypeChoices.Count > 0)
			{
				foreach (var choice in fileTypeChoices)
				{
					picker.FileTypeChoices.Add(choice.Key, choice.Value.ToList());
				}
			}
			else
			{
				// 預設為所有檔案
				picker.FileTypeChoices.Add("所有檔案", new List<string> { "." });
			}

			// 設定預設檔名
			if (!string.IsNullOrEmpty(defaultFileName))
			{
				picker.SuggestedFileName = defaultFileName;
			}

			// 顯示對話框
			var file = await picker.PickSaveFileAsync();
			
			if (file != null)
			{
				_logger.LogInformation("檔案儲存路徑: {Path}", file.Path);
				return file.Path;
			}

			_logger.LogInformation("使用者取消儲存對話框");
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "顯示檔案儲存對話框時發生錯誤");
			throw;
		}
	}
}
#endif

