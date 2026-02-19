using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using ExpenseTracker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class MainPageModel : ObservableObject
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;
    private long _householdId;
    private List<ChargeDto> _allExpenses = [];
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<MainPageModel> _logger;
    private readonly ISigInInThirdParty _sigInInThirdParty;
    private readonly ChargeRepository _chargeRepository;
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;

    [ObservableProperty] private string _userName = string.Empty;

    [ObservableProperty] private List<ChargeDto> _expenses = [];

    [ObservableProperty] private BalanceData _balance = new();

    [ObservableProperty] bool _isBusy;

    [ObservableProperty] bool _isRefreshing;

    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");

    [ObservableProperty] private int _selectedStatusIndex = 1; // Default: Pending

    public MainPageModel(ModalErrorHandler errorHandler,
                        ILogger<MainPageModel> logger,
                        ISigInInThirdParty sigInInThirdParty,
                        ChargeRepository chargeRepository,
                        HouseholdRepository householdRepository,
                        SupabaseService supabaseService)
    {
        _sigInInThirdParty = sigInInThirdParty;
        _errorHandler = errorHandler;
        _logger = logger;
        _chargeRepository = chargeRepository;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _logger.LogDebug($"AppSettings.IsSupabaseConfigured={AppSettings.IsSupabaseConfigured}");
    }

    partial void OnSelectedStatusIndexChanged(int value) => FilterExpenses();

    private void FilterExpenses()
    {
        Expenses = SelectedStatusIndex switch
        {
            1 => _allExpenses.Where(e => string.Equals(e.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase)).ToList(),
            2 => _allExpenses.Where(e => string.Equals(e.Status?.Trim(), "approved", StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => _allExpenses.ToList(), // 0 = All
        };
    }

    [RelayCommand]
    private void NavigatedTo()
    {
        _isNavigatedTo = true;
        _logger.LogDebug("MainPageModel: NavigatedTo - user landed on dashboard");
    }

    [RelayCommand]
    private void NavigatedFrom() => _isNavigatedTo = false;

    [RelayCommand]
    private Task AddExpense() => Shell.Current.GoToAsync("expense");

    [RelayCommand]
    private Task AddPayment() => Shell.Current.GoToAsync("payment");

    [RelayCommand]
    private Task NavigateToExpense(ChargeDto charge) => Shell.Current.GoToAsync($"expense?id={charge.Id}");

    [RelayCommand]
    private async Task Appearing()
    {
        var user = _sigInInThirdParty.GetCurrentUserAsync();
        if (user is not null)
        {
            UserName = user.Name ?? user.Email ?? "User";
        }
        await LoadChargesAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadChargesAsync();
        IsRefreshing = false;
    }

    private async Task LoadChargesAsync()
    {
        try
        {
            IsBusy = true;
            await ResolveHouseholdAsync();
            if (_householdId > 0)
            {
                _allExpenses = await _chargeRepository.ListByHouseholdAsync(_householdId);
                FilterExpenses();
                await LoadBalanceAsync();
            }
            else
            {
                _allExpenses = [];
                Expenses = [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load charges");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResolveHouseholdAsync()
    {
        if (_householdId > 0) return;

        var userId = _supabaseService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var household = await _householdRepository.GetByUserIdAsync(userId);
        if (household is not null)
        {
            _householdId = household.Id;
        }
    }

    private async Task LoadBalanceAsync()
    {
        try
        {
            var balance = await _supabaseService.GetHouseholdBalanceAsync(_householdId);
            if (balance is not null)
            {
                Balance = balance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load balance");
        }
    }
}
