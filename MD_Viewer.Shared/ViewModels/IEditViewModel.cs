using System.ComponentModel;

namespace MD_Viewer.ViewModels;

public interface IEditViewModel : INotifyPropertyChanged
{
    string MarkdownContent { get; set; }
    bool ShowEditor { get; }
    bool ShowEmptyState { get; }
    void LoadContent(string content);
    void ClearContent();

    // Allow MainViewModel to set the preview VM for live preview synchronization
    IPreviewViewModel? PreviewViewModel { get; set; }
}
