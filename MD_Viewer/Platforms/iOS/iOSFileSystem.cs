#if IOS
using MD_Viewer.Models;
using MD_Viewer.Services.Platform;

namespace MD_Viewer.Platforms.iOS;

/// <summary>
/// iOS 平台檔案系統實作
/// </summary>
public class iOSFileSystem : IPlatformFileSystem
{
	public Task<List<DriveInfo>> GetDrivesAsync()
	{
		// iOS 沒有傳統的磁碟機概念
		var drives = new List<DriveInfo>
		{
			new DriveInfo
			{
				Name = "文件",
				Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				TotalSize = 0,
				AvailableFreeSpace = 0
			}
		};
		return Task.FromResult(drives);
	}

	public Task<string?> RequestFileAccessAsync()
	{
		return Task.FromResult<string?>(null);
	}

	public Task<string?> RequestDirectoryAccessAsync()
	{
		return Task.FromResult<string?>(null);
	}

	public Task<bool> HasFileAccessAsync(string path)
	{
		return Task.FromResult(File.Exists(path) || Directory.Exists(path));
	}

	public string GetAppDataDirectory()
	{
		return FileSystem.AppDataDirectory;
	}

	public string GetTempDirectory()
	{
		return FileSystem.CacheDirectory;
	}
}
#endif
