namespace ExpenseTracker.Pages;

public partial class CardDetailPage : ContentPage
{
    public CardDetailPage(CardDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//main");
    }
}
