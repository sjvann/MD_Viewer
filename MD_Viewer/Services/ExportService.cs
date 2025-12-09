using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
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
	/// 匯出為 HTML（使用預覽的完整 HTML 樣式）
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

			// 包裝為完整的 HTML 文件（與預覽一致的樣式）
			var fullHtml = WrapHtmlForExport(htmlContent, options);

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
	/// 包裝 HTML 內容為完整的匯出 HTML（與預覽樣式一致）
	/// </summary>
	private string WrapHtmlForExport(string content, ExportOptions? options)
	{
		var title = options?.Title ?? "Markdown Document";
		
		return $@"<!DOCTYPE html>
<html lang=""zh-TW"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{System.Web.HttpUtility.HtmlEncode(title)}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Microsoft JhengHei', 'Microsoft YaHei', sans-serif;
            padding: 40px;
            line-height: 1.6;
            color: #24292e;
            max-width: 900px;
            margin: 0 auto;
            background-color: #ffffff;
        }}
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
            color: #24292e;
        }}
        h1 {{ font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h2 {{ font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h3 {{ font-size: 1.25em; }}
        h4 {{ font-size: 1em; }}
        h5 {{ font-size: 0.875em; }}
        h6 {{ font-size: 0.85em; color: #6a737d; }}
        p {{ margin-top: 0; margin-bottom: 16px; }}
        ul, ol {{ margin-top: 0; margin-bottom: 16px; padding-left: 2em; }}
        li {{ margin-top: 0.25em; }}
        blockquote {{
            padding: 0 1em;
            color: #6a737d;
            border-left: 0.25em solid #dfe2e5;
            margin: 0 0 16px 0;
        }}
        code {{
            background-color: rgba(27, 31, 35, 0.05);
            padding: 0.2em 0.4em;
            border-radius: 3px;
            font-size: 85%;
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
        }}
        pre {{
            background-color: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            line-height: 1.45;
        }}
        pre code {{
            background-color: transparent;
            padding: 0;
            font-size: 100%;
        }}
        table {{
            border-collapse: collapse;
            margin-top: 0;
            margin-bottom: 16px;
            width: 100%;
        }}
        table th, table td {{
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
        }}
        table th {{
            background-color: #f6f8fa;
            font-weight: 600;
        }}
        table tr:nth-child(2n) {{
            background-color: #f6f8fa;
        }}
        img {{ max-width: 100%; height: auto; }}
        a {{ color: #0366d6; text-decoration: none; }}
        a:hover {{ text-decoration: underline; }}
        hr {{
            height: 0.25em;
            padding: 0;
            margin: 24px 0;
            background-color: #e1e4e8;
            border: 0;
        }}
        @media print {{
            body {{ padding: 20px; }}
            pre {{ white-space: pre-wrap; word-wrap: break-word; }}
        }}
    </style>
</head>
<body>
    {content}
</body>
</html>";
	}

	/// <summary>
	/// 匯出為 PDF（使用 Markdig AST 解析，保持與預覽一致的格式）
	/// </summary>
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

			// 使用 Markdig AST 解析並生成 PDF
			var pdfBytes = GeneratePdfFromMarkdownAst(markdown, options);

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
	/// 使用 Markdig AST 產生 PDF（更準確的格式映射）
	/// </summary>
	private byte[] GeneratePdfFromMarkdownAst(string markdown, ExportOptions? options)
	{
		QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

		// 使用 Markdig 解析為 AST
		var pipeline = new MarkdownPipelineBuilder()
			.UseAdvancedExtensions()
			.Build();
		var document = Markdown.Parse(markdown, pipeline);

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
				page.Size(isLandscape ? pageSize.Landscape() : pageSize);
				page.Margin(40);
				page.DefaultTextStyle(ts => ts.FontSize(11).FontFamily("Microsoft JhengHei"));

				// 頁首
				if (!string.IsNullOrWhiteSpace(options?.Title))
				{
					page.Header().PaddingBottom(10).Row(row =>
					{
						row.RelativeItem().Text(options!.Title).SemiBold().FontSize(10).FontColor(QuestPdfColors.Grey.Darken1);
					});
				}

				// 頁尾（頁碼）
				if (options?.IncludePageNumbers == true)
				{
					page.Footer().AlignCenter().Text(x =>
					{
						x.Span("第 ").FontSize(9);
						x.CurrentPageNumber().FontSize(9);
						x.Span(" 頁，共 ").FontSize(9);
						x.TotalPages().FontSize(9);
						x.Span(" 頁").FontSize(9);
					});
				}

				// 內容
				page.Content().PaddingVertical(5).Column(col =>
				{
					col.Spacing(8);
					RenderMarkdownDocument(col, document);
				});
			});
		}).GeneratePdf(stream);

		return stream.ToArray();
	}

	/// <summary>
	/// 遞迴渲染 Markdown 文件
	/// </summary>
	private void RenderMarkdownDocument(ColumnDescriptor col, MarkdownDocument document)
	{
		foreach (var block in document)
		{
			RenderBlock(col, block);
		}
	}

	/// <summary>
	/// 渲染單個區塊
	/// </summary>
	private void RenderBlock(ColumnDescriptor col, Block block)
	{
		switch (block)
		{
			case HeadingBlock heading:
				var fontSize = heading.Level switch
				{
					1 => 24,
					2 => 20,
					3 => 16,
					4 => 14,
					5 => 12,
					_ => 11
				};
				col.Item().PaddingTop(heading.Level <= 2 ? 16 : 8).Text(text =>
				{
					text.Span(GetInlineText(heading.Inline)).FontSize(fontSize).SemiBold();
				});
				if (heading.Level <= 2)
				{
					col.Item().PaddingTop(4).LineHorizontal(1).LineColor(QuestPdfColors.Grey.Lighten2);
				}
				break;

			case ParagraphBlock paragraph:
				col.Item().Text(text => RenderInlines(text, paragraph.Inline));
				break;

			case ListBlock list:
				RenderList(col, list);
				break;

			case FencedCodeBlock fencedCode:
				var codeText = string.Join("\n", fencedCode.Lines);
				col.Item()
					.Background(QuestPdfColors.Grey.Lighten4)
					.Border(1)
					.BorderColor(QuestPdfColors.Grey.Lighten2)
					.Padding(12)
					.Text(codeText)
					.FontFamily("Consolas")
					.FontSize(10);
				break;

			case CodeBlock code:
				var plainCode = string.Join("\n", code.Lines);
				col.Item()
					.Background(QuestPdfColors.Grey.Lighten4)
					.Padding(12)
					.Text(plainCode)
					.FontFamily("Consolas")
					.FontSize(10);
				break;

			case QuoteBlock quote:
				col.Item()
					.BorderLeft(3)
					.BorderColor(QuestPdfColors.Grey.Medium)
					.PaddingLeft(12)
					.Column(quoteCol =>
					{
						foreach (var child in quote)
						{
							if (child is ParagraphBlock p)
							{
								quoteCol.Item().Text(text =>
								{
									text.DefaultTextStyle(ts => ts.FontColor(QuestPdfColors.Grey.Darken1).Italic());
									RenderInlines(text, p.Inline);
								});
							}
						}
					});
				break;

			case ThematicBreakBlock:
				col.Item().PaddingVertical(8).LineHorizontal(2).LineColor(QuestPdfColors.Grey.Lighten2);
				break;

			case HtmlBlock:
				// HTML 區塊跳過（PDF 不支援）
				break;

			default:
				// 其他區塊嘗試取得文字
				break;
		}
	}

	/// <summary>
	/// 渲染清單
	/// </summary>
	private void RenderList(ColumnDescriptor col, ListBlock list)
	{
		int index = 1;
		foreach (var item in list)
		{
			if (item is ListItemBlock listItem)
			{
				col.Item().Row(row =>
				{
					var bullet = list.IsOrdered ? $"{index++}." : "•";
					row.ConstantItem(20).Text(bullet).FontSize(11);
					row.RelativeItem().Column(itemCol =>
					{
						foreach (var child in listItem)
						{
							if (child is ParagraphBlock p)
							{
								itemCol.Item().Text(text => RenderInlines(text, p.Inline));
							}
							else if (child is ListBlock nestedList)
							{
								itemCol.Item().PaddingLeft(15).Column(nestedCol => RenderList(nestedCol, nestedList));
							}
						}
					});
				});
			}
		}
	}

	/// <summary>
	/// 渲染行內元素
	/// </summary>
	private void RenderInlines(TextDescriptor text, ContainerInline? container)
	{
		if (container == null) return;

		foreach (var inline in container)
		{
			switch (inline)
			{
				case LiteralInline literal:
					text.Span(literal.Content.ToString());
					break;

				case EmphasisInline emphasis:
					var emphasisText = GetInlineText(emphasis);
					if (emphasis.DelimiterCount == 2)
					{
						text.Span(emphasisText).Bold();
					}
					else
					{
						text.Span(emphasisText).Italic();
					}
					break;

				case CodeInline code:
					text.Span(code.Content)
						.FontFamily("Consolas")
						.FontSize(10)
						.BackgroundColor(QuestPdfColors.Grey.Lighten4);
					break;

				case LinkInline link:
					var linkText = GetInlineText(link);
					text.Span(linkText).FontColor(QuestPdfColors.Blue.Medium).Underline();
					break;

				case LineBreakInline:
					text.Span("\n");
					break;

				default:
					// 其他行內元素
					break;
			}
		}
	}

	/// <summary>
	/// 取得行內元素的純文字
	/// </summary>
	private string GetInlineText(ContainerInline? container)
	{
		if (container == null) return string.Empty;

		var sb = new System.Text.StringBuilder();
		foreach (var inline in container)
		{
			switch (inline)
			{
				case LiteralInline literal:
					sb.Append(literal.Content.ToString());
					break;
				case EmphasisInline emphasis:
					sb.Append(GetInlineText(emphasis));
					break;
				case CodeInline code:
					sb.Append(code.Content);
					break;
				case LinkInline link:
					sb.Append(GetInlineText(link));
					break;
			}
		}
		return sb.ToString();
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
			new ExportFormat { Name = "PDF", Extension = ".pdf", Description = "PDF 文件格式", IsEnabled = true },
			new ExportFormat { Name = "HTML", Extension = ".html", Description = "HTML 網頁格式", IsEnabled = true },
		};
	}
}

