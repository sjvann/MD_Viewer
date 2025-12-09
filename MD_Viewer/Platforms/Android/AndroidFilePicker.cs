#if ANDROID
using Android.Content;
using Android.Provider;
using Microsoft.Extensions.Logging;
using MD_Viewer.Services.Platform;

namespace MD_Viewer.Platforms.Android;

/// <summary>
/// Android 平台檔案選擇器實作
/// </summary>
public class AndroidFilePicker : IPlatformFilePicker
{
	private readonly ILogger<AndroidFilePicker> _logger;
	private TaskCompletionSource<string?>? _taskCompletionSource;

	public AndroidFilePicker(ILogger<AndroidFilePicker> logger)
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
			// 取得檔案類型的 MIME type
			var mimeType = GetMimeType(fileTypeChoices);

			// 使用 SAF (Storage Access Framework) 建立文件
			var intent = new Intent(Intent.ActionCreateDocument);
			intent.AddCategory(Intent.CategoryOpenable);
			intent.SetType(mimeType);

			if (!string.IsNullOrEmpty(defaultFileName))
			{
				intent.PutExtra(Intent.ExtraTitle, defaultFileName);
			}

			// 取得當前 Activity
			var activity = Platform.CurrentActivity;
			if (activity == null)
			{
				_logger.LogError("無法取得 Android Activity");
				return null;
			}

			_taskCompletionSource = new TaskCompletionSource<string?>();

			// 啟動 Activity 並等待結果
			activity.StartActivityForResult(intent, 1001);

			// 注意：這個簡化版本需要額外處理 OnActivityResult
			// 這裡使用替代方案：直接存到下載資料夾
			var downloadsPath = global::Android.OS.Environment.GetExternalStoragePublicDirectory(
				global::Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;

			if (string.IsNullOrEmpty(downloadsPath))
			{
				downloadsPath = FileSystem.AppDataDirectory;
			}

			var fileName = defaultFileName ?? "document";
			var filePath = Path.Combine(downloadsPath, fileName);

			// 確保檔名唯一
			var counter = 1;
			var baseName = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			while (File.Exists(filePath))
			{
				filePath = Path.Combine(downloadsPath, $"{baseName}_{counter++}{extension}");
			}

			_logger.LogInformation("Android 檔案儲存路徑: {Path}", filePath);
			return filePath;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Android 檔案選擇器發生錯誤");
			return null;
		}
	}

	/// <summary>
	/// 根據副檔名取得 MIME type
	/// </summary>
	private string GetMimeType(Dictionary<string, string[]>? fileTypeChoices)
	{
		if (fileTypeChoices == null || fileTypeChoices.Count == 0)
		{
			return "*/*";
		}

		foreach (var choice in fileTypeChoices)
		{
			foreach (var ext in choice.Value)
			{
				return ext.ToLowerInvariant() switch
				{
					".pdf" => "application/pdf",
					".html" or ".htm" => "text/html",
					".txt" => "text/plain",
					".md" => "text/markdown",
					_ => "*/*"
				};
			}
		}

		return "*/*";
	}
}
#endif
