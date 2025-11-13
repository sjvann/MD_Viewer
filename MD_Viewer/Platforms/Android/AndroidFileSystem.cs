#if ANDROID
using MD_Viewer.Models;
using MD_Viewer.Services.Platform;

namespace MD_Viewer.Platforms.Android;

/// <summary>
/// Android 平台檔案系統實作（暫時為空實作）
/// </summary>
public class AndroidFileSystem : IPlatformFileSystem
{
	public Task<List<DriveInfo>> GetDrivesAsync()
	{
		throw new NotImplementedException();
	}

	public Task<string?> RequestFileAccessAsync()
	{
		throw new NotImplementedException();
	}

	public Task<string?> RequestDirectoryAccessAsync()
	{
		throw new NotImplementedException();
	}

	public Task<bool> HasFileAccessAsync(string path)
	{
		throw new NotImplementedException();
	}

	public string GetAppDataDirectory()
	{
		throw new NotImplementedException();
	}

	public string GetTempDirectory()
	{
		throw new NotImplementedException();
	}
}
#endif

