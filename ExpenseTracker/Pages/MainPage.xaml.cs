namespace ExpenseTracker.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    private void OnChargesFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is MainPageModel vm && e.Parameter is string s && int.TryParse(s, out var idx))
            vm.SelectedStatusIndex = idx;
    }

    private void OnPaymentsFilterTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is MainPageModel vm && e.Parameter is string s && int.TryParse(s, out var idx))
            vm.SelectedPaymentStatusIndex = idx;
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        if (BindingContext is MainPageModel vm)
            await vm.RefreshCommand.ExecuteAsync(null);
    }
}
