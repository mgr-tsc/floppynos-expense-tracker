using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;

namespace ExpenseTracker.PageModels;

public partial class CardListPageModel : ObservableObject
{
    private readonly CardRepository _cardRepository;

    [ObservableProperty] private List<Card> _cards = [];

    [ObservableProperty] private Card? selectedCard;

    public CardListPageModel(CardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Cards = await _cardRepository.ListAsync();
    }

    [RelayCommand]
    Task? NavigateToCard(Card card)
        => card is null ? Task.CompletedTask : Shell.Current.GoToAsync($"card?id={card.ID}");

    [RelayCommand]
    Task AddCard()
        => Shell.Current.GoToAsync("card");
}
