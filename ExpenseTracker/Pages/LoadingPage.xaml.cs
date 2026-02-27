namespace ExpenseTracker.Pages;

public partial class LoadingPage : ContentPage
{
    private CancellationTokenSource? _spinnerCts;

    public LoadingPage(LoadingPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _spinnerCts = new CancellationTokenSource();
        StartSpinnerAsync(_spinnerCts.Token).FireAndForgetSafeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _spinnerCts?.Cancel();
        _spinnerCts = null;
    }

    private async Task StartSpinnerAsync(CancellationToken ct)
    {
        SpinnerArc.Rotation = 0;
        while (!ct.IsCancellationRequested)
        {
            await SpinnerArc.RotateTo(360, 1100, Easing.Linear);
            SpinnerArc.Rotation = 0;
        }
    }
}
