namespace ExpenseTracker.Pages;

public partial class SignInPage : ContentPage
{
    public SignInPage(SignInPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
