using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class CardDetailPageModel : ObservableObject, IQueryAttributable
{
    private CardDto? _card;
    private readonly CardRepository _cardRepository;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<CardDetailPageModel> _logger;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _symbol = string.Empty;

    [ObservableProperty]
    private string _lastDigits = string.Empty;

    // Card network options for dropdown
    public List<string> CardNetworks { get; } = new()
    {
        "Visa",
        "MasterCard",
        "American Express",
        "Discover",
        "Other"
    };

    private int _selectedNetworkIndex = -1;
    public int SelectedNetworkIndex
    {
        get => _selectedNetworkIndex;
        set
        {
            if (SetProperty(ref _selectedNetworkIndex, value) && value >= 0 && value < CardNetworks.Count)
            {
                Symbol = CardNetworks[value];
            }
        }
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

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

    public CardDetailPageModel(
        CardRepository cardRepository,
        ModalErrorHandler errorHandler,
        ILogger<CardDetailPageModel> logger)
    {
        _cardRepository = cardRepository;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            long id = Convert.ToInt64(query["id"]);
            LoadCard(id).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            // New card
            _card = new CardDto();
            CanDelete = false;
            _logger.LogInformation("Creating new card");
        }
    }

    private async Task LoadCard(long id)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Loading card ID: {Id}", id);
            _card = await _cardRepository.GetAsync(id);

            if (_card is null)
            {
                ErrorMessage = "Card not found";
                _logger.LogWarning("Card ID {Id} not found", id);
                return;
            }

            // Populate form fields
            Name = _card.Name;
            Symbol = _card.Symbol;
            LastDigits = _card.LastDigits.ToString();

            // Set network dropdown index
            SelectedNetworkIndex = CardNetworks.IndexOf(_card.Symbol);

            CanDelete = true;
            _logger.LogInformation("Card loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load card");
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
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Please enter a card name";
            return;
        }

        if (string.IsNullOrWhiteSpace(Symbol))
        {
            ErrorMessage = "Please select a card network";
            return;
        }

        if (string.IsNullOrWhiteSpace(LastDigits) || !int.TryParse(LastDigits, out int digits) || LastDigits.Length != 4)
        {
            ErrorMessage = "Please enter exactly 4 digits";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (_card is null)
            {
                ErrorMessage = "Card data is missing";
                return;
            }

            // Update card object
            _card.Name = Name;
            _card.Symbol = Symbol;
            _card.LastDigits = digits;

            _logger.LogInformation("Saving card: {Name}", Name);
            await _cardRepository.SaveAsync(_card);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Card saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save card");
            ErrorMessage = "Failed to save card. Please try again.";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        if (_card is null || _card.Id == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Card",
            $"Are you sure you want to delete {Symbol} {Name}?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Deleting card ID: {Id}", _card.Id);
            await _cardRepository.DeleteAsync(_card.Id);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Card deleted");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete card with existing charges");
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete card");
            ErrorMessage = "Failed to delete card. Please try again.";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
