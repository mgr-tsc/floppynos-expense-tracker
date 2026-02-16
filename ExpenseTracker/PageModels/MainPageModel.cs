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
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<MainPageModel> _logger;
    private readonly ISigInInThirdParty _sigInInThirdParty;
    
    [ObservableProperty] private string _userName = string.Empty;
    
    [ObservableProperty] private List<ChargeDto> _expenses = [];

    [ObservableProperty] private BalanceData _balance = new();

    [ObservableProperty] bool _isBusy;

    [ObservableProperty] bool _isRefreshing;

    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");

    public MainPageModel(ModalErrorHandler errorHandler,
                        ILogger<MainPageModel> logger, 
                        ISigInInThirdParty sigInInThirdParty)
    {
        _sigInInThirdParty = sigInInThirdParty;
        _errorHandler = errorHandler;
        _logger = logger;
        _logger.LogDebug($"AppSettings.IsSupabaseConfigured={AppSettings.IsSupabaseConfigured}");
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
    private Task NavigateToExpense(ChargeDto charge) => Shell.Current.GoToAsync($"charge?id={charge.Id}");

    [RelayCommand]
    private async Task Appearing()
    {
        var user = _sigInInThirdParty.GetCurrentUserAsync();
        if (user is not null)
        {
            UserName = user.Name ?? user.Email ?? "User";
        }
    }
}
