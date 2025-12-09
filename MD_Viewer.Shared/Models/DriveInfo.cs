namespace MD_Viewer.Models;

/// <summary>
/// ºÏºÐ¸ê°T¼Ò«¬
/// </summary>
public class DriveInfo
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public long? TotalSize { get; set; }
    public long? AvailableSpace { get; set; }
}
