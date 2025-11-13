using CommunityToolkit.Mvvm.Messaging.Messages;
using MD_Viewer.Models;

namespace MD_Viewer.Messages;

/// <summary>
/// 檔案選擇訊息
/// </summary>
public class FileSelectedMessage : ValueChangedMessage<FileNode>
{
	/// <summary>
	/// 初始化檔案選擇訊息
	/// </summary>
	/// <param name="file">選中的檔案節點</param>
	public FileSelectedMessage(FileNode file) : base(file)
	{
	}
}

