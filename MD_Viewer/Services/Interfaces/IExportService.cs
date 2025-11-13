using MD_Viewer.Models;

namespace MD_Viewer.Services.Interfaces;

/// <summary>
/// 匯出服務介面
/// </summary>
public interface IExportService
{
	/// <summary>
	/// 匯出為 HTML
	/// </summary>
	Task ExportToHtmlAsync(string markdown, string outputPath, ExportOptions? options = null);

	/// <summary>
	/// 匯出為 PDF
	/// </summary>
	Task ExportToPdfAsync(string markdown, string outputPath, ExportOptions? options = null);

	/// <summary>
	/// 匯出為 DOCX
	/// </summary>
	Task ExportToDocxAsync(string markdown, string outputPath, ExportOptions? options = null);

	/// <summary>
	/// 匯出為 ODF
	/// </summary>
	Task ExportToOdfAsync(string markdown, string outputPath, ExportOptions? options = null);

	/// <summary>
	/// 取得支援的匯出格式
	/// </summary>
	List<ExportFormat> GetSupportedFormats();
}

