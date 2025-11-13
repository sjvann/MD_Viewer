namespace MD_Viewer.Services.Platform;

/// <summary>
/// 平台檔案選擇器介面
/// </summary>
public interface IPlatformFilePicker
{
	/// <summary>
	/// 顯示檔案儲存對話框
	/// </summary>
	/// <param name="defaultFileName">預設檔名</param>
	/// <param name="fileTypeChoices">檔案類型選擇（例如：{ "HTML", new[] { ".html" } }）</param>
	/// <returns>選擇的檔案路徑，如果使用者取消則返回 null</returns>
	Task<string?> PickSaveFileAsync(string? defaultFileName = null, Dictionary<string, string[]>? fileTypeChoices = null);
}

