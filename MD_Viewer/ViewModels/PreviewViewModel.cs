using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.ViewModels;
using System.Text.RegularExpressions;

namespace MD_Viewer.ViewModels;

/// <summary>
/// 預覽 ViewModel
/// </summary>
public partial class PreviewViewModel : ObservableObject, IPreviewViewModel
{
	private readonly IMarkdownService _markdownService;
	private readonly IMessenger _messenger;
	private readonly ILogger<PreviewViewModel> _logger;

	private string _renderedHtml = string.Empty;
	private bool _isLoading;
	private string? _errorMessage;

	public PreviewViewModel(
		IMarkdownService markdownService,
		IMessenger messenger,
		ILogger<PreviewViewModel> logger)
	{
		_markdownService = markdownService;
		_messenger = messenger;
		_logger = logger;
	}

	/// <summary>
	/// 渲染後的 HTML
	/// </summary>
	public string RenderedHtml
	{
		get => _renderedHtml;
		set
		{
			if (SetProperty(ref _renderedHtml, value))
			{
				OnPropertyChanged(nameof(ShowWebView));
				OnPropertyChanged(nameof(ShowEmptyState));
			}
		}
	}

	/// <summary>
	/// 是否載入中
	/// </summary>
	public bool IsLoading
	{
		get => _isLoading;
		set
		{
			if (SetProperty(ref _isLoading, value))
			{
				OnPropertyChanged(nameof(ShowWebView));
				OnPropertyChanged(nameof(ShowEmptyState));
			}
		}
	}

	/// <summary>
	/// 錯誤訊息
	/// </summary>
	public string? ErrorMessage
	{
		get => _errorMessage;
		set
		{
			if (SetProperty(ref _errorMessage, value))
			{
				OnPropertyChanged(nameof(ShowWebView));
				OnPropertyChanged(nameof(ShowEmptyState));
			}
		}
	}

	/// <summary>
	/// 是否顯示 WebView（有內容且非載入狀態且無錯誤）
	/// </summary>
	public bool ShowWebView => !string.IsNullOrEmpty(RenderedHtml) && !IsLoading && string.IsNullOrEmpty(ErrorMessage);

	/// <summary>
	/// 是否顯示空狀態（無內容且非載入狀態且無錯誤）
	/// </summary>
	public bool ShowEmptyState => string.IsNullOrEmpty(RenderedHtml) && !IsLoading && string.IsNullOrEmpty(ErrorMessage);

	/// <summary>
	/// 更新預覽命令
	/// </summary>
	public void UpdatePreview(string? markdown)
	{
		try
		{
			IsLoading = true;
			ErrorMessage = null;

			if (string.IsNullOrEmpty(markdown))
			{
				RenderedHtml = string.Empty;
				return;
			}

			var html = _markdownService.RenderToHtml(markdown);
			
			// 處理 Mermaid 圖表
			html = ProcessMermaidBlocks(html);
			
			RenderedHtml = WrapHtmlContent(html);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "預覽渲染失敗");
			ErrorMessage = $"預覽渲染失敗: {ex.Message}";
			RenderedHtml = string.Empty;
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>
	/// 處理 Mermaid 程式碼區塊，轉換為 Mermaid 可渲染的 div
	/// </summary>
	private string ProcessMermaidBlocks(string html)
	{
		// 將 <pre><code class="language-mermaid">...</code></pre> 
		// 轉換為 <div class="mermaid">...</div>
		var pattern = @"<pre><code class=""language-mermaid"">([\s\S]*?)</code></pre>";
		return Regex.Replace(html, pattern, match =>
		{
			var mermaidCode = match.Groups[1].Value;
			// 解碼 HTML 實體
			mermaidCode = System.Net.WebUtility.HtmlDecode(mermaidCode);
			return $"<div class=\"mermaid\">{mermaidCode}</div>";
		}, RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// 包裝 HTML 內容為完整的 HTML 文件（含 Mermaid 支援）
	/// </summary>
	private string WrapHtmlContent(string content)
	{
		if (string.IsNullOrEmpty(content))
			return string.Empty;

		// 檢查是否包含 Mermaid 圖表
		bool hasMermaid = content.Contains("class=\"mermaid\"", StringComparison.OrdinalIgnoreCase);

		// Mermaid 腳本（僅在需要時載入）
		var mermaidScript = hasMermaid ? @"
    <!-- Mermaid.js for diagram rendering -->
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js""></script>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            mermaid.initialize({ 
                startOnLoad: true,
                theme: 'default',
                securityLevel: 'loose',
                flowchart: {
                    useMaxWidth: true,
                    htmlLabels: true
                }
            });
        });
    </script>" : "";

		// 包裝為完整的 HTML 文件，包含 CSS 樣式
		return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        /* 基本 Markdown 樣式 */
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Microsoft YaHei', sans-serif;
            padding: 20px;
            line-height: 1.6;
            color: #333;
            max-width: 100%;
            word-wrap: break-word;
        }}
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
        }}
        h1 {{ font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h2 {{ font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h3 {{ font-size: 1.25em; }}
        h4 {{ font-size: 1em; }}
        h5 {{ font-size: 0.875em; }}
        h6 {{ font-size: 0.85em; color: #6a737d; }}
        p {{
            margin-top: 0;
            margin-bottom: 16px;
        }}
        ul, ol {{
            margin-top: 0;
            margin-bottom: 16px;
            padding-left: 2em;
        }}
        li {{
            margin-top: 0.25em;
        }}
        blockquote {{
            padding: 0 1em;
            color: #6a737d;
            border-left: 0.25em solid #dfe2e5;
            margin: 0;
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
        img {{
            max-width: 100%;
            height: auto;
        }}
        a {{
            color: #0366d6;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        hr {{
            height: 0.25em;
            padding: 0;
            margin: 24px 0;
            background-color: #e1e4e8;
            border: 0;
        }}
        /* 數學公式樣式 */
        .math {{
            font-family: 'Times New Roman', serif;
            font-style: italic;
        }}
        /* Mermaid 圖表樣式 */
        .mermaid {{
            text-align: center;
            margin: 16px 0;
            padding: 16px;
            background-color: #f8f9fa;
            border-radius: 6px;
        }}
        .mermaid svg {{
            max-width: 100%;
            height: auto;
        }}
    </style>
    {mermaidScript}
</head>
<body>
    {content}
</body>
</html>";
	}
}

