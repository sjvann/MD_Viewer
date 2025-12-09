using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MD_Viewer.Services.Interfaces;
using MD_Viewer.Services;
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
			})
			.ConfigureMauiHandlers(handlers =>
			{
#if WINDOWS
				handlers.AddHandler<BlazorWebView, BlazorWebViewHandler>();
#endif
			});

#if WINDOWS
		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif
#endif

		// 註冊核心服務
		builder.Services.AddSingleton<IFileSystemService, FileSystemService>();
		builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
		builder.Services.AddSingleton<IExportService, ExportService>();

		// 註冊平台服務
#if WINDOWS
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.Windows.WindowsFileSystem>();
		builder.Services.AddSingleton<IPlatformFilePicker, Platforms.Windows.WindowsFilePicker>();
#elif ANDROID
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.Android.AndroidFileSystem>();
		builder.Services.AddSingleton<IPlatformFilePicker, Platforms.Android.AndroidFilePicker>();
#elif IOS
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.iOS.iOSFileSystem>();
		builder.Services.AddSingleton<IPlatformFilePicker, Platforms.iOS.iOSFilePicker>();
#elif MACCATALYST
		builder.Services.AddSingleton<IPlatformFileSystem, Platforms.MacCatalyst.MacFileSystem>();
		builder.Services.AddSingleton<IPlatformFilePicker, Platforms.MacCatalyst.MacFilePicker>();
#endif

		// ViewModels - 註冊具體類別和介面
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<IMainViewModel>(sp => sp.GetRequiredService<MainViewModel>());
		
		builder.Services.AddSingleton<PreviewViewModel>();
		builder.Services.AddSingleton<IPreviewViewModel>(sp => sp.GetRequiredService<PreviewViewModel>());
		
		builder.Services.AddSingleton<EditViewModel>();
		builder.Services.AddSingleton<IEditViewModel>(sp => sp.GetRequiredService<EditViewModel>());
		
		builder.Services.AddTransient<FileTreeViewModel>();

		// Messenger
		builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

		// Pages
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
