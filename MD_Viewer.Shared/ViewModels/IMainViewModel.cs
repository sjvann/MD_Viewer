using MD_Viewer.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MD_Viewer.ViewModels;

public interface IMainViewModel : INotifyPropertyChanged
{
    string EditButtonText { get; }
    bool CanSave { get; }
	    bool CanReload { get; }
	    string CurrentFileDisplayName { get; }
	    bool HasUnsavedChanges { get; }
    string? SaveMessage { get; }
    bool IsExporting { get; }
    string? ExportMessage { get; }
    bool ShowExportMenu { get; set; }

    bool IncludePageNumbers { get; set; }
    PdfPageSize ExportPageSize { get; set; }
    PdfPageOrientation ExportOrientation { get; set; }

    IEnumerable<ExportFormat> SupportedExportFormats { get; }

    bool IsEditMode { get; }

    bool CanExport { get; }

    IPreviewViewModel? PreviewViewModel { get; }
    IEditViewModel? EditViewModel { get; }

    void ToggleEditMode();
    Task SaveFileAsync();
	    Task ReloadFileAsync();
    void ToggleExportMenu();
    void CloseExportMenu();
    void SetPageSizeA4();
    void SetPageSizeLetter();
    void SetPageSizeLegal();
    void SetOrientationPortrait();
    void SetOrientationLandscape();
    Task ExportAsync(ExportFormat? format);
}
