using MD_Viewer.Models;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.ViewModels;
using System.ComponentModel;

namespace MD_Viewer;

public partial class MainPage : ContentPage
{
	private readonly MainViewModel _viewModel;
	private readonly IMarkdownService _markdownService;
	private string _currentTheme = "淺色";

	// 主題配色定義 - 參考 VS Theme Pack 風格
	private static readonly Dictionary<string, ThemeColors> Themes = new()
	{
		// 基本主題
		["淺色"] = new ThemeColors("#FFFFFF", "#24292e", "#0366d6", "#f6f8fa", "Light"),
		["深色"] = new ThemeColors("#1e1e1e", "#d4d4d4", "#569cd6", "#2d2d2d", "Dark"),
		
		// VS Theme Pack 風格
		["One Dark Pro"] = new ThemeColors("#282c34", "#abb2bf", "#61afef", "#21252b", "OneDark"),
		["Dracula"] = new ThemeColors("#282a36", "#f8f8f2", "#bd93f9", "#44475a", "Dracula"),
		["Nord"] = new ThemeColors("#2e3440", "#d8dee9", "#88c0d0", "#3b4252", "Nord"),
		["Monokai"] = new ThemeColors("#272822", "#f8f8f2", "#66d9ef", "#3e3d32", "Monokai"),
		["Solarized Dark"] = new ThemeColors("#002b36", "#839496", "#268bd2", "#073642", "SolarizedDark"),
		["Solarized Light"] = new ThemeColors("#fdf6e3", "#657b83", "#268bd2", "#eee8d5", "SolarizedLight"),
		["GitHub Dark"] = new ThemeColors("#0d1117", "#c9d1d9", "#58a6ff", "#161b22", "GitHubDark"),
		["GitHub Light"] = new ThemeColors("#ffffff", "#24292f", "#0969da", "#f6f8fa", "GitHubLight"),
		
		// 護眼主題
		["護眼綠"] = new ThemeColors("#c7edcc", "#2c3e2c", "#1a5c1a", "#b8debb", "EyeCare"),
		["暖色米黃"] = new ThemeColors("#f5f0e1", "#5c5347", "#8b6914", "#e8e0d0", "Warm"),
		
		// 高對比
		["高對比"] = new ThemeColors("#000000", "#ffffff", "#ffff00", "#1a1a1a", "HighContrast"),
	};

	public MainPage(MainViewModel viewModel, IMarkdownService markdownService)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
		_markdownService = markdownService;
		
		// 設定預設主題
		ThemePicker.SelectedIndex = 0;
		
		// 監聽 PreviewViewModel 的 RenderedHtml 變更
		if (_viewModel.PreviewViewModel != null)
		{
			_viewModel.PreviewViewModel.PropertyChanged += OnPreviewViewModelPropertyChanged;
		}
		
		// 監聽 EditViewModel 的內容變更（用於即時預覽）
		if (_viewModel.EditViewModel != null)
		{
			_viewModel.EditViewModel.PropertyChanged += OnEditViewModelPropertyChanged;
		}
		
		// 監聯 MainViewModel 的模式變更
		_viewModel.PropertyChanged += OnMainViewModelPropertyChanged;
	}

	/// <summary>
	/// 匯出 PDF 按鈕點擊
	/// </summary>
	private async void OnExportPdfClicked(object? sender, EventArgs e)
	{
		var pdfFormat = _viewModel.SupportedExportFormats.FirstOrDefault(f => f.Extension == ".pdf");
		if (pdfFormat != null)
		{
			await _viewModel.ExportAsync(pdfFormat);
		}
	}

	/// <summary>
	/// 匯出 HTML 按鈕點擊
	/// </summary>
	private async void OnExportHtmlClicked(object? sender, EventArgs e)
	{
		var htmlFormat = _viewModel.SupportedExportFormats.FirstOrDefault(f => f.Extension == ".html");
		if (htmlFormat != null)
		{
			await _viewModel.ExportAsync(htmlFormat);
		}
	}

	/// <summary>
	/// 格式化按鈕點擊
	/// </summary>
	private void OnFormatClicked(object? sender, EventArgs e)
	{
		// 如果不是編輯模式，先切換到編輯模式
		if (!_viewModel.IsEditMode)
		{
			// 檢查是否有內容可以格式化
			if (string.IsNullOrWhiteSpace(_viewModel.CurrentFileContent))
			{
				_viewModel.FormatMessage = "請先開啟一個 Markdown 檔案";
				ClearFormatMessageAfterDelay();
				return;
			}
			
			// 切換到編輯模式
			_viewModel.ToggleEditMode();
		}

		if (_viewModel.EditViewModel == null)
		{
			_viewModel.FormatMessage = "編輯器未初始化";
			ClearFormatMessageAfterDelay();
			return;
		}

		try
		{
			var currentContent = _viewModel.EditViewModel.MarkdownContent;
			if (string.IsNullOrWhiteSpace(currentContent))
			{
				_viewModel.FormatMessage = "沒有可格式化的內容";
				ClearFormatMessageAfterDelay();
				return;
			}

			// 格式化 Markdown
			var formattedContent = _markdownService.FormatMarkdown(currentContent);
			
			// 檢查是否有變化
			if (formattedContent == currentContent)
			{
				_viewModel.FormatMessage = "內容已經是最佳格式";
				ClearFormatMessageAfterDelay();
				return;
			}
			
			// 更新編輯器內容
			_viewModel.EditViewModel.LoadContent(formattedContent);
			
			_viewModel.FormatMessage = "✓ 格式化完成";
			ClearFormatMessageAfterDelay();
		}
		catch (Exception ex)
		{
			_viewModel.FormatMessage = $"格式化失敗: {ex.Message}";
			ClearFormatMessageAfterDelay();
		}
	}

	/// <summary>
	/// 3 秒後清除格式化訊息
	/// </summary>
	private void ClearFormatMessageAfterDelay()
	{
		_ = Task.Delay(3000).ContinueWith(_ =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				_viewModel.FormatMessage = null;
			});
		});
	}

	private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainViewModel.IsEditMode))
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				// 當切換到編輯模式時，更新編輯預覽
				if (_viewModel.IsEditMode)
				{
					UpdateEditPreviewWebView();
				}
			});
		}
	}

	private void OnEditViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(IEditViewModel.MarkdownContent))
		{
			// 編輯內容變更時，更新即時預覽
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UpdateEditPreviewWebView();
			});
		}
	}

	private void UpdateEditPreviewWebView()
	{
		if (_viewModel.EditViewModel == null || !_viewModel.IsEditMode) return;
		
		var markdown = _viewModel.EditViewModel.MarkdownContent;
		if (string.IsNullOrEmpty(markdown))
		{
			EditPreviewWebView.Source = new HtmlWebViewSource
			{
				Html = "<html><body style='color:#666;padding:20px;'>開始編輯以預覽內容...</body></html>"
			};
			return;
		}
		
		// 使用 PreviewViewModel 渲染 HTML
		_viewModel.PreviewViewModel?.UpdatePreview(markdown);
		
		var html = _viewModel.PreviewViewModel?.RenderedHtml;
		if (!string.IsNullOrEmpty(html))
		{
			var theme = Themes.GetValueOrDefault(_currentTheme, Themes["淺色"]);
			var themedHtml = ApplyThemeToHtml(html, theme);
			
			EditPreviewWebView.Source = new HtmlWebViewSource
			{
				Html = themedHtml
			};
		}
	}

	private void OnThemeChanged(object? sender, EventArgs e)
	{
		if (ThemePicker.SelectedItem is string themeName && Themes.ContainsKey(themeName))
		{
			_currentTheme = themeName;
			var theme = Themes[themeName];
			
			// 更新預覽區域背景色
			PreviewContainer.BackgroundColor = Color.FromArgb(theme.Background);
			EmptyStateLabel.TextColor = Color.FromArgb(theme.Text);
			
			// 如果有內容，重新渲染以套用新主題
			if (_viewModel.PreviewViewModel != null && !string.IsNullOrEmpty(_viewModel.PreviewViewModel.RenderedHtml))
			{
				UpdateWebViewContent();
			}
			
			// 編輯模式下也更新預覽
			if (_viewModel.IsEditMode)
			{
				UpdateEditPreviewWebView();
			}
		}
	}

	private void OnPreviewViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(IPreviewViewModel.RenderedHtml))
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UpdateWebViewContent();
			});
		}
	}

	private void UpdateWebViewContent()
	{
		if (_viewModel.PreviewViewModel == null) return;
		
		var html = _viewModel.PreviewViewModel.RenderedHtml;
		if (string.IsNullOrEmpty(html)) return;
		
		// 取得當前主題配色
		var theme = Themes.GetValueOrDefault(_currentTheme, Themes["淺色"]);
		
		// 替換 HTML 中的顏色樣式
		var themedHtml = ApplyThemeToHtml(html, theme);
		
		PreviewWebView.Source = new HtmlWebViewSource
		{
			Html = themedHtml
		};
	}

	private static string ApplyThemeToHtml(string html, ThemeColors theme)
	{
		// 根據主題決定程式碼區塊樣式
		var codeTextColor = IsLightTheme(theme.Background) ? "#24292e" : "#e6e6e6";
		var codeBorderColor = IsLightTheme(theme.Background) ? "#e1e4e8" : "#404040";
		var blockquoteColor = IsLightTheme(theme.Background) ? "#6a737d" : "#8b949e";
		var hrColor = IsLightTheme(theme.Background) ? "#e1e4e8" : "#30363d";
		
		// 建立主題樣式覆蓋
		var themeStyle = $@"
		<style>
			body {{
				background-color: {theme.Background} !important;
				color: {theme.Text} !important;
			}}
			a {{
				color: {theme.Link} !important;
			}}
			a:hover {{
				color: {theme.Link} !important;
				opacity: 0.8;
			}}
			pre {{
				background-color: {theme.CodeBg} !important;
				border: 1px solid {codeBorderColor} !important;
			}}
			code {{
				background-color: {theme.CodeBg} !important;
				color: {codeTextColor} !important;
			}}
			pre code {{
				color: {codeTextColor} !important;
			}}
			table {{
				font-family: 'Consolas', 'Monaco', 'Courier New', monospace !important;
			}}
			table th {{
				background-color: {theme.CodeBg} !important;
				color: {theme.Text} !important;
				border-color: {codeBorderColor} !important;
				white-space: nowrap !important;
			}}
			table td {{
				border-color: {codeBorderColor} !important;
				white-space: nowrap !important;
			}}
			table tr:nth-child(2n) {{
				background-color: {theme.CodeBg} !important;
			}}
			h1, h2 {{
				border-bottom-color: {codeBorderColor} !important;
				color: {theme.Text} !important;
			}}
			h3, h4, h5, h6 {{
				color: {theme.Text} !important;
			}}
			blockquote {{
				border-left-color: {theme.Link} !important;
				color: {blockquoteColor} !important;
			}}
			hr {{
				background-color: {hrColor} !important;
			}}
			img {{
				border-radius: 4px;
			}}
			::selection {{
				background-color: {theme.Link}44 !important;
			}}
		</style>
		";
		
		// 在 </head> 前插入主題樣式
		if (html.Contains("</head>"))
		{
			return html.Replace("</head>", themeStyle + "</head>");
		}
		
		return html;
	}

	private static bool IsLightTheme(string backgroundColor)
	{
		// 簡單判斷是否為淺色主題（根據背景色亮度）
		if (backgroundColor.StartsWith("#"))
		{
			var hex = backgroundColor.TrimStart('#');
			if (hex.Length >= 6)
			{
				var r = Convert.ToInt32(hex.Substring(0, 2), 16);
				var g = Convert.ToInt32(hex.Substring(2, 2), 16);
				var b = Convert.ToInt32(hex.Substring(4, 2), 16);
				var brightness = (r * 299 + g * 587 + b * 114) / 1000;
				return brightness > 128;
			}
		}
		return true;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		try
		{
			await _viewModel.FileTreeViewModel.LoadDrivesAsync();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"載入磁碟列表錯誤: {ex.Message}");
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		
		if (_viewModel.PreviewViewModel != null)
		{
			_viewModel.PreviewViewModel.PropertyChanged -= OnPreviewViewModelPropertyChanged;
		}
		
		if (_viewModel.EditViewModel != null)
		{
			_viewModel.EditViewModel.PropertyChanged -= OnEditViewModelPropertyChanged;
		}
		
		_viewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
	}

	private async void OnExportFormatTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Grid grid && grid.BindingContext is ExportFormat format)
		{
			await _viewModel.ExportAsync(format);
		}
	}
}

/// <summary>
/// 主題配色定義
/// </summary>
public record ThemeColors(string Background, string Text, string Link, string CodeBg, string Name);
