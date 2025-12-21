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

