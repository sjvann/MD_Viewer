#if MACCATALYST
using MD_Viewer.Models;
using MD_Viewer.Services.Platform;
using DriveInfo = MD_Viewer.Models.DriveInfo;

namespace MD_Viewer.Platforms.MacCatalyst;

/// <summary>
/// macOS 平台檔案系統實作（暫時為空實作）
/// </summary>
public class MacFileSystem : IPlatformFileSystem
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

