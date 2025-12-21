using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Microsoft.Extensions.Logging;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;

namespace MD_Viewer.Services;

/// <summary>
/// Markdown 服務實作
/// </summary>
public partial class MarkdownService : IMarkdownService
{
	private readonly MarkdownPipeline _pipeline;
	private readonly ILogger<MarkdownService> _logger;

	public MarkdownService(ILogger<MarkdownService> logger)
	{
		_logger = logger;
		_pipeline = new MarkdownPipelineBuilder()
			.UseAdvancedExtensions()
			.UseMathematics()
			.Build();
	}

	/// <summary>
	/// 將 Markdown 轉換為 HTML
	/// </summary>
	public string RenderToHtml(string markdown)
	{
		try
		{
			if (string.IsNullOrEmpty(markdown))
				return string.Empty;

			return Markdig.Markdown.ToHtml(markdown, _pipeline);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "轉換 Markdown 為 HTML 時發生錯誤");
			return string.Empty;
		}
	}

	/// <summary>
	/// 格式化 Markdown 內容（優化排版）
	/// </summary>
	public string FormatMarkdown(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return markdown;

		try
		{
			var result = markdown;

			// 1. 格式化表格
			result = FormatTables(result);

			// 2. 確保標題前後有空行
			result = FormatHeadings(result);

			// 3. 確保程式碼區塊前後有空行
			result = FormatCodeBlocks(result);

			// 4. 移除多餘的空行（超過2行的空行縮減為2行）
			result = RemoveExcessiveBlankLines(result);

			// 5. 確保檔案結尾有換行
			if (!result.EndsWith('\n'))
				result += "\n";

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "格式化 Markdown 時發生錯誤");
			return markdown;
		}
	}

	/// <summary>
	/// 格式化 Markdown 表格（對齊欄位）
	/// </summary>
	public string FormatTables(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return markdown;

		try
		{
			// 使用正規表達式找出表格區塊
			var lines = markdown.Split('\n');
			var result = new StringBuilder();
			var tableLines = new List<string>();
			var inTable = false;

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var trimmedLine = line.Trim();

				// 檢查是否為表格行（包含 | 符號）
				if (trimmedLine.Contains('|'))
				{
					inTable = true;
					tableLines.Add(line);
				}
				else
				{
					// 如果之前在表格中，現在離開表格
					if (inTable && tableLines.Count > 0)
					{
						var formattedTable = FormatTableBlock(tableLines);
						result.Append(formattedTable);
						tableLines.Clear();
						inTable = false;
					}
					result.AppendLine(line);
				}
			}

			// 處理最後一個表格（如果有）
			if (tableLines.Count > 0)
			{
				var formattedTable = FormatTableBlock(tableLines);
				result.Append(formattedTable);
			}

			return result.ToString().TrimEnd('\r', '\n') + "\n";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "格式化表格時發生錯誤");
			return markdown;
		}
	}

	/// <summary>
	/// 格式化單個表格區塊
	/// </summary>
	private string FormatTableBlock(List<string> tableLines)
	{
		if (tableLines.Count < 2)
		{
			// 不是有效的表格，直接返回
			var sb = new StringBuilder();
			foreach (var line in tableLines)
				sb.AppendLine(line);
			return sb.ToString();
		}

		// 解析表格
		var rows = new List<List<string>>();
		int separatorIndex = -1;

		for (int i = 0; i < tableLines.Count; i++)
		{
			var line = tableLines[i].Trim();
			
			// 移除首尾的 | 符號
			if (line.StartsWith('|'))
				line = line.Substring(1);
			if (line.EndsWith('|'))
				line = line.Substring(0, line.Length - 1);

			var cells = line.Split('|').Select(c => c.Trim()).ToList();
			rows.Add(cells);

			// 檢查是否為分隔行（包含 --- 或 :---: 等）
			if (i > 0 && separatorIndex < 0 && cells.All(c => IsSeparatorCell(c)))
			{
				separatorIndex = i;
			}
		}

		if (separatorIndex < 0 || rows.Count < 2)
		{
			// 不是有效的表格
			var sb = new StringBuilder();
			foreach (var line in tableLines)
				sb.AppendLine(line);
			return sb.ToString();
		}

		// 計算每欄的最大寬度
		int columnCount = rows.Max(r => r.Count);
		var columnWidths = new int[columnCount];
		var columnAlignments = new TextAlignment[columnCount];

		// 解析對齊方式
		if (separatorIndex > 0 && separatorIndex < rows.Count)
		{
			var separatorRow = rows[separatorIndex];
			for (int col = 0; col < columnCount; col++)
			{
				if (col < separatorRow.Count)
				{
					columnAlignments[col] = GetAlignment(separatorRow[col]);
				}
			}
		}

		// 計算最大寬度
		for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
		{
			if (rowIndex == separatorIndex)
				continue; // 跳過分隔行

			var row = rows[rowIndex];
			for (int col = 0; col < row.Count && col < columnCount; col++)
			{
				var cellWidth = GetDisplayWidth(row[col]);
				columnWidths[col] = Math.Max(columnWidths[col], Math.Max(cellWidth, 3));
			}
		}

		// 建立格式化的表格
		var result = new StringBuilder();
		for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
		{
			var row = rows[rowIndex];
			result.Append('|');

			for (int col = 0; col < columnCount; col++)
			{
				var cell = col < row.Count ? row[col] : "";
				var width = columnWidths[col];

				if (rowIndex == separatorIndex)
				{
					// 分隔行
					var separator = new string('-', width);
					switch (columnAlignments[col])
					{
						case TextAlignment.Center:
							result.Append($":{separator}:");
							break;
						case TextAlignment.Right:
							result.Append($"{separator}:");
							break;
						default:
							result.Append($" {separator} ");
							break;
					}
				}
				else
				{
					// 資料行
					var paddedCell = PadCell(cell, width, columnAlignments[col]);
					result.Append($" {paddedCell} ");
				}
				result.Append('|');
			}
			result.AppendLine();
		}

		return result.ToString();
	}

	/// <summary>
	/// 檢查是否為分隔符儲存格
	/// </summary>
	private bool IsSeparatorCell(string cell)
	{
		if (string.IsNullOrWhiteSpace(cell))
			return false;

		var trimmed = cell.Trim().Trim(':');
		return trimmed.Length > 0 && trimmed.All(c => c == '-');
	}

	/// <summary>
	/// 取得對齊方式
	/// </summary>
	private TextAlignment GetAlignment(string separatorCell)
	{
		var trimmed = separatorCell.Trim();
		bool leftColon = trimmed.StartsWith(':');
		bool rightColon = trimmed.EndsWith(':');

		if (leftColon && rightColon)
			return TextAlignment.Center;
		if (rightColon)
			return TextAlignment.Right;
		return TextAlignment.Left;
	}

	/// <summary>
	/// 取得字串的顯示寬度（考慮中文字元）
	/// </summary>
	private int GetDisplayWidth(string text)
	{
		if (string.IsNullOrEmpty(text))
			return 0;

		int width = 0;
		foreach (char c in text)
		{
			// 中文字元佔用2個寬度
			if (c >= 0x4E00 && c <= 0x9FFF ||  // CJK Unified Ideographs
				c >= 0x3400 && c <= 0x4DBF ||  // CJK Extension A
				c >= 0xFF00 && c <= 0xFFEF)    // Fullwidth Forms
			{
				width += 2;
			}
			else
			{
				width += 1;
			}
		}
		return width;
	}

	/// <summary>
	/// 填充儲存格內容
	/// </summary>
	private string PadCell(string cell, int width, TextAlignment alignment)
	{
		var displayWidth = GetDisplayWidth(cell);
		var padding = width - displayWidth;

		if (padding <= 0)
			return cell;

		return alignment switch
		{
			TextAlignment.Center => new string(' ', padding / 2) + cell + new string(' ', padding - padding / 2),
			TextAlignment.Right => new string(' ', padding) + cell,
			_ => cell + new string(' ', padding)
		};
	}

	/// <summary>
	/// 格式化標題（確保前後有空行）
	/// </summary>
	private string FormatHeadings(string markdown)
	{
		var lines = markdown.Split('\n').ToList();
		var result = new List<string>();

		for (int i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			var trimmed = line.TrimStart();

			// 檢查是否為標題行
			if (HeadingRegex().IsMatch(trimmed))
			{
				// 確保標題前有空行（除非是第一行）
				if (i > 0 && result.Count > 0 && !string.IsNullOrWhiteSpace(result[^1]))
				{
					result.Add("");
				}
				result.Add(line);
				// 確保標題後有空行（除非下一行也是空行）
				if (i + 1 < lines.Count && !string.IsNullOrWhiteSpace(lines[i + 1]))
				{
					result.Add("");
				}
			}
			else
			{
				result.Add(line);
			}
		}

		return string.Join("\n", result);
	}

	/// <summary>
	/// 格式化程式碼區塊（確保前後有空行）
	/// </summary>
	private string FormatCodeBlocks(string markdown)
	{
		var lines = markdown.Split('\n').ToList();
		var result = new List<string>();
		bool inCodeBlock = false;

		for (int i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			var trimmed = line.TrimStart();

			if (trimmed.StartsWith("```"))
			{
				if (!inCodeBlock)
				{
					// 進入程式碼區塊，確保前面有空行
					if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[^1]))
					{
						result.Add("");
					}
					inCodeBlock = true;
				}
				else
				{
					// 離開程式碼區塊
					inCodeBlock = false;
					result.Add(line);
					// 確保後面有空行
					if (i + 1 < lines.Count && !string.IsNullOrWhiteSpace(lines[i + 1]))
					{
						result.Add("");
					}
					continue;
				}
			}

			result.Add(line);
		}

		return string.Join("\n", result);
	}

	/// <summary>
	/// 移除多餘的空行
	/// </summary>
	private string RemoveExcessiveBlankLines(string markdown)
	{
		// 將連續3個以上的換行縮減為2個換行
		return ExcessiveNewlinesRegex().Replace(markdown, "\n\n");
	}

	[GeneratedRegex(@"^#{1,6}\s")]
	private static partial Regex HeadingRegex();

	[GeneratedRegex(@"\n{3,}")]
	private static partial Regex ExcessiveNewlinesRegex();

	private enum TextAlignment
	{
		Left,
		Center,
		Right
	}

	// ... 以下保留原有的方法 ...

	/// <summary>
	/// 驗證 Markdown 格式
	/// </summary>
	public bool ValidateMarkdown(string markdown, out string? errorMessage)
	{
		errorMessage = null;

		try
		{
			if (string.IsNullOrWhiteSpace(markdown))
			{
				errorMessage = "Markdown 內容不能為空";
				return false;
			}

			var openBrackets = markdown.Count(c => c == '[');
			var closeBrackets = markdown.Count(c => c == ']');
			if (openBrackets != closeBrackets)
			{
				errorMessage = "Markdown 格式錯誤：未配對的方括號";
				return false;
			}

			var codeBlockCount = Regex.Matches(markdown, @"```").Count;
			if (codeBlockCount % 2 != 0)
			{
				errorMessage = "Markdown 格式錯誤：未配對的程式碼區塊標記";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "驗證 Markdown 格式時發生錯誤");
			errorMessage = $"驗證時發生錯誤: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// 取得 Markdown 的元資料（標題、作者等）
	/// </summary>
	public MarkdownMetadata ExtractMetadata(string markdown)
	{
		var metadata = new MarkdownMetadata();

		try
		{
			if (string.IsNullOrWhiteSpace(markdown))
				return metadata;

			if (markdown.StartsWith("---", StringComparison.Ordinal))
			{
				var endIndex = markdown.IndexOf("---", 3, StringComparison.Ordinal);
				if (endIndex > 0)
				{
					var frontMatter = markdown.Substring(3, endIndex - 3).Trim();
					ParseYamlFrontMatter(frontMatter, metadata);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "提取 Markdown 元資料時發生錯誤");
		}

		return metadata;
	}

	private void ParseYamlFrontMatter(string yaml, MarkdownMetadata metadata)
	{
		if (string.IsNullOrWhiteSpace(yaml))
			return;

		var lines = yaml.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		foreach (var line in lines)
		{
			var trimmedLine = line.Trim();
			if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
				continue;

			var colonIndex = trimmedLine.IndexOf(':');
			if (colonIndex <= 0)
				continue;

			var key = trimmedLine.Substring(0, colonIndex).Trim().ToLowerInvariant();
			var value = trimmedLine.Substring(colonIndex + 1).Trim();

			if (value.StartsWith('"') && value.EndsWith('"'))
				value = value.Substring(1, value.Length - 2);
			else if (value.StartsWith('\'') && value.EndsWith('\''))
				value = value.Substring(1, value.Length - 2);

			switch (key)
			{
				case "title":
					metadata.Title = value;
					break;
				case "author":
					metadata.Author = value;
					break;
				case "date":
					if (DateTime.TryParse(value, out var date))
						metadata.Date = date;
					break;
				case "description":
					metadata.Description = value;
					break;
				case "tags":
					if (value.StartsWith('[') && value.EndsWith(']'))
					{
						var tags = value.Substring(1, value.Length - 2)
							.Split(',', StringSplitOptions.RemoveEmptyEntries)
							.Select(t => t.Trim().Trim('"', '\''))
							.Where(t => !string.IsNullOrWhiteSpace(t));
						metadata.Tags.AddRange(tags);
					}
					break;
			}
		}

		var tagLines = lines.Where(l => l.Trim().StartsWith("- ", StringComparison.Ordinal));
		foreach (var tagLine in tagLines)
		{
			var tag = tagLine.Trim().Substring(2).Trim().Trim('"', '\'');
			if (!string.IsNullOrWhiteSpace(tag) && !metadata.Tags.Contains(tag))
				metadata.Tags.Add(tag);
		}
	}
}

