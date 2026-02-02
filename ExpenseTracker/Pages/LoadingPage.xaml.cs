namespace ExpenseTracker.Pages;

public partial class LoadingPage : ContentPage
{
    public LoadingPage(LoadingPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
