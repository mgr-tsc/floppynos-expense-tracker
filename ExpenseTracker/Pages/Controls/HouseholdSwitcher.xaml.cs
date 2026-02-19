namespace ExpenseTracker.Pages.Controls;

public partial class HouseholdSwitcher : ContentView
{
    public HouseholdSwitcher()
    {
        InitializeComponent();
    }

    public string HouseholdName
    {
        get => (string)GetValue(HouseholdNameProperty);
        set => SetValue(HouseholdNameProperty, value);
    }

    public static readonly BindableProperty HouseholdNameProperty = BindableProperty.Create(
        nameof(HouseholdName),
        typeof(string),
        typeof(HouseholdSwitcher),
        defaultValue: string.Empty);
}
