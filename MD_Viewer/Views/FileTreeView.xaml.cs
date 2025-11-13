using MD_Viewer.Models;
using MD_Viewer.ViewModels;

namespace MD_Viewer.Views;

public partial class FileTreeView : ContentView
{
	public FileTreeView()
	{
		InitializeComponent();
	}

	/// <summary>
	/// 處理節點點擊事件
	/// </summary>
	private void OnNodeTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Grid grid && grid.BindingContext is FileNode node && BindingContext is FileTreeViewModel viewModel)
		{
			viewModel.SelectNode(node);
		}
	}

	/// <summary>
	/// 處理展開/收合按鈕點擊事件
	/// </summary>
	private async void OnExpandButtonClicked(object? sender, EventArgs e)
	{
		if (sender is Button button && button.BindingContext is FileNode node && BindingContext is FileTreeViewModel viewModel)
		{
			await viewModel.ExpandNodeAsync(node);
		}
	}
}

