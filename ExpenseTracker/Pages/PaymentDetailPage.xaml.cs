namespace ExpenseTracker.Pages;

public partial class PaymentDetailPage : ContentPage
{
    public PaymentDetailPage(PaymentDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnCancelTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
