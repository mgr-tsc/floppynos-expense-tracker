namespace ExpenseTracker.Pages;

public partial class HouseholdSetupPage : ContentPage
{
    public HouseholdSetupPage(HouseholdSetupPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}