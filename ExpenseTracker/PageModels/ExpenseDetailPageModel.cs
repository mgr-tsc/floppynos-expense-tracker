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
    private readonly ChargeCategoryRepository _categoryRepository;
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
    private List<ChargeCategoryDto> _categories = [];

    [ObservableProperty]
    private int _categoryIndex = -1;

    [ObservableProperty]
    private List<PolicyDto> _splitOptions = [];

    [ObservableProperty]
    private int _splitOptionIndex = -1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _canApprove;

    [ObservableProperty]
    private bool _isExisting;

    [ObservableProperty]
    private bool _isOwner;

    /// <summary>True when form fields should be interactive (new charge or owner editing a rejected one).</summary>
    [ObservableProperty]
    private bool _isEditable;

    /// <summary>True when an existing rejected charge can be re-submitted by its owner.</summary>
    [ObservableProperty]
    private bool _canResubmit;

    [ObservableProperty]
    private string _pageTitle = "NEW CHARGE";

    public DateTime MaxDate => DateTime.Today;

    public ExpenseDetailPageModel(
        ChargeRepository chargeRepository,
        CardRepository cardRepository,
        PolicyRepository policyRepository,
        ChargeCategoryRepository categoryRepository,
        HouseholdRepository householdRepository,
        SupabaseService supabaseService,
        ModalErrorHandler errorHandler,
        ILogger<ExpenseDetailPageModel> logger)
    {
        _chargeRepository = chargeRepository;
        _cardRepository = cardRepository;
        _policyRepository = policyRepository;
        _categoryRepository = categoryRepository;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) && long.TryParse(idValue?.ToString(), out var chargeId) && chargeId > 0)
        {
            _logger.LogInformation("Loading existing charge {Id}", chargeId);
            LoadExistingCharge(chargeId).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            _expense = new ChargeDto();
            IsExisting = false;
            CanApprove = false;
            IsOwner = true;
            IsEditable = true;
            CanResubmit = false;
            _logger.LogInformation("Creating new charge");
            LoadFormData().FireAndForgetSafeAsync(_errorHandler);
        }
    }

    private async Task LoadExistingCharge(long chargeId)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _expense = await _chargeRepository.GetAsync(chargeId);
            if (_expense is null)
            {
                ErrorMessage = "Charge not found";
                return;
            }

            IsExisting = true;
            PageTitle = "VIEW CHARGE";

            // Populate form fields
            Description = _expense.Description ?? string.Empty;
            Amount = _expense.Amount;
            Date = _expense.TransactionDate ?? DateTime.Today;

            // Load dropdowns
            await LoadFormData();

            // Set selected indexes based on existing charge
            CardIndex = Cards.FindIndex(c => c.Id == _expense.CardIdFk);
            CategoryIndex = Categories.FindIndex(c => c.Id == _expense.CategoryIdFk);
            SplitOptionIndex = SplitOptions.FindIndex(p => p.Id == _expense.PolicyIdFk);

            // Determine ownership and editability
            var currentUserId = _supabaseService.GetCurrentUserId();
            IsOwner = _expense.IsOwnCharge(currentUserId ?? "");
            var isRejected = string.Equals(_expense.Status?.Trim(), "rejected", StringComparison.OrdinalIgnoreCase);
            CanApprove = !string.IsNullOrEmpty(currentUserId)
                         && !IsOwner
                         && string.Equals(_expense.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase);
            IsEditable = IsOwner && isRejected;
            CanResubmit = IsEditable;

            _logger.LogInformation("Loaded charge {Id}, IsOwner={IsOwner}, CanApprove={CanApprove}, CanResubmit={CanResubmit}",
                chargeId, IsOwner, CanApprove, CanResubmit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load charge {Id}", chargeId);
            ErrorMessage = "Failed to load charge";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFormData()
    {
        try
        {
            if (!IsExisting) IsBusy = true;
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

            Cards = await _cardRepository.ListAsync();
            Categories = await _categoryRepository.ListAsync();
            SplitOptions = await _policyRepository.ListAvailableAsync();
            _logger.LogInformation("Loaded {CardCount} cards, {CategoryCount} categories, {PolicyCount} policies",
                Cards.Count, Categories.Count, SplitOptions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load form data");
            _errorHandler.HandleError(ex);
        }
        finally
        {
            if (!IsExisting) IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        var name = await Shell.Current.CurrentPage.DisplayPromptAsync(
            "New Category", "Enter category name:", "Add", "Cancel", placeholder: "e.g., Groceries");

        if (string.IsNullOrWhiteSpace(name))
            return;

        try
        {
            var newCategory = new ChargeCategoryDto { Name = name.Trim() };
            var saved = await _categoryRepository.SaveAsync(newCategory);

            Categories = await _categoryRepository.ListAsync();
            CategoryIndex = Categories.FindIndex(c => c.Id == saved.Id);

            _logger.LogInformation("Category created: {Name} (ID: {Id})", saved.Name, saved.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category");
            _errorHandler.HandleError(ex);
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
    private async Task Approve()
    {
        if (_expense is null) return;

        try
        {
            IsBusy = true;
            _expense.Status = "approved";
            await _chargeRepository.SaveAsync(_expense);
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Charge approved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve charge");
            ErrorMessage = "Failed to approve charge";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Reject()
    {
        if (_expense is null) return;

        try
        {
            IsBusy = true;
            _expense.Status = "rejected";
            await _chargeRepository.SaveAsync(_expense);
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Charge rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject charge");
            ErrorMessage = "Failed to reject charge";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
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

        if (CategoryIndex < 0 || CategoryIndex >= Categories.Count)
        {
            ErrorMessage = "Please select a category";
            return;
        }

        if (SplitOptionIndex < 0 || SplitOptionIndex >= SplitOptions.Count)
        {
            ErrorMessage = "Please select a split policy";
            return;
        }

        if (Date > DateTime.Today)
        {
            ErrorMessage = "Date cannot be in the future";
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
            _expense.CategoryIdFk = Categories[CategoryIndex].Id;
            _expense.HouseholdIdFk = _householdId;

            // Re-submitting a rejected charge resets it to pending for partner review
            if (CanResubmit)
                _expense.Status = "pending";

            _logger.LogInformation("Saving charge: {Description}, Amount: {Amount}, CanResubmit={CanResubmit}",
                Description, Amount, CanResubmit);

            await _chargeRepository.SaveAsync(_expense);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync(CanResubmit ? "Charge re-submitted" : "Charge saved");
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
