using MD_Viewer.Models;
using MD_Viewer.ViewModels;

namespace MD_Viewer;

public partial class MainPage : ContentPage
{
	private readonly MainViewModel _viewModel;

	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
		
		// 設定 FileTreeView 的 BindingContext
		FileTreeViewControl.BindingContext = viewModel.FileTreeViewModel;
		
		// 設定 PreviewView 的 BindingContext（預覽模式）
		PreviewViewControl.BindingContext = viewModel.PreviewViewModel;
		
		// 設定 EditView 的 BindingContext
		EditViewControl.BindingContext = viewModel.EditViewModel;
		
		// 設定 EditModePreviewView 的 BindingContext（編輯模式）
		EditModePreviewViewControl.BindingContext = viewModel.PreviewViewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// 應用程式啟動時自動載入磁碟列表
		try
		{
			await _viewModel.FileTreeViewModel.LoadDrivesAsync();
		}
		catch (Exception ex)
		{
			// LoadDrivesAsync 內部已有錯誤處理，此處僅作為額外保護
			// 如果內部錯誤處理失敗，這裡可以記錄或顯示錯誤
			System.Diagnostics.Debug.WriteLine($"載入磁碟列表時發生錯誤: {ex.Message}");
		}
	}

	/// <summary>
	/// 處理匯出格式選擇
	/// </summary>
	private async void OnExportFormatTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Grid grid && grid.BindingContext is ExportFormat format)
		{
			await _viewModel.ExportAsync(format);
		}
	}
}
