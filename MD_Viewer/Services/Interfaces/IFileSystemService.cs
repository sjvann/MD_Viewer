using MD_Viewer.Models;
using DriveInfo = MD_Viewer.Models.DriveInfo;

namespace MD_Viewer.Services.Interfaces;

/// <summary>
/// 檔案系統服務介面，提供跨平台檔案操作
/// </summary>
public interface IFileSystemService
{
	/// <summary>
	/// 取得可用的磁碟/根目錄列表
	/// </summary>
	Task<List<DriveInfo>> GetDrivesAsync();

	/// <summary>
	/// 讀取目錄結構（遞迴）
	/// </summary>
	/// <param name="path">目錄路徑</param>
	/// <param name="cancellationToken">取消令牌</param>
	Task<List<FileNode>> ReadDirectoryAsync(string path, CancellationToken cancellationToken = default);

	/// <summary>
	/// 讀取檔案內容
	/// </summary>
	Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// 寫入檔案內容
	/// </summary>
	Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default);

	/// <summary>
	/// 檢查是否為 Markdown 檔案
	/// </summary>
	bool IsMarkdownFile(string filePath);

	/// <summary>
	/// 請求檔案存取權限（平台特定）
	/// </summary>
	Task<string?> RequestFileAccessAsync();
}

