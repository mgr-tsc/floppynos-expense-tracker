namespace ExpenseTracker.Pages;

public partial class CardListPage : ContentPage
{
    public CardListPage(CardListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}
