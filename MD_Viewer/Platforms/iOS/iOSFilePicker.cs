#if IOS
using Foundation;
using Microsoft.Extensions.Logging;
using MD_Viewer.Services.Platform;
using UIKit;
using UniformTypeIdentifiers;

namespace MD_Viewer.Platforms.iOS;

/// <summary>
/// iOS 平台檔案選擇器實作
/// </summary>
public class iOSFilePicker : IPlatformFilePicker
{
	private readonly ILogger<iOSFilePicker> _logger;

	public iOSFilePicker(ILogger<iOSFilePicker> logger)
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
			var tcs = new TaskCompletionSource<string?>();

			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				// 取得副檔名
				var extension = ".pdf";
				if (fileTypeChoices != null)
				{
					foreach (var choice in fileTypeChoices)
					{
						if (choice.Value.Length > 0)
						{
							extension = choice.Value[0];
							break;
						}
					}
				}

				// iOS 不直接支援「另存新檔」對話框
				// 使用分享功能讓使用者選擇儲存位置
				var fileName = defaultFileName ?? $"document{extension}";
				var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// 先回傳暫存路徑，實際檔案會在匯出後存在這裡
				// 然後可以用 Share API 分享出去
				tcs.SetResult(tempPath);
			});

			var result = await tcs.Task;
			_logger.LogInformation("iOS 檔案儲存路徑: {Path}", result);
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "iOS 檔案選擇器發生錯誤");
			return null;
		}
	}
}
#endif
