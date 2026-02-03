using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class MainPageModel : ObservableObject
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<MainPageModel> _logger;
    

    [ObservableProperty] private List<Charge> _expenses = [];

    [ObservableProperty] private BalanceData _balance = new();

    [ObservableProperty] bool _isBusy;

    [ObservableProperty] bool _isRefreshing;

    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");

    public MainPageModel(ModalErrorHandler errorHandler, ILogger<MainPageModel> logger)
    {
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
    private void NavigatedFrom() =>
        _isNavigatedTo = false;
    
    [RelayCommand]
    private Task AddExpense()
        => Shell.Current.GoToAsync("expense");

    [RelayCommand]
    private Task NavigateToExpense(Charge charge)
        => Shell.Current.GoToAsync($"charge?id={charge.Id}");
}
