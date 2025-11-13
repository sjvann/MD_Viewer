using MD_Viewer.Models;
using DriveInfo = MD_Viewer.Models.DriveInfo;

namespace MD_Viewer.Services.Platform;

/// <summary>
/// 平台特定檔案系統介面
/// </summary>
public interface IPlatformFileSystem
{
	/// <summary>
	/// 取得平台特定的磁碟/根目錄列表
	/// </summary>
	Task<List<DriveInfo>> GetDrivesAsync();

	/// <summary>
	/// 請求檔案存取權限
	/// </summary>
	Task<string?> RequestFileAccessAsync();

	/// <summary>
	/// 請求目錄存取權限
	/// </summary>
	Task<string?> RequestDirectoryAccessAsync();

	/// <summary>
	/// 檢查是否有檔案存取權限
	/// </summary>
	Task<bool> HasFileAccessAsync(string path);

	/// <summary>
	/// 取得應用程式資料目錄
	/// </summary>
	string GetAppDataDirectory();

	/// <summary>
	/// 取得暫存目錄
	/// </summary>
	string GetTempDirectory();
}

