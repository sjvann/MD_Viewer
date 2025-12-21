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
	/// 注意：此方法會保護程式碼區塊內容，不進行任何格式化
	/// </summary>
	public string FormatMarkdown(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return markdown;

		try
		{
			// 步驟 1：提取並保護程式碼區塊
			var (processedMarkdown, codeBlocks) = ExtractCodeBlocks(markdown);

			// 步驟 2：只格式化標準 Markdown 表格
			processedMarkdown = FormatTables(processedMarkdown);

			// 步驟 3：移除多餘的空行（超過2行的空行縮減為2行）
			processedMarkdown = RemoveExcessiveBlankLines(processedMarkdown);

			// 步驟 4：還原程式碼區塊
			processedMarkdown = RestoreCodeBlocks(processedMarkdown, codeBlocks);

			// 步驟 5：確保檔案結尾有換行
			if (!processedMarkdown.EndsWith('\n'))
				processedMarkdown += "\n";

			return processedMarkdown;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "格式化 Markdown 時發生錯誤");
			return markdown;
		}
	}

	/// <summary>
	/// 提取程式碼區塊，用佔位符替換
	/// </summary>
	private (string processed, List<string> codeBlocks) ExtractCodeBlocks(string markdown)
	{
		var codeBlocks = new List<string>();
		var result = new StringBuilder();
		var lines = markdown.Split('\n');
		var codeBlockContent = new StringBuilder();
		bool inCodeBlock = false;
		string codeBlockStart = "";

		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			var trimmed = line.TrimStart();

			// 檢查是否為程式碼區塊開始/結束
			if (trimmed.StartsWith("```"))
			{
				if (!inCodeBlock)
				{
					// 開始程式碼區塊
					inCodeBlock = true;
					codeBlockStart = line;
					codeBlockContent.Clear();
					codeBlockContent.AppendLine(line);
				}
				else
				{
					// 結束程式碼區塊
					codeBlockContent.AppendLine(line);
					inCodeBlock = false;
					
					// 保存程式碼區塊並用佔位符替換
					var blockIndex = codeBlocks.Count;
					codeBlocks.Add(codeBlockContent.ToString().TrimEnd('\r', '\n'));
					result.AppendLine($"{{{{CODE_BLOCK_{blockIndex}}}}}");
				}
			}
			else if (inCodeBlock)
			{
				// 在程式碼區塊內，原樣保留
				codeBlockContent.AppendLine(line);
			}
			else
			{
				// 不在程式碼區塊內
				result.AppendLine(line);
			}
		}

		// 處理未結束的程式碼區塊
		if (inCodeBlock)
		{
			var blockIndex = codeBlocks.Count;
			codeBlocks.Add(codeBlockContent.ToString().TrimEnd('\r', '\n'));
			result.AppendLine($"{{{{CODE_BLOCK_{blockIndex}}}}}");
		}

		return (result.ToString().TrimEnd('\r', '\n'), codeBlocks);
	}

	/// <summary>
	/// 還原程式碼區塊
	/// </summary>
	private string RestoreCodeBlocks(string markdown, List<string> codeBlocks)
	{
		var result = markdown;
		for (int i = 0; i < codeBlocks.Count; i++)
		{
			result = result.Replace($"{{{{CODE_BLOCK_{i}}}}}", codeBlocks[i]);
		}
		return result;
	}

	/// <summary>
	/// 格式化 Markdown 表格（對齊欄位）
	/// 只處理有效的 Markdown 表格（必須有標題行和分隔行）
	/// </summary>
	public string FormatTables(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return markdown;

		try
		{
			var lines = markdown.Split('\n');
			var result = new StringBuilder();
			var tableLines = new List<string>();
			bool potentialTable = false;

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var trimmedLine = line.Trim();

				// 檢查是否為有效的表格行
				// 有效的表格行：以 | 開頭或結尾，或中間包含 |
				bool isTableLine = IsValidTableLine(trimmedLine);

				if (isTableLine)
				{
					potentialTable = true;
					tableLines.Add(line);
				}
				else
				{
					// 不是表格行，處理之前收集的表格行
					if (potentialTable && tableLines.Count > 0)
					{
						// 檢查是否為有效的表格（至少2行，且第2行是分隔行）
						if (IsValidTable(tableLines))
						{
							var formattedTable = FormatTableBlock(tableLines);
							result.Append(formattedTable);
						}
						else
						{
							// 不是有效表格，原樣輸出
							foreach (var tableLine in tableLines)
							{
								result.AppendLine(tableLine);
							}
						}
						tableLines.Clear();
						potentialTable = false;
					}
					result.AppendLine(line);
				}
			}

			// 處理最後的表格
			if (tableLines.Count > 0)
			{
				if (IsValidTable(tableLines))
				{
					var formattedTable = FormatTableBlock(tableLines);
					result.Append(formattedTable);
				}
				else
				{
					foreach (var tableLine in tableLines)
					{
						result.AppendLine(tableLine);
					}
				}
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
	/// 檢查是否為有效的表格行
	/// </summary>
	private bool IsValidTableLine(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return false;

		// 有效的表格行必須：
		// 1. 以 | 開頭，或
		// 2. 以 | 結尾，或
		// 3. 中間包含 | 且兩側都有內容
		
		// 排除 ASCII 藝術中的 | 符號（通常伴隨著 ─ ├ └ ┌ ┐ 等字元）
		if (ContainsBoxDrawingChars(line))
			return false;

		// 必須以 | 開頭和結尾才算是有效的表格行
		return line.StartsWith('|') && line.EndsWith('|') && line.Count(c => c == '|') >= 2;
	}

	/// <summary>
	/// 檢查是否包含 Box Drawing 字元（ASCII 藝術）
	/// </summary>
	private bool ContainsBoxDrawingChars(string line)
	{
		// Box Drawing 字元範圍：U+2500 到 U+257F
		foreach (char c in line)
		{
			if (c >= '\u2500' && c <= '\u257F')
				return true;
			// 也檢查一些常用的樹狀結構字元
			if (c == '├' || c == '└' || c == '┌' || c == '┐' || c == '┘' || c == '│' || c == '─' || c == '┬' || c == '┴' || c == '┼')
				return true;
		}
		return false;
	}

	/// <summary>
	/// 檢查是否為有效的 Markdown 表格
	/// </summary>
	private bool IsValidTable(List<string> lines)
	{
		if (lines.Count < 2)
			return false;

		// 第二行必須是分隔行（只包含 |, -, :, 空格）
		var secondLine = lines[1].Trim();
		if (!IsSeparatorLine(secondLine))
			return false;

		// 所有行必須有相同數量的 | 分隔符
		var firstLineCount = lines[0].Count(c => c == '|');
		foreach (var line in lines)
		{
			if (line.Count(c => c == '|') != firstLineCount)
				return false;
		}

		return true;
	}

	/// <summary>
	/// 檢查是否為分隔行
	/// </summary>
	private bool IsSeparatorLine(string line)
	{
		// 分隔行格式：| --- | --- | 或 |:---:|:---:|
		var trimmed = line.Trim();
		if (!trimmed.StartsWith('|') || !trimmed.EndsWith('|'))
			return false;

		// 移除首尾的 |
		trimmed = trimmed.Substring(1, trimmed.Length - 2);

		// 分割成各欄
		var cells = trimmed.Split('|');
		if (cells.Length == 0)
			return false;

		// 每個儲存格必須只包含 -, :, 空格
		foreach (var cell in cells)
		{
			var cellTrimmed = cell.Trim();
			if (string.IsNullOrEmpty(cellTrimmed))
				continue;
			
			// 移除首尾的 : 後，必須只剩下 -
			var withoutColons = cellTrimmed.Trim(':');
			if (string.IsNullOrEmpty(withoutColons))
				return false;
			
			if (!withoutColons.All(c => c == '-'))
				return false;
		}

		return true;
	}

	/// <summary>
	/// 格式化單個表格區塊
	/// </summary>
	private string FormatTableBlock(List<string> tableLines)
	{
		if (tableLines.Count < 2)
		{
			var sb = new StringBuilder();
			foreach (var line in tableLines)
				sb.AppendLine(line);
			return sb.ToString();
		}

		// 解析表格
		var rows = new List<List<string>>();
		int separatorIndex = 1; // 標準 Markdown 表格的分隔行固定在第2行

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
		}

		// 計算每欄的最大寬度
		int columnCount = rows.Max(r => r.Count);
		var columnWidths = new int[columnCount];
		var columnAlignments = new TextAlignment[columnCount];

		// 解析對齊方式（從分隔行）
		if (separatorIndex < rows.Count)
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
				continue;

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
					var separator = new string('-', width);
					switch (columnAlignments[col])
					{
						case TextAlignment.Center:
							result.Append($":{separator}:");
							break;
						case TextAlignment.Right:
							result.Append($" {separator}:");
							break;
						default:
							result.Append($" {separator} ");
							break;
					}
				}
				else
				{
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
			if (c >= 0x4E00 && c <= 0x9FFF ||
				c >= 0x3400 && c <= 0x4DBF ||
				c >= 0xFF00 && c <= 0xFFEF)
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
	/// 移除多餘的空行
	/// </summary>
	private string RemoveExcessiveBlankLines(string markdown)
	{
		return ExcessiveNewlinesRegex().Replace(markdown, "\n\n");
	}

	[GeneratedRegex(@"\n{3,}")]
	private static partial Regex ExcessiveNewlinesRegex();

	private enum TextAlignment
	{
		Left,
		Center,
		Right
	}

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

