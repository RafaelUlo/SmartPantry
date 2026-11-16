using Microsoft.Extensions.Logging;
using SmartPantry.Infrastructure.SqlLite;
using ZXing.Net.Maui.Controls;

namespace SmartPantry.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSmartPantrySqlite(
            Path.Combine(FileSystem.AppDataDirectory, "smartpantry.db"),
            Path.Combine(FileSystem.AppDataDirectory, "households.json"));
        return builder.Build();
	}
}



