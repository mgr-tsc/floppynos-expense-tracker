using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using ExpenseTracker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class MainPageModel : ObservableObject
{
    private static readonly DateTime _defaultFromDate = new DateTime(DateTime.Now.Year, 1, 1);
    private static readonly DateTime _defaultToDate   = DateTime.Now.Date;

    private bool _isNavigatedTo;
    private bool _dataLoaded;
    private long _householdId;
    private HouseHoldDto? _household;
    private List<ChargeDto> _allExpenses = [];
    private List<PaymentDto> _allPayments = [];

    public event EventHandler? FilterPanelCloseRequested;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<MainPageModel> _logger;
    private readonly ISigInInThirdParty _sigInInThirdParty;
    private readonly ChargeRepository _chargeRepository;
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;
    private readonly PaymentRepository _paymentRepository;

    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _userInitials = string.Empty;
    [ObservableProperty] private string _householdName = "My Household";
    [ObservableProperty] private List<ChargeDto> _expenses = [];
    [ObservableProperty] private List<PaymentDto> _payments = [];
    [ObservableProperty] private BalanceData _balance = new();
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");
    [ObservableProperty] private int _selectedStatusIndex = 1;        // Default: Pending
    [ObservableProperty] private int _selectedPaymentStatusIndex = 1; // Default: Pending
    [ObservableProperty] private bool _isChargesExpanded = true;
    [ObservableProperty] private bool _isPaymentsExpanded = true;
    [ObservableProperty] private DateTime _filterFromDate  = new DateTime(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _filterToDate    = DateTime.Now.Date;
    [ObservableProperty] private string   _filterMinAmount = string.Empty;
    [ObservableProperty] private string   _filterMaxAmount = string.Empty;
    [ObservableProperty] private bool     _isFilterActive  = false;

    public MainPageModel(ModalErrorHandler errorHandler,
                        ILogger<MainPageModel> logger,
                        ISigInInThirdParty sigInInThirdParty,
                        ChargeRepository chargeRepository,
                        HouseholdRepository householdRepository,
                        SupabaseService supabaseService,
                        PaymentRepository paymentRepository)
    {
        _sigInInThirdParty = sigInInThirdParty;
        _errorHandler = errorHandler;
        _logger = logger;
        _chargeRepository = chargeRepository;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _paymentRepository = paymentRepository;
        _logger.LogDebug($"AppSettings.IsSupabaseConfigured={AppSettings.IsSupabaseConfigured}");
    }

    partial void OnSelectedStatusIndexChanged(int value) => FilterExpenses();
    partial void OnSelectedPaymentStatusIndexChanged(int value) => FilterPayments();

    private void FilterExpenses()
    {
        var filtered = SelectedStatusIndex switch
        {
            1 => _allExpenses.Where(e => string.Equals(e.Status?.Trim(), "pending",  StringComparison.OrdinalIgnoreCase)),
            2 => _allExpenses.Where(e => string.Equals(e.Status?.Trim(), "approved", StringComparison.OrdinalIgnoreCase)),
            3 => _allExpenses.Where(e => string.Equals(e.Status?.Trim(), "rejected", StringComparison.OrdinalIgnoreCase)),
            _ => _allExpenses.AsEnumerable(),
        };
        Expenses = ApplyDateAmountToExpenses(filtered).ToList();
    }

    private void FilterPayments()
    {
        var filtered = SelectedPaymentStatusIndex switch
        {
            1 => _allPayments.Where(p => string.Equals(p.Status?.Trim(), "pending",  StringComparison.OrdinalIgnoreCase)),
            2 => _allPayments.Where(p => string.Equals(p.Status?.Trim(), "approved", StringComparison.OrdinalIgnoreCase)),
            3 => _allPayments.Where(p => string.Equals(p.Status?.Trim(), "rejected", StringComparison.OrdinalIgnoreCase)),
            _ => _allPayments.AsEnumerable(),
        };
        Payments = ApplyDateAmountToPayments(filtered).ToList();
    }

    private IEnumerable<ChargeDto> ApplyDateAmountToExpenses(IEnumerable<ChargeDto> source)
    {
        var from = FilterFromDate;
        var to   = FilterToDate.Date.AddDays(1).AddTicks(-1);
        source = source.Where(e => { var d = e.TransactionDate ?? e.CreatedAt.DateTime; return d >= from && d <= to; });
        if (decimal.TryParse(FilterMinAmount, out var min)) source = source.Where(e => e.Amount >= min);
        if (decimal.TryParse(FilterMaxAmount, out var max)) source = source.Where(e => e.Amount <= max);
        return source;
    }

    private IEnumerable<PaymentDto> ApplyDateAmountToPayments(IEnumerable<PaymentDto> source)
    {
        var from = FilterFromDate;
        var to   = FilterToDate.Date.AddDays(1).AddTicks(-1);
        source = source.Where(p => { var d = p.PaymentDate ?? p.CreatedAt.DateTime; return d >= from && d <= to; });
        if (decimal.TryParse(FilterMinAmount, out var min)) source = source.Where(p => p.Amount >= min);
        if (decimal.TryParse(FilterMaxAmount, out var max)) source = source.Where(p => p.Amount <= max);
        return source;
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
    private void ToggleCharges() => IsChargesExpanded = !IsChargesExpanded;

    [RelayCommand]
    private void TogglePayments() => IsPaymentsExpanded = !IsPaymentsExpanded;

    [RelayCommand]
    private void ApplyFilters()
    {
        FilterExpenses();
        FilterPayments();
        IsFilterActive = FilterFromDate != _defaultFromDate || FilterToDate != _defaultToDate
                      || !string.IsNullOrWhiteSpace(FilterMinAmount) || !string.IsNullOrWhiteSpace(FilterMaxAmount);
        FilterPanelCloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ResetFilters()
    {
        FilterFromDate  = _defaultFromDate;
        FilterToDate    = _defaultToDate;
        FilterMinAmount = string.Empty;
        FilterMaxAmount = string.Empty;
        IsFilterActive  = false;
        FilterExpenses();
        FilterPayments();
        FilterPanelCloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private Task NavigateToExpense(ChargeDto charge) => Shell.Current.GoToAsync($"expense?id={charge.Id}");

    [RelayCommand]
    private Task NavigateToPayment(PaymentDto payment) => Shell.Current.GoToAsync($"payment?id={payment.Id}");

    [RelayCommand]
    private async Task Appearing()
    {
        var user = _sigInInThirdParty.GetCurrentUserAsync();
        if (user is not null)
        {
            UserName = user.Name ?? user.Email ?? "User";
            UserInitials = ComputeInitials(UserName);
        }
        _ = await LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            var refreshTask = LoadDashboardAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completed = await Task.WhenAny(refreshTask, timeoutTask);

            if (completed == timeoutTask)
                await AppShell.DisplayToastAsync("Refresh timed out. Please try again.");
            else if (!await refreshTask)
                await AppShell.DisplayToastAsync("Failed to refresh data. Please try again.");
        }
        catch (OperationCanceledException)
        {
            await AppShell.DisplayToastAsync("Refresh timed out. Please try again.");
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() => IsRefreshing = false);
        }
    }

    private async Task<bool> LoadDashboardAsync()
    {
        try
        {
            IsBusy = true;
            await ResolveHouseholdAsync();
            if (_householdId > 0)
            {
                await LoadChargesAsync();
                await LoadBalanceAsync();
                await LoadPaymentsAsync();
            }
            else
            {
                _allExpenses = [];
                _allPayments = [];
                Expenses = [];
                Payments = [];
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadChargesAsync()
    {
        _allExpenses = await _chargeRepository.ListByHouseholdAsync(_householdId);
        FilterExpenses();
    }

    private async Task LoadPaymentsAsync()
    {
        _allPayments = await _paymentRepository.ListByHouseholdAsync(_householdId);
        FilterPayments();
    }

    private async Task ResolveHouseholdAsync()
    {
        if (_householdId > 0) return;

        var userId = _supabaseService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var household = await _householdRepository.GetByUserIdAsync(userId);
        if (household is not null)
        {
            _household = household;
            _householdId = household.Id;
            HouseholdName = $"#{household.Code}";
        }
    }

    private async Task LoadBalanceAsync()
    {
        try
        {
            var balance = await _supabaseService.GetHouseholdBalanceAsync(_householdId);
            if (balance is not null)
            {
                var userId = _supabaseService.GetCurrentUserId();
                var currentUserIsA = _household?.UserAIdFk == userId;
                balance.CurrentUserIsA = currentUserIsA;
                balance.UserAName = currentUserIsA ? UserName : "Partner";
                balance.UserBName = currentUserIsA ? "Partner" : UserName;
                Balance = balance;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load balance");
        }
    }

    private static string ComputeInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length >= 2
            ? $"{char.ToUpper(words[0][0])}{char.ToUpper(words[1][0])}"
            : char.ToUpper(words[0][0]).ToString();
    }
}
