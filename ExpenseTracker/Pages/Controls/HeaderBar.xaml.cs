namespace ExpenseTracker.Pages.Controls;

public partial class HeaderBar : ContentView
{
    public HeaderBar()
    {
        InitializeComponent();
    }

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

    public string UserInitials
    {
        get => (string)GetValue(UserInitialsProperty);
        set => SetValue(UserInitialsProperty, value);
    }

    public static readonly BindableProperty UserInitialsProperty = BindableProperty.Create(
        nameof(UserInitials),
        typeof(string),
        typeof(HeaderBar),
        defaultValue: string.Empty);

    #endregion
}
