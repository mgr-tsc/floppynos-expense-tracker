namespace ExpenseTracker.Pages;

public partial class CardDetailPage : ContentPage
{
    public CardDetailPage(CardDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
