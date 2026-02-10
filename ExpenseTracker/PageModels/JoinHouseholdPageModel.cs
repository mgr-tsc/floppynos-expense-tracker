using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class JoinHouseholdPageModel : ObservableObject
{
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<JoinHouseholdPageModel> _logger;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public JoinHouseholdPageModel(
        HouseholdRepository householdRepository,
        SupabaseService supabaseService,
        ILogger<JoinHouseholdPageModel> logger)
    {
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task Join()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = "Please enter a code";
            return;
        }

        if (!long.TryParse(Code, out var codeValue))
        {
            ErrorMessage = "Invalid code format";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var household = await _householdRepository.GetByCodeAsync(codeValue);
            if (household == null)
            {
                ErrorMessage = "Invalid code. No household found.";
                return;
            }

            if (!string.IsNullOrEmpty(household.UserBIdFk))
            {
                ErrorMessage = "This household is already full.";
                return;
            }

            var userId = _supabaseService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Unable to get current user. Please sign in again.";
                return;
            }

            household.UserBIdFk = userId;
            await _householdRepository.SaveAsync(household);

            _logger.LogInformation("Successfully joined household {HouseholdId}", household.Id);
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join household");
            ErrorMessage = "Failed to join household. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}