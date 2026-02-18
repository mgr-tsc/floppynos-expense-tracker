using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class ExpenseDetailPageModel : ObservableObject, IQueryAttributable
{
    private ChargeDto? _expense;
    private long _householdId;

    private readonly ChargeRepository _chargeRepository;
    private readonly CardRepository _cardRepository;
    private readonly PolicyRepository _policyRepository;
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<ExpenseDetailPageModel> _logger;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private List<CardDto> _cards = [];

    [ObservableProperty]
    private int _cardIndex = -1;

    [ObservableProperty]
    private List<PolicyDto> _splitOptions = [];

    [ObservableProperty]
    private int _splitOptionIndex = -1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ExpenseDetailPageModel(
        ChargeRepository chargeRepository,
        CardRepository cardRepository,
        PolicyRepository policyRepository,
        HouseholdRepository householdRepository,
        SupabaseService supabaseService,
        ModalErrorHandler errorHandler,
        ILogger<ExpenseDetailPageModel> logger)
    {
        _chargeRepository = chargeRepository;
        _cardRepository = cardRepository;
        _policyRepository = policyRepository;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _expense = new ChargeDto();
        _logger.LogInformation("Creating new charge");
        LoadFormData().FireAndForgetSafeAsync(_errorHandler);
    }

    private async Task LoadFormData()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Load user's household
            var userId = _supabaseService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "No authenticated user";
                return;
            }

            var household = await _householdRepository.GetByUserIdAsync(userId);
            if (household is null)
            {
                ErrorMessage = "No household found. Please join or create one first.";
                return;
            }
            _householdId = household.Id;
            _logger.LogInformation("Household resolved: {Id}", _householdId);

            //load cards 
            Cards = await _cardRepository.ListAsync();
            // load policies
            SplitOptions = await _policyRepository.ListAvailableAsync();
            _logger.LogInformation("Loaded {CardCount} cards and {PolicyCount} policies", Cards.Count, SplitOptions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load form data");
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowPolicyInfo()
    {
        if (SplitOptionIndex < 0 || SplitOptionIndex >= SplitOptions.Count)
        {
            await Shell.Current.CurrentPage.DisplayAlert("Split Policy", "Select a policy first to see its details.", "OK");
            return;
        }

        var policy = SplitOptions[SplitOptionIndex];
        var message = !string.IsNullOrEmpty(policy.Description)
            ? policy.Description
            : $"User A pays {policy.UserAPercentage}% and User B pays {policy.UserBPercentage}%";

        await Shell.Current.CurrentPage.DisplayAlert(policy.DisplayLabel, message, "OK");
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Please enter a description";
            return;
        }

        if (Amount <= 0)
        {
            ErrorMessage = "Please enter a valid amount";
            return;
        }

        if (CardIndex < 0 || CardIndex >= Cards.Count)
        {
            ErrorMessage = "Please select a card";
            return;
        }

        if (SplitOptionIndex < 0 || SplitOptionIndex >= SplitOptions.Count)
        {
            ErrorMessage = "Please select a split policy";
            return;
        }

        if (_expense is null)
        {
            ErrorMessage = "Charge data is missing";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _expense.Description = Description;
            _expense.Amount = Amount;
            _expense.TransactionDate = Date;
            _expense.CardIdFk = Cards[CardIndex].Id;
            _expense.PolicyIdFk = SplitOptions[SplitOptionIndex].Id;
            _expense.UserIdFk = _supabaseService.GetCurrentUserId()!;
            _expense.CategoryIdFk = 1; // Default "General" category
            _expense.HouseholdIdFk = _householdId;

            _logger.LogInformation("Saving charge: {Description}, Amount: {Amount}, Card: {CardId}, Policy: {PolicyId}, Household: {HouseholdId}",
                Description, Amount, _expense.CardIdFk, _expense.PolicyIdFk, _householdId);

            await _chargeRepository.SaveAsync(_expense);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Charge saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save charge");
            ErrorMessage = "Failed to save charge. Please try again.";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
