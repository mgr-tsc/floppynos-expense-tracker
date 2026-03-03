namespace ExpenseTracker.Pages;

public partial class MainPage : ContentPage
{
    private bool _filterPanelOpen = false;

    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
        model.FilterPanelCloseRequested += async (_, _) => await CloseFilterPanelAsync();
    }

    private async void OnFiltersTapped(object? sender, TappedEventArgs e)
    {
        if (_filterPanelOpen) return;
        await OpenFilterPanelAsync();
    }

    private async void OnScrimTapped(object? sender, TappedEventArgs e)
        => await CloseFilterPanelAsync();

    private async void OnFilterPanelCloseRequested(object? sender, EventArgs e)
        => await CloseFilterPanelAsync();

    private async Task OpenFilterPanelAsync()
    {
        _filterPanelOpen = true;
        FilterScrim.IsVisible = true;
        FilterScrim.InputTransparent = false;
        await Task.WhenAll(
            FilterScrim.FadeToAsync(0.45, 250, Easing.CubicOut),
            FilterPanelView.OpenAsync());
    }

    private async Task CloseFilterPanelAsync()
    {
        if (!_filterPanelOpen) return;
        _filterPanelOpen = false;
        await Task.WhenAll(
            FilterScrim.FadeToAsync(0, 200, Easing.CubicIn),
            FilterPanelView.CloseAsync());
        FilterScrim.IsVisible = false;
        FilterScrim.InputTransparent = true;
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
