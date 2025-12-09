using System.ComponentModel;

namespace MD_Viewer.ViewModels;

public interface IPreviewViewModel : INotifyPropertyChanged
{
    string RenderedHtml { get; }
    bool IsLoading { get; }
    string? ErrorMessage { get; }
    bool ShowWebView { get; }

    void UpdatePreview(string? markdown);
}
