using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.Services.Platform;
using DriveInfo = MD_Viewer.Models.DriveInfo;
using Microsoft.Extensions.Logging;

namespace MD_Viewer.Services;

/// <summary>
/// 檔案系統服務實作
/// </summary>
public class FileSystemService : IFileSystemService
{
	private readonly IPlatformFileSystem _platformFileSystem;
	private readonly ILogger<FileSystemService>? _logger;
	private static readonly string[] MarkdownExtensions =
		{ ".md", ".markdown", ".mdown", ".mkd", ".mkdn", ".mdwn" };

	public FileSystemService(IPlatformFileSystem platformFileSystem, ILogger<FileSystemService>? logger = null)
	{
		_platformFileSystem = platformFileSystem;
		_logger = logger;
	}

	public Task<List<Models.DriveInfo>> GetDrivesAsync()
	{
		return _platformFileSystem.GetDrivesAsync();
	}

	public Task<List<FileNode>> ReadDirectoryAsync(string path, CancellationToken cancellationToken = default)
	{
		var nodes = new List<FileNode>();

		try
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				_logger?.LogWarning("目錄路徑為空");
				return Task.FromResult(nodes);
			}

			if (!Directory.Exists(path))
			{
				_logger?.LogWarning("目錄不存在: {Path}", path);
				return Task.FromResult(nodes);
			}

			var items = Directory.GetFileSystemEntries(path);

			foreach (var item in items)
			{
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					if (Directory.Exists(item))
					{
						nodes.Add(new FileNode
						{
							Type = FileNodeType.Directory,
							Name = Path.GetFileName(item),
							Path = item
						});
					}
					else if (File.Exists(item) && IsMarkdownFile(item))
					{
						nodes.Add(new FileNode
						{
							Type = FileNodeType.File,
							Name = Path.GetFileName(item),
							Path = item
						});
					}
				}
				catch (UnauthorizedAccessException ex)
				{
					_logger?.LogWarning(ex, "無法存取項目: {Item}", item);
					// 繼續處理其他項目，不中斷
				}
				catch (Exception ex)
				{
					_logger?.LogWarning(ex, "讀取項目時發生錯誤: {Item}", item);
					// 繼續處理其他項目，不中斷
				}
			}
		}
		catch (OperationCanceledException)
		{
			_logger?.LogInformation("讀取目錄操作已取消: {Path}", path);
			return Task.FromException<List<FileNode>>(new OperationCanceledException());
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger?.LogError(ex, "無權限存取目錄: {Path}", path);
			return Task.FromException<List<FileNode>>(ex);
		}
		catch (DirectoryNotFoundException ex)
		{
			_logger?.LogError(ex, "目錄不存在: {Path}", path);
			return Task.FromException<List<FileNode>>(ex);
		}
		catch (ArgumentException ex)
		{
			_logger?.LogError(ex, "無效的目錄路徑: {Path}", path);
			return Task.FromException<List<FileNode>>(ex);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "讀取目錄時發生錯誤: {Path}", path);
			return Task.FromException<List<FileNode>>(ex);
		}

		return Task.FromResult(nodes);
	}

	public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("檔案路徑不能為空", nameof(filePath));
			}

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("檔案不存在", filePath);
			}

			return await File.ReadAllTextAsync(filePath, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			_logger?.LogInformation("讀取檔案操作已取消: {FilePath}", filePath);
			throw;
		}
		catch (FileNotFoundException ex)
		{
			_logger?.LogError(ex, "檔案不存在: {FilePath}", filePath);
			throw;
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger?.LogError(ex, "無權限讀取檔案: {FilePath}", filePath);
			throw;
		}
		catch (DirectoryNotFoundException ex)
		{
			_logger?.LogError(ex, "檔案所在目錄不存在: {FilePath}", filePath);
			throw;
		}
		catch (ArgumentException ex)
		{
			_logger?.LogError(ex, "無效的檔案路徑: {FilePath}", filePath);
			throw;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "讀取檔案時發生錯誤: {FilePath}", filePath);
			throw;
		}
	}

	public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("檔案路徑不能為空", nameof(filePath));
			}

			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			// 確保目錄存在
			var directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			await File.WriteAllTextAsync(filePath, content, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			_logger?.LogInformation("寫入檔案操作已取消: {FilePath}", filePath);
			throw;
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger?.LogError(ex, "無權限寫入檔案: {FilePath}", filePath);
			throw;
		}
		catch (DirectoryNotFoundException ex)
		{
			_logger?.LogError(ex, "檔案所在目錄不存在: {FilePath}", filePath);
			throw;
		}
		catch (ArgumentException ex)
		{
			_logger?.LogError(ex, "無效的檔案路徑: {FilePath}", filePath);
			throw;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "寫入檔案時發生錯誤: {FilePath}", filePath);
			throw;
		}
	}

	public bool IsMarkdownFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return false;
		}

		var extension = Path.GetExtension(filePath).ToLowerInvariant();
		return MarkdownExtensions.Contains(extension);
	}

	public async Task<string?> RequestFileAccessAsync()
	{
		return await _platformFileSystem.RequestFileAccessAsync();
	}
}

