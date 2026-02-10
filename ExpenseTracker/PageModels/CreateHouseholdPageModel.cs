using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class CreateHouseholdPageModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;
    private readonly ILogger<CreateHouseholdPageModel> _logger;

    [ObservableProperty]
    private string _householdName = string.Empty;

    [ObservableProperty]
    private string _generatedCode = string.Empty;

    [ObservableProperty]
    private bool _isCodeVisible;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreateHouseholdPageModel(SupabaseService supabaseService, ILogger<CreateHouseholdPageModel> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(HouseholdName))
        {
            ErrorMessage = "Please enter a household name";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _supabaseService.CreateHouseHoldTrackerAsync(HouseholdName);
            GeneratedCode = result.Item2;
            IsCodeVisible = true;

            _logger.LogInformation("Household created successfully with code {Code}", GeneratedCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create household");
            ErrorMessage = "Failed to create household. Please try again.";
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
        {
            await Clipboard.SetTextAsync(GeneratedCode);
        }
    }

    [RelayCommand]
    private Task Continue() => Shell.Current.GoToAsync("//main");
}