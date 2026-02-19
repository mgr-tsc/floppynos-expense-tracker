namespace ExpenseTracker.Pages.Controls;

public partial class CustomTabBar : ContentView
{
    public CustomTabBar()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ActiveTabIndexProperty = BindableProperty.Create(
        nameof(ActiveTabIndex),
        typeof(int),
        typeof(CustomTabBar),
        defaultValue: 0,
        propertyChanged: OnActiveTabIndexChanged);

    public int ActiveTabIndex
    {
        get => (int)GetValue(ActiveTabIndexProperty);
        set => SetValue(ActiveTabIndexProperty, value);
    }

    private static void OnActiveTabIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomTabBar bar && bar.Tab0Pill is not null)
            bar.UpdateTabState((int)newValue);
    }

    private void UpdateTabState(int activeIndex)
    {
        var gold = new SolidColorBrush(Color.FromArgb("#D4AF37"));
        var transparent = new SolidColorBrush(Colors.Transparent);
        var darkText = Color.FromArgb("#111111");
        var mutedText = Color.FromArgb("#666666");

        Tab0Pill.Background = activeIndex == 0 ? gold : transparent;
        Tab1Pill.Background = activeIndex == 1 ? gold : transparent;
        Tab2Pill.Background = activeIndex == 2 ? gold : transparent;

        Tab0Icon.TextColor = activeIndex == 0 ? darkText : mutedText;
        Tab1Icon.TextColor = activeIndex == 1 ? darkText : mutedText;
        Tab2Icon.TextColor = activeIndex == 2 ? darkText : mutedText;

        Tab0Label.TextColor = activeIndex == 0 ? darkText : mutedText;
        Tab1Label.TextColor = activeIndex == 1 ? darkText : mutedText;
        Tab2Label.TextColor = activeIndex == 2 ? darkText : mutedText;
    }

    private async void OnHomeTabTapped(object sender, TappedEventArgs e)
    {
        ActiveTabIndex = 0;
        await Shell.Current.GoToAsync("//main");
    }

    private async void OnCardsTabTapped(object sender, TappedEventArgs e)
    {
        ActiveTabIndex = 1;
        await Shell.Current.GoToAsync("//cards");
    }

    private async void OnSettingsTabTapped(object sender, TappedEventArgs e)
    {
        ActiveTabIndex = 2;
        await Shell.Current.GoToAsync("//settings");
    }
}
