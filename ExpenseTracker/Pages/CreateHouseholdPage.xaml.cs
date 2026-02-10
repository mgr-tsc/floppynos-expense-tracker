namespace ExpenseTracker.Pages;

public partial class CreateHouseholdPage : ContentPage
{
    public CreateHouseholdPage(CreateHouseholdPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}