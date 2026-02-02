namespace ExpenseTracker.Pages;

public partial class ManageProfilesPage : ContentPage
{
    public ManageProfilesPage(ManageProfilesPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
