namespace MD_Viewer.Models;

/// <summary>
/// Markdown 元資料
/// </summary>
public class MarkdownMetadata
{
	public string? Title { get; set; }
	public string? Author { get; set; }
	public DateTime? Date { get; set; }
	public List<string> Tags { get; set; } = new();
	public string? Description { get; set; }
}

