using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MD_Viewer.Services;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.Services.Platform;
using MD_Viewer.ViewModels;

namespace MD_Viewer;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// 註冊服務（暫時使用空實作，後續故事會實作）
		builder.Services.AddSingleton<IFileSystemService, FileSystemService>();
		builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
		builder.Services.AddSingleton<IExportService, ExportService>();

		// 註冊平台服務（條件編譯）
#if WINDOWS
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.Windows.WindowsFileSystem>();
		builder.Services.AddSingleton<IPlatformFilePicker, Platforms.Windows.WindowsFilePicker>();
#elif MACCATALYST
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.MacCatalyst.MacFileSystem>();
		// TODO: 實作 Mac 平台的 IPlatformFilePicker
#elif ANDROID
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.Android.AndroidFileSystem>();
		// TODO: 實作 Android 平台的 IPlatformFilePicker
#endif

		// 註冊 ViewModels
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<FileTreeViewModel>();
		builder.Services.AddTransient<PreviewViewModel>();
		builder.Services.AddTransient<EditViewModel>();

		// 註冊 Messenger
		builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
