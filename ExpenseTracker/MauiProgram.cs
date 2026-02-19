using CommunityToolkit.Maui;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Resources.Fonts;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ExpenseTracker.Services;
using Supabase;
using ExpenseTracker.Services.Interfaces;

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
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUi.fontFamily);
            });

        // Enable console logging (so you can see logs in the terminal) and set minimum level to Debug for the POC.
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif
        
        builder.Services.AddSingleton<ModalErrorHandler>();
        // Register third-party sign-in implementation
        builder.Services.AddSingleton<ISigInInThirdParty, GoogleSignIn>();
        try
        {
            Task.Run(AppSettings.InitializeAsync).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize AppSettings: {ex.Message}");
        }

        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<CardRepository>();
        builder.Services.AddSingleton<ChargeRepository>();
        builder.Services.AddSingleton<PolicyRepository>();
        builder.Services.AddSingleton<ChargeCategoryRepository>();
        builder.Services.AddSingleton<HouseholdRepository>();
        builder.Services.AddSingleton<PaymentRepository>();

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
        builder.Services.AddSingleton<HouseholdSetupPageModel>();
        builder.Services.AddSingleton<HouseholdSetupPage>();
        builder.Services.AddSingleton<MockHomePage>();

        builder.Services.AddTransientWithShellRoute<ExpenseDetailPage, ExpenseDetailPageModel>("expense");
        builder.Services.AddTransientWithShellRoute<PaymentDetailPage, PaymentDetailPageModel>("payment");
        builder.Services.AddTransientWithShellRoute<CreateHouseholdPage, CreateHouseholdPageModel>("createhousehold");
        builder.Services.AddTransientWithShellRoute<JoinHouseholdPage, JoinHouseholdPageModel>("joinhousehold");
        builder.Services.AddTransientWithShellRoute<CardDetailPage, CardDetailPageModel>("card");

        return builder.Build();
    }
}
