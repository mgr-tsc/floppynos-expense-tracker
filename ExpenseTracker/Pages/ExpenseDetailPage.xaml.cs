namespace ExpenseTracker.Pages;

public partial class ExpenseDetailPage : ContentPage
{
    public ExpenseDetailPage(ExpenseDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
