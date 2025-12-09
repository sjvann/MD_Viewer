#if MACCATALYST
using Foundation;
using Microsoft.Extensions.Logging;
using MD_Viewer.Services.Platform;
using UIKit;
using UniformTypeIdentifiers;

namespace MD_Viewer.Platforms.MacCatalyst;

/// <summary>
/// Mac Catalyst 平台檔案選擇器實作
/// </summary>
public class MacFilePicker : IPlatformFilePicker
{
	private readonly ILogger<MacFilePicker> _logger;

	public MacFilePicker(ILogger<MacFilePicker> logger)
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

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				// 取得副檔名和 UTType
				UTType[] allowedTypes = GetAllowedTypes(fileTypeChoices);

				// 建立文件選擇器（儲存模式）
				var picker = new UIDocumentPickerViewController(allowedTypes, asCopy: false);
				picker.DirectoryUrl = NSUrl.FromFilename(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

				// 設定預設檔名
				if (!string.IsNullOrEmpty(defaultFileName))
				{
					// Mac Catalyst 的 UIDocumentPickerViewController 不直接支援預設檔名
					// 改用暫存檔方式
					var tempPath = Path.Combine(FileSystem.CacheDirectory, defaultFileName);
					tcs.SetResult(tempPath);
					return;
				}

				// 簡化實作：直接返回文件目錄路徑
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

				var fileName = defaultFileName ?? $"document{extension}";
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				var filePath = Path.Combine(documentsPath, fileName);

				// 確保檔名唯一
				var counter = 1;
				var baseName = Path.GetFileNameWithoutExtension(fileName);
				while (File.Exists(filePath))
				{
					filePath = Path.Combine(documentsPath, $"{baseName}_{counter++}{extension}");
				}

				tcs.SetResult(filePath);
			});

			var result = await tcs.Task;
			_logger.LogInformation("Mac 檔案儲存路徑: {Path}", result);
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Mac 檔案選擇器發生錯誤");
			return null;
		}
	}

	/// <summary>
	/// 根據檔案類型取得 UTType
	/// </summary>
	private UTType[] GetAllowedTypes(Dictionary<string, string[]>? fileTypeChoices)
	{
		var types = new List<UTType>();

		if (fileTypeChoices != null)
		{
			foreach (var choice in fileTypeChoices)
			{
				foreach (var ext in choice.Value)
				{
					var utType = ext.ToLowerInvariant() switch
					{
						".pdf" => UTTypes.Pdf,
						".html" or ".htm" => UTTypes.Html,
						".txt" => UTTypes.PlainText,
						".md" => UTTypes.PlainText,
						_ => UTTypes.Data
					};
					if (!types.Contains(utType))
					{
						types.Add(utType);
					}
				}
			}
		}

		return types.Count > 0 ? types.ToArray() : new[] { UTTypes.Data };
	}
}
#endif
