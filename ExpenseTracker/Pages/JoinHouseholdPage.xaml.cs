namespace ExpenseTracker.Pages;

public partial class JoinHouseholdPage : ContentPage
{
    public JoinHouseholdPage(JoinHouseholdPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}