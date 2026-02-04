using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;

namespace ExpenseTracker.PageModels;

public partial class CardListPageModel : ObservableObject
{
    [ObservableProperty] private List<CardDTO> _cards = [];

    [ObservableProperty] private CardDTO? selectedCard;
}
