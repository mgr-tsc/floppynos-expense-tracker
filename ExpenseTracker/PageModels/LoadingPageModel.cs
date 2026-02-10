using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Services.Interfaces;

namespace ExpenseTracker.PageModels;

public partial class LoadingPageModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;
    private readonly ISigInInThirdParty _signIn;
    private readonly HouseholdRepository _householdRepository;

    public LoadingPageModel(
        SupabaseService supabaseService,
        ISigInInThirdParty signIn,
        HouseholdRepository householdRepository)
    {
        _supabaseService = supabaseService;
        _signIn = signIn;
        _householdRepository = householdRepository;
    }

    [RelayCommand]
    private async Task Appearing(CancellationToken ct)
    {
        await AppSettings.InitializeAsync();
        await _supabaseService.InitializeAsync();

        // Try to restore a previous session from stored tokens
        var sessionRestored = await _signIn.TryRestoreSessionAsync();
        if (sessionRestored)
        {
            await NavigateBasedOnHouseholdAsync();
        }
        else
        {
            await Shell.Current.GoToAsync("//signin");
        }
    }

    private async Task NavigateBasedOnHouseholdAsync()
    {
        var userId = _supabaseService.GetCurrentUserId();
        System.Diagnostics.Debug.WriteLine($"[LoadingPageModel] Current user ID: {userId}");

        if (string.IsNullOrEmpty(userId))
        {
            System.Diagnostics.Debug.WriteLine("[LoadingPageModel] No user ID, going to signin");
            await Shell.Current.GoToAsync("//signin");
            return;
        }

        var household = await _householdRepository.GetByUserIdAsync(userId);
        System.Diagnostics.Debug.WriteLine($"[LoadingPageModel] Household found: {household != null}");
        if (household != null)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadingPageModel] Household ID: {household.Id}, Code: {household.Code}");
        }

        await Shell.Current.GoToAsync(household != null ? "//main" : "//householdsetup");
    }
}
