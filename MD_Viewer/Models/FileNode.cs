using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MD_Viewer.Models;

/// <summary>
/// 檔案節點模型
/// </summary>
public class FileNode : ObservableObject
{
	private bool _isExpanded;
	private bool _isSelected;

	/// <summary>
	/// 節點類型
	/// </summary>
	public FileNodeType Type { get; set; }

	/// <summary>
	/// 名稱
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// 完整路徑
	/// </summary>
	public string Path { get; set; } = string.Empty;

	/// <summary>
	/// 子節點（僅目錄有）
	/// </summary>
	public ObservableCollection<FileNode> Children { get; set; } = new();

	/// <summary>
	/// 是否展開（僅目錄）
	/// </summary>
	public bool IsExpanded
	{
		get => _isExpanded;
		set => SetProperty(ref _isExpanded, value);
	}

	/// <summary>
	/// 是否選中
	/// </summary>
	public bool IsSelected
	{
		get => _isSelected;
		set => SetProperty(ref _isSelected, value);
	}

	/// <summary>
	/// 圖示（根據類型）
	/// </summary>
	public string Icon => Type == FileNodeType.Directory ? "folder.png" : "file.png";

	/// <summary>
	/// 是否有子節點
	/// </summary>
	public bool HasChildren => Children?.Count > 0;
}

/// <summary>
/// 檔案節點類型
/// </summary>
public enum FileNodeType
{
	Directory,
	File
}

