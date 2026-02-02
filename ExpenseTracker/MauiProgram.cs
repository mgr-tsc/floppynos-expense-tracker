using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace ExpenseTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
#if WINDOWS
				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
				{
					handler.PlatformView.SingleSelectionFollowsFocus = false;
				});
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        builder.Services.AddSingleton<ProfileRepository>();
        builder.Services.AddSingleton<CardRepository>();
        builder.Services.AddSingleton<ExpenseRepository>();
        builder.Services.AddSingleton<SeedDataService>();
        builder.Services.AddSingleton<ModalErrorHandler>();

        builder.Services.AddSingleton<LoadingPageModel>();
        builder.Services.AddSingleton<LoadingPage>();
        builder.Services.AddSingleton<SignInPageModel>();
        builder.Services.AddSingleton<SignInPage>();
        builder.Services.AddSingleton<MainPageModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<CardListPageModel>();
        builder.Services.AddSingleton<CardListPage>();
        builder.Services.AddSingleton<ManageProfilesPageModel>();
        builder.Services.AddSingleton<ManageProfilesPage>();

        builder.Services.AddTransientWithShellRoute<ExpenseDetailPage, ExpenseDetailPageModel>("expense");
        builder.Services.AddTransientWithShellRoute<CardDetailPage, CardDetailPageModel>("card");

        return builder.Build();
    }
}
