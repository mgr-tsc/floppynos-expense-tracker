namespace ExpenseTracker.Pages.Controls;

public partial class HeaderBar : ContentView
{
    public HeaderBar()
    {
        InitializeComponent();
    }
    
    // Example on how to create a bindable property for a control.
    // This allows us to set the UserName property from XAML when using the HeaderBar control.
    #region Bindables

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public static readonly BindableProperty UserNameProperty = BindableProperty.Create(
        nameof(UserName),
        typeof(string),
        typeof(HeaderBar),
        defaultValue: string.Empty);
    #endregion
    
}