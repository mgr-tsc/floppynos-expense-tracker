using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;

namespace ExpenseTracker.PageModels;

public partial class MainPageModel : ObservableObject
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;
    private readonly ExpenseRepository _expenseRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly ModalErrorHandler _errorHandler;
    private readonly SeedDataService _seedDataService;

    [ObservableProperty] private List<Expense> _expenses = [];

    [ObservableProperty] private BalanceData _balance = new();

    [ObservableProperty] bool _isBusy;

    [ObservableProperty] bool _isRefreshing;

    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");

    public MainPageModel(SeedDataService seedDataService, ExpenseRepository expenseRepository,
        ProfileRepository profileRepository, ModalErrorHandler errorHandler)
    {
        _expenseRepository = expenseRepository;
        _profileRepository = profileRepository;
        _errorHandler = errorHandler;
        _seedDataService = seedDataService;
    }

    private async Task LoadData()
    {
        try
        {
            IsBusy = true;

            Expenses = await _expenseRepository.ListAsync();

            var profiles = await _profileRepository.ListAsync();
            var personA = profiles.Count > 0 ? profiles[0] : null;
            var personB = profiles.Count > 1 ? profiles[1] : null;

            decimal personAOwes = 0;
            decimal personBOwes = 0;

            foreach (var expense in Expenses)
            {
                var parts = expense.SplitPolicy.Split('/');
                if (parts.Length != 2) continue;

                if (!int.TryParse(parts[0], out int payerPercent) ||
                    !int.TryParse(parts[1], out int otherPercent)) continue;

                decimal otherShare = expense.Amount * otherPercent / 100;

                if (expense.PayerProfileID == personA?.ID)
                    personBOwes += otherShare;
                else if (expense.PayerProfileID == personB?.ID)
                    personAOwes += otherShare;
            }

            Balance = new BalanceData
            {
                PersonA = personA,
                PersonB = personB,
                PersonAOwes = personAOwes,
                PersonBOwes = personBOwes,
                NetBalance = personBOwes - personAOwes
            };
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task InitData(SeedDataService seedDataService)
    {
        bool isSeeded = Preferences.Default.ContainsKey("is_seeded");

        if (!isSeeded)
        {
            await seedDataService.LoadSeedDataAsync();
        }

        Preferences.Default.Set("is_seeded", true);
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            IsRefreshing = true;
            await LoadData();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private void NavigatedTo() =>
        _isNavigatedTo = true;

    [RelayCommand]
    private void NavigatedFrom() =>
        _isNavigatedTo = false;

    [RelayCommand]
    private async Task Appearing()
    {
        if (!_dataLoaded)
        {
            await InitData(_seedDataService);
            _dataLoaded = true;
            await Refresh();
        }
        else if (!_isNavigatedTo)
        {
            await Refresh();
        }
    }

    [RelayCommand]
    private Task AddExpense()
        => Shell.Current.GoToAsync("expense");

    [RelayCommand]
    private Task NavigateToExpense(Expense expense)
        => Shell.Current.GoToAsync($"expense?id={expense.ID}");
}
