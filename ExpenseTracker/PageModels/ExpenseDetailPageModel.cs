using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
namespace ExpenseTracker.PageModels;

public partial class ExpenseDetailPageModel : ObservableObject, IQueryAttributable
{
    private ChargeDTO? _expense;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] private string _description = string.Empty;

    [ObservableProperty] private decimal _amount;

    [ObservableProperty] private DateTime _date = DateTime.Today;

    [ObservableProperty] private List<CardDTO> _cards = [];

    [ObservableProperty] private CardDTO? _selectedCard;

    [ObservableProperty] private int _cardIndex = -1;
    
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

    public ExpenseDetailPageModel(ModalErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
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
            _expense = new();
        }
    }
    

    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;
            // if (_expense.IsNullOrNew())
            // {
            //     _errorHandler.HandleError(new Exception($"Charge with id {id} could not be found."));
            //     return;
            // }

            Description = _expense.Description;
            Amount = _expense.Amount;
            //Date = _expense.Date;
            SelectedCard = Cards.FirstOrDefault(c => c.Id == _expense.CardIdFk);
            CardIndex = Cards.FindIndex(c => c.Id == _expense.CardIdFk);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
            //CanDelete = !_expense.IsNullOrNew();
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_expense is null)
        {
            _errorHandler.HandleError(new Exception("Charge is null. Cannot Save."));
            return;
        }

        _expense.Description = Description;
        _expense.Amount = Amount;
        // _expense.Date = Date;
        // _expense.CardId = SelectedCard?.Id ?? 0;
        // _expense.PayerProfileId = SelectedCard?.ProfileId ?? 0;

        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Charge saved");
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        // if (_expense.IsNullOrNew())
        // {
        //     await Shell.Current.GoToAsync("..");
        //     return;
        // }
        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Charge deleted");
    }
}
