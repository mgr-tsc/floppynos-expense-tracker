using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class CardDetailPageModel : ObservableObject
{
    private CardDto? _card;
    private readonly CardRepository _cardRepository;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<CardDetailPageModel> _logger;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _symbol = string.Empty;
    [ObservableProperty] private string _lastDigits = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private List<CardDto> _cards = [];
    [ObservableProperty] private string _cardsCountText = "0 REGISTERED";
    [ObservableProperty] private string _pageTitle = "ADD CARD";
    [ObservableProperty] private bool _isCardsExpanded = true;
    [ObservableProperty] private bool _isCardsEmpty = true;

    public List<string> CardNetworks { get; } = ["Visa", "MasterCard", "American Express"];

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

    public CardDetailPageModel(
        CardRepository cardRepository,
        ModalErrorHandler errorHandler,
        ILogger<CardDetailPageModel> logger)
    {
        _cardRepository = cardRepository;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    [RelayCommand]
    private async Task Appearing() => await LoadCardsAsync();

    [RelayCommand]
    private void ToggleCards() => IsCardsExpanded = !IsCardsExpanded;

    [RelayCommand]
    private async Task SelectCardForEdit(CardDto card) => await LoadCard(card.Id);

    [RelayCommand]
    private async Task DeleteCardInList(CardDto card)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Card",
            $"Delete {card.Symbol} {card.Name}?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            _logger.LogInformation("Deleting card ID: {Id}", card.Id);
            await _cardRepository.DeleteAsync(card.Id);

            if (_card?.Id == card.Id) ResetForm();

            await LoadCardsAsync();
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
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => ResetForm();

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

            Name = _card.Name;
            Symbol = _card.Symbol;
            LastDigits = _card.LastDigits.ToString("D4");
            SelectedNetworkIndex = CardNetworks.IndexOf(_card.Symbol);
            PageTitle = "EDIT CARD";

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

        if (string.IsNullOrWhiteSpace(LastDigits) || LastDigits.Length != 4 || !int.TryParse(LastDigits, out int digits))
        {
            ErrorMessage = "Please enter exactly 4 digits";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _card ??= new CardDto();
            _card.Name = Name;
            _card.Symbol = Symbol;
            _card.LastDigits = digits;

            _logger.LogInformation("Saving card: {Name}", Name);
            await _cardRepository.SaveAsync(_card);

            await LoadCardsAsync();
            ResetForm();
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

    private async Task LoadCardsAsync()
    {
        try
        {
            var cards = await _cardRepository.ListAsync();
            Cards = cards;
            CardsCountText = $"{cards.Count} REGISTERED";
            IsCardsEmpty = cards.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cards");
        }
    }

    private void ResetForm()
    {
        _card = new CardDto();
        Name = string.Empty;
        Symbol = string.Empty;
        LastDigits = string.Empty;
        SelectedNetworkIndex = -1;
        PageTitle = "ADD CARD";
        ErrorMessage = string.Empty;
    }
}
