using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MD_Viewer.Messages;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using DriveInfo = MD_Viewer.Models.DriveInfo;

namespace MD_Viewer.ViewModels;

/// <summary>
/// 檔案樹 ViewModel
/// </summary>
public partial class FileTreeViewModel : ObservableObject
{
	private readonly IFileSystemService _fileSystemService;
	private readonly IMessenger _messenger;

	private FileNode? _selectedNode;
	private DriveInfo? _selectedDrive;
	private bool _isLoading;
	private string? _errorMessage;

	public FileTreeViewModel(
		IFileSystemService fileSystemService,
		IMessenger messenger)
	{
		_fileSystemService = fileSystemService;
		_messenger = messenger;
	}

	/// <summary>
	/// 磁碟列表
	/// </summary>
	public ObservableCollection<DriveInfo> Drives { get; } = new();

	/// <summary>
	/// 檔案樹
	/// </summary>
	public ObservableCollection<FileNode> FileTree { get; } = new();

	/// <summary>
	/// 選中的節點
	/// </summary>
	public FileNode? SelectedNode
	{
		get => _selectedNode;
		set => SetProperty(ref _selectedNode, value);
	}

	/// <summary>
	/// 選中的磁碟
	/// </summary>
	public DriveInfo? SelectedDrive
	{
		get => _selectedDrive;
		set
		{
			if (SetProperty(ref _selectedDrive, value) && value != null)
			{
				_ = LoadDirectoryAsync(value.Path);
			}
		}
	}

	/// <summary>
	/// 是否載入中
	/// </summary>
	public bool IsLoading
	{
		get => _isLoading;
		set => SetProperty(ref _isLoading, value);
	}

	/// <summary>
	/// 錯誤訊息
	/// </summary>
	public string? ErrorMessage
	{
		get => _errorMessage;
		set => SetProperty(ref _errorMessage, value);
	}

	/// <summary>
	/// 載入磁碟列表命令
	/// </summary>
	[RelayCommand]
	public async Task LoadDrivesAsync()
	{
		try
		{
			IsLoading = true;
			ErrorMessage = null;

			var drives = await _fileSystemService.GetDrivesAsync();
			Drives.Clear();
			foreach (var drive in drives)
			{
				Drives.Add(drive);
			}
		}
		catch (Exception ex)
		{
			ErrorMessage = $"無法載入磁碟列表: {ex.Message}";
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>
	/// 載入目錄命令
	/// </summary>
	[RelayCommand]
	public async Task LoadDirectoryAsync(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return;

		try
		{
			IsLoading = true;
			ErrorMessage = null;

			var nodes = await _fileSystemService.ReadDirectoryAsync(path);
			FileTree.Clear();
			foreach (var node in nodes)
			{
				FileTree.Add(node);
			}
		}
		catch (Exception ex)
		{
			ErrorMessage = $"無法載入目錄: {ex.Message}";
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>
	/// 展開節點命令
	/// </summary>
	[RelayCommand]
	public async Task ExpandNodeAsync(FileNode? node)
	{
		if (node == null || node.Type != FileNodeType.Directory)
			return;

		if (node.IsExpanded)
		{
			// 收合
			node.IsExpanded = false;
			return;
		}

		// 展開：載入子節點（懶載入）
		if (node.Children.Count == 0)
		{
			try
			{
				IsLoading = true;
				var children = await _fileSystemService.ReadDirectoryAsync(node.Path);
				foreach (var child in children)
				{
					node.Children.Add(child);
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = $"無法載入目錄內容: {ex.Message}";
			}
			finally
			{
				IsLoading = false;
			}
		}

		node.IsExpanded = true;
	}

	/// <summary>
	/// 選擇節點命令
	/// </summary>
	[RelayCommand]
	public void SelectNode(FileNode? node)
	{
		SelectedNode = node;

		if (node?.Type == FileNodeType.File)
		{
			_messenger.Send(new FileSelectedMessage(node));
		}
	}
}

