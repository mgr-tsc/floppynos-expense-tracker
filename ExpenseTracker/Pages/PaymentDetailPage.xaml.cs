namespace ExpenseTracker.Pages;

public partial class PaymentDetailPage : ContentPage
{
    public PaymentDetailPage(PaymentDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}
