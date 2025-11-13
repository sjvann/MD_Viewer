using MD_Viewer.Models;

namespace MD_Viewer.Services.Interfaces;

/// <summary>
/// Markdown 處理服務介面
/// </summary>
public interface IMarkdownService
{
	/// <summary>
	/// 將 Markdown 轉換為 HTML
	/// </summary>
	string RenderToHtml(string markdown);

	/// <summary>
	/// 驗證 Markdown 格式
	/// </summary>
	bool ValidateMarkdown(string markdown, out string? errorMessage);

	/// <summary>
	/// 取得 Markdown 的元資料（標題、作者等）
	/// </summary>
	MarkdownMetadata ExtractMetadata(string markdown);
}

