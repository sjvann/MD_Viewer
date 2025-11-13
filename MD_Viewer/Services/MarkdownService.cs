using System.Text.RegularExpressions;
using Markdig;
using Microsoft.Extensions.Logging;
using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;

namespace MD_Viewer.Services;

/// <summary>
/// Markdown 服務實作
/// </summary>
public class MarkdownService : IMarkdownService
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

			// 基本格式檢查：檢查是否有未配對的標記
			// 注意：這是簡化版本的檢查，只計算方括號數量，不考慮上下文
			// 例如：[text](url) 中的方括號是配對的，但此檢查只驗證數量是否相等
			var openBrackets = markdown.Count(c => c == '[');
			var closeBrackets = markdown.Count(c => c == ']');
			if (openBrackets != closeBrackets)
			{
				errorMessage = "Markdown 格式錯誤：未配對的方括號";
				return false;
			}

			// 檢查是否有未配對的程式碼區塊標記
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
	/// <remarks>
	/// 此方法使用簡化的 YAML Front Matter 解析，支援基本的 key-value 格式。
	/// 不支援複雜的 YAML 格式（例如：多層嵌套、多行字串、引用等）。
	/// 如需完整的 YAML 支援，可考慮使用 YamlDotNet 套件。
	/// </remarks>
	public MarkdownMetadata ExtractMetadata(string markdown)
	{
		var metadata = new MarkdownMetadata();

		try
		{
			if (string.IsNullOrWhiteSpace(markdown))
				return metadata;

			// 檢查是否有 Front Matter（YAML 格式）
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

	/// <summary>
	/// 解析 YAML Front Matter
	/// </summary>
	/// <remarks>
	/// 此方法使用簡化的字串解析，支援基本的 key-value 格式。
	/// 支援的格式：
	/// - 簡單值：key: value
	/// - 引號值：key: "value" 或 key: 'value'
	/// - 列表格式：tags: [tag1, tag2] 或 tags: - tag1
	/// 不支援複雜的 YAML 格式（例如：多層嵌套、多行字串、引用等）。
	/// </remarks>
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

			// 移除引號
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
					// 處理列表格式：tags: [tag1, tag2] 或 tags: - tag1
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

		// 處理多行 tags 格式（- tag1 格式）
		var tagLines = lines.Where(l => l.Trim().StartsWith("- ", StringComparison.Ordinal));
		foreach (var tagLine in tagLines)
		{
			var tag = tagLine.Trim().Substring(2).Trim().Trim('"', '\'');
			if (!string.IsNullOrWhiteSpace(tag) && !metadata.Tags.Contains(tag))
				metadata.Tags.Add(tag);
		}
	}
}

