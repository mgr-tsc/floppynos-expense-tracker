using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class SignInPageModel : ObservableObject
{
    private readonly ISigInInThirdParty _thirdParty;
    private readonly ILogger<SignInPageModel> _logger;
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;

    public SignInPageModel(
        ISigInInThirdParty thirdParty,
        ILogger<SignInPageModel> logger,
        HouseholdRepository householdRepository,
        SupabaseService supabaseService)
    {
        _thirdParty = thirdParty;
        _logger = logger;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
    }

    [RelayCommand]
    private async Task SignInWithGoogle()
    {
        try
        {
            _logger.LogDebug("SignInWithGoogle: starting third party sign-in");
            var result = await _thirdParty.SignInWithGoogleAsync();
            if (result is not null)
            {
                _logger.LogInformation("SignInWithGoogle: sign-in successful, checking household");
                await NavigateBasedOnHouseholdAsync();
            }
            else
            {
                _logger.LogWarning("SignInWithGoogle: sign-in returned null result");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SignInWithGoogle failed");
            Console.WriteLine(e);
        }
    }

    private async Task NavigateBasedOnHouseholdAsync()
    {
        var userId = _supabaseService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unable to get current user ID after sign-in");
            await Shell.Current.GoToAsync("//main"); // Fallback
            return;
        }

        var household = await _householdRepository.GetByUserIdAsync(userId);
        if (household != null)
        {
            _logger.LogInformation("User has household, navigating to main");
            await Shell.Current.GoToAsync("//main");
        }
        else
        {
            _logger.LogInformation("User has no household, navigating to setup");
            await Shell.Current.GoToAsync("//householdsetup");
        }
    }
}
