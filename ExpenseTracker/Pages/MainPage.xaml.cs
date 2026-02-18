namespace ExpenseTracker.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    private void StatusFilter_SelectionChanged(object? sender,
        Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        if (BindingContext is MainPageModel vm)
        {
            vm.SelectedStatusIndex = e.NewIndex ?? 0;
        }
    }
}
