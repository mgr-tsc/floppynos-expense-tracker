namespace ExpenseTracker.Pages.Controls;

public partial class FilterPanel : ContentView
{
    public event EventHandler? CloseRequested;

    public FilterPanel()
    {
        InitializeComponent();
        TranslationX = 320;   // off-screen right, matches WidthRequest
        IsVisible = false;
    }

    public async Task OpenAsync()
    {
        IsVisible = true;
        await this.TranslateToAsync(0, 0, 250, Easing.CubicOut);
    }

    public async Task CloseAsync()
    {
        await this.TranslateToAsync(320, 0, 200, Easing.CubicIn);
        IsVisible = false;
    }

    private void OnCloseButtonTapped(object? sender, TappedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);
}
