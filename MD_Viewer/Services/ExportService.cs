using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfColors = QuestPDF.Helpers.Colors;

namespace MD_Viewer.Services;

/// <summary>
/// 匯出服務實作
/// </summary>
public class ExportService : IExportService
{
	private readonly IMarkdownService _markdownService;
	private readonly IFileSystemService _fileSystemService;
	private readonly ILogger<ExportService> _logger;

	public ExportService(
		IMarkdownService markdownService,
		IFileSystemService fileSystemService,
		ILogger<ExportService> logger)
	{
		_markdownService = markdownService;
		_fileSystemService = fileSystemService;
		_logger = logger;
	}

	/// <summary>
	/// 匯出為 HTML
	/// </summary>
	public async Task ExportToHtmlAsync(string markdown, string outputPath, ExportOptions? options = null)
	{
		try
		{
			if (string.IsNullOrEmpty(markdown))
			{
				throw new ArgumentException("Markdown 內容不能為空", nameof(markdown));
			}

			if (string.IsNullOrEmpty(outputPath))
			{
				throw new ArgumentException("輸出路徑不能為空", nameof(outputPath));
			}

			// 使用 MarkdownService 將 Markdown 轉換為 HTML
			var htmlContent = _markdownService.RenderToHtml(markdown);

			// 包裝為完整的 HTML 文件
			var fullHtml = WrapHtmlContent(htmlContent, options);

			// 寫入檔案
			await _fileSystemService.WriteFileAsync(outputPath, fullHtml);

			_logger.LogInformation("HTML 匯出成功: {OutputPath}", outputPath);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "匯出 HTML 時發生錯誤: {OutputPath}", outputPath);
			throw;
		}
	}

	/// <summary>
	/// 包裝 HTML 內容為完整的 HTML 文件
	/// </summary>
	private string WrapHtmlContent(string content, ExportOptions? options)
	{
		if (string.IsNullOrEmpty(content))
			return string.Empty;

		var includeStyles = options?.IncludeStyles ?? true;
		var customCssPath = options?.CustomCssPath;

		// 讀取自訂 CSS（如果提供）
		string? customCss = null;
		if (!string.IsNullOrEmpty(customCssPath) && File.Exists(customCssPath))
		{
			try
			{
				customCss = File.ReadAllText(customCssPath);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "無法讀取自訂 CSS 檔案: {CssPath}", customCssPath);
			}
		}

		// 基本 CSS 樣式（參考 PreviewViewModel）
		var defaultCss = includeStyles ? GetDefaultCss() : string.Empty;
		var cssContent = string.IsNullOrEmpty(customCss) ? defaultCss : $"{defaultCss}\n{customCss}";

		// 包裝為完整的 HTML 文件
		return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    {(includeStyles ? $@"<style>
{cssContent}
</style>" : "")}
</head>
<body>
    {content}
</body>
</html>";
	}

	/// <summary>
	/// 取得預設 CSS 樣式
	/// </summary>
	private string GetDefaultCss()
	{
		return @"        /* 基本 Markdown 樣式 */
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Microsoft YaHei', sans-serif;
            padding: 20px;
            line-height: 1.6;
            color: #333;
            max-width: 100%;
            word-wrap: break-word;
        }
        h1, h2, h3, h4, h5, h6 {
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
        }
        h1 { font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }
        h2 { font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }
        h3 { font-size: 1.25em; }
        h4 { font-size: 1em; }
        h5 { font-size: 0.875em; }
        h6 { font-size: 0.85em; color: #6a737d; }
        p {
            margin-top: 0;
            margin-bottom: 16px;
        }
        ul, ol {
            margin-top: 0;
            margin-bottom: 16px;
            padding-left: 2em;
        }
        li {
            margin-top: 0.25em;
        }
        blockquote {
            padding: 0 1em;
            color: #6a737d;
            border-left: 0.25em solid #dfe2e5;
            margin: 0;
        }
        code {
            background-color: rgba(27, 31, 35, 0.05);
            padding: 0.2em 0.4em;
            border-radius: 3px;
            font-size: 85%;
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        }
        pre {
            background-color: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            line-height: 1.45;
        }
        pre code {
            background-color: transparent;
            padding: 0;
            font-size: 100%;
        }
        table {
            border-collapse: collapse;
            margin-top: 0;
            margin-bottom: 16px;
            width: 100%;
        }
        table th, table td {
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
        }
        table th {
            background-color: #f6f8fa;
            font-weight: 600;
        }
        table tr:nth-child(2n) {
            background-color: #f6f8fa;
        }
        img {
            max-width: 100%;
            height: auto;
        }
        a {
            color: #0366d6;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        hr {
            height: 0.25em;
            padding: 0;
            margin: 24px 0;
            background-color: #e1e4e8;
            border: 0;
        }
        .math {
            font-family: 'Times New Roman', serif;
            font-style: italic;
        }";
	}

	public async Task ExportToPdfAsync(string markdown, string outputPath, ExportOptions? options = null)
	{
		try
		{
			if (string.IsNullOrEmpty(markdown))
			{
				throw new ArgumentException("Markdown 內容不能為空", nameof(markdown));
			}

			if (string.IsNullOrEmpty(outputPath))
			{
				throw new ArgumentException("輸出路徑不能為空", nameof(outputPath));
			}

			// 基於 Markdown 內容，建立簡易的 PDF 版面（MVP：支援標題、段落、清單、程式碼區塊）
			var pdfBytes = GeneratePdfFromMarkdown(markdown, options);

			// 直接寫入檔案
			await File.WriteAllBytesAsync(outputPath, pdfBytes);

			_logger.LogInformation("PDF 匯出成功: {OutputPath}", outputPath);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "匯出 PDF 時發生錯誤: {OutputPath}", outputPath);
			throw;
		}
	}

	/// <summary>
	/// 使用 QuestPDF 產生 PDF（MVP：基本 Markdown 映射）
	/// </summary>
	private byte[] GeneratePdfFromMarkdown(string markdown, ExportOptions? options)
	{
		// QuestPDF
		QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

		// 解析為行（簡易方案）：可改為 Markdig AST 以提高完整性
		var lines = markdown.Replace("\r\n", "\n").Split('\n');

		// 版面設定
		var pageSize = options?.PageSize switch
		{
			PdfPageSize.Letter => PageSizes.Letter,
			PdfPageSize.Legal => PageSizes.Legal,
			_ => PageSizes.A4
		};
		var isLandscape = options?.Orientation == PdfPageOrientation.Landscape;

		using var stream = new MemoryStream();

		Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(pageSize);
				if (isLandscape) page.Size(pageSize.Landscape());
				page.Margin(30);
				page.DefaultTextStyle(ts => ts.FontSize(12));

				// 頁首頁尾
				if (!string.IsNullOrWhiteSpace(options?.Title))
				{
					page.Header().Row(row =>
					{
						row.RelativeItem().Text(options!.Title).SemiBold().FontSize(14);
						if (!string.IsNullOrWhiteSpace(options?.Author))
							row.ConstantItem(200).AlignRight().Text(options!.Author).FontSize(10);
					});
				}

				if (options?.IncludePageNumbers == true)
				{
					page.Footer().AlignCenter().Text(x =>
					{
						x.Span("第 ");
						x.CurrentPageNumber();
						x.Span(" 頁 / 共 ");
						x.TotalPages();
						x.Span(" 頁");
					});
				}

				page.Content().PaddingVertical(10).Column(col =>
				{
					bool inCodeBlock = false;

					foreach (var raw in lines)
					{
						var line = raw ?? string.Empty;

						// 圍欄程式碼塊 ```
						if (line.StartsWith("```"))
						{
							inCodeBlock = !inCodeBlock;
							// 起訖行不輸出內容
							continue;
						}

						if (inCodeBlock)
						{
							col.Item().Background(QuestPdfColors.Grey.Lighten3).Padding(6).Text(line).FontSize(10);
							continue;
						}

						// 引用區塊（> ）
						if (line.StartsWith("> "))
						{
							var quoteText = line.Substring(2);
							col.Item().Background(QuestPdfColors.Grey.Lighten4).Padding(8).BorderLeft(3)
								.BorderColor(QuestPdfColors.Grey.Medium)
								.Text(t => RenderInlineMarkdown(t, quoteText));
							continue;
						}

						// 標題（# 至 ######）
						if (line.StartsWith("###### "))
						{
							col.Item().Text(t => { t.Span(line.Substring(7)).FontSize(12).SemiBold(); });
							continue;
						}
						if (line.StartsWith("##### "))
						{
							col.Item().Text(t => { t.Span(line.Substring(6)).FontSize(13).SemiBold(); });
							continue;
						}
						if (line.StartsWith("#### "))
						{
							col.Item().Text(t => { t.Span(line.Substring(5)).FontSize(14).SemiBold(); });
							continue;
						}
						if (line.StartsWith("### "))
						{
							col.Item().Text(t => { t.Span(line.Substring(4)).FontSize(16).SemiBold(); });
							continue;
						}
						if (line.StartsWith("## "))
						{
							col.Item().Text(t => { t.Span(line.Substring(3)).FontSize(18).SemiBold(); });
							continue;
						}
						if (line.StartsWith("# "))
						{
							col.Item().Text(t => { t.Span(line.Substring(2)).FontSize(22).SemiBold(); });
							continue;
						}

						// 無序清單
						if (line.StartsWith("- ") || line.StartsWith("* "))
						{
							col.Item().Row(row =>
							{
								row.ConstantItem(10).Text("•");
								row.RelativeItem().Text(t => RenderInlineMarkdown(t, line.Substring(2)));
							});
							continue;
						}

						// 有序清單（簡化偵測：數字. 空白）
						var trimmed = line.TrimStart();
						if (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && trimmed.IndexOf('.') == 1 && trimmed[2] == ' ')
						{
							col.Item().Row(row =>
							{
								row.ConstantItem(16).Text(trimmed.Substring(0, 2));
								row.RelativeItem().Text(t => RenderInlineMarkdown(t, trimmed.Substring(3)));
							});
							continue;
						}

						// 表格（簡化：直接原文等寬字體輸出）
						if (line.Contains('|'))
						{
							col.Item().Text(line).FontSize(10);
							continue;
						}

						// 圖片（簡化：以文字方式輸出 alt 和 url） ![alt](url)
						var imgMatch = Regex.Match(line, @"!\[(.*?)\]\((.*?)\)");
						if (imgMatch.Success)
						{
							var alt = imgMatch.Groups[1].Value;
							var url = imgMatch.Groups[2].Value;
							col.Item().Text($"[圖片] {alt} ({url})");
							continue;
						}

						// 空行 → 段落間距
						if (string.IsNullOrWhiteSpace(line))
						{
							col.Item().Text(string.Empty).LineHeight(0.8f);
							continue;
						}

						// 一般段落
						col.Item().Text(t => RenderInlineMarkdown(t, line));
					}
				});
			});
		}).GeneratePdf(stream);

		return stream.ToArray();
	}

	/// <summary>
	/// 簡易處理行內 Markdown（行內程式碼與連結）：`code`、[text](url)
	/// </summary>
	private void RenderInlineMarkdown(TextDescriptor text, string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			text.Span(string.Empty);
			return;
		}

		// 先處理連結，將其切成片段
		var linkPattern = new Regex(@"\[(.+?)\]\((.+?)\)");
		var parts = new List<(string type, string value, string? extra)>();
		int lastIndex = 0;
		foreach (Match m in linkPattern.Matches(content))
		{
			if (m.Index > lastIndex)
			{
				parts.Add(("text", content.Substring(lastIndex, m.Index - lastIndex), null));
			}
			parts.Add(("link", m.Groups[1].Value, m.Groups[2].Value)); // (text, url)
			lastIndex = m.Index + m.Length;
		}
		if (lastIndex < content.Length)
		{
			parts.Add(("text", content.Substring(lastIndex), null));
		}

		// 對每一段再處理行內程式碼
		foreach (var part in parts)
		{
			if (part.type == "link")
			{
				text.Span(part.value);
				if (!string.IsNullOrEmpty(part.extra))
				{
					text.Span($" ({part.extra})").FontSize(10);
				}
				continue;
			}

			// 行內程式碼以 ` 分割
			var segments = part.value.Split('`');
			for (int i = 0; i < segments.Length; i++)
			{
				var seg = segments[i];
				if (i % 2 == 1)
				{
					text.Span(seg).FontSize(10);
				}
				else
				{
					text.Span(seg);
				}
			}
		}
	}

	public Task ExportToDocxAsync(string markdown, string outputPath, ExportOptions? options = null)
	{
		throw new NotImplementedException();
	}

	public Task ExportToOdfAsync(string markdown, string outputPath, ExportOptions? options = null)
	{
		throw new NotImplementedException();
	}

	public List<ExportFormat> GetSupportedFormats()
	{
		return new List<ExportFormat>
		{
			new ExportFormat { Name = "HTML", Extension = ".html", Description = "HTML 網頁格式", IsEnabled = true },
			new ExportFormat { Name = "PDF", Extension = ".pdf", Description = "PDF 文件格式", IsEnabled = true },
			new ExportFormat { Name = "DOCX", Extension = ".docx", Description = "Microsoft Word 格式", IsEnabled = false },
			new ExportFormat { Name = "ODF", Extension = ".odt", Description = "OpenDocument 格式", IsEnabled = false }
		};
	}
}

