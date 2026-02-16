using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class CardListPageModel : ObservableObject
{
    private readonly CardRepository _cardRepository;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<CardListPageModel> _logger;

    [ObservableProperty]
    private List<CardDto> _cards = [];

    [ObservableProperty]
    private CardDto? _selectedCard;

    [ObservableProperty]
    private bool _isRefreshing;

    public CardListPageModel(
        CardRepository cardRepository,
        ModalErrorHandler errorHandler,
        ILogger<CardListPageModel> logger)
    {
        _cardRepository = cardRepository;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await LoadCards();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadCards();
        IsRefreshing = false;
    }

    private async Task LoadCards()
    {
        try
        {
            _logger.LogInformation("Loading cards");
            var cards = await _cardRepository.ListAsync();
            Cards = cards;
            _logger.LogInformation("Loaded {Count} cards", cards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cards");
            _errorHandler.HandleError(ex);
        }
    }

    [RelayCommand]
    private async Task AddCard()
    {
        await Shell.Current.GoToAsync("card");
    }

    [RelayCommand]
    private async Task NavigateToCard()
    {
        if (SelectedCard is null)
            return;

        await Shell.Current.GoToAsync($"card?id={SelectedCard.Id}");

        // Clear selection for next time
        SelectedCard = null;
    }
}
