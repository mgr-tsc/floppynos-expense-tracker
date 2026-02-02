using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;

namespace ExpenseTracker.PageModels;

public partial class ExpenseDetailPageModel : ObservableObject, IQueryAttributable
{
    private Expense? _expense;
    private readonly ExpenseRepository _expenseRepository;
    private readonly CardRepository _cardRepository;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] private string _description = string.Empty;

    [ObservableProperty] private decimal _amount;

    [ObservableProperty] private DateTime _date = DateTime.Today;

    [ObservableProperty] private List<Card> _cards = [];

    [ObservableProperty] private Card? _selectedCard;

    [ObservableProperty] private int _cardIndex = -1;

    [ObservableProperty] private List<SplitOption> _splitOptions = SplitOption.Presets;

    [ObservableProperty] private SplitOption? _selectedSplitOption;

    [ObservableProperty] private int _splitOptionIndex;

    [ObservableProperty] bool _isBusy;

    private bool _canDelete;

    public bool CanDelete
    {
        get => _canDelete;
        set
        {
            _canDelete = value;
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }

    public ExpenseDetailPageModel(ExpenseRepository expenseRepository, CardRepository cardRepository,
        ModalErrorHandler errorHandler)
    {
        _expenseRepository = expenseRepository;
        _cardRepository = cardRepository;
        _errorHandler = errorHandler;
        _selectedSplitOption = _splitOptions[0];
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            int id = Convert.ToInt32(query["id"]);
            LoadData(id).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            LoadCards().FireAndForgetSafeAsync(_errorHandler);
            _expense = new();
        }
    }

    private async Task LoadCards() =>
        Cards = await _cardRepository.ListAsync();

    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _expense = await _expenseRepository.GetAsync(id);

            if (_expense.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Expense with id {id} could not be found."));
                return;
            }

            Description = _expense.Description;
            Amount = _expense.Amount;
            Date = _expense.Date;

            Cards = await _cardRepository.ListAsync();
            SelectedCard = Cards.FirstOrDefault(c => c.ID == _expense.CardID);
            CardIndex = Cards.FindIndex(c => c.ID == _expense.CardID);

            SelectedSplitOption = SplitOptions.FirstOrDefault(s => s.Label == _expense.SplitPolicy);
            SplitOptionIndex = SplitOptions.FindIndex(s => s.Label == _expense.SplitPolicy);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
            CanDelete = !_expense.IsNullOrNew();
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_expense is null)
        {
            _errorHandler.HandleError(new Exception("Expense is null. Cannot Save."));
            return;
        }

        _expense.Description = Description;
        _expense.Amount = Amount;
        _expense.Date = Date;
        _expense.SplitPolicy = SelectedSplitOption?.Label ?? "50/50";
        _expense.CardID = SelectedCard?.ID ?? 0;
        _expense.PayerProfileID = SelectedCard?.ProfileID ?? 0;
        await _expenseRepository.SaveItemAsync(_expense);

        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Expense saved");
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        if (_expense.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _expenseRepository.DeleteItemAsync(_expense);
        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Expense deleted");
    }
}
