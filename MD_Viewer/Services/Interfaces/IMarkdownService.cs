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

	/// <summary>
	/// 格式化 Markdown 內容（優化排版）
	/// </summary>
	/// <param name="markdown">原始 Markdown 內容</param>
	/// <returns>格式化後的 Markdown 內容</returns>
	string FormatMarkdown(string markdown);

	/// <summary>
	/// 格式化 Markdown 表格（對齊欄位）
	/// </summary>
	/// <param name="markdown">包含表格的 Markdown 內容</param>
	/// <returns>表格對齊後的 Markdown 內容</returns>
	string FormatTables(string markdown);
}

