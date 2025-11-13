#if WINDOWS
using MD_Viewer.Models;
using MD_Viewer.Services.Platform;
using DriveInfo = MD_Viewer.Models.DriveInfo;
using Microsoft.Extensions.Logging;

namespace MD_Viewer.Platforms.Windows;

/// <summary>
/// Windows 平台檔案系統實作
/// </summary>
public class WindowsFileSystem : IPlatformFileSystem
{
	private readonly ILogger<WindowsFileSystem>? _logger;

	public WindowsFileSystem(ILogger<WindowsFileSystem>? logger = null)
	{
		_logger = logger;
	}

	public async Task<List<DriveInfo>> GetDrivesAsync()
	{
		return await Task.Run(() =>
		{
			var drives = new List<DriveInfo>();

			try
			{
				foreach (var drive in System.IO.DriveInfo.GetDrives())
				{
					try
					{
						if (drive.IsReady)
						{
							drives.Add(new DriveInfo
							{
								Name = drive.Name,
								Label = drive.VolumeLabel,
								Path = drive.RootDirectory.FullName,
								IsReady = true,
								TotalSize = drive.TotalSize,
								AvailableSpace = drive.AvailableFreeSpace
							});
						}
					}
					catch (Exception ex)
					{
						_logger?.LogWarning(ex, "無法讀取磁碟資訊: {DriveName}", drive.Name);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "取得磁碟列表時發生錯誤");
				throw;
			}

			return drives;
		});
	}

	public async Task<string?> RequestFileAccessAsync()
	{
		// Windows 可以直接存取檔案系統，無需特殊權限請求
		// 但可以使用 FilePicker 讓使用者選擇檔案
		return await Task.FromResult<string?>(null);
	}

	public async Task<string?> RequestDirectoryAccessAsync()
	{
		try
		{
			var folderPicker = new global::Windows.Storage.Pickers.FolderPicker();
			folderPicker.FileTypeFilter.Add("*");

			var folder = await folderPicker.PickSingleFolderAsync();
			return folder?.Path;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "請求目錄存取權限時發生錯誤");
			return null;
		}
	}

	public async Task<bool> HasFileAccessAsync(string path)
	{
		return await Task.Run(() =>
		{
			try
			{
				return Directory.Exists(path) || File.Exists(path);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "檢查檔案存取權限時發生錯誤: {Path}", path);
				return false;
			}
		});
	}

	public string GetAppDataDirectory()
	{
		var appDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"MD_Viewer"
		);

		// 確保目錄存在
		if (!Directory.Exists(appDataPath))
		{
			Directory.CreateDirectory(appDataPath);
		}

		return appDataPath;
	}

	public string GetTempDirectory()
	{
		return Path.GetTempPath();
	}
}
#endif

