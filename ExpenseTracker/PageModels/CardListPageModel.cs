using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class CardListPageModel : ObservableObject
{
    [ObservableProperty] private List<Card> _cards = [];

    [ObservableProperty] private Card? selectedCard;
}
