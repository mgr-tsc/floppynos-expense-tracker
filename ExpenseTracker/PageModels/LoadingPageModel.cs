using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Services.Interfaces;

namespace ExpenseTracker.PageModels;

public partial class LoadingPageModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;
    private readonly ISigInInThirdParty _signIn;

    public LoadingPageModel(SupabaseService supabaseService, ISigInInThirdParty signIn)
    {
        _supabaseService = supabaseService;
        _signIn = signIn;
    }

    [RelayCommand]
    private async Task Appearing(CancellationToken ct)
    {
        await AppSettings.InitializeAsync();
        await _supabaseService.InitializeAsync();

        // Try to restore a previous session from stored tokens
        var sessionRestored = await _signIn.TryRestoreSessionAsync();
        if (sessionRestored)
            await Shell.Current.GoToAsync("//main");
        else
            await Shell.Current.GoToAsync("//signin");
    }
}
