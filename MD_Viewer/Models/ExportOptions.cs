namespace MD_Viewer.Models;

/// <summary>
/// 匯出選項
/// </summary>
public class ExportOptions
{
	/// <summary>
	/// 是否包含 CSS 樣式
	/// </summary>
	public bool IncludeStyles { get; set; } = true;

	/// <summary>
	/// 自訂 CSS 樣式路徑
	/// </summary>
	public string? CustomCssPath { get; set; }

	/// <summary>
	/// PDF 頁面大小
	/// </summary>
	public PdfPageSize PageSize { get; set; } = PdfPageSize.A4;

	/// <summary>
	/// PDF 頁面方向
	/// </summary>
	public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

	/// <summary>
	/// 是否包含頁碼
	/// </summary>
	public bool IncludePageNumbers { get; set; } = true;

	/// <summary>
	/// 標題
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// 作者
	/// </summary>
	public string? Author { get; set; }
}

/// <summary>
/// PDF 頁面大小
/// </summary>
public enum PdfPageSize
{
	A4,
	Letter,
	Legal
}

/// <summary>
/// PDF 頁面方向
/// </summary>
public enum PdfPageOrientation
{
	Portrait,
	Landscape
}

