namespace MD_Viewer.Models;

/// <summary>
/// 匯出格式
/// </summary>
public class ExportFormat
{
	public string Name { get; set; } = string.Empty;
	public string Extension { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool IsEnabled { get; set; } = true;
}

