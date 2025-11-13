using MD_Viewer.Models;
using MD_Viewer.ViewModels;

namespace MD_Viewer.Views;

public partial class TreeViewItem : ContentView
{
	public TreeViewItem()
	{
		InitializeComponent();
	}

	/// <summary>
	/// 處理節點點擊事件
	/// </summary>
	private void OnNodeTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Grid grid && grid.BindingContext is FileNode node)
		{
			// 向上查找 FileTreeView 的 BindingContext
			var parent = this.Parent;
			while (parent != null)
			{
				if (parent is FileTreeView fileTreeView && fileTreeView.BindingContext is FileTreeViewModel viewModel)
				{
					viewModel.SelectNode(node);
					return;
				}
				parent = parent.Parent;
			}
		}
	}

	/// <summary>
	/// 處理展開/收合按鈕點擊事件
	/// </summary>
	private async void OnExpandButtonClicked(object? sender, EventArgs e)
	{
		if (sender is Button button && BindingContext is FileNode node && node.Type == FileNodeType.Directory)
		{
			// 向上查找 FileTreeView 的 BindingContext
			var parent = this.Parent;
			while (parent != null)
			{
				if (parent is FileTreeView fileTreeView && fileTreeView.BindingContext is FileTreeViewModel viewModel)
				{
					await viewModel.ExpandNodeAsync(node);
					return;
				}
				parent = parent.Parent;
			}
		}
	}
}

