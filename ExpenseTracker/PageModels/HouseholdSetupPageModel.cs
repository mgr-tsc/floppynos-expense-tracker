using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class HouseholdSetupPageModel : ObservableObject
{
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<HouseholdSetupPageModel> _logger;

    // ── Join flow ──────────────────────────────────────────────
    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _joinErrorMessage = string.Empty;

    // ── Create flow ────────────────────────────────────────────
    [ObservableProperty]
    private string _householdName = string.Empty;

    [ObservableProperty]
    private string _generatedCode = string.Empty;

    [ObservableProperty]
    private bool _isCodeVisible;

    [ObservableProperty]
    private string _createErrorMessage = string.Empty;

    // ── Shared ─────────────────────────────────────────────────
    [ObservableProperty]
    private bool _isBusy;

    public HouseholdSetupPageModel(
        HouseholdRepository householdRepository,
        SupabaseService supabaseService,
        ILogger<HouseholdSetupPageModel> logger)
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
            JoinErrorMessage = "Please enter a code";
            return;
        }

        if (!long.TryParse(Code, out var codeValue))
        {
            JoinErrorMessage = "Invalid code format";
            return;
        }

        try
        {
            IsBusy = true;
            JoinErrorMessage = string.Empty;

            var household = await _householdRepository.GetByCodeAsync(codeValue);
            if (household is null)
            {
                JoinErrorMessage = "Invalid code. No household found.";
                return;
            }

            if (!string.IsNullOrEmpty(household.UserBIdFk))
            {
                JoinErrorMessage = "This household is already full.";
                return;
            }

            var userId = _supabaseService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                JoinErrorMessage = "Unable to get current user. Please sign in again.";
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
            JoinErrorMessage = "Failed to join household. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(HouseholdName))
        {
            CreateErrorMessage = "Please enter a household name";
            return;
        }

        try
        {
            IsBusy = true;
            CreateErrorMessage = string.Empty;

            var result = await _supabaseService.CreateHouseHoldTrackerAsync(HouseholdName);
            GeneratedCode = result.Item2;
            IsCodeVisible = true;

            _logger.LogInformation("Household created with code {Code}", GeneratedCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create household");
            CreateErrorMessage = "Failed to create household. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CopyCode()
    {
        if (!string.IsNullOrEmpty(GeneratedCode))
            await Clipboard.SetTextAsync(GeneratedCode);
    }

    [RelayCommand]
    private Task Continue() => Shell.Current.GoToAsync("//main");
}
